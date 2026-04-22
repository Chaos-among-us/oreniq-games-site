#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public sealed class AndroidReleaseSigningConfig
{
    public string keystoreName;
    public string keystorePassword;
    public string keyaliasName;
    public string keyaliasPassword;
}

public enum AndroidSigningResolutionStatus
{
    Success,
    NotFound,
    Invalid
}

public sealed class ResolvedAndroidReleaseSigningConfig
{
    public AndroidReleaseSigningConfig Config { get; }
    public string ConfigPath { get; }
    public string KeystorePath { get; }
    public string SourceDescription { get; }

    public ResolvedAndroidReleaseSigningConfig(
        AndroidReleaseSigningConfig config,
        string configPath,
        string keystorePath,
        string sourceDescription)
    {
        Config = config;
        ConfigPath = configPath;
        KeystorePath = keystorePath;
        SourceDescription = sourceDescription;
    }
}

public static class AndroidSigningConfigResolver
{
    public const string LocalConfigRelativePath = "UserSettings/Android/release-signing.json";
    public const string LocalTemplateRelativePath = "UserSettings/Android/release-signing.template.json";
    public const string SharedConfigFileName = "release-signing.json";
    public const string SharedTemplateFileName = "release-signing.template.json";
    public const string SharedKeystoreFileName = "oreniq-release.keystore";
    public const string SharedFolderRelativePath = "EndlessDodge1/SharedSigning/Android";
    public const string SigningConfigPathEnvironmentVariable = "ENDLESSDODGE_SIGNING_CONFIG";
    public const string SigningRootPathEnvironmentVariable = "ENDLESSDODGE_SIGNING_ROOT";

    private sealed class SigningCandidate
    {
        public string ConfigPath;
        public string SourceDescription;
    }

    public static AndroidSigningResolutionStatus Resolve(
        out ResolvedAndroidReleaseSigningConfig resolvedConfig,
        out string message)
    {
        resolvedConfig = null;
        List<SigningCandidate> candidates = GetCandidatePaths();

        for (int i = 0; i < candidates.Count; i++)
        {
            SigningCandidate candidate = candidates[i];

            if (!File.Exists(candidate.ConfigPath))
                continue;

            if (!TryLoadCandidate(candidate, out resolvedConfig, out message))
                return AndroidSigningResolutionStatus.Invalid;

            return AndroidSigningResolutionStatus.Success;
        }

        message =
            "Android signing config was not found. Looked for:\n" +
            "- " + Path.Combine(GetProjectRoot(), LocalConfigRelativePath) + "\n" +
            "- %" + SigningConfigPathEnvironmentVariable + "%\n" +
            "- %" + SigningRootPathEnvironmentVariable + "%\\" + SharedConfigFileName + "\n" +
            "- " + GetPreferredSharedSigningConfigPath() + "\n\n" +
            "Keep signing secrets out of Git. Store them in the shared external signing folder instead.";
        return AndroidSigningResolutionStatus.NotFound;
    }

