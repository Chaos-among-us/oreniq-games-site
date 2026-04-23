using System;
using System.Collections.Generic;
using UnityEngine;

public enum ProgressionMilestoneStat
{
    BestScore,
    LifetimeCoinsCollected,
    CompletedRuns,
    NearMisses,
    DailyChallengeClears,
    LifetimeShares,
    BestDangerCombo
}

public struct PlayerProfileSnapshot
{
    public int Level;
    public string RankTitle;
    public int TotalExperience;
    public int ExperienceIntoLevel;
    public int ExperienceForNextLevel;
    public int BestScore;
    public int CompletedRuns;
    public int LifetimeCoinsCollected;
    public int LifetimeNearMisses;
    public int BestDangerCombo;
    public int DailyChallengesCleared;
    public int LifetimeShares;
    public string[] FeaturedGoals;
}

public struct PlayerProgressionRunResult
{
    public int ExperienceGained;
    public int LevelBefore;
    public int LevelAfter;
    public int BonusCoinsGranted;
    public int NearMissesThisRun;
    public int PeakDangerCombo;
    public string LevelUpLabel;
    public string RewardBundleLabel;
    public string MilestoneSummaryLine;
    public string PrimaryGoalSummary;

    public bool LeveledUp
    {
        get { return LevelAfter > LevelBefore; }
    }

    public bool HasRewardBundle
    {
        get { return !string.IsNullOrEmpty(RewardBundleLabel); }
    }

    public bool HasMilestoneSummary
    {
        get { return !string.IsNullOrEmpty(MilestoneSummaryLine); }
    }
}

public struct ProgressionTitleSnapshot
{
    public string Title;
    public int UnlockLevel;
    public string RequirementLabel;
    public string ProgressLabel;
    public string StatusLabel;
    public bool IsCurrent;
    public bool IsUnlocked;
}

public struct ProgressionMilestoneSnapshot
{
    public string Title;
    public string RequirementLabel;
    public string ProgressLabel;
    public string RewardLabel;
    public float Completion01;
    public bool IsClaimed;
    public bool IsReadyToClaim;
    public ProgressionMilestoneStat StatType;
}

internal sealed class ProgressionMilestoneDefinition
{
    public string Id { get; private set; }
    public ProgressionMilestoneStat StatType { get; private set; }
    public int TargetValue { get; private set; }
    public string CompactLabel { get; private set; }
    public int RewardCoins { get; private set; }
    public UpgradeType RewardUpgrade { get; private set; }
    public int RewardUpgradeAmount { get; private set; }

    public ProgressionMilestoneDefinition(
        string id,
        ProgressionMilestoneStat statType,
        int targetValue,
        string compactLabel,
        int rewardCoins,
        UpgradeType rewardUpgrade,
        int rewardUpgradeAmount)
    {
        Id = id;
        StatType = statType;
        TargetValue = targetValue;
        CompactLabel = compactLabel;
        RewardCoins = rewardCoins;
        RewardUpgrade = rewardUpgrade;
        RewardUpgradeAmount = rewardUpgradeAmount;
    }
}

internal struct ProgressionGoalCandidate
{
    public ProgressionMilestoneDefinition Definition;
    public int ProgressValue;
    public int TargetValue;
    public float Completion01;
}

public static class PlayerProgressionSystem
{
    private const string TotalExperienceKey = "Progression_TotalExperience";
    private const string LifetimeCoinsCollectedKey = "Progression_LifetimeCoinsCollected";
    private const string LifetimeNearMissesKey = "Progression_LifetimeNearMisses";
    private const string DailyChallengeClearsKey = "Progression_DailyChallengeClears";
    private const string BestDangerComboKey = "Progression_BestDangerCombo";
    private const string LastCountedChallengeDateKey = "Progression_LastChallengeClearDate";
    private const string TotalCoinsKey = "TotalCoins";
    private const string MilestoneClaimedPrefix = "ProgressionMilestone_";
    private const string BestScoreKey = "BestScore";

