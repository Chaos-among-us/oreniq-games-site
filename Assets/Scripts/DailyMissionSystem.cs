using System;
using System.Collections.Generic;
using UnityEngine;

public enum DailyMissionType
{
    CollectCoins,
    ReachScore,
    PlayRuns
}

public struct DailyMissionData
{
    public DailyMissionType type;
    public int target;
    public int progress;
    public int rewardCoins;
    public UpgradeType rewardUpgrade;
    public int rewardUpgradeAmount;
    public bool claimed;
}

public static class DailyMissionSystem
{
    private const int MissionCount = 3;
    private const string MissionDateKey = "DailyMission_Date";
    private const string TotalCoinsKey = "TotalCoins";
    private const string MissionPrefix = "DailyMission_";

    public static void EnsureInitializedForToday()
    {
        string todayKey = DateTime.Today.ToString("yyyy-MM-dd");

        if (PlayerPrefs.GetString(MissionDateKey, string.Empty) == todayKey)
            return;

        CreateTodayMissions(todayKey);
    }

    public static DailyMissionData[] GetMissions()
    {
        EnsureInitializedForToday();

        DailyMissionData[] missions = new DailyMissionData[MissionCount];

        for (int i = 0; i < MissionCount; i++)
            missions[i] = LoadMission(i);

        return missions;
    }

    public static void RegisterCoinsCollected(int amount)
    {
        EnsureInitializedForToday();
        UpdateMissionProgress(DailyMissionType.CollectCoins, amount, false);
    }

    public static void RegisterRunFinished(int score, int levelReached)
    {
        EnsureInitializedForToday();
        UpdateMissionProgress(DailyMissionType.PlayRuns, 1, false);
        UpdateMissionProgress(DailyMissionType.ReachScore, score, true);
    }

    public static int GetClaimableCount()
    {
        DailyMissionData[] missions = GetMissions();
        int count = 0;

        for (int i = 0; i < missions.Length; i++)
        {
            if (missions[i].progress >= missions[i].target && !missions[i].claimed)
                count += 1;
        }

        return count;
    }

    public static int GetCompletedCount()
    {
        DailyMissionData[] missions = GetMissions();
        int count = 0;

        for (int i = 0; i < missions.Length; i++)
        {
            if (missions[i].progress >= missions[i].target)
                count += 1;
        }

        return count;
    }

    public static bool ClaimCompletedRewards(out int coinsGranted, out List<string> upgradesGranted)
    {
        EnsureInitializedForToday();

        coinsGranted = 0;
        upgradesGranted = new List<string>();
        bool claimedAny = false;

        for (int i = 0; i < MissionCount; i++)
        {
            DailyMissionData mission = LoadMission(i);

            if (mission.claimed || mission.progress < mission.target)
                continue;

            mission.claimed = true;
            SaveMission(i, mission);

            coinsGranted += mission.rewardCoins;

            if (mission.rewardUpgradeAmount > 0 && UpgradeInventory.Instance != null)
            {
                UpgradeInventory.Instance.AddUpgrade(mission.rewardUpgrade, mission.rewardUpgradeAmount);
                upgradesGranted.Add(mission.rewardUpgradeAmount + " " + UpgradeInventory.GetDisplayName(mission.rewardUpgrade));
            }

            claimedAny = true;
        }

        if (claimedAny)
        {
            int totalCoins = PlayerPrefs.GetInt(TotalCoinsKey, 0);
            totalCoins += coinsGranted;
            PlayerPrefs.SetInt(TotalCoinsKey, totalCoins);
            PlayerPrefs.Save();
            LaunchAnalytics.RecordMissionRewardsClaimed(coinsGranted, upgradesGranted.Count);
        }

        return claimedAny;
    }

    public static string GetMissionSummary(DailyMissionData mission)
    {
        switch (mission.type)
        {
            case DailyMissionType.CollectCoins:
                return "Collect " + mission.target + " coins";
            case DailyMissionType.ReachScore:
                return "Reach score " + mission.target;
            default:
                return "Play " + mission.target + " runs";
        }
    }

    public static string GetMissionProgressLabel(DailyMissionData mission)
    {
        switch (mission.type)
        {
            case DailyMissionType.CollectCoins:
                return "Coins " + mission.progress + "/" + mission.target;
            case DailyMissionType.ReachScore:
                return "Score " + mission.progress + "/" + mission.target;
            default:
                return "Runs " + mission.progress + "/" + mission.target;
        }
    }

    public static string GetMissionRewardLabel(DailyMissionData mission)
    {
        return mission.rewardCoins + "c + " + GetShortUpgradeName(mission.rewardUpgrade);
    }

