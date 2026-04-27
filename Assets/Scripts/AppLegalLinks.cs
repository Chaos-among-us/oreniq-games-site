using System;
using UnityEngine;

public static class AppLegalLinks
{
    [Serializable]
    private sealed class LegalConfig
    {
        public string privacyPolicyUrl;
        public string termsOfServiceUrl;
        public string supportEmail;

        public static LegalConfig CreateDefault()
        {
            return new LegalConfig
            {
                privacyPolicyUrl = string.Empty,
                termsOfServiceUrl = string.Empty,
                supportEmail = string.Empty
            };
        }
    }

    private const string ResourcePath = "AppLegalConfig";
    private static bool configLoaded;
    private static LegalConfig cachedConfig;

    public static bool HasPrivacyPolicyUrl()
    {
        return !string.IsNullOrWhiteSpace(GetConfig().privacyPolicyUrl);
    }

    public static string GetPrivacyPolicyUrl()
    {
        return (GetConfig().privacyPolicyUrl ?? string.Empty).Trim();
    }

    public static bool HasTermsOfServiceUrl()
    {
        return !string.IsNullOrWhiteSpace(GetConfig().termsOfServiceUrl);
    }

    public static string GetTermsOfServiceUrl()
    {
        return (GetConfig().termsOfServiceUrl ?? string.Empty).Trim();
    }

    public static string GetSupportEmail()
    {
        return (GetConfig().supportEmail ?? string.Empty).Trim();
    }

    private static LegalConfig GetConfig()
    {
        if (configLoaded)
            return cachedConfig;

        configLoaded = true;
        cachedConfig = LegalConfig.CreateDefault();
        TextAsset configAsset = Resources.Load<TextAsset>(ResourcePath);

        if (configAsset == null || string.IsNullOrWhiteSpace(configAsset.text))
            return cachedConfig;

        try
        {
            LegalConfig loadedConfig = JsonUtility.FromJson<LegalConfig>(configAsset.text);

            if (loadedConfig != null)
                cachedConfig = loadedConfig;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("App legal config could not be parsed: " + exception.Message);
        }

        cachedConfig.privacyPolicyUrl = cachedConfig.privacyPolicyUrl ?? string.Empty;
        cachedConfig.termsOfServiceUrl = cachedConfig.termsOfServiceUrl ?? string.Empty;
        cachedConfig.supportEmail = cachedConfig.supportEmail ?? string.Empty;
        return cachedConfig;
    }
}
