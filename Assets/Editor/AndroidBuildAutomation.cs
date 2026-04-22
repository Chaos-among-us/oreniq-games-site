#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AndroidBuildAutomation
{
    private const string OutputDirectoryRelativePath = "Builds/Android";
    private const string OutputFileName = "EndlessDodge-dev.apk";
    private const string PhoneOutputFileName = "EndlessDodge-phone.apk";
    private const string LocalSigningConfigRelativePath = "UserSettings/Android/release-signing.json";

    [MenuItem("Tools/Android/Build Development APK")]
    public static void BuildDevelopmentApk()
    {
        BuildApk(OutputFileName, BuildOptions.Development | BuildOptions.AllowDebugging, isDevelopmentBuild: true);
    }

    [MenuItem("Tools/Android/Build Phone Update APK")]
    public static void BuildPhoneApk()
    {
        BuildApk(PhoneOutputFileName, BuildOptions.None, isDevelopmentBuild: false);
    }

    private static void ApplyLocalAndroidSigningIfAvailable()
    {
        string projectRoot = GetProjectRoot();
        string configPath = Path.GetFullPath(Path.Combine(projectRoot, LocalSigningConfigRelativePath));

        if (!File.Exists(configPath))
        {
            Debug.LogWarning("Local Android signing config was not found. Falling back to Unity's current Android signing settings.");
            return;
        }

        AndroidReleaseSigningConfig config = JsonUtility.FromJson<AndroidReleaseSigningConfig>(File.ReadAllText(configPath));

        if (config == null ||
            string.IsNullOrWhiteSpace(config.keystoreName) ||
            string.IsNullOrWhiteSpace(config.keystorePassword) ||
            string.IsNullOrWhiteSpace(config.keyaliasName) ||
            string.IsNullOrWhiteSpace(config.keyaliasPassword))
        {
            throw new InvalidOperationException("Local Android signing config is missing required fields.");
        }

        string keystoreFullPath = Path.GetFullPath(Path.Combine(projectRoot, config.keystoreName));

        if (!File.Exists(keystoreFullPath))
            throw new FileNotFoundException("Configured Android keystore file was not found.", keystoreFullPath);

        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = config.keystoreName.Replace('\\', '/');
        PlayerSettings.Android.keystorePass = config.keystorePassword;
        PlayerSettings.Android.keyaliasName = config.keyaliasName;
        PlayerSettings.Android.keyaliasPass = config.keyaliasPassword;
        AssetDatabase.SaveAssets();
    }

    private static string GetProjectRoot()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
    }

    private static void BuildApk(string outputFileName, BuildOptions buildOptions, bool isDevelopmentBuild)
    {
        ApplyLocalAndroidSigningIfAvailable();

        string[] enabledScenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (enabledScenes.Length == 0)
            throw new InvalidOperationException("No enabled scenes were found in EditorBuildSettings.");

        string projectRoot = GetProjectRoot();
        string outputDirectory = Path.Combine(projectRoot, OutputDirectoryRelativePath);
        Directory.CreateDirectory(outputDirectory);

        string outputPath = Path.Combine(outputDirectory, outputFileName);

        EditorUserBuildSettings.development = isDevelopmentBuild;
        EditorUserBuildSettings.connectProfiler = false;
        EditorUserBuildSettings.allowDebugging = isDevelopmentBuild;
        EditorUserBuildSettings.buildAppBundle = false;

        BuildPlayerOptions playerBuildOptions = new BuildPlayerOptions
        {
            scenes = enabledScenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = buildOptions
        };

        BuildReport report = BuildPipeline.BuildPlayer(playerBuildOptions);

        if (report.summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException("Android build failed with result: " + report.summary.result);

        Debug.Log("BUILD_APK_PATH=" + outputPath.Replace('\\', '/'));
    }
}
#endif