    private static readonly string[] RankTitles =
    {
        "Tunnel Rookie",
        "Stone Scout",
        "Echo Runner",
        "Crystal Dodger",
        "Cavern Sprinter",
        "Vault Hunter",
        "Night Drifter",
        "Deep Surge",
        "Mythic Weaver"
    };

    private static readonly ProgressionMilestoneDefinition[] Milestones =
    {
        new ProgressionMilestoneDefinition("best_25", ProgressionMilestoneStat.BestScore, 25, "Best 25", 50, UpgradeType.Shield, 1),
        new ProgressionMilestoneDefinition("best_50", ProgressionMilestoneStat.BestScore, 50, "Best 50", 90, UpgradeType.ExtraLife, 1),
        new ProgressionMilestoneDefinition("best_90", ProgressionMilestoneStat.BestScore, 90, "Best 90", 150, UpgradeType.SlowTime, 1),
        new ProgressionMilestoneDefinition("coins_120", ProgressionMilestoneStat.LifetimeCoinsCollected, 120, "120 coins", 80, UpgradeType.CoinMagnet, 1),
        new ProgressionMilestoneDefinition("coins_400", ProgressionMilestoneStat.LifetimeCoinsCollected, 400, "400 coins", 140, UpgradeType.DoubleCoins, 1),
        new ProgressionMilestoneDefinition("coins_1000", ProgressionMilestoneStat.LifetimeCoinsCollected, 1000, "1000 coins", 240, UpgradeType.RareCoinBoost, 1),
        new ProgressionMilestoneDefinition("runs_5", ProgressionMilestoneStat.CompletedRuns, 5, "5 runs", 70, UpgradeType.SpeedBoost, 1),
        new ProgressionMilestoneDefinition("runs_15", ProgressionMilestoneStat.CompletedRuns, 15, "15 runs", 130, UpgradeType.DoubleCoins, 1),
        new ProgressionMilestoneDefinition("runs_35", ProgressionMilestoneStat.CompletedRuns, 35, "35 runs", 210, UpgradeType.Bomb, 1),
        new ProgressionMilestoneDefinition("near_12", ProgressionMilestoneStat.NearMisses, 12, "12 near misses", 70, UpgradeType.Shield, 1),
        new ProgressionMilestoneDefinition("near_40", ProgressionMilestoneStat.NearMisses, 40, "40 near misses", 130, UpgradeType.SmallerPlayer, 1),
        new ProgressionMilestoneDefinition("shares_1", ProgressionMilestoneStat.LifetimeShares, 1, "1 share", 60, UpgradeType.Shield, 1),
        new ProgressionMilestoneDefinition("shares_5", ProgressionMilestoneStat.LifetimeShares, 5, "5 shares", 140, UpgradeType.ScoreBooster, 1),
        new ProgressionMilestoneDefinition("shares_12", ProgressionMilestoneStat.LifetimeShares, 12, "12 shares", 260, UpgradeType.Bomb, 1),
        new ProgressionMilestoneDefinition("combo_6", ProgressionMilestoneStat.BestDangerCombo, 6, "Danger x6", 110, UpgradeType.ScoreBooster, 1),
        new ProgressionMilestoneDefinition("challenge_3", ProgressionMilestoneStat.DailyChallengeClears, 3, "3 challenges", 180, UpgradeType.SlowTime, 1)
    };

    public static PlayerProfileSnapshot GetProfileSnapshot()
    {
        EnsurePendingMilestonesClaimed();
        int totalExperience = PlayerPrefs.GetInt(TotalExperienceKey, 0);
        int level = GetLevelForExperience(totalExperience);

        PlayerProfileSnapshot snapshot = new PlayerProfileSnapshot();
        snapshot.Level = level;
        snapshot.RankTitle = GetRankTitle(level);
        snapshot.TotalExperience = totalExperience;
        snapshot.ExperienceIntoLevel = GetExperienceIntoCurrentLevel(totalExperience);
        snapshot.ExperienceForNextLevel = GetExperienceRequiredForLevel(level);
        snapshot.BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        snapshot.CompletedRuns = GameSettings.GetCompletedRunCount();
        snapshot.LifetimeCoinsCollected = PlayerPrefs.GetInt(LifetimeCoinsCollectedKey, 0);
        snapshot.LifetimeNearMisses = PlayerPrefs.GetInt(LifetimeNearMissesKey, 0);
        snapshot.BestDangerCombo = PlayerPrefs.GetInt(BestDangerComboKey, 0);
        snapshot.DailyChallengesCleared = PlayerPrefs.GetInt(DailyChallengeClearsKey, 0);
        snapshot.LifetimeShares = ShareGrowthSystem.GetLifetimeShares();
        snapshot.FeaturedGoals = GetFeaturedGoalLabels(2);
        return snapshot;
    }

