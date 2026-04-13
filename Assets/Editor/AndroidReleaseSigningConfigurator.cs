#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[Serializable]
public sealed class AndroidReleaseSigningConfig
{
    public string keystoreName;
    public string keystorePassword;
    public string keyaliasName;
    public string keyaliasPassword;
}

public static class AndroidReleaseSigningConfigurator
{
    private const string ConfigRelativePath = "UserSettings/Android/release-signing.json";

    [InitializeOnLoadMethod]
    private static void TryApplyOnEditorLoad()
    {
        TryApply(interactiveFailure: false, logSuccess: false);
    }

    [MenuItem("Tools/Android/Apply Local Release Signing")]
    private static void ApplyFromMenu()
    {
        if (TryApply(interactiveFailure: true, logSuccess: true))
        {
            EditorUtility.DisplayDialog(
                "Android Release Signing",
                "Applied local Android release signing from UserSettings/Android/release-signing.json.",
                "OK");
        }
    }

    [MenuItem("Tools/Android/Open Local Signing Folder")]
    private static void OpenLocalSigningFolder()
    {
        string folderPath = Path.GetFullPath(Path.Combine(GetProjectRoot(), "UserSettings/Android"));
        Directory.CreateDirectory(folderPath);
        EditorUtility.RevealInFinder(folderPath);
    }

    private static bool TryApply(bool interactiveFailure, bool logSuccess)
    {
        try
        {
            string configFullPath = Path.GetFullPath(Path.Combine(GetProjectRoot(), ConfigRelativePath));

            if (!File.Exists(configFullPath))
            {
                return Fail("Local Android signing config was not found in UserSettings/Android.", interactiveFailure);
            }

            AndroidReleaseSigningConfig config = JsonUtility.FromJson<AndroidReleaseSigningConfig>(File.ReadAllText(configFullPath));

            if (config == null)
            {
                return Fail("Local Android signing config could not be parsed.", interactiveFailure);
            }

            if (string.IsNullOrWhiteSpace(config.keystoreName) ||
                string.IsNullOrWhiteSpace(config.keystorePassword) ||
                string.IsNullOrWhiteSpace(config.keyaliasName) ||
                string.IsNullOrWhiteSpace(config.keyaliasPassword))
            {
                return Fail("Local Android signing config is missing required fields.", interactiveFailure);
            }

            string keystoreFullPath = Path.GetFullPath(Path.Combine(GetProjectRoot(), config.keystoreName));

            if (!File.Exists(keystoreFullPath))
            {
                return Fail("Configured Android keystore file was not found.", interactiveFailure);
            }

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = NormalizeProjectRelativePath(config.keystoreName);
            PlayerSettings.Android.keystorePass = config.keystorePassword;
            PlayerSettings.Android.keyaliasName = config.keyaliasName;
            PlayerSettings.Android.keyaliasPass = config.keyaliasPassword;

            AssetDatabase.SaveAssets();

            if (logSuccess)
            {
                Debug.Log("Applied local Android release signing from UserSettings/Android.");
            }

            return true;
        }
        catch (Exception ex)
        {
            return Fail("Failed to apply local Android signing: " + ex.Message, interactiveFailure);
        }
    }

    private static bool Fail(string message, bool interactiveFailure)
    {
        if (interactiveFailure)
        {
            EditorUtility.DisplayDialog("Android Release Signing", message, "OK");
        }

        Debug.LogWarning(message);
        return false;
    }

    private static string GetProjectRoot()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
    }

    private static string NormalizeProjectRelativePath(string path)
    {
        return path.Replace('\\', '/');
    }
}
#endif
