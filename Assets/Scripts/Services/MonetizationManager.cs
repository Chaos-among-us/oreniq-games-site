using System;
using System.Collections;
using UnityEngine;

public enum RewardedOfferType
{
    PostRunDoubleCoins
}

public class MonetizationManager : MonoBehaviour
{
    public static MonetizationManager Instance { get; private set; }

    [SerializeField] private bool simulateRewardedAdsInEditor = true;
    [SerializeField] private float simulatedRewardDelaySeconds = 0.75f;

    private bool isShowingRewardedAd;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureExists()
    {
        if (FindAnyObjectByType<MonetizationManager>() != null)
            return;

        GameObject monetizationObject = new GameObject("MonetizationManager");
        monetizationObject.AddComponent<MonetizationManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool CanShowRewardedAd(RewardedOfferType offerType)
    {
        if (isShowingRewardedAd)
            return false;

#if UNITY_EDITOR
        return simulateRewardedAdsInEditor;
#else
        return false;
#endif
    }

    public void ShowRewardedAd(RewardedOfferType offerType, Action<bool> onCompleted)
    {
        if (!CanShowRewardedAd(offerType))
        {
            onCompleted?.Invoke(false);
            return;
        }

        StartCoroutine(SimulateRewardedAdRoutine(onCompleted));
    }

    IEnumerator SimulateRewardedAdRoutine(Action<bool> onCompleted)
    {
        isShowingRewardedAd = true;
        yield return new WaitForSecondsRealtime(simulatedRewardDelaySeconds);
        isShowingRewardedAd = false;
        onCompleted?.Invoke(true);
    }
}