    public static void EnsurePendingMilestonesClaimed()
    {
        Dictionary<UpgradeType, int> upgradeGrants = new Dictionary<UpgradeType, int>();
        List<string> unlockedMilestones = new List<string>();

        if (!AwardEligibleMilestones(out int bonusCoinsGranted, upgradeGrants, unlockedMilestones))
            return;

        if (bonusCoinsGranted > 0)
        {
            int totalCoins = PlayerPrefs.GetInt(TotalCoinsKey, 0);
            PlayerPrefs.SetInt(TotalCoinsKey, totalCoins + bonusCoinsGranted);
        }

        ApplyUpgradeGrants(upgradeGrants);
        PlayerPrefs.Save();
    }

    public static PlayerProgressionRunResult RegisterRunCompleted(
        int finalScore,
        int coinsEarned,
        int levelReached,
        bool isDailyChallengeRun,
        bool newBestScore,
        int nearMissesThisRun,
        int peakDangerCombo,
        bool clearedDailyChallenge)
    {
        int startingExperience = PlayerPrefs.GetInt(TotalExperienceKey, 0);
        int updatedExperience = startingExperience + CalculateRunExperience(
            finalScore,
            coinsEarned,
            levelReached,
            isDailyChallengeRun,
            newBestScore,
            nearMissesThisRun,
            peakDangerCombo,
            clearedDailyChallenge);

        int levelBefore = GetLevelForExperience(startingExperience);
        int levelAfter = GetLevelForExperience(updatedExperience);
        int bonusCoinsGranted = 0;
        Dictionary<UpgradeType, int> upgradeGrants = new Dictionary<UpgradeType, int>();
        List<string> unlockedMilestones = new List<string>();

        PlayerPrefs.SetInt(TotalExperienceKey, updatedExperience);
        PlayerPrefs.SetInt(
            LifetimeCoinsCollectedKey,
            PlayerPrefs.GetInt(LifetimeCoinsCollectedKey, 0) + Mathf.Max(0, coinsEarned));
        PlayerPrefs.SetInt(
            LifetimeNearMissesKey,
            PlayerPrefs.GetInt(LifetimeNearMissesKey, 0) + Mathf.Max(0, nearMissesThisRun));
        PlayerPrefs.SetInt(
            BestDangerComboKey,
            Mathf.Max(PlayerPrefs.GetInt(BestDangerComboKey, 0), Mathf.Max(0, peakDangerCombo)));

        if (clearedDailyChallenge)
            RegisterDailyChallengeClear();

        for (int level = levelBefore + 1; level <= levelAfter; level++)
        {
            bonusCoinsGranted += 30 + (level * 12);

            if (level % 2 == 0)
                AddUpgradeGrant(upgradeGrants, ResolveLevelRewardUpgrade(level), 1);
        }

        AwardEligibleMilestones(out int milestoneCoinsGranted, upgradeGrants, unlockedMilestones);
        bonusCoinsGranted += milestoneCoinsGranted;

        if (bonusCoinsGranted > 0)
        {
            int totalCoins = PlayerPrefs.GetInt(TotalCoinsKey, 0);
            PlayerPrefs.SetInt(TotalCoinsKey, totalCoins + bonusCoinsGranted);
        }

        ApplyUpgradeGrants(upgradeGrants);
        PlayerPrefs.Save();

        PlayerProgressionRunResult result = new PlayerProgressionRunResult();
        result.ExperienceGained = updatedExperience - startingExperience;
        result.LevelBefore = levelBefore;
        result.LevelAfter = levelAfter;
        result.BonusCoinsGranted = bonusCoinsGranted;
        result.NearMissesThisRun = Mathf.Max(0, nearMissesThisRun);
        result.PeakDangerCombo = Mathf.Max(0, peakDangerCombo);
        result.LevelUpLabel = levelAfter > levelBefore
            ? GetRankTitle(levelAfter) + " Lv " + levelAfter + " reached"
            : string.Empty;
        result.RewardBundleLabel = BuildRewardBundleLabel(bonusCoinsGranted, upgradeGrants);
        result.MilestoneSummaryLine = unlockedMilestones.Count > 0
            ? "Unlocked: " + string.Join("  |  ", unlockedMilestones.ToArray())
            : string.Empty;
        result.PrimaryGoalSummary = GetPrimaryGoalSummary();
        return result;
    }

