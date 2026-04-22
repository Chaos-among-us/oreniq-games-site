using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public enum RewardedOfferType
{
    PostRunDoubleCoins,
    MidRunRevive
}

public enum MonetizationOfferId
{
    StarterPack,
    CoinPackSmall,
    CoinPackMedium,
    CoinPackLarge
}

public struct BonusUpgradeGrant
{
    public UpgradeType type;
    public int amount;

    public BonusUpgradeGrant(UpgradeType upgradeType, int upgradeAmount)
    {
        type = upgradeType;
        amount = upgradeAmount;
    }
}

public sealed class MonetizationOfferDefinition
{
    public MonetizationOfferId OfferId { get; private set; }
    public string ProductId { get; private set; }
    public ProductType ProductType { get; private set; }
    public string DisplayName { get; private set; }
    public string Description { get; private set; }
    public string HighlightLabel { get; private set; }
    public string FallbackPriceLabel { get; private set; }
    public int CoinsGranted { get; private set; }
    public BonusUpgradeGrant[] BonusUpgrades { get; private set; }

    public bool IsStarterOffer
    {
        get { return OfferId == MonetizationOfferId.StarterPack; }
    }

    public MonetizationOfferDefinition(
        MonetizationOfferId offerId,
        string productId,
        ProductType productType,
        string displayName,
        string description,
        string highlightLabel,
        string fallbackPriceLabel,
        int coinsGranted,
        params BonusUpgradeGrant[] bonusUpgrades)
    {
        OfferId = offerId;
        ProductId = productId;
        ProductType = productType;
        DisplayName = displayName;
        Description = description;
        HighlightLabel = highlightLabel;
        FallbackPriceLabel = fallbackPriceLabel;
        CoinsGranted = coinsGranted;
        BonusUpgrades = bonusUpgrades ?? Array.Empty<BonusUpgradeGrant>();
    }
}

public struct MonetizationOfferSnapshot
{
    public MonetizationOfferDefinition Definition;
    public string PriceLabel;
    public string StatusLabel;
    public bool CanPurchase;
    public bool IsOwned;
}

public struct MonetizationPurchaseResult
{
    public MonetizationOfferId OfferId;
    public bool Success;
    public string Message;
}

public class MonetizationManager : MonoBehaviour
{
    public static MonetizationManager Instance { get; private set; }

    public event Action OnOfferCatalogChanged;

    [SerializeField] private bool simulateRewardedAdsInEditor = true;
    [SerializeField] private bool simulateRewardedAdsInDevelopmentBuilds = true;
    [SerializeField] private bool simulateIapPurchasesInEditor = true;
    [SerializeField] private float simulatedRewardDelaySeconds = 0.75f;
    [SerializeField] private float simulatedPurchaseDelaySeconds = 0.6f;

    private const string TotalCoinsKey = "TotalCoins";
    private const string StarterPackOwnedKey = "StarterPackOwned";
    private const string ProductIdPrefix = "com.oreniq.endlessdodge.";

    private readonly List<MonetizationOfferDefinition> offerDefinitions = new List<MonetizationOfferDefinition>();
    private readonly Dictionary<MonetizationOfferId, MonetizationOfferDefinition> offersById =
        new Dictionary<MonetizationOfferId, MonetizationOfferDefinition>();
    private readonly Dictionary<string, MonetizationOfferDefinition> offersByProductId =
        new Dictionary<string, MonetizationOfferDefinition>();
    private readonly Dictionary<string, Action<MonetizationPurchaseResult>> pendingPurchaseCallbacks =
        new Dictionary<string, Action<MonetizationPurchaseResult>>();