    public static string GetCompactMissionLabel(DailyMissionData mission)
    {
        switch (mission.type)
        {
            case DailyMissionType.CollectCoins:
                return "Collect " + mission.target;
            case DailyMissionType.ReachScore:
                return "Score " + mission.target;
            default:
                return "Play " + mission.target + " runs";
        }
    }

    static void CreateTodayMissions(string todayKey)
    {
        int seed = DateTime.Today.DayOfYear + (DateTime.Today.Year * 17);
        PlayerPrefs.SetString(MissionDateKey, todayKey);

        DailyMissionData collectCoins = new DailyMissionData();
        collectCoins.type = DailyMissionType.CollectCoins;
        collectCoins.target = 20 + ((seed % 3) * 10);
        collectCoins.progress = 0;
        collectCoins.rewardCoins = 80;
        collectCoins.rewardUpgrade = UpgradeType.CoinMagnet;
        collectCoins.rewardUpgradeAmount = 1;
        collectCoins.claimed = false;

        DailyMissionData reachScore = new DailyMissionData();
        reachScore.type = DailyMissionType.ReachScore;
        reachScore.target = 40 + ((seed % 4) * 10);
        reachScore.progress = 0;
        reachScore.rewardCoins = 100;
        reachScore.rewardUpgrade = UpgradeType.Shield;
        reachScore.rewardUpgradeAmount = 1;
        reachScore.claimed = false;

        DailyMissionData playRuns = new DailyMissionData();
        playRuns.type = DailyMissionType.PlayRuns;
        playRuns.target = 3 + (seed % 2);
        playRuns.progress = 0;
        playRuns.rewardCoins = 120;
        playRuns.rewardUpgrade = UpgradeType.SpeedBoost;
        playRuns.rewardUpgradeAmount = 1;
        playRuns.claimed = false;

        SaveMission(0, collectCoins);
        SaveMission(1, reachScore);
        SaveMission(2, playRuns);
        PlayerPrefs.Save();
    }

    static void UpdateMissionProgress(DailyMissionType missionType, int amount, bool keepHighestValue)
    {
        for (int i = 0; i < MissionCount; i++)
        {
            DailyMissionData mission = LoadMission(i);

            if (mission.type != missionType)
                continue;

            if (keepHighestValue)
                mission.progress = Mathf.Max(mission.progress, amount);
            else
                mission.progress += amount;

            mission.progress = Mathf.Min(mission.progress, mission.target);
            SaveMission(i, mission);
            PlayerPrefs.Save();
            return;
        }
    }

    static DailyMissionData LoadMission(int index)
    {
        DailyMissionData mission = new DailyMissionData();
        mission.type = (DailyMissionType)PlayerPrefs.GetInt(GetKey(index, "Type"), 0);
        mission.target = PlayerPrefs.GetInt(GetKey(index, "Target"), 1);
        mission.progress = PlayerPrefs.GetInt(GetKey(index, "Progress"), 0);
        mission.rewardCoins = PlayerPrefs.GetInt(GetKey(index, "RewardCoins"), 0);
        mission.rewardUpgrade = (UpgradeType)PlayerPrefs.GetInt(GetKey(index, "RewardUpgrade"), 0);
        mission.rewardUpgradeAmount = PlayerPrefs.GetInt(GetKey(index, "RewardUpgradeAmount"), 0);
        mission.claimed = PlayerPrefs.GetInt(GetKey(index, "Claimed"), 0) == 1;
        return mission;
    }

    static void SaveMission(int index, DailyMissionData mission)
    {
        PlayerPrefs.SetInt(GetKey(index, "Type"), (int)mission.type);
        PlayerPrefs.SetInt(GetKey(index, "Target"), mission.target);
        PlayerPrefs.SetInt(GetKey(index, "Progress"), mission.progress);
        PlayerPrefs.SetInt(GetKey(index, "RewardCoins"), mission.rewardCoins);
        PlayerPrefs.SetInt(GetKey(index, "RewardUpgrade"), (int)mission.rewardUpgrade);
        PlayerPrefs.SetInt(GetKey(index, "RewardUpgradeAmount"), mission.rewardUpgradeAmount);
        PlayerPrefs.SetInt(GetKey(index, "Claimed"), mission.claimed ? 1 : 0);
    }

    static string GetKey(int index, string suffix)
    {
        return MissionPrefix + index + "_" + suffix;
    }

    static string GetShortUpgradeName(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.SpeedBoost:
                return "Speed";
            case UpgradeType.CoinMagnet:
                return "Magnet";
            case UpgradeType.DoubleCoins:
                return "Double";
            case UpgradeType.SlowTime:
                return "Slow";
            case UpgradeType.SmallerPlayer:
                return "Small";
            case UpgradeType.ScoreBooster:
                return "Score";
            case UpgradeType.RareCoinBoost:
                return "Rare Coin";
            default:
                return UpgradeInventory.GetDisplayName(type);
        }
    }
}