    public static string GetPrimaryGoalSummary()
    {
        string[] goals = GetFeaturedGoalLabels(1);

        if (goals.Length == 0)
            return "Keep pushing for the next mastery level.";

        return "Next goal: " + goals[0];
    }

    public static ProgressionTitleSnapshot[] GetTitleRoadmap(int maxCount)
    {
        PlayerProfileSnapshot profile = GetProfileSnapshot();
        int currentRankIndex = Mathf.Clamp((Mathf.Max(1, profile.Level) - 1) / 3, 0, RankTitles.Length - 1);
        int count = Mathf.Min(Mathf.Max(1, maxCount), RankTitles.Length - currentRankIndex);
        ProgressionTitleSnapshot[] snapshots = new ProgressionTitleSnapshot[count];

        for (int i = 0; i < count; i++)
        {
            int rankIndex = currentRankIndex + i;
            int unlockLevel = GetRankUnlockLevel(rankIndex);
            int unlockRangeMax = unlockLevel + 2;
            bool isCurrent = rankIndex == currentRankIndex;
            bool isUnlocked = profile.Level >= unlockLevel;

            ProgressionTitleSnapshot snapshot = new ProgressionTitleSnapshot();
            snapshot.Title = RankTitles[rankIndex];
            snapshot.UnlockLevel = unlockLevel;
            snapshot.IsCurrent = isCurrent;
            snapshot.IsUnlocked = isUnlocked;
            snapshot.StatusLabel = isCurrent
                ? "Current"
                : isUnlocked
                    ? "Unlocked"
                    : "Upcoming";
            snapshot.RequirementLabel = isCurrent
                ? "Active for Lv " + unlockLevel + "-" + unlockRangeMax
                : "Unlocks at Lv " + unlockLevel;
            snapshot.ProgressLabel = isCurrent
                ? "Lv " + profile.Level + " active"
                : isUnlocked
                    ? "Unlocked"
                    : "Lv " + profile.Level + "/" + unlockLevel;
            snapshots[i] = snapshot;
        }

        return snapshots;
    }

    public static ProgressionMilestoneSnapshot[] GetMilestoneRoadmap(int maxCount)
    {
        List<ProgressionGoalCandidate> candidates = GetSortedGoalCandidates();
        int count = Mathf.Min(Mathf.Max(0, maxCount), candidates.Count);
        ProgressionMilestoneSnapshot[] snapshots = new ProgressionMilestoneSnapshot[count];

        for (int i = 0; i < count; i++)
        {
            ProgressionGoalCandidate candidate = candidates[i];
            ProgressionMilestoneDefinition milestone = candidate.Definition;
            bool isClaimed = IsMilestoneClaimed(milestone.Id);
            bool isReadyToClaim = !isClaimed && candidate.ProgressValue >= candidate.TargetValue;

            ProgressionMilestoneSnapshot snapshot = new ProgressionMilestoneSnapshot();
            snapshot.Title = milestone.CompactLabel;
            snapshot.RequirementLabel = GetMilestoneRequirementLabel(milestone);
            snapshot.ProgressLabel = GetMilestoneProgressLabel(milestone, candidate.ProgressValue, isClaimed);
            snapshot.RewardLabel = GetMilestoneRewardLabel(milestone);
            snapshot.Completion01 = candidate.Completion01;
            snapshot.IsClaimed = isClaimed;
            snapshot.IsReadyToClaim = isReadyToClaim;
            snapshot.StatType = milestone.StatType;
            snapshots[i] = snapshot;
        }

        return snapshots;
    }

