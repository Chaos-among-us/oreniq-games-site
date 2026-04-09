using System;
using UnityEngine;

public enum DailyChallengeType
{
    PrecisionRun,
    GoldRush,
    RushHour,
    EnduranceTest
}

public enum DailyChallengeGoalType
{
    ReachScore,
    CollectCoins
}

public struct DailyChallengeData
{
    public DailyChallengeType type;
    public DailyChallengeGoalType goalType;
    public int targetScore;
    public int bestScore;
    public int rewardCoins;
    public UpgradeType rewardUpgrade;
    public int rewardUpgradeAmount;
    public float worldSpeedMultiplier;
    public float coinSpawnMultiplier;
    public bool disableConsumables;
    public bool rewardClaimed;
    public string title;
    public string description;
}

public static class DailyChallengeSystem
{
    private const string ChallengeDateKey = "DailyChallenge_Date";
    private const string ChallengePrefix = "DailyChallenge_";
    private const string ActiveRunDateKey = "DailyChallenge_ActiveDate";
    private const string ActiveRunFlagKey = "DailyChallenge_ActiveFlag";
    private const string TotalCoinsKey = "TotalCoins";

    public static void EnsureInitializedForToday()
    {
        string todayKey = DateTime.Today.ToString("yyyy-MM-dd");

        if (PlayerPrefs.GetString(ChallengeDateKey, string.Empty) == todayKey)
            return;

        CreateTodayChallenge(todayKey);
    }

    public static DailyChallengeData GetTodayChallenge()
    {
        EnsureInitializedForToday();
        DailyChallengeData challenge = LoadChallenge();
        ApplyPresentation(ref challenge);
        return challenge;
    }

    public static void BeginTodayChallengeRun()
    {
        EnsureInitializedForToday();

        DailyChallengeData challenge = LoadChallenge();

        if (challenge.rewardClaimed)
            return;

        PlayerPrefs.SetString(ActiveRunDateKey, DateTime.Today.ToString("yyyy-MM-dd"));
        PlayerPrefs.SetInt(ActiveRunFlagKey, 1);
        PlayerPrefs.Save();
    }

    public static bool IsDailyChallengeRunActive()
    {
        EnsureInitializedForToday();

        string todayKey = DateTime.Today.ToString("yyyy-MM-dd");

        return PlayerPrefs.GetInt(ActiveRunFlagKey, 0) == 1 &&
               PlayerPrefs.GetString(ActiveRunDateKey, string.Empty) == todayKey;
    }

    public static void ClearActiveRun()
    {
        PlayerPrefs.DeleteKey(ActiveRunDateKey);
        PlayerPrefs.SetInt(ActiveRunFlagKey, 0);
        PlayerPrefs.Save();
    }

    public static DailyChallengeData RegisterRunResult(int score, int coinsCollected)
    {
        EnsureInitializedForToday();

        DailyChallengeData challenge = LoadChallenge();
        int progressValue = GetProgressValue(challenge, score, coinsCollected);
        challenge.bestScore = Mathf.Max(challenge.bestScore, progressValue);
        SaveChallenge(challenge);
        ClearActiveRun();
        ApplyPresentation(ref challenge);
        return challenge;
    }

    public static bool HasCompletedToday()
    {
        DailyChallengeData challenge = GetTodayChallenge();
        return challenge.bestScore >= challenge.targetScore;
    }

    public static bool CanClaimReward()
    {
        DailyChallengeData challenge = GetTodayChallenge();
        return challenge.bestScore >= challenge.targetScore && !challenge.rewardClaimed;
    }

    public static string GetObjectiveLabel(DailyChallengeData challenge)
    {
        if (UsesCoinGoal(challenge))
            return "Collect " + challenge.targetScore + " coins";

        return "Reach score " + challenge.targetScore;
    }

    public static bool TryClaimReward(out int coinsGranted, out UpgradeType rewardUpgrade, out int rewardAmount)
    {
        coinsGranted = 0;
        rewardUpgrade = UpgradeType.Shield;
        rewardAmount = 0;

        if (!CanClaimReward())
            return false;

        DailyChallengeData challenge = LoadChallenge();
        challenge.rewardClaimed = true;
        SaveChallenge(challenge);

        coinsGranted = challenge.rewardCoins;
        rewardUpgrade = challenge.rewardUpgrade;
        rewardAmount = challenge.rewardUpgradeAmount;

        int totalCoins = PlayerPrefs.GetInt(TotalCoinsKey, 0);
        totalCoins += coinsGranted;
        PlayerPrefs.SetInt(TotalCoinsKey, totalCoins);

        if (UpgradeInventory.Instance != null && rewardAmount > 0)
            UpgradeInventory.Instance.AddUpgrade(rewardUpgrade, rewardAmount);

        PlayerPrefs.Save();
        return true;
    }

