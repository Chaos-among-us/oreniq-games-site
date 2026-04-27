#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AndroidBuildUtility
{
    private const string OutputDirectoryRelativePath = "Builds/Android";
    private const string ProductionApplicationIdentifier = "com.oreniq.endlessdodge";
    private const string ProductionProductName = "Cavern Veerfall";
    private const string DebugApkFileName = "CavernVeerfall-debug.apk";
    private const string PlayInternalTestAabFileName = "CavernVeerfall-internal-test.aab";
    private const string SecondaryDebugApkFileName = "CavernVeerfall-test-debug.apk";
    private const string SecondaryApplicationIdentifier = "com.oreniq.endlessdodge.secondary";
    private const string SecondaryProductName = "Cavern Veerfall Test";
    private const string GeneratedAndroidProjectRelativePath = "Library/Bee/Android/Prj";
    private static readonly NamedBuildTarget AndroidNamedBuildTarget = NamedBuildTarget.Android;

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

    [MenuItem("Tools/Android/Build Play Internal Test AAB")]
    private static void BuildPlayInternalTestAabFromMenu()
    {
        if (BuildPlayInternalTestAab(interactive: true, out string outputPath))
        {
            EditorUtility.RevealInFinder(outputPath);
        }
    }

    [MenuItem("Tools/Android/Build Secondary Test APK")]
    private static void BuildSecondaryDebugApkFromMenu()
    {
        if (BuildSecondaryDebugApk(interactive: true, out string outputPath))
        {
            EditorUtility.RevealInFinder(outputPath);
        }
    }

    [MenuItem("Tools/Android/Build And Install Secondary Test APK")]
    private static void BuildAndInstallSecondaryDebugApkFromMenu()
    {
        if (!BuildSecondaryDebugApk(interactive: true, out string outputPath))
            return;

        InstallDebugApk(outputPath, interactive: true);
    }

    public static void BuildDebugApkBatchmode()
    {
        if (!BuildDebugApk(interactive: false, out string outputPath))
            throw new Exception("Android debug build failed.");

        UnityEngine.Debug.Log("Android debug APK created at: " + outputPath);
    }

    public static void BuildPlayInternalTestAabBatchmode()
    {
        if (!BuildPlayInternalTestAab(interactive: false, out string outputPath))
            throw new Exception("Android Play internal test AAB build failed.");

        UnityEngine.Debug.Log("Android Play internal test AAB created at: " + outputPath);
    }

    public static void BuildSecondaryDebugApkBatchmode()
    {
        if (!BuildSecondaryDebugApk(interactive: false, out string outputPath))
            throw new Exception("Android secondary debug build failed.");

        UnityEngine.Debug.Log("Android secondary debug APK created at: " + outputPath);
    }

    private static bool BuildDebugApk(bool interactive, out string outputPath)
    {
        return BuildDebugVariant(
            interactive,
            DebugApkFileName,
            ProductionApplicationIdentifier,
            ProductionProductName,
            "debug",
            out outputPath);
    }

    private static bool BuildSecondaryDebugApk(bool interactive, out string outputPath)
    {
        return BuildDebugVariant(
            interactive,
            SecondaryDebugApkFileName,
            SecondaryApplicationIdentifier,
            SecondaryProductName,
            "secondary debug",
            out outputPath);
    }

    private static bool BuildPlayInternalTestAab(bool interactive, out string outputPath)
    {
        string requestedOutputPath = Path.GetFullPath(Path.Combine(GetProjectRoot(), OutputDirectoryRelativePath, PlayInternalTestAabFileName));
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
        bool previousDevelopment = EditorUserBuildSettings.development;
        bool previousConnectProfiler = EditorUserBuildSettings.connectProfiler;
        bool previousAllowDebugging = EditorUserBuildSettings.allowDebugging;
        ScriptingImplementation previousScriptingBackend = PlayerSettings.GetScriptingBackend(AndroidNamedBuildTarget);
        AndroidArchitecture previousTargetArchitectures = PlayerSettings.Android.targetArchitectures;
        AndroidApplicationEntry previousApplicationEntry = PlayerSettings.Android.applicationEntry;
        int previousArchitectureValue = GetArchitectureValue(previousTargetArchitectures);
        string previousApplicationIdentifier = PlayerSettings.GetApplicationIdentifier(AndroidNamedBuildTarget);
        string previousProductName = PlayerSettings.productName;
        AndroidSigningResolutionStatus signingResolutionStatus = AndroidSigningConfigResolver.Resolve(
            out ResolvedAndroidReleaseSigningConfig resolvedSigningConfig,
            out string signingResolutionMessage);

        try
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                return Fail("Unity could not switch the active build target to Android.", interactive);

            if (signingResolutionStatus != AndroidSigningResolutionStatus.Success)
                return Fail(signingResolutionMessage, interactive);

            if (IsDebugBridgeSigning(resolvedSigningConfig))
            {
                return Fail(
                    "The resolved signing config uses the shared debug bridge, which must not be uploaded to Google Play.\n\n" +
                    "Create or recover the real Play upload keystore, place it in the shared signing folder, and update release-signing.json before building the Play AAB.",
                    interactive);
            }

            EditorUserBuildSettings.buildAppBundle = true;
            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.connectProfiler = false;
            EditorUserBuildSettings.allowDebugging = false;
            ConfigureSupportedReleaseBuildSettings(previousScriptingBackend, previousTargetArchitectures);
            ConfigureGameActivityApplicationEntry("Play internal test AAB");
            PlayerSettings.SetApplicationIdentifier(AndroidNamedBuildTarget, ProductionApplicationIdentifier);
            PlayerSettings.productName = ProductionProductName;
            ApplySigning(resolvedSigningConfig);

            UnityEngine.Debug.Log(
                "Using Android signing from " +
                resolvedSigningConfig.ConfigPath +
                " for the Play internal test AAB.");

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = enabledScenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result != BuildResult.Succeeded)
            {
                return Fail(
                    "Android Play internal test AAB build failed. Check the Unity Console or batchmode log for details.",
                    interactive);
            }

            if (!TryFinalizeOutputAab(report, requestedOutputPath, out outputPath, out string finalizeError))
                return Fail(finalizeError, interactive);

            UnityEngine.Debug.Log("Android Play internal test AAB created at: " + outputPath);

            if (interactive)
            {
                EditorUtility.DisplayDialog(
                    "Android Build",
                    "Play internal test AAB created:\n" + outputPath,
                    "OK");
            }

            return true;
        }
        catch (Exception ex)
        {
            return Fail("Android Play internal test AAB build failed: " + ex.Message, interactive);
        }
        finally
        {
            PlayerSettings.Android.useCustomKeystore = previousUseCustomKeystore;
            PlayerSettings.Android.keystoreName = previousKeystoreName;
            PlayerSettings.Android.keystorePass = previousKeystorePass;
            PlayerSettings.Android.keyaliasName = previousAliasName;
            PlayerSettings.Android.keyaliasPass = previousAliasPass;
            EditorUserBuildSettings.buildAppBundle = previousBuildAppBundle;
            EditorUserBuildSettings.development = previousDevelopment;
            EditorUserBuildSettings.connectProfiler = previousConnectProfiler;
            EditorUserBuildSettings.allowDebugging = previousAllowDebugging;
            PlayerSettings.SetScriptingBackend(AndroidNamedBuildTarget, previousScriptingBackend);
            PlayerSettings.SetArchitecture(AndroidNamedBuildTarget, previousArchitectureValue);
            PlayerSettings.Android.targetArchitectures = previousTargetArchitectures;
            PlayerSettings.Android.applicationEntry = previousApplicationEntry;
            PlayerSettings.SetApplicationIdentifier(AndroidNamedBuildTarget, previousApplicationIdentifier);
            PlayerSettings.productName = previousProductName;
            AssetDatabase.SaveAssets();
        }
    }

    private static bool BuildDebugVariant(
        bool interactive,
        string apkFileName,
        string applicationIdentifier,
        string productName,
        string buildLabel,
        out string outputPath)
    {
        string requestedOutputPath = Path.GetFullPath(Path.Combine(GetProjectRoot(), OutputDirectoryRelativePath, apkFileName));
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
        bool previousDevelopment = EditorUserBuildSettings.development;
        bool previousConnectProfiler = EditorUserBuildSettings.connectProfiler;
        bool previousAllowDebugging = EditorUserBuildSettings.allowDebugging;
        ScriptingImplementation previousScriptingBackend = PlayerSettings.GetScriptingBackend(AndroidNamedBuildTarget);
        AndroidArchitecture previousTargetArchitectures = PlayerSettings.Android.targetArchitectures;
        AndroidApplicationEntry previousApplicationEntry = PlayerSettings.Android.applicationEntry;
        int previousArchitectureValue = GetArchitectureValue(previousTargetArchitectures);
        string previousApplicationIdentifier = PlayerSettings.GetApplicationIdentifier(AndroidNamedBuildTarget);
        string previousProductName = PlayerSettings.productName;
        AndroidSigningResolutionStatus signingResolutionStatus = AndroidSigningConfigResolver.Resolve(
            out ResolvedAndroidReleaseSigningConfig resolvedSigningConfig,
            out string signingResolutionMessage);

        try
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                return Fail("Unity could not switch the active build target to Android.", interactive);

            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.development = true;
            EditorUserBuildSettings.connectProfiler = false;
            EditorUserBuildSettings.allowDebugging = true;
            ConfigureSupportedDebugBuildSettings(previousScriptingBackend, previousTargetArchitectures);
            ConfigureGameActivityApplicationEntry(buildLabel);
            PlayerSettings.SetApplicationIdentifier(AndroidNamedBuildTarget, applicationIdentifier);
            PlayerSettings.productName = productName;

            if (signingResolutionStatus == AndroidSigningResolutionStatus.Success)
            {
                ApplySigning(resolvedSigningConfig);
                UnityEngine.Debug.Log(
                    "Using shared Android signing from " +
                    resolvedSigningConfig.ConfigPath +
                    " for the " + buildLabel + " build.");
            }
            else if (signingResolutionStatus == AndroidSigningResolutionStatus.Invalid)
            {
                return Fail(signingResolutionMessage, interactive);
            }
            else
            {
                // Fall back to the machine-local debug keystore so day-to-day testing is never blocked.
                PlayerSettings.Android.useCustomKeystore = false;
                PlayerSettings.Android.keystoreName = string.Empty;
                PlayerSettings.Android.keystorePass = string.Empty;
                PlayerSettings.Android.keyaliasName = string.Empty;
                PlayerSettings.Android.keyaliasPass = string.Empty;
                UnityEngine.Debug.Log(
                    "Shared Android signing was not found. Falling back to machine-local debug signing.\n" +
                    signingResolutionMessage);
            }

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
                    "Android " + buildLabel + " build failed. Check the Unity Console or batchmode log for details.",
                    interactive);
            }

            if (!TryFinalizeOutputApk(report, requestedOutputPath, out outputPath, out string finalizeError))
                return Fail(finalizeError, interactive);

            UnityEngine.Debug.Log("Android " + buildLabel + " APK created at: " + outputPath);

            if (interactive)
            {
                EditorUtility.DisplayDialog(
                    "Android Build",
                    buildLabel + " APK created:\n" + outputPath,
                    "OK");
            }

            return true;
        }
        catch (Exception ex)
        {
            return Fail("Android " + buildLabel + " build failed: " + ex.Message, interactive);
        }
        finally
        {
            PlayerSettings.Android.useCustomKeystore = previousUseCustomKeystore;
            PlayerSettings.Android.keystoreName = previousKeystoreName;
            PlayerSettings.Android.keystorePass = previousKeystorePass;
            PlayerSettings.Android.keyaliasName = previousAliasName;
            PlayerSettings.Android.keyaliasPass = previousAliasPass;
            EditorUserBuildSettings.buildAppBundle = previousBuildAppBundle;
            EditorUserBuildSettings.development = previousDevelopment;
            EditorUserBuildSettings.connectProfiler = previousConnectProfiler;
            EditorUserBuildSettings.allowDebugging = previousAllowDebugging;
            PlayerSettings.SetScriptingBackend(AndroidNamedBuildTarget, previousScriptingBackend);
            PlayerSettings.SetArchitecture(AndroidNamedBuildTarget, previousArchitectureValue);
            PlayerSettings.Android.targetArchitectures = previousTargetArchitectures;
            PlayerSettings.Android.applicationEntry = previousApplicationEntry;
            PlayerSettings.SetApplicationIdentifier(AndroidNamedBuildTarget, previousApplicationIdentifier);
            PlayerSettings.productName = previousProductName;
            AssetDatabase.SaveAssets();
        }
    }

    private static void ConfigureSupportedDebugBuildSettings(
        ScriptingImplementation previousScriptingBackend,
        AndroidArchitecture previousTargetArchitectures)
    {
        if (previousScriptingBackend != ScriptingImplementation.IL2CPP)
        {
            PlayerSettings.SetScriptingBackend(AndroidNamedBuildTarget, ScriptingImplementation.IL2CPP);
            UnityEngine.Debug.Log(
                "Using IL2CPP for Android debug builds so the batch pipeline matches the supported " +
                "phone build configuration.");
        }

        int architectureValue = GetArchitectureValue(AndroidArchitecture.ARM64);
        PlayerSettings.SetArchitecture(AndroidNamedBuildTarget, architectureValue);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        UnityEngine.Debug.Log(
            "Using ARM64 as the Android debug-build architecture for device testing. " +
            "Previous architecture setting was: " + previousTargetArchitectures + ".");
    }

    private static void ConfigureSupportedReleaseBuildSettings(
        ScriptingImplementation previousScriptingBackend,
        AndroidArchitecture previousTargetArchitectures)
    {
        if (previousScriptingBackend != ScriptingImplementation.IL2CPP)
        {
            PlayerSettings.SetScriptingBackend(AndroidNamedBuildTarget, ScriptingImplementation.IL2CPP);
            UnityEngine.Debug.Log(
                "Using IL2CPP for Android release bundles so the Play build matches the supported " +
                "phone configuration.");
        }

        int architectureValue = GetArchitectureValue(AndroidArchitecture.ARM64);
        PlayerSettings.SetArchitecture(AndroidNamedBuildTarget, architectureValue);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        UnityEngine.Debug.Log(
            "Using ARM64 as the Android release-bundle architecture. " +
            "Previous architecture setting was: " + previousTargetArchitectures + ".");
    }

    private static void ConfigureGameActivityApplicationEntry(string buildLabel)
    {
        PlayerSettings.Android.applicationEntry = AndroidApplicationEntry.GameActivity;
        UnityEngine.Debug.Log(
            "Using GameActivity as the Android application entry point for the " +
            buildLabel +
            " build.");
    }

    private static int GetArchitectureValue(AndroidArchitecture architecture)
    {
        if (architecture == AndroidArchitecture.None)
            return 0;

        if (architecture == AndroidArchitecture.ARM64)
            return 1;

        return 2;
    }

    private static bool IsDebugBridgeSigning(ResolvedAndroidReleaseSigningConfig resolvedConfig)
    {
        if (resolvedConfig == null || resolvedConfig.Config == null)
            return true;

        string keystoreFileName = Path.GetFileName(resolvedConfig.KeystorePath);

        return string.Equals(resolvedConfig.Config.keyaliasName, "androiddebugkey", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(keystoreFileName, "debug.keystore", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(keystoreFileName, "shared-debug.keystore", StringComparison.OrdinalIgnoreCase);
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

    private static bool TryFinalizeOutputAab(
        BuildReport report,
        string requestedOutputPath,
        out string resolvedOutputPath,
        out string errorMessage)
    {
        resolvedOutputPath = requestedOutputPath;
        errorMessage = string.Empty;

        string builtAabPath = ResolveBuiltAabPath(report, requestedOutputPath);

        if (string.IsNullOrEmpty(builtAabPath))
        {
            errorMessage = "Android build completed, but the resulting AAB could not be located.";
            return false;
        }

        string normalizedSourcePath = Path.GetFullPath(builtAabPath);
        string normalizedOutputPath = Path.GetFullPath(requestedOutputPath);

        if (!string.Equals(normalizedSourcePath, normalizedOutputPath, StringComparison.OrdinalIgnoreCase))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(normalizedOutputPath) ?? string.Empty);
            File.Copy(normalizedSourcePath, normalizedOutputPath, true);
        }

        if (!File.Exists(normalizedOutputPath))
        {
            errorMessage = "Android build completed, but the AAB could not be copied to the output folder.";
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

    private static string ResolveBuiltAabPath(BuildReport report, string requestedOutputPath)
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
                string aabFromReportDirectory = GetNewestFile(normalizedReportOutputPath, "*.aab");

                if (!string.IsNullOrEmpty(aabFromReportDirectory))
                    return aabFromReportDirectory;
            }
        }

        string generatedProjectRoot = Path.Combine(GetProjectRoot(), GeneratedAndroidProjectRelativePath);

        if (!Directory.Exists(generatedProjectRoot))
            return string.Empty;

        return GetNewestFile(generatedProjectRoot, "*.aab");
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

        if (!TryResolveInstallTargetSerial(devicesOutput, out string targetSerial, out string targetDescription))
        {
            Fail(
                "ADB found multiple install targets and could not choose safely.\n\n" +
                devicesOutput +
                "\n\nDisconnect duplicate sessions or run `adb disconnect` for old wireless sessions, then try again.",
                interactive);
            return;
        }

        UnityEngine.Debug.Log("Installing Android debug APK to " + targetDescription + ".");
        string installOutput = RunProcess(adbPath, "-s \"" + targetSerial + "\" install -r \"" + apkPath + "\"");

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

    private static void ApplySigning(ResolvedAndroidReleaseSigningConfig resolvedConfig)
    {
        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = NormalizePathForUnity(resolvedConfig.KeystorePath);
        PlayerSettings.Android.keystorePass = resolvedConfig.Config.keystorePassword;
        PlayerSettings.Android.keyaliasName = resolvedConfig.Config.keyaliasName;
        PlayerSettings.Android.keyaliasPass = resolvedConfig.Config.keyaliasPassword;
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

    private static bool TryResolveInstallTargetSerial(
        string adbDevicesOutput,
        out string targetSerial,
        out string targetDescription)
    {
        targetSerial = string.Empty;
        targetDescription = string.Empty;

        string[] lines = adbDevicesOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        string[] connectedLines = lines
            .Where(line => line.EndsWith("\tdevice", StringComparison.OrdinalIgnoreCase) ||
                           line.IndexOf("\tdevice ", StringComparison.OrdinalIgnoreCase) >= 0)
            .ToArray();

        if (connectedLines.Length == 0)
            return false;

        if (connectedLines.Length == 1)
        {
            targetSerial = GetSerialFromDeviceLine(connectedLines[0]);
            targetDescription = targetSerial;
            return !string.IsNullOrWhiteSpace(targetSerial);
        }

        string[] wirelessTargets = connectedLines
            .Select(GetSerialFromDeviceLine)
            .Where(serial => !string.IsNullOrWhiteSpace(serial) && serial.Contains(":"))
            .ToArray();

        if (wirelessTargets.Length == 1)
        {
            targetSerial = wirelessTargets[0];
            targetDescription = targetSerial + " (wireless)";
            return true;
        }

        return false;
    }

    private static string GetSerialFromDeviceLine(string deviceLine)
    {
        if (string.IsNullOrWhiteSpace(deviceLine))
            return string.Empty;

        string[] tokens = deviceLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        return tokens.Length > 0 ? tokens[0] : string.Empty;
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

    private static string NormalizePathForUnity(string path)
    {
        return path.Replace('\\', '/');
    }
}
#endif