    public static string GetLoadoutRecommendation()
    {
        ProgressionMilestoneDefinition priorityGoal = GetPriorityGoalDefinition();

        if (priorityGoal == null)
            return "Pick a balanced trio: Shield, Coin Magnet, and Extra Life.";

        switch (priorityGoal.StatType)
        {
            case ProgressionMilestoneStat.BestScore:
                return "Loadout focus: Shield, Extra Life, and Slow Time help push your best-score goals.";
            case ProgressionMilestoneStat.LifetimeCoinsCollected:
                return "Loadout focus: Coin Magnet, Double Coins, and Rare Coin Boost speed up coin milestones.";
            case ProgressionMilestoneStat.CompletedRuns:
                return "Loadout focus: Shield, Speed Boost, and Extra Life keep runs moving for completion goals.";
            case ProgressionMilestoneStat.NearMisses:
                return "Loadout focus: Smaller Player, Shield, and Slow Time make danger plays safer.";
            case ProgressionMilestoneStat.DailyChallengeClears:
                return "Loadout focus: build a stable challenge set with Shield, Coin Magnet, and Extra Life.";
            case ProgressionMilestoneStat.LifetimeShares:
                return "Loadout focus: Shield, Score Booster, and Coin Magnet help produce cleaner, share-worthy runs.";
            case ProgressionMilestoneStat.BestDangerCombo:
                return "Loadout focus: Score Booster, Smaller Player, and Shield support longer danger chains.";
            default:
                return "Pick a balanced trio: Shield, Coin Magnet, and Extra Life.";
        }
    }

    public static string GetShopRecommendation()
    {
        ProgressionMilestoneDefinition priorityGoal = GetPriorityGoalDefinition();

        if (priorityGoal == null)
            return "Stock up on Shield, Coin Magnet, and Extra Life to keep your runs healthy.";

        switch (priorityGoal.StatType)
        {
            case ProgressionMilestoneStat.BestScore:
                return "Buy focus: Shield, Extra Life, Slow Time.";
            case ProgressionMilestoneStat.LifetimeCoinsCollected:
                return "Buy focus: Coin Magnet, Double Coins, Rare Coin Boost.";
            case ProgressionMilestoneStat.CompletedRuns:
                return "Buy focus: Shield and Speed Boost.";
            case ProgressionMilestoneStat.NearMisses:
                return "Buy focus: Smaller Player, Shield, Slow Time.";
            case ProgressionMilestoneStat.DailyChallengeClears:
                return "Buy focus: Shield, Coin Magnet, Extra Life.";
            case ProgressionMilestoneStat.LifetimeShares:
                return "Buy focus: Shield, Score Booster, Coin Magnet.";
            case ProgressionMilestoneStat.BestDangerCombo:
                return "Buy focus: Score Booster, Shield, Smaller Player.";
            default:
                return "Buy focus: Shield, Coin Magnet, Extra Life.";
        }
    }

    static int CalculateRunExperience(
        int finalScore,
        int coinsEarned,
        int levelReached,
        bool isDailyChallengeRun,
        bool newBestScore,
        int nearMissesThisRun,
        int peakDangerCombo,
        bool clearedDailyChallenge)
    {
        int experience =
            18 +
            Mathf.FloorToInt(Mathf.Max(0, finalScore) * 0.7f) +
            (Mathf.Max(0, coinsEarned) * 4) +
            (Mathf.Max(1, levelReached) * 6);

        if (newBestScore)
            experience += 26;

        if (isDailyChallengeRun)
            experience += 12;

        if (clearedDailyChallenge)
            experience += 32;

        experience += Mathf.Min(45, Mathf.Max(0, nearMissesThisRun) * 3);
        experience += Mathf.Min(32, Mathf.Max(0, peakDangerCombo) * 4);
        return Mathf.Clamp(experience, 20, 260);
    }

