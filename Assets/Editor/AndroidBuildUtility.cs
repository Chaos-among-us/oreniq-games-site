#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AndroidBuildUtility
{
    private const string OutputDirectoryRelativePath = "Builds/Android";
    private const string DebugApkFileName = "EndlessDodge1-debug.apk";
    private const string GeneratedAndroidProjectRelativePath = "Library/Bee/Android/Prj";

    [MenuItem("Tools/Android/Build Debug APK")]
    private static void BuildDebugApkFromMenu()
    {
        if (BuildDebugApk(interactive: true, out string outputPath))
        {
            EditorUtility.RevealInFinder(outputPath);
        }
    }

    [MenuItem("Tools/Android/Build And Install Debug APK")]
    private static void BuildAndInstallDebugApkFromMenu()
    {
        if (!BuildDebugApk(interactive: true, out string outputPath))
            return;

        InstallDebugApk(outputPath, interactive: true);
    }

    public static void BuildDebugApkBatchmode()
    {
        if (!BuildDebugApk(interactive: false, out string outputPath))
            throw new Exception("Android debug build failed.");

        UnityEngine.Debug.Log("Android debug APK created at: " + outputPath);
    }

    private static bool BuildDebugApk(bool interactive, out string outputPath)
    {
        string requestedOutputPath = Path.GetFullPath(Path.Combine(GetProjectRoot(), OutputDirectoryRelativePath, DebugApkFileName));
        outputPath = requestedOutputPath;
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);

        string[] enabledScenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (enabledScenes.Length == 0)
            return Fail("No enabled scenes were found in Build Settings.", interactive);

        bool previousUseCustomKeystore = PlayerSettings.Android.useCustomKeystore;
        string previousKeystoreName = PlayerSettings.Android.keystoreName;
        string previousKeystorePass = PlayerSettings.Android.keystorePass;
        string previousAliasName = PlayerSettings.Android.keyaliasName;
        string previousAliasPass = PlayerSettings.Android.keyaliasPass;
        bool previousBuildAppBundle = EditorUserBuildSettings.buildAppBundle;

        try
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                return Fail("Unity could not switch the active build target to Android.", interactive);

            // Force a debuggable APK build even when release signing has not been recovered on this machine yet.
            EditorUserBuildSettings.buildAppBundle = false;
            PlayerSettings.Android.useCustomKeystore = false;
            PlayerSettings.Android.keystoreName = string.Empty;
            PlayerSettings.Android.keystorePass = string.Empty;
            PlayerSettings.Android.keyaliasName = string.Empty;
            PlayerSettings.Android.keyaliasPass = string.Empty;

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = enabledScenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result != BuildResult.Succeeded)
            {
                return Fail(
                    "Android debug build failed. Check the Unity Console or batchmode log for details.",
                    interactive);
            }

            if (!TryFinalizeOutputApk(report, requestedOutputPath, out outputPath, out string finalizeError))
                return Fail(finalizeError, interactive);

            UnityEngine.Debug.Log("Android debug APK created at: " + outputPath);

            if (interactive)
            {
                EditorUtility.DisplayDialog(
                    "Android Build",
                    "Debug APK created:\n" + outputPath,
                    "OK");
            }

            return true;
        }
        catch (Exception ex)
        {
            return Fail("Android debug build failed: " + ex.Message, interactive);
        }
        finally
        {
            PlayerSettings.Android.useCustomKeystore = previousUseCustomKeystore;
            PlayerSettings.Android.keystoreName = previousKeystoreName;
            PlayerSettings.Android.keystorePass = previousKeystorePass;
            PlayerSettings.Android.keyaliasName = previousAliasName;
            PlayerSettings.Android.keyaliasPass = previousAliasPass;
            EditorUserBuildSettings.buildAppBundle = previousBuildAppBundle;
            AssetDatabase.SaveAssets();
        }
    }

    private static bool TryFinalizeOutputApk(
        BuildReport report,
        string requestedOutputPath,
        out string resolvedOutputPath,
        out string errorMessage)
    {
        resolvedOutputPath = requestedOutputPath;
        errorMessage = string.Empty;

        string builtApkPath = ResolveBuiltApkPath(report, requestedOutputPath);

        if (string.IsNullOrEmpty(builtApkPath))
        {
            errorMessage = "Android build completed, but the resulting APK could not be located.";
            return false;
        }

        string normalizedSourcePath = Path.GetFullPath(builtApkPath);
        string normalizedOutputPath = Path.GetFullPath(requestedOutputPath);

        if (!string.Equals(normalizedSourcePath, normalizedOutputPath, StringComparison.OrdinalIgnoreCase))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(normalizedOutputPath) ?? string.Empty);
            File.Copy(normalizedSourcePath, normalizedOutputPath, true);
        }

        if (!File.Exists(normalizedOutputPath))
        {
            errorMessage = "Android build completed, but the APK could not be copied to the output folder.";
            return false;
        }

        resolvedOutputPath = normalizedOutputPath;
        return true;
    }

    private static string ResolveBuiltApkPath(BuildReport report, string requestedOutputPath)
    {
        string normalizedRequestedPath = Path.GetFullPath(requestedOutputPath);

        if (File.Exists(normalizedRequestedPath))
            return normalizedRequestedPath;

        string reportOutputPath = report.summary.outputPath;

        if (!string.IsNullOrWhiteSpace(reportOutputPath))
        {
            string normalizedReportOutputPath = NormalizeBuildPath(reportOutputPath);

            if (File.Exists(normalizedReportOutputPath))
                return normalizedReportOutputPath;

            if (Directory.Exists(normalizedReportOutputPath))
            {
                string apkFromReportDirectory = GetNewestFile(normalizedReportOutputPath, "*-debug.apk");

                if (!string.IsNullOrEmpty(apkFromReportDirectory))
                    return apkFromReportDirectory;

                apkFromReportDirectory = GetNewestFile(normalizedReportOutputPath, "*.apk");

                if (!string.IsNullOrEmpty(apkFromReportDirectory))
                    return apkFromReportDirectory;
            }
        }

        string generatedProjectRoot = Path.Combine(GetProjectRoot(), GeneratedAndroidProjectRelativePath);

        if (!Directory.Exists(generatedProjectRoot))
            return string.Empty;

        string generatedDebugApk = GetNewestFile(generatedProjectRoot, "*-debug.apk");

        if (!string.IsNullOrEmpty(generatedDebugApk))
            return generatedDebugApk;

        return GetNewestFile(generatedProjectRoot, "*.apk");
    }

    private static string NormalizeBuildPath(string path)
    {
        if (Path.IsPathRooted(path))
            return Path.GetFullPath(path);

        return Path.GetFullPath(Path.Combine(GetProjectRoot(), path));
    }

    private static string GetNewestFile(string rootDirectory, string searchPattern)
    {
        if (!Directory.Exists(rootDirectory))
            return string.Empty;

        return Directory
            .EnumerateFiles(rootDirectory, searchPattern, SearchOption.AllDirectories)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault() ?? string.Empty;
    }

    private static void InstallDebugApk(string apkPath, bool interactive)
    {
        string adbPath = GetAdbPath();

        if (string.IsNullOrEmpty(adbPath) || !File.Exists(adbPath))
        {
            Fail("ADB was not found in the Unity Android SDK.", interactive);
            return;
        }

        string devicesOutput = RunProcess(adbPath, "devices");

        if (!HasConnectedDevice(devicesOutput))
        {
            Fail("No Android device detected. Plug in your phone, enable USB debugging, and accept the trust prompt.", interactive);
            return;
        }

        string installOutput = RunProcess(adbPath, "install -r \"" + apkPath + "\"");

        if (installOutput.IndexOf("Success", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            UnityEngine.Debug.Log("Installed Android debug APK via ADB.");

            if (interactive)
            {
                EditorUtility.DisplayDialog(
                    "Android Build",
                    "Installed debug APK on the connected device.",
                    "OK");
            }

            return;
        }

        Fail("ADB install did not report success.\n\n" + installOutput, interactive);
    }

    private static bool HasConnectedDevice(string adbDevicesOutput)
    {
        string[] lines = adbDevicesOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].EndsWith("\tdevice", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static string RunProcess(string fileName, string arguments)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(startInfo))
        {
            if (process == null)
                return string.Empty;

            string standardOutput = process.StandardOutput.ReadToEnd();
            string standardError = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return (standardOutput + Environment.NewLine + standardError).Trim();
        }
    }

    private static string GetAdbPath()
    {
        string sdkRoot = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines", "AndroidPlayer", "SDK");
        return Path.Combine(sdkRoot, "platform-tools", "adb.exe");
    }

    private static bool Fail(string message, bool interactive)
    {
        UnityEngine.Debug.LogError(message);

        if (interactive)
        {
            EditorUtility.DisplayDialog("Android Build", message, "OK");
        }

        return false;
    }

    private static string GetProjectRoot()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
    }
}
#endif