    public static string GetProjectRoot()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
    }

    public static string GetProjectLocalSigningFolder()
    {
        return Path.GetFullPath(Path.Combine(GetProjectRoot(), "UserSettings/Android"));
    }

    public static string GetPreferredSharedSigningFolder()
    {
        string oneDriveRoot = GetOneDriveRoot();

        if (!string.IsNullOrWhiteSpace(oneDriveRoot))
            return Path.GetFullPath(Path.Combine(oneDriveRoot, "Documents", SharedFolderRelativePath));

        return Path.GetFullPath(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            SharedFolderRelativePath));
    }

    public static string GetPreferredSharedSigningConfigPath()
    {
        return Path.Combine(GetPreferredSharedSigningFolder(), SharedConfigFileName);
    }

    public static string GetSharedTemplatePath()
    {
        return Path.Combine(GetPreferredSharedSigningFolder(), SharedTemplateFileName);
    }

    public static string GetTemplateJson()
    {
        AndroidReleaseSigningConfig template = new AndroidReleaseSigningConfig
        {
            keystoreName = SharedKeystoreFileName,
            keystorePassword = "replace-with-your-keystore-password",
            keyaliasName = "oreniq-release",
            keyaliasPassword = "replace-with-your-key-password"
        };

        return JsonUtility.ToJson(template, true);
    }

    private static bool TryLoadCandidate(
        SigningCandidate candidate,
        out ResolvedAndroidReleaseSigningConfig resolvedConfig,
        out string message)
    {
        resolvedConfig = null;
        message = string.Empty;

        AndroidReleaseSigningConfig config;

        try
        {
            config = JsonUtility.FromJson<AndroidReleaseSigningConfig>(File.ReadAllText(candidate.ConfigPath));
        }
        catch (Exception ex)
        {
            message = "Failed to parse Android signing config at " + candidate.ConfigPath + ": " + ex.Message;
            return false;
        }

        if (config == null)
        {
            message = "Android signing config at " + candidate.ConfigPath + " could not be parsed.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(config.keystoreName) ||
            string.IsNullOrWhiteSpace(config.keystorePassword) ||
            string.IsNullOrWhiteSpace(config.keyaliasName) ||
            string.IsNullOrWhiteSpace(config.keyaliasPassword))
        {
            message = "Android signing config at " + candidate.ConfigPath + " is missing required fields.";
            return false;
        }

        string keystorePath = ResolveKeystorePath(candidate.ConfigPath, config.keystoreName);

        if (string.IsNullOrWhiteSpace(keystorePath) || !File.Exists(keystorePath))
        {
            message =
                "Android signing config was found at " + candidate.ConfigPath +
                ", but the keystore file could not be found from `" + config.keystoreName + "`.";
            return false;
        }

        resolvedConfig = new ResolvedAndroidReleaseSigningConfig(
            config,
            Path.GetFullPath(candidate.ConfigPath),
            Path.GetFullPath(keystorePath),
            candidate.SourceDescription);
        return true;
    }

    private static List<SigningCandidate> GetCandidatePaths()
    {
        List<SigningCandidate> candidates = new List<SigningCandidate>();
        HashSet<string> seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AddCandidate(
            candidates,
            seenPaths,
            Path.Combine(GetProjectRoot(), LocalConfigRelativePath),
            "project-local UserSettings/Android");

        string configPathFromEnvironment = Environment.GetEnvironmentVariable(SigningConfigPathEnvironmentVariable);

        if (!string.IsNullOrWhiteSpace(configPathFromEnvironment))
        {
            AddCandidate(
                candidates,
                seenPaths,
                configPathFromEnvironment,
                "%" + SigningConfigPathEnvironmentVariable + "%");
        }

        string rootPathFromEnvironment = Environment.GetEnvironmentVariable(SigningRootPathEnvironmentVariable);

        if (!string.IsNullOrWhiteSpace(rootPathFromEnvironment))
        {
            AddCandidate(
                candidates,
                seenPaths,
                Path.Combine(rootPathFromEnvironment, SharedConfigFileName),
                "%" + SigningRootPathEnvironmentVariable + "%");
        }

        AddCandidate(
            candidates,
            seenPaths,
            GetPreferredSharedSigningConfigPath(),
            "preferred shared signing folder");

        string oneDriveRoot = GetOneDriveRoot();

        if (!string.IsNullOrWhiteSpace(oneDriveRoot))
        {
            AddCandidate(
                candidates,
                seenPaths,
                Path.Combine(oneDriveRoot, "Documents", SharedFolderRelativePath, SharedConfigFileName),
                "OneDrive shared signing folder");
        }

        string documentsSharedPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            SharedFolderRelativePath,
            SharedConfigFileName);

        AddCandidate(
            candidates,
            seenPaths,
            documentsSharedPath,
            "Documents shared signing folder");

        return candidates;
    }

    private static void AddCandidate(
        List<SigningCandidate> candidates,
        HashSet<string> seenPaths,
        string candidatePath,
        string sourceDescription)
    {
        if (string.IsNullOrWhiteSpace(candidatePath))
            return;

        string normalizedPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(candidatePath));

        if (!seenPaths.Add(normalizedPath))
            return;

        candidates.Add(new SigningCandidate
        {
            ConfigPath = normalizedPath,
            SourceDescription = sourceDescription
        });
    }

    private static string ResolveKeystorePath(string configPath, string configuredKeystorePath)
    {
        string expandedPath = Environment.ExpandEnvironmentVariables(configuredKeystorePath.Trim());

        if (Path.IsPathRooted(expandedPath))
            return Path.GetFullPath(expandedPath);

        string configDirectory = Path.GetDirectoryName(configPath) ?? GetProjectRoot();
        string configRelativePath = Path.GetFullPath(Path.Combine(configDirectory, expandedPath));

        if (File.Exists(configRelativePath))
            return configRelativePath;

        return Path.GetFullPath(Path.Combine(GetProjectRoot(), expandedPath));
    }

    private static string GetOneDriveRoot()
    {
        string oneDriveRoot = Environment.GetEnvironmentVariable("OneDrive");

        if (!string.IsNullOrWhiteSpace(oneDriveRoot))
            return oneDriveRoot;

        oneDriveRoot = Environment.GetEnvironmentVariable("OneDriveConsumer");

        if (!string.IsNullOrWhiteSpace(oneDriveRoot))
            return oneDriveRoot;

        return Environment.GetEnvironmentVariable("OneDriveCommercial");
    }
}
#endif