    static int GetLevelForExperience(int totalExperience)
    {
        int currentLevel = 1;
        int remainingExperience = Mathf.Max(0, totalExperience);

        while (remainingExperience >= GetExperienceRequiredForLevel(currentLevel))
        {
            remainingExperience -= GetExperienceRequiredForLevel(currentLevel);
            currentLevel += 1;

            if (currentLevel >= 250)
                break;
        }

        return currentLevel;
    }

    static int GetExperienceIntoCurrentLevel(int totalExperience)
    {
        int currentLevel = 1;
        int remainingExperience = Mathf.Max(0, totalExperience);

        while (remainingExperience >= GetExperienceRequiredForLevel(currentLevel))
        {
            remainingExperience -= GetExperienceRequiredForLevel(currentLevel);
            currentLevel += 1;

            if (currentLevel >= 250)
                break;
        }

        return remainingExperience;
    }

    static int GetExperienceRequiredForLevel(int currentLevel)
    {
        int levelIndex = Mathf.Max(0, currentLevel - 1);
        return 80 + (levelIndex * 22) + Mathf.FloorToInt(levelIndex * levelIndex * 0.45f);
    }

    static string GetRankTitle(int level)
    {
        int rankIndex = Mathf.Clamp((Mathf.Max(level, 1) - 1) / 3, 0, RankTitles.Length - 1);
        return RankTitles[rankIndex];
    }

    static int GetRankUnlockLevel(int rankIndex)
    {
        return 1 + (Mathf.Max(0, rankIndex) * 3);
    }

    static void RegisterDailyChallengeClear()
    {
        string todayKey = DateTime.Today.ToString("yyyy-MM-dd");
        string lastCountedDate = PlayerPrefs.GetString(LastCountedChallengeDateKey, string.Empty);

        if (lastCountedDate == todayKey)
            return;

        PlayerPrefs.SetString(LastCountedChallengeDateKey, todayKey);
        PlayerPrefs.SetInt(
            DailyChallengeClearsKey,
            PlayerPrefs.GetInt(DailyChallengeClearsKey, 0) + 1);
    }

    static bool IsMilestoneClaimed(string milestoneId)
    {
        return PlayerPrefs.GetInt(MilestoneClaimedPrefix + milestoneId, 0) == 1;
    }

    static void MarkMilestoneClaimed(string milestoneId)
    {
        PlayerPrefs.SetInt(MilestoneClaimedPrefix + milestoneId, 1);
    }

    static bool AwardEligibleMilestones(
        out int bonusCoinsGranted,
        Dictionary<UpgradeType, int> upgradeGrants,
        List<string> unlockedMilestones)
    {
        bonusCoinsGranted = 0;
        bool awardedAny = false;

        for (int i = 0; i < Milestones.Length; i++)
        {
            ProgressionMilestoneDefinition milestone = Milestones[i];

            if (IsMilestoneClaimed(milestone.Id))
                continue;

            int currentValue = GetCurrentValue(milestone.StatType);

            if (currentValue < milestone.TargetValue)
                continue;

            MarkMilestoneClaimed(milestone.Id);
            bonusCoinsGranted += milestone.RewardCoins;

            if (milestone.RewardUpgradeAmount > 0)
                AddUpgradeGrant(upgradeGrants, milestone.RewardUpgrade, milestone.RewardUpgradeAmount);

            unlockedMilestones.Add(milestone.CompactLabel);
            awardedAny = true;
        }

        return awardedAny;
    }

