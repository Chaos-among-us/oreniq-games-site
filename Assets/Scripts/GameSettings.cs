using UnityEngine;

public static class GameSettings
{
    private const string HapticsEnabledKey = "Settings_HapticsEnabled";
    private const string TutorialSeenKey = "Settings_TutorialSeen";

    public static bool IsHapticsEnabled()
    {
        return PlayerPrefs.GetInt(HapticsEnabledKey, 1) == 1;
    }

    public static void SetHapticsEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(HapticsEnabledKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool HasSeenTutorial()
    {
        return PlayerPrefs.GetInt(TutorialSeenKey, 0) == 1;
    }

    public static void MarkTutorialSeen()
    {
        PlayerPrefs.SetInt(TutorialSeenKey, 1);
        PlayerPrefs.Save();
    }

    public static void TriggerHaptic()
    {
        if (!IsHapticsEnabled())
            return;

#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }
}