    private StoreController storeController;
    private bool isShowingRewardedAd;
    private bool isIapInitializing;
    private bool hasFetchedProducts;
    private bool isIapPurchaseInProgress;
    private string storeStatusLabel = "Store not initialized";

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
        BuildOfferCatalog();
    }

    async void Start()
    {
        await InitializeIapAsync();
    }

    public bool CanShowRewardedAd(RewardedOfferType offerType)
    {
        if (isShowingRewardedAd)
            return false;

        return IsRewardedAdSimulationEnabled();
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

    public List<MonetizationOfferSnapshot> GetOfferSnapshots()
    {
        List<MonetizationOfferSnapshot> snapshots = new List<MonetizationOfferSnapshot>();

        for (int i = 0; i < offerDefinitions.Count; i++)
        {
            snapshots.Add(BuildSnapshot(offerDefinitions[i]));
        }

        return snapshots;
    }

    public bool CanPurchaseOffer(MonetizationOfferId offerId)
    {
        if (!offersById.TryGetValue(offerId, out MonetizationOfferDefinition definition))
            return false;

        if (isIapPurchaseInProgress)
            return false;

        if (definition.IsStarterOffer && IsStarterPackOwned())
            return false;

        if (IsEditorIapSimulationEnabled())
            return true;

        if (storeController == null || !hasFetchedProducts)
            return false;

        return storeController.GetProductById(definition.ProductId) != null;
    }

    public void PurchaseOffer(MonetizationOfferId offerId, Action<MonetizationPurchaseResult> onCompleted)
    {
        if (!offersById.TryGetValue(offerId, out MonetizationOfferDefinition definition))
        {
            onCompleted?.Invoke(new MonetizationPurchaseResult
            {
                OfferId = offerId,
                Success = false,
                Message = "That offer no longer exists."
            });
            return;
        }

        if (!CanPurchaseOffer(offerId))
        {
            onCompleted?.Invoke(new MonetizationPurchaseResult
            {
                OfferId = offerId,
                Success = false,
                Message = BuildUnavailableMessage(definition)
            });
            return;
        }

        LaunchAnalytics.RecordIapPurchaseRequested(definition.DisplayName, definition.ProductId, IsEditorIapSimulationEnabled());

        if (IsEditorIapSimulationEnabled())
        {
            StartCoroutine(SimulateIapPurchaseRoutine(definition, onCompleted));
            return;
        }

        Product product = storeController.GetProductById(definition.ProductId);

        if (product == null)
        {
            onCompleted?.Invoke(new MonetizationPurchaseResult
            {
                OfferId = offerId,
                Success = false,
                Message = "Store product not fetched yet."
            });
            return;
        }

        isIapPurchaseInProgress = true;
        pendingPurchaseCallbacks[definition.ProductId] = onCompleted;
        NotifyOfferCatalogChanged();
        storeController.PurchaseProduct(product);
    }

    public string GetStoreStatusLabel()
    {
        return storeStatusLabel;
    }

    async System.Threading.Tasks.Task InitializeIapAsync()
    {
        if (isIapInitializing || IsEditorIapSimulationEnabled())
        {
            if (IsEditorIapSimulationEnabled())
            {
                storeStatusLabel = "Editor purchase simulation enabled";
                NotifyOfferCatalogChanged();
            }

            return;
        }

        isIapInitializing = true;
        storeStatusLabel = "Connecting store...";
        NotifyOfferCatalogChanged();

        try
        {
            bool servicesReady = await UnityServicesBootstrap.EnsureInitializedAsync();

            if (!servicesReady)
            {
                storeStatusLabel = "Unity Services not ready";
                return;
            }

            storeController = UnityIAPServices.StoreController();
            RegisterStoreCallbacks(storeController);
            await storeController.Connect();
            storeStatusLabel = "Fetching products...";
            storeController.FetchProducts(BuildProductDefinitions());
        }
        catch (Exception exception)
        {
            storeStatusLabel = "Store setup failed";
            Debug.LogWarning("Monetization store initialization failed: " + exception.Message);
        }
        finally
        {
            isIapInitializing = false;
            NotifyOfferCatalogChanged();
        }
    }

    void RegisterStoreCallbacks(StoreController controller)
    {
        controller.OnProductsFetched += HandleProductsFetched;
        controller.OnProductsFetchFailed += HandleProductsFetchFailed;
        controller.OnPurchasePending += HandlePurchasePending;
        controller.OnPurchaseFailed += HandlePurchaseFailed;
        controller.OnPurchaseDeferred += HandlePurchaseDeferred;
        controller.OnStoreDisconnected += HandleStoreDisconnected;
    }

    void HandleProductsFetched(List<Product> products)
    {
        hasFetchedProducts = true;
        storeStatusLabel = products.Count > 0 ? "Store ready" : "No store products fetched";
        NotifyOfferCatalogChanged();
    }

    void HandleProductsFetchFailed(ProductFetchFailed failure)
    {
        hasFetchedProducts = false;
        storeStatusLabel = "Product fetch failed";
        Debug.LogWarning("Monetization product fetch failed: " + failure);
        NotifyOfferCatalogChanged();
    }

    void HandleStoreDisconnected(StoreConnectionFailureDescription failure)
    {
        hasFetchedProducts = false;
        storeStatusLabel = "Store disconnected";
        Debug.LogWarning("Monetization store disconnected: " + failure);
        NotifyOfferCatalogChanged();
    }

    void HandlePurchasePending(PendingOrder order)
    {
        foreach (CartItem item in order.CartOrdered.Items())
        {
            Product product = item.Product;

            if (!offersByProductId.TryGetValue(product.definition.id, out MonetizationOfferDefinition definition))
                continue;

            GrantOffer(definition);
            CompletePendingPurchase(
                definition,
                true,
                definition.DisplayName + " added.");
        }

        if (storeController != null)
            storeController.ConfirmPurchase(order);

        isIapPurchaseInProgress = false;
        NotifyOfferCatalogChanged();
    }

    void HandlePurchaseFailed(FailedOrder failedOrder)
    {
        foreach (CartItem item in failedOrder.CartOrdered.Items())
        {
            Product product = item.Product;

            if (!offersByProductId.TryGetValue(product.definition.id, out MonetizationOfferDefinition definition))
                continue;

            CompletePendingPurchase(
                definition,
                false,
                definition.DisplayName + " failed.");
        }

        isIapPurchaseInProgress = false;
        NotifyOfferCatalogChanged();
    }

    void HandlePurchaseDeferred(DeferredOrder deferredOrder)
    {
        foreach (CartItem item in deferredOrder.CartOrdered.Items())
        {
            Product product = item.Product;

            if (!offersByProductId.TryGetValue(product.definition.id, out MonetizationOfferDefinition definition))
                continue;

            CompletePendingPurchase(
                definition,
                false,
                definition.DisplayName + " pending.");
        }

        isIapPurchaseInProgress = false;
        NotifyOfferCatalogChanged();
    }

    void CompletePendingPurchase(MonetizationOfferDefinition definition, bool success, string message)
    {
        LaunchAnalytics.RecordIapPurchaseResult(
            definition.DisplayName,
            definition.ProductId,
            success,
            IsEditorIapSimulationEnabled());

        if (pendingPurchaseCallbacks.TryGetValue(definition.ProductId, out Action<MonetizationPurchaseResult> callback))
        {
            pendingPurchaseCallbacks.Remove(definition.ProductId);
            callback?.Invoke(new MonetizationPurchaseResult
            {
                OfferId = definition.OfferId,
                Success = success,
                Message = message
            });
        }
    }

    IEnumerator SimulateRewardedAdRoutine(Action<bool> onCompleted)
    {
        isShowingRewardedAd = true;
        yield return new WaitForSecondsRealtime(simulatedRewardDelaySeconds);
        isShowingRewardedAd = false;
        onCompleted?.Invoke(true);
    }

    IEnumerator SimulateIapPurchaseRoutine(MonetizationOfferDefinition definition, Action<MonetizationPurchaseResult> onCompleted)
    {
        isIapPurchaseInProgress = true;
        NotifyOfferCatalogChanged();
        yield return new WaitForSecondsRealtime(simulatedPurchaseDelaySeconds);
        GrantOffer(definition);
        isIapPurchaseInProgress = false;
        LaunchAnalytics.RecordIapPurchaseResult(definition.DisplayName, definition.ProductId, true, true);
        onCompleted?.Invoke(new MonetizationPurchaseResult
        {
            OfferId = definition.OfferId,
            Success = true,
            Message = definition.DisplayName + " added."
        });
        NotifyOfferCatalogChanged();
    }

    MonetizationOfferSnapshot BuildSnapshot(MonetizationOfferDefinition definition)
    {
        bool isOwned = definition.IsStarterOffer && IsStarterPackOwned();
        bool canPurchase = CanPurchaseOffer(definition.OfferId);
        string statusLabel;

        if (isOwned)
        {
            statusLabel = "Owned";
        }
        else if (isIapPurchaseInProgress)
        {
            statusLabel = "Purchase in progress";
        }
        else if (canPurchase)
        {
            statusLabel = IsEditorIapSimulationEnabled() ? "Editor test purchase" : "Tap to buy";
        }
        else
        {
            statusLabel = BuildUnavailableMessage(definition);
        }

        return new MonetizationOfferSnapshot
        {
            Definition = definition,
            PriceLabel = ResolvePriceLabel(definition),
            StatusLabel = statusLabel,
            CanPurchase = canPurchase,
            IsOwned = isOwned
        };
    }

    string ResolvePriceLabel(MonetizationOfferDefinition definition)
    {
        if (IsEditorIapSimulationEnabled())
            return definition.FallbackPriceLabel + " editor";

        if (storeController != null)
        {
            Product product = storeController.GetProductById(definition.ProductId);

            if (product != null &&
                product.metadata != null &&
                !string.IsNullOrEmpty(product.metadata.localizedPriceString))
            {
                return product.metadata.localizedPriceString;
            }
        }

        return definition.FallbackPriceLabel;
    }

    string BuildUnavailableMessage(MonetizationOfferDefinition definition)
    {
        if (definition.IsStarterOffer && IsStarterPackOwned())
            return "Already owned";

        if (isIapInitializing)
            return "Store starting";

        if (isIapPurchaseInProgress)
            return "Buying...";

        if (IsEditorIapSimulationEnabled())
            return "Editor test";

        if (storeController == null)
            return "Store offline";

        if (!hasFetchedProducts)
            return "Loading offers";

        return "Not ready";
    }

    void GrantOffer(MonetizationOfferDefinition definition)
    {
        int updatedCoins = PlayerPrefs.GetInt(TotalCoinsKey, 0) + definition.CoinsGranted;
        PlayerPrefs.SetInt(TotalCoinsKey, updatedCoins);

        for (int i = 0; i < definition.BonusUpgrades.Length; i++)
        {
            GrantUpgrade(definition.BonusUpgrades[i]);
        }

        if (definition.IsStarterOffer)
            PlayerPrefs.SetInt(StarterPackOwnedKey, 1);

        PlayerPrefs.Save();
        NotifyOfferCatalogChanged();
    }

    void GrantUpgrade(BonusUpgradeGrant grant)
    {
        if (grant.amount <= 0)
            return;

        if (UpgradeInventory.Instance != null)
        {
            UpgradeInventory.Instance.AddUpgrade(grant.type, grant.amount);
            return;
        }

        string key = "Upgrade_" + grant.type;
        int updatedAmount = PlayerPrefs.GetInt(key, 0) + grant.amount;
        PlayerPrefs.SetInt(key, updatedAmount);
    }

    bool IsStarterPackOwned()
    {
        return PlayerPrefs.GetInt(StarterPackOwnedKey, 0) == 1;
    }

    bool IsEditorIapSimulationEnabled()
    {
#if UNITY_EDITOR
        return simulateIapPurchasesInEditor;
#else
        return false;
#endif
    }

    bool IsRewardedAdSimulationEnabled()
    {
        if (Application.isEditor)
            return simulateRewardedAdsInEditor;

        return Debug.isDebugBuild && simulateRewardedAdsInDevelopmentBuilds;
    }

    List<ProductDefinition> BuildProductDefinitions()
    {
        List<ProductDefinition> definitions = new List<ProductDefinition>();

        for (int i = 0; i < offerDefinitions.Count; i++)
        {
            MonetizationOfferDefinition offer = offerDefinitions[i];
            definitions.Add(new ProductDefinition(offer.ProductId, offer.ProductType));
        }

        return definitions;
    }

    void BuildOfferCatalog()
    {
        offerDefinitions.Clear();
        offersById.Clear();
        offersByProductId.Clear();

        AddOffer(new MonetizationOfferDefinition(
            MonetizationOfferId.StarterPack,
            ProductIdPrefix + "starter_pack",
            ProductType.NonConsumable,
            "Starter Pack",
            "450 coins, 3 shields, and 2 extra lives for a strong first push.",
            "Best launch value",
            "$1.99",
            450,
            new BonusUpgradeGrant(UpgradeType.Shield, 3),
            new BonusUpgradeGrant(UpgradeType.ExtraLife, 2)));

        AddOffer(new MonetizationOfferDefinition(
            MonetizationOfferId.CoinPackSmall,
            ProductIdPrefix + "coin_pack_small",
            ProductType.Consumable,
            "Small Coin Pack",
            "A quick top-up for a few extra consumables.",
            "Quick refill",
            "$0.99",
            250));

        AddOffer(new MonetizationOfferDefinition(
            MonetizationOfferId.CoinPackMedium,
            ProductIdPrefix + "coin_pack_medium",
            ProductType.Consumable,
            "Medium Coin Pack",
            "A stronger stock-up for repeat runs and experiments.",
            "Popular pick",
            "$2.99",
            800));

        AddOffer(new MonetizationOfferDefinition(
            MonetizationOfferId.CoinPackLarge,
            ProductIdPrefix + "coin_pack_large",
            ProductType.Consumable,
            "Large Coin Pack",
            "A big stash for loadouts, retries, and challenge prep.",
            "Best coin value",
            "$5.99",
            1900));
    }

    void AddOffer(MonetizationOfferDefinition definition)
    {
        offerDefinitions.Add(definition);
        offersById[definition.OfferId] = definition;
        offersByProductId[definition.ProductId] = definition;
    }

    void NotifyOfferCatalogChanged()
    {
        OnOfferCatalogChanged?.Invoke();
    }
}