    static int GetCurrentValue(ProgressionMilestoneStat statType)
    {
        switch (statType)
        {
            case ProgressionMilestoneStat.BestScore:
                return PlayerPrefs.GetInt(BestScoreKey, 0);
            case ProgressionMilestoneStat.LifetimeCoinsCollected:
                return PlayerPrefs.GetInt(LifetimeCoinsCollectedKey, 0);
            case ProgressionMilestoneStat.CompletedRuns:
                return GameSettings.GetCompletedRunCount();
            case ProgressionMilestoneStat.NearMisses:
                return PlayerPrefs.GetInt(LifetimeNearMissesKey, 0);
            case ProgressionMilestoneStat.DailyChallengeClears:
                return PlayerPrefs.GetInt(DailyChallengeClearsKey, 0);
            case ProgressionMilestoneStat.LifetimeShares:
                return ShareGrowthSystem.GetLifetimeShares();
            case ProgressionMilestoneStat.BestDangerCombo:
                return PlayerPrefs.GetInt(BestDangerComboKey, 0);
            default:
                return 0;
        }
    }

    static void AddUpgradeGrant(Dictionary<UpgradeType, int> upgradeGrants, UpgradeType type, int amount)
    {
        if (!upgradeGrants.ContainsKey(type))
            upgradeGrants[type] = 0;

        upgradeGrants[type] += amount;
    }

    static void ApplyUpgradeGrants(Dictionary<UpgradeType, int> upgradeGrants)
    {
        foreach (KeyValuePair<UpgradeType, int> pair in upgradeGrants)
        {
            if (pair.Value <= 0)
                continue;

            if (UpgradeInventory.Instance != null)
            {
                UpgradeInventory.Instance.AddUpgrade(pair.Key, pair.Value);
            }
            else
            {
                string key = "Upgrade_" + pair.Key;
                PlayerPrefs.SetInt(key, PlayerPrefs.GetInt(key, 0) + pair.Value);
            }
        }
    }

    static UpgradeType ResolveLevelRewardUpgrade(int level)
    {
        switch (Mathf.Abs(level) % 5)
        {
            case 0:
                return UpgradeType.ScoreBooster;
            case 1:
                return UpgradeType.Shield;
            case 2:
                return UpgradeType.SpeedBoost;
            case 3:
                return UpgradeType.CoinMagnet;
            default:
                return UpgradeType.DoubleCoins;
        }
    }

    static string BuildRewardBundleLabel(int bonusCoinsGranted, Dictionary<UpgradeType, int> upgradeGrants)
    {
        List<string> rewardParts = new List<string>();

        if (bonusCoinsGranted > 0)
            rewardParts.Add("+" + bonusCoinsGranted + " coins");

        foreach (KeyValuePair<UpgradeType, int> pair in upgradeGrants)
        {
            if (pair.Value <= 0)
                continue;

            rewardParts.Add("+" + pair.Value + " " + UpgradeInventory.GetDisplayName(pair.Key));
        }

        if (rewardParts.Count == 0)
            return string.Empty;

        return "Mastery rewards: " + string.Join("  |  ", rewardParts.ToArray());
    }

    static string[] GetFeaturedGoalLabels(int maxCount)
    {
        List<ProgressionGoalCandidate> candidates = GetSortedGoalCandidates();
        int count = Mathf.Min(Mathf.Max(0, maxCount), candidates.Count);
        string[] labels = new string[count];

        for (int i = 0; i < count; i++)
        {
            ProgressionGoalCandidate candidate = candidates[i];
            labels[i] =
                candidate.Definition.CompactLabel +
                " (" +
                candidate.ProgressValue +
                "/" +
                candidate.TargetValue +
                ")";
        }

        return labels;
    }

