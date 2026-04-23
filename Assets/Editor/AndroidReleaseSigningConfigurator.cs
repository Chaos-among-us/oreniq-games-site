#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class AndroidReleaseSigningConfigurator
{
    [InitializeOnLoadMethod]
    private static void TryApplyOnEditorLoad()
    {
        TryApply(interactiveFailure: false, logSuccess: false, warnIfConfigMissing: false);
    }

    [MenuItem("Tools/Android/Apply Local Release Signing")]
    private static void ApplyFromMenu()
    {
        if (TryApply(interactiveFailure: true, logSuccess: true, warnIfConfigMissing: true))
        {
            EditorUtility.DisplayDialog(
                "Android Release Signing",
                "Applied Android signing from the resolved shared/local signing config.",
                "OK");
        }
    }

    [MenuItem("Tools/Android/Create Local Signing Config Template")]
    private static void CreateLocalSigningConfigTemplate()
    {
        string templateFullPath = Path.GetFullPath(Path.Combine(
            AndroidSigningConfigResolver.GetProjectRoot(),
            AndroidSigningConfigResolver.LocalTemplateRelativePath));
        Directory.CreateDirectory(Path.GetDirectoryName(templateFullPath) ?? string.Empty);
        File.WriteAllText(templateFullPath, AndroidSigningConfigResolver.GetTemplateJson());

        EditorUtility.RevealInFinder(templateFullPath);
        EditorUtility.DisplayDialog(
            "Android Release Signing",
            "Created a local signing template in UserSettings/Android. Copy it to release-signing.json on this machine only, or use the shared external signing folder instead.",
            "OK");
    }

    [MenuItem("Tools/Android/Open Local Signing Folder")]
    private static void OpenLocalSigningFolder()
    {
        string folderPath = AndroidSigningConfigResolver.GetProjectLocalSigningFolder();
        Directory.CreateDirectory(folderPath);
        EditorUtility.RevealInFinder(folderPath);
    }

    [MenuItem("Tools/Android/Create Shared Signing Config Template")]
    private static void CreateSharedSigningConfigTemplate()
    {
        string templateFullPath = AndroidSigningConfigResolver.GetSharedTemplatePath();
        Directory.CreateDirectory(Path.GetDirectoryName(templateFullPath) ?? string.Empty);
        File.WriteAllText(templateFullPath, AndroidSigningConfigResolver.GetTemplateJson());
        EditorUtility.RevealInFinder(templateFullPath);
        EditorUtility.DisplayDialog(
            "Android Release Signing",
            "Created a shared signing template outside the repo. Put the real release-signing.json and keystore beside it so multiple PCs can reuse the same signing identity without committing secrets.",
            "OK");
    }

    [MenuItem("Tools/Android/Open Shared Signing Folder")]
    private static void OpenSharedSigningFolder()
    {
        string folderPath = AndroidSigningConfigResolver.GetPreferredSharedSigningFolder();
        Directory.CreateDirectory(folderPath);
        EditorUtility.RevealInFinder(folderPath);
    }

    private static bool TryApply(bool interactiveFailure, bool logSuccess, bool warnIfConfigMissing)
    {
        try
        {
            AndroidSigningResolutionStatus resolutionStatus = AndroidSigningConfigResolver.Resolve(
                out ResolvedAndroidReleaseSigningConfig resolvedConfig,
                out string resolutionMessage);

            if (resolutionStatus != AndroidSigningResolutionStatus.Success)
                return Fail(resolutionMessage, interactiveFailure, logWarning: warnIfConfigMissing);

            ApplySigning(resolvedConfig);

            if (logSuccess)
                Debug.Log("Applied Android signing from " + resolvedConfig.ConfigPath + ".");

            return true;
        }
        catch (Exception ex)
        {
            return Fail("Failed to apply Android signing: " + ex.Message, interactiveFailure);
        }
    }

    private static void ApplySigning(ResolvedAndroidReleaseSigningConfig resolvedConfig)
    {
        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = NormalizePathForUnity(resolvedConfig.KeystorePath);
        PlayerSettings.Android.keystorePass = resolvedConfig.Config.keystorePassword;
        PlayerSettings.Android.keyaliasName = resolvedConfig.Config.keyaliasName;
        PlayerSettings.Android.keyaliasPass = resolvedConfig.Config.keyaliasPassword;
        AssetDatabase.SaveAssets();
    }

    private static bool Fail(string message, bool interactiveFailure, bool logWarning = true)
    {
        if (interactiveFailure)
            EditorUtility.DisplayDialog("Android Release Signing", message, "OK");

        if (logWarning)
            Debug.LogWarning(message);

        return false;
    }

    private static string NormalizePathForUnity(string path)
    {
        return path.Replace('\\', '/');
    }
}
#endif