    public static string GetRewardLabel(DailyChallengeData challenge)
    {
        return challenge.rewardCoins + " coins + " +
               challenge.rewardUpgradeAmount + " " +
               UpgradeInventory.GetDisplayName(challenge.rewardUpgrade);
    }

    public static string GetMenuStatusLabel(DailyChallengeData challenge)
    {
        if (challenge.rewardClaimed)
            return "Reward claimed for today";

        if (challenge.bestScore >= challenge.targetScore)
            return "Reward ready";

        if (challenge.bestScore > 0)
            return GetBestProgressLabel(challenge);

        return GetObjectiveLabel(challenge);
    }

    public static string GetRunModifierLabel(DailyChallengeData challenge)
    {
        string speedLabel = challenge.worldSpeedMultiplier > 1f ? "Fast lanes" : "Steady lanes";
        string coinLabel = challenge.coinSpawnMultiplier > 1f ? "more coins" : "normal coins";
        return speedLabel + "   |   " + coinLabel + "   |   no consumables";
    }

    public static string GetCurrentRunLabel(DailyChallengeData challenge, int score, int coinsCollected)
    {
        int progressValue = GetProgressValue(challenge, score, coinsCollected);
        string unitLabel = UsesCoinGoal(challenge) ? "coins" : "score";
        return progressValue + " / " + challenge.targetScore + " " + unitLabel;
    }

    public static string GetBestProgressLabel(DailyChallengeData challenge)
    {
        string unitLabel = UsesCoinGoal(challenge) ? "coins" : "score";
        return "Best " + challenge.bestScore + " / " + challenge.targetScore + " " + unitLabel;
    }

    public static string GetNeedsMoreLabel(DailyChallengeData challenge)
    {
        int neededValue = Mathf.Max(0, challenge.targetScore - challenge.bestScore);
        string unitLabel = UsesCoinGoal(challenge)
            ? (neededValue == 1 ? "coin" : "coins")
            : "score";
        return "Need " + neededValue + " more " + unitLabel;
    }

    private static void CreateTodayChallenge(string todayKey)
    {
        int seed = DateTime.Today.DayOfYear + (DateTime.Today.Year * 37);
        DailyChallengeData challenge = new DailyChallengeData();
        challenge.type = (DailyChallengeType)(seed % 4);
        challenge.goalType = DailyChallengeGoalType.ReachScore;
        challenge.bestScore = 0;
        challenge.disableConsumables = true;
        challenge.rewardClaimed = false;

        int targetOffset = (seed % 5) * 5;

        switch (challenge.type)
        {
            case DailyChallengeType.PrecisionRun:
                challenge.targetScore = 45 + targetOffset;
                challenge.rewardCoins = 140;
                challenge.rewardUpgrade = UpgradeType.Shield;
                challenge.rewardUpgradeAmount = 1;
                challenge.worldSpeedMultiplier = 1.05f;
                challenge.coinSpawnMultiplier = 1.1f;
                break;
            case DailyChallengeType.GoldRush:
                challenge.goalType = DailyChallengeGoalType.CollectCoins;
                challenge.targetScore = 18 + ((seed % 4) * 2);
                challenge.rewardCoins = 165;
                challenge.rewardUpgrade = UpgradeType.CoinMagnet;
                challenge.rewardUpgradeAmount = 1;
                challenge.worldSpeedMultiplier = 1.08f;
                challenge.coinSpawnMultiplier = 1.75f;
                break;
            case DailyChallengeType.RushHour:
                challenge.targetScore = 55 + targetOffset;
                challenge.rewardCoins = 190;
                challenge.rewardUpgrade = UpgradeType.SlowTime;
                challenge.rewardUpgradeAmount = 1;
                challenge.worldSpeedMultiplier = 1.28f;
                challenge.coinSpawnMultiplier = 0.92f;
                break;
            default:
                challenge.targetScore = 50 + targetOffset;
                challenge.rewardCoins = 210;
                challenge.rewardUpgrade = UpgradeType.ExtraLife;
                challenge.rewardUpgradeAmount = 1;
                challenge.worldSpeedMultiplier = 1.18f;
                challenge.coinSpawnMultiplier = 1.2f;
                break;
        }

        PlayerPrefs.SetString(ChallengeDateKey, todayKey);
        SaveChallenge(challenge);
        ClearActiveRun();
        PlayerPrefs.Save();
    }