    static List<ProgressionGoalCandidate> GetSortedGoalCandidates()
    {
        List<ProgressionGoalCandidate> candidates = new List<ProgressionGoalCandidate>();

        for (int i = 0; i < Milestones.Length; i++)
        {
            ProgressionMilestoneDefinition milestone = Milestones[i];

            if (IsMilestoneClaimed(milestone.Id))
                continue;

            int currentValue = GetCurrentValue(milestone.StatType);
            ProgressionGoalCandidate candidate = new ProgressionGoalCandidate();
            candidate.Definition = milestone;
            candidate.ProgressValue = Mathf.Clamp(currentValue, 0, milestone.TargetValue);
            candidate.TargetValue = milestone.TargetValue;
            candidate.Completion01 = milestone.TargetValue <= 0
                ? 1f
                : Mathf.Clamp01(candidate.ProgressValue / (float)milestone.TargetValue);
            candidates.Add(candidate);
        }

        candidates.Sort((left, right) =>
        {
            int completionSort = right.Completion01.CompareTo(left.Completion01);

            if (completionSort != 0)
                return completionSort;

            return left.TargetValue.CompareTo(right.TargetValue);
        });

        return candidates;
    }

    static ProgressionMilestoneDefinition GetPriorityGoalDefinition()
    {
        List<ProgressionGoalCandidate> candidates = GetSortedGoalCandidates();
        return candidates.Count > 0 ? candidates[0].Definition : null;
    }

    static string GetMilestoneRequirementLabel(ProgressionMilestoneDefinition milestone)
    {
        switch (milestone.StatType)
        {
            case ProgressionMilestoneStat.BestScore:
                return "Reach best score " + milestone.TargetValue;
            case ProgressionMilestoneStat.LifetimeCoinsCollected:
                return "Collect " + milestone.TargetValue + " lifetime coins";
            case ProgressionMilestoneStat.CompletedRuns:
                return "Finish " + milestone.TargetValue + " runs";
            case ProgressionMilestoneStat.NearMisses:
                return "Trigger " + milestone.TargetValue + " near misses";
            case ProgressionMilestoneStat.DailyChallengeClears:
                return "Clear " + milestone.TargetValue + " daily challenges";
            case ProgressionMilestoneStat.LifetimeShares:
                return "Share " + milestone.TargetValue + " runs or invites";
            case ProgressionMilestoneStat.BestDangerCombo:
                return "Reach danger combo x" + milestone.TargetValue;
            default:
                return "Complete " + milestone.CompactLabel;
        }
    }

    static string GetMilestoneProgressLabel(
        ProgressionMilestoneDefinition milestone,
        int currentValue,
        bool isClaimed)
    {
        if (isClaimed)
            return "Claimed";

        int clampedValue = Mathf.Clamp(currentValue, 0, milestone.TargetValue);

        switch (milestone.StatType)
        {
            case ProgressionMilestoneStat.BestScore:
                return clampedValue + "/" + milestone.TargetValue + " best score";
            case ProgressionMilestoneStat.LifetimeCoinsCollected:
                return clampedValue + "/" + milestone.TargetValue + " coins";
            case ProgressionMilestoneStat.CompletedRuns:
                return clampedValue + "/" + milestone.TargetValue + " runs";
            case ProgressionMilestoneStat.NearMisses:
                return clampedValue + "/" + milestone.TargetValue + " near misses";
            case ProgressionMilestoneStat.DailyChallengeClears:
                return clampedValue + "/" + milestone.TargetValue + " challenge clears";
            case ProgressionMilestoneStat.LifetimeShares:
                return clampedValue + "/" + milestone.TargetValue + " shares";
            case ProgressionMilestoneStat.BestDangerCombo:
                return "Danger x" + clampedValue + "/x" + milestone.TargetValue;
            default:
                return clampedValue + "/" + milestone.TargetValue;
        }
    }

    static string GetMilestoneRewardLabel(ProgressionMilestoneDefinition milestone)
    {
        List<string> rewardParts = new List<string>();

        if (milestone.RewardCoins > 0)
            rewardParts.Add("+" + milestone.RewardCoins + " coins");

        if (milestone.RewardUpgradeAmount > 0)
        {
            rewardParts.Add(
                "+" +
                milestone.RewardUpgradeAmount +
                " " +
                UpgradeInventory.GetDisplayName(milestone.RewardUpgrade));
        }

        return rewardParts.Count > 0 ? string.Join("  |  ", rewardParts.ToArray()) : "Reward pending";
    }
}
