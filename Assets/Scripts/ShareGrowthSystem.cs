using System;
using UnityEngine;

public struct ShareGrowthReward
{
    public int rewardDay;
    public int coins;
    public UpgradeType bonusUpgrade;
    public int bonusAmount;
    public int streakAfterClaim;
    public bool includesChallengeBonus;
    public bool includesBestScoreBonus;
    public bool includesDepthBonus;
}

public static class ShareGrowthSystem
{
    private const string LastShareDateKey = "ShareGrowth_LastShareDate";
    private const string ShareStreakKey = "ShareGrowth_Streak";
    private const string LifetimeSharesKey = "ShareGrowth_LifetimeShares";
    private const string TotalCoinsKey = "TotalCoins";

    public static bool CanClaimToday()
    {
        if (!TryGetLastClaimDate(out DateTime lastShareDate))
            return true;

        return (DateTime.Today - lastShareDate.Date).Days >= 1;
    }

    public static int GetCurrentStreak()
    {
        return PlayerPrefs.GetInt(ShareStreakKey, 0);
    }

    public static int GetLifetimeShares()
    {
        return PlayerPrefs.GetInt(LifetimeSharesKey, 0);
    }

    public static ShareGrowthReward GetPreviewReward()
    {
        int nextStreak = GetNextStreakValue();
        ShareGrowthReward reward = GetBaseRewardForStreak(nextStreak);
        reward.streakAfterClaim = nextStreak;
        return reward;
    }

    public static string GetPreviewRewardLabel()
    {
        ShareGrowthReward reward = GetPreviewReward();
        return
            "Day " + reward.rewardDay +
            " Echo Cache: +" + reward.coins +
            " coins + " +
            reward.bonusAmount + " " +
            UpgradeInventory.GetDisplayName(reward.bonusUpgrade);
    }

    public static string GetRewardBreakdownLabel(ShareGrowthReward reward)
    {
        string label =
            "Day " + reward.rewardDay +
            " Echo Cache: +" + reward.coins +
            " coins + " +
            reward.bonusAmount + " " +
            UpgradeInventory.GetDisplayName(reward.bonusUpgrade);

        if (reward.includesChallengeBonus || reward.includesBestScoreBonus || reward.includesDepthBonus)
        {
            int bonusCoins = GetContextBonusCoins(
                reward.includesChallengeBonus,
                reward.includesBestScoreBonus,
                reward.includesDepthBonus);
            label += " + " + bonusCoins + " bonus";
        }

        return label;
    }

    public static int GetPotentialRewardCoins(
        bool includeChallengeBonus,
        bool includeBestScoreBonus,
        bool includeDepthBonus)
    {
        ShareGrowthReward reward = GetPreviewReward();
        return reward.coins + GetContextBonusCoins(includeChallengeBonus, includeBestScoreBonus, includeDepthBonus);
    }

    public static string GetCountdownText()
    {
        DateTime now = DateTime.Now;
        DateTime nextShareTime = now.Date.AddDays(1);
        TimeSpan remaining = nextShareTime - now;

        if (remaining.TotalMinutes < 1d)
            return "Ready soon";

        int hours = Mathf.Max(0, (int)remaining.TotalHours);
        int minutes = Mathf.Max(0, remaining.Minutes);

        if (hours > 0)
            return hours + "h " + minutes + "m";

        return minutes + "m";
    }

