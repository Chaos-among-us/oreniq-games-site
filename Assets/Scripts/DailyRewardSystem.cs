using System;
using UnityEngine;

public struct DailyRewardPackage
{
    public int rewardDay;
    public int coins;
    public UpgradeType bonusUpgrade;
    public int bonusAmount;
}

public static class DailyRewardSystem
{
    private const string LastClaimDateKey = "DailyReward_LastClaimDate";
    private const string StreakKey = "DailyReward_Streak";
    private const string TotalCoinsKey = "TotalCoins";

    public static bool CanClaimToday()
    {
        if (!TryGetLastClaimDate(out DateTime lastClaimDate))
            return true;

        return (DateTime.Today - lastClaimDate.Date).Days >= 1;
    }

    public static int GetCurrentStreak()
    {
        return PlayerPrefs.GetInt(StreakKey, 0);
    }

    public static DailyRewardPackage GetPreviewReward()
    {
        int nextStreak = GetNextStreakValue();
        return GetRewardForStreak(nextStreak);
    }

    public static string GetNextClaimCountdownText()
    {
        DateTime now = DateTime.Now;
        DateTime nextClaimTime = now.Date.AddDays(1);
        TimeSpan remaining = nextClaimTime - now;

        if (remaining.TotalMinutes < 1)
            return "Ready soon";

        int hours = Mathf.Max(0, (int)remaining.TotalHours);
        int minutes = Mathf.Max(0, remaining.Minutes);

        if (hours > 0)
            return hours + "h " + minutes + "m";

        return minutes + "m";
    }

    public static bool TryClaimReward(out DailyRewardPackage rewardPackage)
    {
        rewardPackage = default;

        if (!CanClaimToday())
            return false;

        int nextStreak = GetNextStreakValue();
        rewardPackage = GetRewardForStreak(nextStreak);

        int totalCoins = PlayerPrefs.GetInt(TotalCoinsKey, 0);
        totalCoins += rewardPackage.coins;
        PlayerPrefs.SetInt(TotalCoinsKey, totalCoins);
        PlayerPrefs.SetString(LastClaimDateKey, DateTime.Today.ToString("yyyy-MM-dd"));
        PlayerPrefs.SetInt(StreakKey, nextStreak);

        if (UpgradeInventory.Instance != null && rewardPackage.bonusAmount > 0)
            UpgradeInventory.Instance.AddUpgrade(rewardPackage.bonusUpgrade, rewardPackage.bonusAmount);

        PlayerPrefs.Save();
        LaunchAnalytics.RecordDailyRewardClaimed(rewardPackage, nextStreak);
        return true;
    }

    static int GetNextStreakValue()
    {
        int currentStreak = GetCurrentStreak();

        if (!TryGetLastClaimDate(out DateTime lastClaimDate))
            return 1;

        int daysSinceLastClaim = (DateTime.Today - lastClaimDate.Date).Days;

        if (daysSinceLastClaim <= 0)
            return Mathf.Max(1, currentStreak);

        if (daysSinceLastClaim == 1)
            return currentStreak + 1;

        return 1;
    }

    static DailyRewardPackage GetRewardForStreak(int streakValue)
    {
        int rewardDay = ((Mathf.Max(streakValue, 1) - 1) % 7) + 1;

        switch (rewardDay)
        {
            case 1:
                return CreateReward(rewardDay, 60, UpgradeType.Shield, 1);
            case 2:
                return CreateReward(rewardDay, 75, UpgradeType.SpeedBoost, 1);
            case 3:
                return CreateReward(rewardDay, 90, UpgradeType.CoinMagnet, 1);
            case 4:
                return CreateReward(rewardDay, 110, UpgradeType.DoubleCoins, 1);
            case 5:
                return CreateReward(rewardDay, 130, UpgradeType.ExtraLife, 1);
            case 6:
                return CreateReward(rewardDay, 155, UpgradeType.SlowTime, 1);
            default:
                return CreateReward(rewardDay, 220, UpgradeType.Bomb, 1);
        }
    }

    static DailyRewardPackage CreateReward(int rewardDay, int coins, UpgradeType bonusUpgrade, int bonusAmount)
    {
        DailyRewardPackage rewardPackage = new DailyRewardPackage();
        rewardPackage.rewardDay = rewardDay;
        rewardPackage.coins = coins;
        rewardPackage.bonusUpgrade = bonusUpgrade;
        rewardPackage.bonusAmount = bonusAmount;
        return rewardPackage;
    }

    static bool TryGetLastClaimDate(out DateTime lastClaimDate)
    {
        string savedDate = PlayerPrefs.GetString(LastClaimDateKey, string.Empty);

        if (!string.IsNullOrEmpty(savedDate) && DateTime.TryParse(savedDate, out lastClaimDate))
            return true;

        lastClaimDate = default;
        return false;
    }
}