    private static DailyChallengeData LoadChallenge()
    {
        DailyChallengeData challenge = new DailyChallengeData();
        challenge.type = (DailyChallengeType)PlayerPrefs.GetInt(GetKey("Type"), 0);
        challenge.goalType = (DailyChallengeGoalType)PlayerPrefs.GetInt(GetKey("GoalType"), 0);
        challenge.targetScore = PlayerPrefs.GetInt(GetKey("TargetScore"), 40);
        challenge.bestScore = PlayerPrefs.GetInt(GetKey("BestScore"), 0);
        challenge.rewardCoins = PlayerPrefs.GetInt(GetKey("RewardCoins"), 100);
        challenge.rewardUpgrade = (UpgradeType)PlayerPrefs.GetInt(GetKey("RewardUpgrade"), 0);
        challenge.rewardUpgradeAmount = PlayerPrefs.GetInt(GetKey("RewardUpgradeAmount"), 1);
        challenge.worldSpeedMultiplier = PlayerPrefs.GetFloat(GetKey("WorldSpeedMultiplier"), 1f);
        challenge.coinSpawnMultiplier = PlayerPrefs.GetFloat(GetKey("CoinSpawnMultiplier"), 1f);
        challenge.disableConsumables = PlayerPrefs.GetInt(GetKey("DisableConsumables"), 1) == 1;
        challenge.rewardClaimed = PlayerPrefs.GetInt(GetKey("RewardClaimed"), 0) == 1;
        NormalizeLegacyChallenge(ref challenge);
        return challenge;
    }

    private static void SaveChallenge(DailyChallengeData challenge)
    {
        PlayerPrefs.SetInt(GetKey("Type"), (int)challenge.type);
        PlayerPrefs.SetInt(GetKey("GoalType"), (int)challenge.goalType);
        PlayerPrefs.SetInt(GetKey("TargetScore"), challenge.targetScore);
        PlayerPrefs.SetInt(GetKey("BestScore"), challenge.bestScore);
        PlayerPrefs.SetInt(GetKey("RewardCoins"), challenge.rewardCoins);
        PlayerPrefs.SetInt(GetKey("RewardUpgrade"), (int)challenge.rewardUpgrade);
        PlayerPrefs.SetInt(GetKey("RewardUpgradeAmount"), challenge.rewardUpgradeAmount);
        PlayerPrefs.SetFloat(GetKey("WorldSpeedMultiplier"), challenge.worldSpeedMultiplier);
        PlayerPrefs.SetFloat(GetKey("CoinSpawnMultiplier"), challenge.coinSpawnMultiplier);
        PlayerPrefs.SetInt(GetKey("DisableConsumables"), challenge.disableConsumables ? 1 : 0);
        PlayerPrefs.SetInt(GetKey("RewardClaimed"), challenge.rewardClaimed ? 1 : 0);
    }

    private static string GetKey(string suffix)
    {
        return ChallengePrefix + suffix;
    }

    private static bool UsesCoinGoal(DailyChallengeData challenge)
    {
        return challenge.goalType == DailyChallengeGoalType.CollectCoins;
    }

    private static int GetProgressValue(DailyChallengeData challenge, int score, int coinsCollected)
    {
        return UsesCoinGoal(challenge) ? coinsCollected : score;
    }

    private static void NormalizeLegacyChallenge(ref DailyChallengeData challenge)
    {
        if (challenge.type == DailyChallengeType.GoldRush)
        {
            challenge.goalType = DailyChallengeGoalType.CollectCoins;

            int seed = DateTime.Today.DayOfYear + (DateTime.Today.Year * 37);
            int expectedTarget = 18 + ((seed % 4) * 2);

            challenge.targetScore = expectedTarget;
            challenge.bestScore = Mathf.Clamp(challenge.bestScore, 0, expectedTarget);
            challenge.rewardCoins = Mathf.Max(challenge.rewardCoins, 165);
            challenge.rewardUpgrade = UpgradeType.CoinMagnet;
            challenge.rewardUpgradeAmount = Mathf.Max(challenge.rewardUpgradeAmount, 1);
            challenge.worldSpeedMultiplier = Mathf.Max(challenge.worldSpeedMultiplier, 1.08f);
            challenge.coinSpawnMultiplier = Mathf.Max(challenge.coinSpawnMultiplier, 1.75f);
            return;
        }

        challenge.goalType = DailyChallengeGoalType.ReachScore;
    }

    private static void ApplyPresentation(ref DailyChallengeData challenge)
    {
        switch (challenge.type)
        {
            case DailyChallengeType.PrecisionRun:
                challenge.title = "Precision Run";
                challenge.description = "Reach the target score with no consumables.";
                break;
            case DailyChallengeType.GoldRush:
                challenge.title = "Gold Rush";
                challenge.description = "Collect coins under pressure with no consumables.";
                break;
            case DailyChallengeType.RushHour:
                challenge.title = "Rush Hour";
                challenge.description = "Reach the target score in faster lanes.";
                break;
            default:
                challenge.title = "Endurance Test";
                challenge.description = "Survive long enough to hit the target score.";
                break;
        }
    }
}
