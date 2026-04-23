using UnityEngine;

public static class GameSettings
{
    private const string HapticsEnabledKey = "Settings_HapticsEnabled";
    private const string TutorialSeenKey = "Settings_TutorialSeen";
    private const string CompletedRunsKey = "Growth_CompletedRuns";
    private const string NextReviewPromptRunKey = "Growth_NextReviewPromptRun";
    private const string ReviewPromptCompletedKey = "Growth_ReviewPromptCompleted";

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

    public static int RegisterCompletedRun()
    {
        int completedRuns = PlayerPrefs.GetInt(CompletedRunsKey, 0) + 1;
        PlayerPrefs.SetInt(CompletedRunsKey, completedRuns);
        PlayerPrefs.Save();
        return completedRuns;
    }

    public static int GetCompletedRunCount()
    {
        return PlayerPrefs.GetInt(CompletedRunsKey, 0);
    }

    public static bool HasCompletedReviewPrompt()
    {
        return PlayerPrefs.GetInt(ReviewPromptCompletedKey, 0) == 1;
    }

    public static bool ShouldShowReviewPrompt(int completedRuns, int finalScore, bool newBestScore, bool isDailyChallengeRun)
    {
        if (isDailyChallengeRun || HasCompletedReviewPrompt())
            return false;

        int nextEligibleRun = PlayerPrefs.GetInt(NextReviewPromptRunKey, 4);
        int minimumRuns = Mathf.Max(4, nextEligibleRun);

        if (completedRuns < minimumRuns)
            return false;

        return newBestScore || finalScore >= 45;
    }

    public static void DeferReviewPrompt(int additionalRuns)
    {
        int completedRuns = GetCompletedRunCount();
        int nextEligibleRun = completedRuns + Mathf.Max(1, additionalRuns);
        int existingNextPromptRun = PlayerPrefs.GetInt(NextReviewPromptRunKey, 4);
        PlayerPrefs.SetInt(NextReviewPromptRunKey, Mathf.Max(existingNextPromptRun, nextEligibleRun));
        PlayerPrefs.Save();
    }

    public static void MarkReviewPromptCompleted()
    {
        PlayerPrefs.SetInt(ReviewPromptCompletedKey, 1);
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
