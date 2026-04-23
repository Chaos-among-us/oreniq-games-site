using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class LaunchAnalytics
{
    private sealed class PendingAnalyticsEvent
    {
        public string EventName;
        public Dictionary<string, object> Parameters;
    }

    private static readonly List<PendingAnalyticsEvent> pendingEvents = new List<PendingAnalyticsEvent>();
    private static readonly string sessionId = Guid.NewGuid().ToString("N");

    private static bool sessionStartedRecorded;
    private static bool analyticsUnavailableLogged;

    public static void RecordSessionStarted()
    {
        if (sessionStartedRecorded)
            return;

        sessionStartedRecorded = true;
        RecordEvent("session_started");
    }

    public static void RecordRunStarted(bool isDailyChallenge, int loadoutCount)
    {
        RecordEvent(
            "run_started",
            ("daily_challenge", isDailyChallenge),
            ("loadout_count", loadoutCount));
    }

    public static void RecordRunFinished(
        int finalScore,
        int coinsEarned,
        int levelReached,
        bool isDailyChallenge,
        bool newBestScore)
    {
        RecordEvent(
            "run_finished",
            ("final_score", finalScore),
            ("coins_earned", coinsEarned),
            ("level_reached", levelReached),
            ("daily_challenge", isDailyChallenge),
            ("new_best_score", newBestScore));
    }

    public static void RecordSoftCurrencyPurchase(UpgradeType type, int cost, int remainingCoins)
    {
        RecordEvent(
            "soft_shop_purchase",
            ("upgrade_type", type.ToString()),
            ("cost", cost),
            ("remaining_total_coins", remainingCoins));
    }

    public static void RecordDailyRewardClaimed(DailyRewardPackage rewardPackage, int resultingStreak)
    {
        RecordEvent(
            "daily_reward_claimed",
            ("reward_day", rewardPackage.rewardDay),
            ("coins", rewardPackage.coins),
            ("bonus_upgrade", rewardPackage.bonusUpgrade.ToString()),
            ("bonus_amount", rewardPackage.bonusAmount),
            ("streak", resultingStreak));
    }

    public static void RecordMissionRewardsClaimed(int coinsGranted, int upgradeCount)
    {
        RecordEvent(
            "daily_mission_rewards_claimed",
            ("coins_granted", coinsGranted),
            ("upgrade_count", upgradeCount));
    }

    public static void RecordDailyChallengeStarted(DailyChallengeData challenge)
    {
        RecordEvent(
            "daily_challenge_started",
            ("challenge_type", challenge.type.ToString()),
            ("goal_type", challenge.goalType.ToString()),
            ("target", challenge.targetScore));
    }

    public static void RecordDailyChallengeRewardClaimed(DailyChallengeData challenge)
    {
        RecordEvent(
            "daily_challenge_reward_claimed",
            ("challenge_type", challenge.type.ToString()),
            ("goal_type", challenge.goalType.ToString()),
            ("target", challenge.targetScore),
            ("reward_coins", challenge.rewardCoins),
            ("reward_upgrade", challenge.rewardUpgrade.ToString()),
            ("reward_upgrade_amount", challenge.rewardUpgradeAmount));
    }

    public static void RecordRewardedOfferRequested(string offerName, int rewardAmount)
    {
        RecordEvent(
            "rewarded_offer_requested",
            ("offer_name", offerName),
            ("reward_amount", rewardAmount));
    }

    public static void RecordRewardedOfferResult(string offerName, bool rewarded, int rewardAmount)
    {
        RecordEvent(
            "rewarded_offer_result",
            ("offer_name", offerName),
            ("rewarded", rewarded),
            ("reward_amount", rewardAmount));
    }

    public static void RecordShareTapped(string surface, bool isDailyChallenge, int finalScore, int levelReached)
    {
        RecordEvent(
            "share_tapped",
            ("surface", surface),
            ("daily_challenge", isDailyChallenge),
            ("final_score", finalScore),
            ("level_reached", levelReached));
    }

    public static void RecordReviewPromptShown(string surface, int completedRuns, int finalScore, bool newBestScore)
    {
        RecordEvent(
            "review_prompt_shown",
            ("surface", surface),
            ("completed_runs", completedRuns),
            ("final_score", finalScore),
            ("new_best_score", newBestScore));
    }

    public static void RecordReviewTapped(string surface, bool launchedStoreListing)
    {
        RecordEvent(
            "review_tapped",
            ("surface", surface),
            ("launched_store_listing", launchedStoreListing));
    }

    public static void RecordIapPurchaseRequested(string offerName, string productId, bool simulated)
    {
        RecordEvent(
            "iap_purchase_requested",
            ("offer_name", offerName),
            ("product_id", productId),
            ("simulated", simulated));
    }

    public static void RecordIapPurchaseResult(string offerName, string productId, bool success, bool simulated)
    {
        RecordEvent(
            "iap_purchase_result",
            ("offer_name", offerName),
            ("product_id", productId),
            ("success", success),
            ("simulated", simulated));
    }

    public static void FlushPendingEvents()
    {
        if (!UnityServicesBootstrap.IsReady || pendingEvents.Count == 0)
            return;

        for (int i = pendingEvents.Count - 1; i >= 0; i--)
        {
            if (TrySendEvent(pendingEvents[i]))
                pendingEvents.RemoveAt(i);
        }
    }

    static void RecordEvent(string eventName, params (string key, object value)[] parameters)
    {
        Dictionary<string, object> values = new Dictionary<string, object>();
        values["session_id"] = sessionId;
        values["event_recorded_at"] = DateTime.UtcNow.ToString("o");

        for (int i = 0; i < parameters.Length; i++)
        {
            if (string.IsNullOrEmpty(parameters[i].key) || parameters[i].value == null)
                continue;

            values[parameters[i].key] = parameters[i].value;
        }

        PendingAnalyticsEvent analyticsEvent = new PendingAnalyticsEvent
        {
            EventName = eventName,
            Parameters = values
        };

        if (!TrySendEvent(analyticsEvent))
            pendingEvents.Add(analyticsEvent);
    }

    static bool TrySendEvent(PendingAnalyticsEvent analyticsEvent)
    {
        if (!UnityServicesBootstrap.IsReady)
            return false;

        Type analyticsServiceType = FindType("Unity.Services.Analytics.AnalyticsService");
        Type customEventType = FindType("Unity.Services.Analytics.CustomEvent");

        if (analyticsServiceType == null || customEventType == null)
        {
            if (!analyticsUnavailableLogged)
            {
                analyticsUnavailableLogged = true;
                Debug.LogWarning("Unity Analytics types are not available yet. Create the custom event schemas in the dashboard after the packages finish importing.");
            }

            return false;
        }

        try
        {
            PropertyInfo instanceProperty = analyticsServiceType.GetProperty(
                "Instance",
                BindingFlags.Public | BindingFlags.Static);

            if (instanceProperty == null)
                return false;

            object analyticsInstance = instanceProperty.GetValue(null);

            if (analyticsInstance == null)
                return false;

            object customEvent = Activator.CreateInstance(customEventType, analyticsEvent.EventName);

            if (customEvent == null)
                return false;

            MethodInfo addMethod = customEventType.GetMethod(
                "Add",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(string), typeof(object) },
                null);

            if (addMethod == null)
                return false;

            foreach (KeyValuePair<string, object> pair in analyticsEvent.Parameters)
            {
                if (!IsSupportedAnalyticsValue(pair.Value))
                    continue;

                addMethod.Invoke(customEvent, new[] { pair.Key, pair.Value });
            }

            MethodInfo recordEventMethod = analyticsInstance.GetType().GetMethod(
                "RecordEvent",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { customEventType },
                null);

            if (recordEventMethod == null)
                return false;

            recordEventMethod.Invoke(analyticsInstance, new[] { customEvent });
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Analytics event send failed for " + analyticsEvent.EventName + ": " + exception.Message);
            return false;
        }
    }

    static bool IsSupportedAnalyticsValue(object value)
    {
        return value is string ||
               value is int ||
               value is long ||
               value is float ||
               value is double ||
               value is bool ||
               value is DateTime;
    }

    static Type FindType(string fullName)
    {
        Type directMatch = Type.GetType(fullName);

        if (directMatch != null)
            return directMatch;

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        for (int i = 0; i < assemblies.Length; i++)
        {
            Type assemblyMatch = assemblies[i].GetType(fullName);

            if (assemblyMatch != null)
                return assemblyMatch;
        }

        return null;
    }
}