    public static bool TryClaimShareReward(
        bool sharedDailyChallenge,
        bool sharedNewBest,
        int finalScore,
        int levelReached,
        out ShareGrowthReward reward)
    {
        reward = default;

        if (!CanClaimToday())
            return false;

        int nextStreak = GetNextStreakValue();
        reward = GetBaseRewardForStreak(nextStreak);
        reward.streakAfterClaim = nextStreak;
        reward.includesChallengeBonus = sharedDailyChallenge;
        reward.includesBestScoreBonus = sharedNewBest;
        reward.includesDepthBonus = finalScore >= 60 || levelReached >= 7;

        int totalCoins = PlayerPrefs.GetInt(TotalCoinsKey, 0);
        totalCoins += reward.coins;
        totalCoins += GetContextBonusCoins(
            reward.includesChallengeBonus,
            reward.includesBestScoreBonus,
            reward.includesDepthBonus);

        PlayerPrefs.SetInt(TotalCoinsKey, totalCoins);
        PlayerPrefs.SetString(LastShareDateKey, DateTime.Today.ToString("yyyy-MM-dd"));
        PlayerPrefs.SetInt(ShareStreakKey, nextStreak);
        PlayerPrefs.SetInt(LifetimeSharesKey, GetLifetimeShares() + 1);

        if (UpgradeInventory.Instance != null && reward.bonusAmount > 0)
            UpgradeInventory.Instance.AddUpgrade(reward.bonusUpgrade, reward.bonusAmount);

        PlayerPrefs.Save();
        LaunchAnalytics.RecordShareRewardClaimed(
            reward.rewardDay,
            reward.coins,
            reward.bonusUpgrade,
            reward.bonusAmount,
            reward.streakAfterClaim,
            reward.includesChallengeBonus,
            reward.includesBestScoreBonus,
            reward.includesDepthBonus);
        return true;
    }

    public static bool ShouldGrantDepthBonus(int finalScore, int levelReached)
    {
        return finalScore >= 60 || levelReached >= 7;
    }

    static int GetNextStreakValue()
    {
        int currentStreak = GetCurrentStreak();

        if (!TryGetLastClaimDate(out DateTime lastShareDate))
            return 1;

        int daysSinceLastShare = (DateTime.Today - lastShareDate.Date).Days;

        if (daysSinceLastShare <= 0)
            return Mathf.Max(1, currentStreak);

        if (daysSinceLastShare == 1)
            return currentStreak + 1;

        return 1;
    }

    static ShareGrowthReward GetBaseRewardForStreak(int streakValue)
    {
        int rewardDay = ((Mathf.Max(streakValue, 1) - 1) % 7) + 1;

        switch (rewardDay)
        {
            case 1:
                return CreateReward(rewardDay, 45, UpgradeType.Shield, 1);
            case 2:
                return CreateReward(rewardDay, 55, UpgradeType.SpeedBoost, 1);
            case 3:
                return CreateReward(rewardDay, 70, UpgradeType.CoinMagnet, 1);
            case 4:
                return CreateReward(rewardDay, 85, UpgradeType.DoubleCoins, 1);
            case 5:
                return CreateReward(rewardDay, 100, UpgradeType.ScoreBooster, 1);
            case 6:
                return CreateReward(rewardDay, 120, UpgradeType.SlowTime, 1);
            default:
                return CreateReward(rewardDay, 160, UpgradeType.Bomb, 1);
        }
    }

    static ShareGrowthReward CreateReward(int rewardDay, int coins, UpgradeType bonusUpgrade, int bonusAmount)
    {
        ShareGrowthReward reward = new ShareGrowthReward();
        reward.rewardDay = rewardDay;
        reward.coins = coins;
        reward.bonusUpgrade = bonusUpgrade;
        reward.bonusAmount = bonusAmount;
        reward.streakAfterClaim = rewardDay;
        return reward;
    }

    static int GetContextBonusCoins(bool sharedDailyChallenge, bool sharedNewBest, bool sharedDeepRun)
    {
        int bonusCoins = 0;

        if (sharedDailyChallenge)
            bonusCoins += 20;

        if (sharedNewBest)
            bonusCoins += 15;

        if (sharedDeepRun)
            bonusCoins += 10;

        return bonusCoins;
    }

    static bool TryGetLastClaimDate(out DateTime lastShareDate)
    {
        string savedDate = PlayerPrefs.GetString(LastShareDateKey, string.Empty);

        if (!string.IsNullOrEmpty(savedDate) && DateTime.TryParse(savedDate, out lastShareDate))
            return true;

        lastShareDate = default;
        return false;
    }
}
