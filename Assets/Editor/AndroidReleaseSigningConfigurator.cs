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
    private const string TemplateRelativePath = "UserSettings/Android/release-signing.template.json";

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
                "Applied local Android release signing from UserSettings/Android/release-signing.json.",
                "OK");
        }
    }

    [MenuItem("Tools/Android/Create Local Signing Config Template")]
    private static void CreateLocalSigningConfigTemplate()
    {
        string templateFullPath = Path.GetFullPath(Path.Combine(GetProjectRoot(), TemplateRelativePath));
        Directory.CreateDirectory(Path.GetDirectoryName(templateFullPath) ?? string.Empty);

        AndroidReleaseSigningConfig template = new AndroidReleaseSigningConfig
        {
            keystoreName = "UserSettings/Android/oreniq-release.keystore",
            keystorePassword = "replace-with-your-keystore-password",
            keyaliasName = "oreniq-release",
            keyaliasPassword = "replace-with-your-key-password"
        };

        File.WriteAllText(templateFullPath, JsonUtility.ToJson(template, prettyPrint: true));

        EditorUtility.RevealInFinder(templateFullPath);
        EditorUtility.DisplayDialog(
            "Android Release Signing",
            "Created UserSettings/Android/release-signing.template.json. Copy its values into release-signing.json on this machine and fill in the real passwords.",
            "OK");
    }

    [MenuItem("Tools/Android/Open Local Signing Folder")]
    private static void OpenLocalSigningFolder()
    {
        string folderPath = Path.GetFullPath(Path.Combine(GetProjectRoot(), "UserSettings/Android"));
        Directory.CreateDirectory(folderPath);
        EditorUtility.RevealInFinder(folderPath);
    }

    private static bool TryApply(bool interactiveFailure, bool logSuccess, bool warnIfConfigMissing)
    {
        try
        {
            string configFullPath = Path.GetFullPath(Path.Combine(GetProjectRoot(), ConfigRelativePath));

            if (!File.Exists(configFullPath))
            {
                return Fail(
                    "Local Android signing config was not found in UserSettings/Android. This is expected on a fresh machine until release-signing.json is created locally.",
                    interactiveFailure,
                    logWarning: warnIfConfigMissing);
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

    private static bool Fail(string message, bool interactiveFailure, bool logWarning = true)
    {
        if (interactiveFailure)
        {
            EditorUtility.DisplayDialog("Android Release Signing", message, "OK");
        }

        if (logWarning)
        {
            Debug.LogWarning(message);
        }

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
