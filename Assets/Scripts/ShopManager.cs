using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public TextMeshProUGUI totalCoinsText;

    public TextMeshProUGUI speedUpgradeText;
    public TextMeshProUGUI shieldUpgradeText;
    public TextMeshProUGUI extraLifeUpgradeText;
    public TextMeshProUGUI coinMagnetUpgradeText;
    public TextMeshProUGUI doubleCoinsUpgradeText;
    public TextMeshProUGUI slowTimeUpgradeText;
    public TextMeshProUGUI smallerPlayerUpgradeText;
    public TextMeshProUGUI scoreBoosterUpgradeText;
    public TextMeshProUGUI bombUpgradeText;
    public TextMeshProUGUI rareCoinBoostUpgradeText;

    public string titleObjectName = "ShopTitle";
    public string backButtonObjectName = "BackButton";

    public float sidePadding = 18f;
    public float topPadding = 140f;
    public float bottomPadding = 84f;
    public float rowSpacing = 12f;
    public float offerItemHeight = 228f;
    public float itemHeight = 236f;
    public float sectionHeaderHeight = 42f;
    public float sectionSpacing = 12f;
    public float sectionGap = 20f;
    [SerializeField] private bool seedEditorCoinsIfEmpty = true;
    [SerializeField] private int editorSeedCoinsAmount = 250;

    private int totalCoins;
    private TMP_FontAsset runtimeFont;
    private TextMeshProUGUI shopTitleText;
    private RectTransform shopTitleRect;
    private RectTransform backButtonRect;
    private TextMeshProUGUI backButtonText;
    private TextMeshProUGUI feedbackText;
    private TextMeshProUGUI monetizationHeaderText;
    private TextMeshProUGUI consumablesHeaderText;
    private RectTransform viewportRect;
    private RectTransform contentRect;
    private ScrollRect scrollRect;
    private RectTransform uiRootRect;
    private bool shouldSnapScrollToTop;
    private readonly UpgradeType[] shopOrder =
    {
        UpgradeType.Shield,
        UpgradeType.SpeedBoost,
        UpgradeType.ExtraLife,
        UpgradeType.CoinMagnet,
        UpgradeType.DoubleCoins,
        UpgradeType.SlowTime,
        UpgradeType.SmallerPlayer,
        UpgradeType.ScoreBooster,
        UpgradeType.Bomb,
        UpgradeType.RareCoinBoost
    };

    private readonly Dictionary<UpgradeType, ShopUpgradeButtonUI> runtimeButtons =
        new Dictionary<UpgradeType, ShopUpgradeButtonUI>();
    private readonly Dictionary<MonetizationOfferId, ShopOfferCardUI> runtimeOfferCards =
        new Dictionary<MonetizationOfferId, ShopOfferCardUI>();
    private readonly List<MonetizationOfferId> offerOrder = new List<MonetizationOfferId>();

    private const string FeedbackObjectName = "ShopFeedbackText";
    private const string ViewportObjectName = "ShopViewport";
    private const string ContentObjectName = "ShopContent";
    private const string MonetizationHeaderObjectName = "MonetizationHeader";
    private const string ConsumablesHeaderObjectName = "ConsumablesHeader";

#if UNITY_EDITOR
    private static bool seededEditorCoinsThisSession;
#endif

    void Awake()
    {
        FindStaticReferences();
        NormalizeLegacyRootLayout();
        runtimeFont = GetRuntimeFont();
        EnsureRuntimeUI();
    }

    void Start()
    {
        ResolveMonetizationManager(true);
        EnsureEditorTestBalance();
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        BuildShopButtons();
        shouldSnapScrollToTop = true;
        RefreshUI();
    }

    void OnEnable()
    {
        ResolveMonetizationManager(true);
        SubscribeMonetizationEvents();
        EnsureEditorTestBalance();
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        BuildShopButtons();
        shouldSnapScrollToTop = true;
        RefreshUI();
    }

    void OnDisable()
    {
        UnsubscribeMonetizationEvents();
    }

    void OnRectTransformDimensionsChange()
    {
        if (!isActiveAndEnabled)
            return;

        RefreshUI();
    }

    void FindStaticReferences()
    {
        if (totalCoinsText == null)
        {
            totalCoinsText = FindTextObject("TotalCoinsText");
        }
        if (shopTitleText == null)
        {
            shopTitleText = FindTextObject(titleObjectName);
        }

        if (shopTitleText != null)
            shopTitleRect = shopTitleText.rectTransform;

        if (backButtonRect == null)
        {
            GameObject backButtonObject = GameObject.Find(backButtonObjectName);

            if (backButtonObject != null)
            {
                backButtonRect = backButtonObject.GetComponent<RectTransform>();
                backButtonText = backButtonObject.GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }
        if (uiRootRect == null)
            uiRootRect = GetUIRootRect();
    }

    TextMeshProUGUI FindTextObject(string objectName)
    {
        GameObject textObject = GameObject.Find(objectName);

        if (textObject == null)
            return null;

        return textObject.GetComponent<TextMeshProUGUI>();
    }

    RectTransform GetUIRootRect()
    {
        if (totalCoinsText != null && totalCoinsText.rectTransform.parent != null)
            return totalCoinsText.rectTransform.parent as RectTransform;

        Canvas canvas = FindAnyObjectByType<Canvas>();

        if (canvas != null)
            return canvas.GetComponent<RectTransform>();

        return null;
    }

    TMP_FontAsset GetRuntimeFont()
    {
        if (totalCoinsText != null)
            return totalCoinsText.font;

        if (shopTitleText != null)
            return shopTitleText.font;

        return null;
    }

    void NormalizeLegacyRootLayout()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();

        if (canvas != null)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();

            if (canvasRect != null)
            {
                canvasRect.localScale = Vector3.one;
                canvasRect.anchorMin = Vector2.zero;
                canvasRect.anchorMax = Vector2.zero;
                canvasRect.sizeDelta = Vector2.zero;
            }
        }

        if (uiRootRect == null && totalCoinsText != null)
            uiRootRect = totalCoinsText.rectTransform.parent as RectTransform;

        if (uiRootRect != null)
        {
            uiRootRect.localScale = Vector3.one;
            uiRootRect.anchorMin = Vector2.zero;
            uiRootRect.anchorMax = Vector2.one;
            uiRootRect.pivot = new Vector2(0.5f, 0.5f);
            uiRootRect.anchoredPosition = Vector2.zero;
            uiRootRect.sizeDelta = Vector2.zero;
            uiRootRect.offsetMin = Vector2.zero;
            uiRootRect.offsetMax = Vector2.zero;
        }
    }

    void EnsureRuntimeUI()
    {
        HideLegacyShopButtons();
        HideLegacyShopLabels();
        EnsureFeedbackText();
        EnsureScrollArea();
        EnsureSectionHeaders();
        LayoutStaticElements();
    }

    void SubscribeMonetizationEvents()
    {
        MonetizationManager manager = ResolveMonetizationManager(true);

        if (manager == null)
            return;

        manager.OnOfferCatalogChanged -= HandleOfferCatalogChanged;
        manager.OnOfferCatalogChanged += HandleOfferCatalogChanged;
    }

    void UnsubscribeMonetizationEvents()
    {
        MonetizationManager manager = ResolveMonetizationManager(false);

        if (manager == null)
            return;

        manager.OnOfferCatalogChanged -= HandleOfferCatalogChanged;
    }

    void HandleOfferCatalogChanged()
    {
        if (!isActiveAndEnabled)
            return;

        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        RefreshUI();
    }

    MonetizationManager ResolveMonetizationManager(bool createIfMissing)
    {
        if (MonetizationManager.Instance != null)
            return MonetizationManager.Instance;

        MonetizationManager existingManager = FindAnyObjectByType<MonetizationManager>();

        if (existingManager != null)
            return existingManager;

        if (!createIfMissing || !Application.isPlaying)
            return null;

        GameObject managerObject = new GameObject("MonetizationManager");
        return managerObject.AddComponent<MonetizationManager>();
    }

    void EnsureEditorTestBalance()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying || !seedEditorCoinsIfEmpty || seededEditorCoinsThisSession)
            return;

        int savedCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        seededEditorCoinsThisSession = true;

        if (savedCoins > 0)
            return;

        PlayerPrefs.SetInt("TotalCoins", editorSeedCoinsAmount);
        PlayerPrefs.Save();
        totalCoins = editorSeedCoinsAmount;

        if (feedbackText != null)
            feedbackText.text = "Editor coins: " + editorSeedCoinsAmount;
#endif
    }

    void HideLegacyShopButtons()
    {
        string[] legacyButtonNames =
        {
            "SpeedUpgradeButton",
            "ShieldUpgradeButton",
            "CoinUpgradeButton"
        };

        for (int i = 0; i < legacyButtonNames.Length; i++)
        {
            GameObject legacyObject = GameObject.Find(legacyButtonNames[i]);

            if (legacyObject != null)
                legacyObject.SetActive(false);
        }
    }

    void HideLegacyShopLabels()
    {
        TextMeshProUGUI[] labels = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include);

        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] != null && labels[i].text == "New Text")
                labels[i].gameObject.SetActive(false);
        }
    }

    void EnsureFeedbackText()
    {
        if (uiRootRect == null)
            return;

        GameObject existingObject = GameObject.Find(FeedbackObjectName);

        if (existingObject != null)
        {
            feedbackText = existingObject.GetComponent<TextMeshProUGUI>();
            return;
        }

        GameObject labelObject = new GameObject(FeedbackObjectName, typeof(RectTransform));
        labelObject.transform.SetParent(uiRootRect, false);

        feedbackText = labelObject.AddComponent<TextMeshProUGUI>();
        feedbackText.alignment = TextAlignmentOptions.Center;
        feedbackText.enableAutoSizing = true;
        feedbackText.fontSizeMin = 16;
        feedbackText.fontSizeMax = 22;
        feedbackText.color = new Color(0.16f, 0.18f, 0.24f, 1f);

        if (runtimeFont != null)
            feedbackText.font = runtimeFont;
    }

    void EnsureScrollArea()
    {
        if (uiRootRect == null)
            return;

        Transform existingViewport = uiRootRect.Find(ViewportObjectName);

        if (existingViewport != null)
        {
            viewportRect = existingViewport as RectTransform;
        }
        else
        {
            GameObject viewportObject = new GameObject(
                ViewportObjectName,
                typeof(RectTransform),
                typeof(Image),
                typeof(RectMask2D),
                typeof(ScrollRect));

            viewportObject.transform.SetParent(uiRootRect, false);
            viewportRect = viewportObject.GetComponent<RectTransform>();

            Image viewportImage = viewportObject.GetComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.02f);
        }

        if (viewportRect.GetComponent<RectMask2D>() == null)
            viewportRect.gameObject.AddComponent<RectMask2D>();

        Transform existingContent = viewportRect.Find(ContentObjectName);

        if (existingContent != null)
        {
            contentRect = existingContent as RectTransform;
        }
        else
        {
            GameObject contentObject = new GameObject(ContentObjectName, typeof(RectTransform));
            contentObject.transform.SetParent(viewportRect, false);
            contentRect = contentObject.GetComponent<RectTransform>();
        }

        scrollRect = viewportRect.GetComponent<ScrollRect>();

        if (scrollRect == null)
            scrollRect = viewportRect.gameObject.AddComponent<ScrollRect>();

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 32f;
    }

    void EnsureSectionHeaders()
    {
        if (contentRect == null)
            return;

        monetizationHeaderText = FindOrCreateSectionHeader(
            MonetizationHeaderObjectName,
            "Coin Packs & Offers");
        consumablesHeaderText = FindOrCreateSectionHeader(
            ConsumablesHeaderObjectName,
            "Consumables");
    }

    TextMeshProUGUI FindOrCreateSectionHeader(string objectName, string textValue)
    {
        Transform existing = contentRect.Find(objectName);
        GameObject textObject;

        if (existing != null)
        {
            textObject = existing.gameObject;
        }
        else
        {
            textObject = new GameObject(objectName, typeof(RectTransform));
            textObject.transform.SetParent(contentRect, false);
        }

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();

        if (text == null)
            text = textObject.AddComponent<TextMeshProUGUI>();

        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.sizeDelta = new Vector2(0f, sectionHeaderHeight);

        text.text = textValue;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.enableAutoSizing = true;
        text.fontSizeMin = 18;
        text.fontSizeMax = 28;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color(0.11f, 0.17f, 0.27f, 0.92f);

        if (runtimeFont != null && text.font == null)
            text.font = runtimeFont;

        return text;
    }

    void LayoutStaticElements()
    {
        if (uiRootRect == null)
            return;

        Canvas.ForceUpdateCanvases();
        float rootWidth = uiRootRect.rect.width;

        if (shopTitleRect != null)
        {
            shopTitleRect.anchorMin = new Vector2(0.5f, 1f);
            shopTitleRect.anchorMax = new Vector2(0.5f, 1f);
            shopTitleRect.pivot = new Vector2(0.5f, 1f);
            shopTitleRect.sizeDelta = new Vector2(rootWidth - (sidePadding * 2f), 52f);
            shopTitleRect.anchoredPosition = new Vector2(0f, -18f);
        }

        if (shopTitleText != null)
        {
            shopTitleText.text = "Shop";
            shopTitleText.alignment = TextAlignmentOptions.Center;
            shopTitleText.enableAutoSizing = true;
            shopTitleText.fontSizeMin = 20;
            shopTitleText.fontSizeMax = 30;
        }

        if (totalCoinsText != null)
        {
            RectTransform coinsRect = totalCoinsText.rectTransform;
            coinsRect.anchorMin = new Vector2(0.5f, 1f);
            coinsRect.anchorMax = new Vector2(0.5f, 1f);
            coinsRect.pivot = new Vector2(0.5f, 1f);
            coinsRect.sizeDelta = new Vector2(rootWidth - (sidePadding * 2f), 32f);
            coinsRect.anchoredPosition = new Vector2(0f, -60f);
            totalCoinsText.alignment = TextAlignmentOptions.Center;
            totalCoinsText.enableAutoSizing = true;
            totalCoinsText.fontSizeMin = 16;
            totalCoinsText.fontSizeMax = 24;
        }

        if (feedbackText != null)
        {
            RectTransform feedbackRect = feedbackText.rectTransform;
            feedbackRect.anchorMin = new Vector2(0.5f, 1f);
            feedbackRect.anchorMax = new Vector2(0.5f, 1f);
            feedbackRect.pivot = new Vector2(0.5f, 1f);
            feedbackRect.sizeDelta = new Vector2(rootWidth - (sidePadding * 2f), 28f);
            feedbackRect.anchoredPosition = new Vector2(0f, -92f);
            feedbackText.fontSizeMin = 16;
            feedbackText.fontSizeMax = 22;
        }

        if (backButtonRect != null)
        {
            backButtonRect.anchorMin = new Vector2(0.5f, 0f);
            backButtonRect.anchorMax = new Vector2(0.5f, 0f);
            backButtonRect.pivot = new Vector2(0.5f, 0f);
            backButtonRect.sizeDelta = new Vector2(220f, 56f);
            backButtonRect.anchoredPosition = new Vector2(0f, 16f);
        }

        if (backButtonText != null)
        {
            backButtonText.enableAutoSizing = true;
            backButtonText.fontSizeMin = 18;
            backButtonText.fontSizeMax = 28;
        }

        if (viewportRect != null)
        {
            viewportRect.anchorMin = new Vector2(0f, 0f);
            viewportRect.anchorMax = new Vector2(1f, 1f);
            viewportRect.pivot = new Vector2(0.5f, 0.5f);
            viewportRect.offsetMin = new Vector2(sidePadding, bottomPadding);
            viewportRect.offsetMax = new Vector2(-sidePadding, -topPadding);
        }
    }

    void BuildShopButtons()
    {
        if (contentRect == null)
            return;

        EnsureSectionHeaders();
        BindSceneShopButtons();

        for (int i = 0; i < shopOrder.Length; i++)
        {
            UpgradeType type = shopOrder[i];
            if (runtimeButtons.ContainsKey(type))
                continue;

            ShopUpgradeButtonUI buttonUI = CreateShopButton(type);

            if (buttonUI != null)
                runtimeButtons[type] = buttonUI;
        }

        SyncOfferCards();
        LayoutShopList();
    }

    void BindSceneShopButtons()
    {
        if (contentRect == null)
            return;

        for (int i = 0; i < contentRect.childCount; i++)
        {
            Transform child = contentRect.GetChild(i);
            UpgradeType type;

            if (!TryGetUpgradeTypeFromObjectName(child.name, out type))
                continue;

            Button sceneButton = child.GetComponent<Button>();
            Image sceneImage = child.GetComponent<Image>();

            if (sceneButton == null || sceneImage == null)
                continue;

            ShopUpgradeButtonUI buttonUI = child.GetComponent<ShopUpgradeButtonUI>();

            if (buttonUI == null)
                buttonUI = child.gameObject.AddComponent<ShopUpgradeButtonUI>();

            buttonUI.Initialize(this, type, sceneImage, sceneButton, runtimeFont);
            runtimeButtons[type] = buttonUI;
        }
    }

    bool TryGetUpgradeTypeFromObjectName(string objectName, out UpgradeType type)
    {
        switch (objectName)
        {
            case "ShieldShopButton":
                type = UpgradeType.Shield;
                return true;
            case "SpeedBoostShopButton":
                type = UpgradeType.SpeedBoost;
                return true;
            case "ExtraLifeShopButton":
                type = UpgradeType.ExtraLife;
                return true;
            case "CoinMagnetShopButton":
                type = UpgradeType.CoinMagnet;
                return true;
            case "DoubleCoinsShopButton":
                type = UpgradeType.DoubleCoins;
                return true;
            case "SlowTimeShopButton":
                type = UpgradeType.SlowTime;
                return true;
            case "SmallerPlayerShopButton":
                type = UpgradeType.SmallerPlayer;
                return true;
            case "ScoreBoosterShopButton":
                type = UpgradeType.ScoreBooster;
                return true;
            case "BombShopButton":
                type = UpgradeType.Bomb;
                return true;
            case "RareCoinBoostShopButton":
                type = UpgradeType.RareCoinBoost;
                return true;
            default:
                type = UpgradeType.Shield;
                return false;
        }
    }

    ShopUpgradeButtonUI CreateShopButton(UpgradeType type)
    {
        GameObject buttonObject = new GameObject(
            UpgradeInventory.GetDisplayName(type) + "ShopButton",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button),
            typeof(ShopUpgradeButtonUI));

        buttonObject.transform.SetParent(contentRect, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0f, 1f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.pivot = new Vector2(0.5f, 1f);
        buttonRect.sizeDelta = new Vector2(0f, itemHeight);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(1f, 1f, 1f, 0.94f);

        ShopUpgradeButtonUI buttonUI = buttonObject.GetComponent<ShopUpgradeButtonUI>();
        buttonUI.Initialize(this, type, buttonImage, buttonObject.GetComponent<Button>(), runtimeFont);
        return buttonUI;
    }

    void SyncOfferCards()
    {
        offerOrder.Clear();

        List<MonetizationOfferSnapshot> snapshots = GetOfferSnapshots();

        for (int i = 0; i < snapshots.Count; i++)
        {
            MonetizationOfferId offerId = snapshots[i].Definition.OfferId;
            offerOrder.Add(offerId);

            if (runtimeOfferCards.ContainsKey(offerId))
                continue;

            ShopOfferCardUI offerCard = CreateOfferCard(offerId);

            if (offerCard != null)
                runtimeOfferCards[offerId] = offerCard;
        }

        List<MonetizationOfferId> staleIds = new List<MonetizationOfferId>();

        foreach (KeyValuePair<MonetizationOfferId, ShopOfferCardUI> pair in runtimeOfferCards)
        {
            if (!offerOrder.Contains(pair.Key))
                staleIds.Add(pair.Key);
        }

        for (int i = 0; i < staleIds.Count; i++)
        {
            MonetizationOfferId offerId = staleIds[i];

            if (runtimeOfferCards.TryGetValue(offerId, out ShopOfferCardUI offerCard) && offerCard != null)
                Destroy(offerCard.gameObject);

            runtimeOfferCards.Remove(offerId);
        }

        if (monetizationHeaderText != null)
            monetizationHeaderText.gameObject.SetActive(offerOrder.Count > 0);
    }

    ShopOfferCardUI CreateOfferCard(MonetizationOfferId offerId)
    {
        GameObject offerObject = new GameObject(
            offerId + "OfferCard",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button),
            typeof(ShopOfferCardUI));

        offerObject.transform.SetParent(contentRect, false);

        RectTransform offerRect = offerObject.GetComponent<RectTransform>();
        offerRect.anchorMin = new Vector2(0f, 1f);
        offerRect.anchorMax = new Vector2(1f, 1f);
        offerRect.pivot = new Vector2(0.5f, 1f);
        offerRect.sizeDelta = new Vector2(0f, offerItemHeight);

        Image offerImage = offerObject.GetComponent<Image>();
        offerImage.color = new Color(1f, 1f, 1f, 0.96f);

        ShopOfferCardUI offerCard = offerObject.GetComponent<ShopOfferCardUI>();
        offerCard.Initialize(this, offerId, offerImage, offerObject.GetComponent<Button>(), runtimeFont);
        return offerCard;
    }

    void LayoutShopList()
    {
        if (viewportRect == null || contentRect == null)
            return;

        Canvas.ForceUpdateCanvases();
        float currentY = 0f;
        float viewportHeight = viewportRect.rect.height;

        if (offerOrder.Count > 0)
        {
            if (monetizationHeaderText != null)
            {
                LayoutSectionHeader(monetizationHeaderText.rectTransform, currentY);
                currentY += sectionHeaderHeight + sectionSpacing;
            }

            for (int i = 0; i < offerOrder.Count; i++)
            {
                if (!runtimeOfferCards.TryGetValue(offerOrder[i], out ShopOfferCardUI offerCard) || offerCard == null)
                    continue;

                RectTransform offerRect = offerCard.GetComponent<RectTransform>();

                if (offerRect == null)
                    continue;

                LayoutCard(offerRect, offerItemHeight, currentY);
                currentY += offerItemHeight + rowSpacing;
            }

            currentY -= rowSpacing;
            currentY += sectionGap;
        }

        if (consumablesHeaderText != null)
        {
            LayoutSectionHeader(consumablesHeaderText.rectTransform, currentY);
            currentY += sectionHeaderHeight + sectionSpacing;
        }

        for (int i = 0; i < shopOrder.Length; i++)
        {
            if (!runtimeButtons.TryGetValue(shopOrder[i], out ShopUpgradeButtonUI buttonUI) || buttonUI == null)
                continue;

            RectTransform buttonRect = buttonUI.GetComponent<RectTransform>();

            if (buttonRect == null)
                continue;

            LayoutCard(buttonRect, itemHeight, currentY);
            currentY += itemHeight + rowSpacing;
        }

        if (shopOrder.Length > 0)
            currentY -= rowSpacing;

        float contentHeight = currentY;

        if (contentHeight < viewportHeight)
            contentHeight = viewportHeight;

        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0f, contentHeight);
        contentRect.anchoredPosition = Vector2.zero;
    }

    void LayoutSectionHeader(RectTransform headerRect, float yOffset)
    {
        if (headerRect == null)
            return;

        headerRect.anchorMin = new Vector2(0f, 1f);
        headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.sizeDelta = new Vector2(0f, sectionHeaderHeight);
        headerRect.anchoredPosition = new Vector2(0f, -yOffset);
    }

    void LayoutCard(RectTransform cardRect, float height, float yOffset)
    {
        cardRect.anchorMin = new Vector2(0f, 1f);
        cardRect.anchorMax = new Vector2(1f, 1f);
        cardRect.pivot = new Vector2(0.5f, 1f);
        cardRect.sizeDelta = new Vector2(0f, height);
        cardRect.anchoredPosition = new Vector2(0f, -yOffset);
    }

    void RefreshUI()
    {
        FindStaticReferences();
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        BuildShopButtons();
        LayoutStaticElements();
        LayoutShopList();
        SnapScrollToTopIfNeeded();

        if (totalCoinsText != null)
            totalCoinsText.text = "Coins: " + totalCoins;

        if (feedbackText != null && string.IsNullOrEmpty(feedbackText.text))
            feedbackText.text = "Tap a card to buy coins or consumables";

        RefreshOfferCards();

        for (int i = 0; i < shopOrder.Length; i++)
        {
            UpgradeType type = shopOrder[i];

            if (!runtimeButtons.TryGetValue(type, out ShopUpgradeButtonUI buttonUI) || buttonUI == null)
                continue;

            int owned = 0;
            int cost = GetUpgradeCost(type);

            if (UpgradeInventory.Instance != null)
                owned = UpgradeInventory.Instance.GetAmount(type);

            buttonUI.RefreshView(
                UpgradeInventory.GetDisplayName(type),
                GetUpgradeDescription(type),
                owned,
                cost,
                totalCoins >= cost);
        }
    }

    void RefreshOfferCards()
    {
        if (monetizationHeaderText != null)
            monetizationHeaderText.text = "Coin Packs & Offers";

        if (consumablesHeaderText != null)
            consumablesHeaderText.text = "Consumables";

        List<MonetizationOfferSnapshot> snapshots = GetOfferSnapshots();

        for (int i = 0; i < snapshots.Count; i++)
        {
            MonetizationOfferSnapshot snapshot = snapshots[i];

            if (!runtimeOfferCards.TryGetValue(snapshot.Definition.OfferId, out ShopOfferCardUI offerCard) ||
                offerCard == null)
            {
                continue;
            }

            offerCard.RefreshView(snapshot, BuildOfferValueSummary(snapshot.Definition));
        }
    }

    List<MonetizationOfferSnapshot> GetOfferSnapshots()
    {
        MonetizationManager manager = ResolveMonetizationManager(true);

        if (manager == null)
            return new List<MonetizationOfferSnapshot>();

        return manager.GetOfferSnapshots();
    }

    string BuildOfferValueSummary(MonetizationOfferDefinition definition)
    {
        List<string> parts = new List<string>();

        if (definition.CoinsGranted > 0)
            parts.Add(definition.CoinsGranted + " coins");

        for (int i = 0; i < definition.BonusUpgrades.Length; i++)
        {
            BonusUpgradeGrant grant = definition.BonusUpgrades[i];
            parts.Add(grant.amount + " " + UpgradeInventory.GetDisplayName(grant.type));
        }

        return string.Join(" + ", parts);
    }

    bool TryBuyUpgrade(UpgradeType type)
    {
        int cost = GetUpgradeCost(type);

        if (totalCoins < cost)
        {
            SetFeedback("Not enough coins for " + UpgradeInventory.GetDisplayName(type));
            RefreshUI();
            return false;
        }

        totalCoins -= cost;
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.Save();

        if (UpgradeInventory.Instance != null)
            UpgradeInventory.Instance.AddUpgrade(type, 1);

        LaunchAnalytics.RecordSoftCurrencyPurchase(type, cost, totalCoins);
        SetFeedback(UpgradeInventory.GetDisplayName(type) + " purchased");
        RefreshUI();
        return true;
    }

    string GetUpgradeDescription(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Shield:
                return "Block one hit";
            case UpgradeType.SpeedBoost:
                return "Move faster";
            case UpgradeType.ExtraLife:
                return "Revive once";
            case UpgradeType.CoinMagnet:
                return "Pull in coins";
            case UpgradeType.DoubleCoins:
                return "Double coin value";
            case UpgradeType.SlowTime:
                return "Slow obstacles";
            case UpgradeType.SmallerPlayer:
                return "Shrink your hitbox";
            case UpgradeType.ScoreBooster:
                return "Double score gain";
            case UpgradeType.Bomb:
                return "Clear the screen";
            case UpgradeType.RareCoinBoost:
                return "More coin spawns";
            default:
                return "Consumable upgrade";
        }
    }

    public void BuyUpgradeCard(UpgradeType type)
    {
        TryBuyUpgrade(type);
    }

    public void BuyOfferCard(MonetizationOfferId offerId)
    {
        MonetizationManager manager = ResolveMonetizationManager(true);

        if (manager == null)
        {
            SetFeedback("Store manager unavailable.");
            RefreshUI();
            return;
        }

        SetFeedback("Opening purchase flow...");
        RefreshUI();

        manager.PurchaseOffer(
            offerId,
            result =>
            {
                if (this == null)
                    return;

                totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
                SetFeedback(result.Message);
                RefreshUI();
            });
    }

    void SnapScrollToTopIfNeeded()
    {
        if (!shouldSnapScrollToTop || scrollRect == null)
            return;

        Canvas.ForceUpdateCanvases();
        scrollRect.StopMovement();
        scrollRect.verticalNormalizedPosition = 1f;
        shouldSnapScrollToTop = false;
    }

    int GetUpgradeCost(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.SpeedBoost:
                return 12;
            case UpgradeType.Shield:
                return 15;
            case UpgradeType.ExtraLife:
                return 24;
            case UpgradeType.CoinMagnet:
                return 18;
            case UpgradeType.DoubleCoins:
                return 20;
            case UpgradeType.SlowTime:
                return 18;
            case UpgradeType.SmallerPlayer:
                return 16;
            case UpgradeType.ScoreBooster:
                return 14;
            case UpgradeType.Bomb:
                return 28;
            case UpgradeType.RareCoinBoost:
                return 20;
            default:
                return 15;
        }
    }

    void SetFeedback(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;
    }

    public void BuySpeedUpgrade()
    {
        TryBuyUpgrade(UpgradeType.SpeedBoost);
    }

    public void BuyShieldUpgrade()
    {
        TryBuyUpgrade(UpgradeType.Shield);
    }

    public void BuyExtraLifeUpgrade()
    {
        TryBuyUpgrade(UpgradeType.ExtraLife);
    }

    public void BuyCoinMagnetUpgrade()
    {
        TryBuyUpgrade(UpgradeType.CoinMagnet);
    }

    public void BuyDoubleCoinsUpgrade()
    {
        TryBuyUpgrade(UpgradeType.DoubleCoins);
    }

    public void BuySlowTimeUpgrade()
    {
        TryBuyUpgrade(UpgradeType.SlowTime);
    }

    public void BuySmallerPlayerUpgrade()
    {
        TryBuyUpgrade(UpgradeType.SmallerPlayer);
    }

    public void BuyScoreBoosterUpgrade()
    {
        TryBuyUpgrade(UpgradeType.ScoreBooster);
    }

    public void BuyBombUpgrade()
    {
        TryBuyUpgrade(UpgradeType.Bomb);
    }

    public void BuyRareCoinBoostUpgrade()
    {
        TryBuyUpgrade(UpgradeType.RareCoinBoost);
    }

    public void BuyCoinUpgrade()
    {
        TryBuyUpgrade(UpgradeType.DoubleCoins);
    }

    public void GoBack()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

public class ShopUpgradeButtonUI : MonoBehaviour
{
    private ShopManager shopManager;
    private UpgradeType upgradeType;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI descriptionText;
    private TextMeshProUGUI metaText;
    private Image backgroundImage;
    private Button button;

    private readonly Color affordableColor = new Color(1f, 1f, 1f, 0.96f);
    private readonly Color expensiveColor = new Color(0.87f, 0.88f, 0.92f, 0.88f);
    private readonly Color titleColor = new Color(0.13f, 0.17f, 0.26f, 1f);
    private readonly Color bodyColor = new Color(0.2f, 0.24f, 0.32f, 1f);

    public void Initialize(
        ShopManager manager,
        UpgradeType type,
        Image image,
        Button sourceButton,
        TMP_FontAsset runtimeFont)
    {
        shopManager = manager;
        upgradeType = type;
        backgroundImage = image;
        button = sourceButton;

        CreateLabels(runtimeFont);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnPressed);
        }
    }

    void CreateLabels(TMP_FontAsset runtimeFont)
    {
        titleText = FindOrCreateStretchText(
            "Title",
            new Vector2(20f, 156f),
            new Vector2(-20f, -18f),
            TextAlignmentOptions.Top,
            28f,
            40f,
            runtimeFont);

        descriptionText = FindOrCreateStretchText(
            "Description",
            new Vector2(20f, 88f),
            new Vector2(-20f, -88f),
            TextAlignmentOptions.Center,
            22f,
            30f,
            runtimeFont);

        metaText = FindOrCreateStretchText(
            "Meta",
            new Vector2(20f, 18f),
            new Vector2(-20f, -170f),
            TextAlignmentOptions.Bottom,
            20f,
            28f,
            runtimeFont);
    }

    TextMeshProUGUI FindOrCreateStretchText(
        string objectName,
        Vector2 leftBottom,
        Vector2 rightTop,
        TextAlignmentOptions alignment,
        float minSize,
        float maxSize,
        TMP_FontAsset runtimeFont)
    {
        Transform existing = transform.Find(objectName);
        GameObject textObject;

        bool created = existing == null;

        if (existing != null)
        {
            textObject = existing.gameObject;
        }
        else
        {
            textObject = new GameObject(objectName, typeof(RectTransform));
            textObject.transform.SetParent(transform, false);
        }

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(leftBottom.x, leftBottom.y);
        textRect.offsetMax = new Vector2(rightTop.x, rightTop.y);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();

        if (text == null)
            text = textObject.AddComponent<TextMeshProUGUI>();

        text.alignment = alignment;
        text.color = bodyColor;

        if (created)
        {
            text.enableAutoSizing = true;
            text.fontSizeMin = minSize;
            text.fontSizeMax = maxSize;
        }

        if (runtimeFont != null && text.font == null)
            text.font = runtimeFont;

        return text;
    }

    public void RefreshView(string displayName, string description, int ownedAmount, int cost, bool canAfford)
    {
        if (titleText != null)
        {
            titleText.text = displayName;
            titleText.color = titleColor;
            titleText.fontStyle = FontStyles.Bold;
        }

        if (descriptionText != null)
        {
            descriptionText.text = description;
            descriptionText.color = bodyColor;
        }

        if (metaText != null)
        {
            string stateText = canAfford ? "Tap" : "Locked";
            string priceColorHex = canAfford ? "E9AA33" : "8A8FA0";
            metaText.text =
                "Owned: " + ownedAmount +
                "  |  <color=#" + priceColorHex + ">" + cost + " coins</color>" +
                "  |  " + stateText;
            metaText.fontStyle = FontStyles.Bold;
        }

        if (backgroundImage != null)
        {
            if (canAfford)
                backgroundImage.color = affordableColor;
            else
                backgroundImage.color = expensiveColor;
        }
    }

    void OnPressed()
    {
        if (shopManager != null)
            shopManager.BuyUpgradeCard(upgradeType);
    }
}

public class ShopOfferCardUI : MonoBehaviour
{
    private ShopManager shopManager;
    private MonetizationOfferId offerId;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI descriptionText;
    private TextMeshProUGUI metaText;
    private Image backgroundImage;
    private Button button;

    private readonly Color starterAvailableColor = new Color(1f, 0.95f, 0.86f, 0.98f);
    private readonly Color coinPackAvailableColor = new Color(0.89f, 0.97f, 0.96f, 0.98f);
    private readonly Color ownedColor = new Color(0.88f, 0.97f, 0.9f, 0.98f);
    private readonly Color unavailableColor = new Color(0.88f, 0.9f, 0.94f, 0.88f);
    private readonly Color titleColor = new Color(0.12f, 0.16f, 0.24f, 1f);
    private readonly Color bodyColor = new Color(0.19f, 0.23f, 0.31f, 1f);

    public void Initialize(
        ShopManager manager,
        MonetizationOfferId id,
        Image image,
        Button sourceButton,
        TMP_FontAsset runtimeFont)
    {
        shopManager = manager;
        offerId = id;
        backgroundImage = image;
        button = sourceButton;

        CreateLabels(runtimeFont);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnPressed);
        }
    }

    void CreateLabels(TMP_FontAsset runtimeFont)
    {
        titleText = FindOrCreateStretchText(
            "Title",
            new Vector2(20f, 150f),
            new Vector2(-20f, -20f),
            TextAlignmentOptions.Top,
            24f,
            38f,
            runtimeFont);

        descriptionText = FindOrCreateStretchText(
            "Description",
            new Vector2(20f, 76f),
            new Vector2(-20f, -90f),
            TextAlignmentOptions.Center,
            18f,
            26f,
            runtimeFont);

        metaText = FindOrCreateStretchText(
            "Meta",
            new Vector2(20f, 18f),
            new Vector2(-20f, -170f),
            TextAlignmentOptions.Bottom,
            16f,
            24f,
            runtimeFont);
    }

    TextMeshProUGUI FindOrCreateStretchText(
        string objectName,
        Vector2 leftBottom,
        Vector2 rightTop,
        TextAlignmentOptions alignment,
        float minSize,
        float maxSize,
        TMP_FontAsset runtimeFont)
    {
        Transform existing = transform.Find(objectName);
        GameObject textObject;

        bool created = existing == null;

        if (existing != null)
        {
            textObject = existing.gameObject;
        }
        else
        {
            textObject = new GameObject(objectName, typeof(RectTransform));
            textObject.transform.SetParent(transform, false);
        }

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(leftBottom.x, leftBottom.y);
        textRect.offsetMax = new Vector2(rightTop.x, rightTop.y);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();

        if (text == null)
            text = textObject.AddComponent<TextMeshProUGUI>();

        text.alignment = alignment;
        text.color = bodyColor;

        if (created)
        {
            text.enableAutoSizing = true;
            text.fontSizeMin = minSize;
            text.fontSizeMax = maxSize;
            text.margin = new Vector4(2f, 0f, 2f, 0f);
        }

        if (runtimeFont != null && text.font == null)
            text.font = runtimeFont;

        return text;
    }

    public void RefreshView(MonetizationOfferSnapshot snapshot, string valueSummary)
    {
        if (titleText != null)
        {
            titleText.text = snapshot.Definition.DisplayName;
            titleText.color = titleColor;
            titleText.fontStyle = FontStyles.Bold;
        }

        if (descriptionText != null)
        {
            descriptionText.text =
                valueSummary +
                (string.IsNullOrEmpty(snapshot.Definition.HighlightLabel)
                    ? string.Empty
                    : "\n<color=#2F6FDB>" + snapshot.Definition.HighlightLabel + "</color>");
            descriptionText.color = bodyColor;
        }

        if (metaText != null)
        {
            string priceColorHex = snapshot.CanPurchase ? "E9AA33" : "8A8FA0";
            string statusColorHex = snapshot.IsOwned
                ? "30945D"
                : (snapshot.CanPurchase ? "2F6FDB" : "8A8FA0");

            metaText.text =
                "<color=#" + priceColorHex + ">" + snapshot.PriceLabel + "</color>" +
                "\n<color=#" + statusColorHex + ">" + snapshot.StatusLabel + "</color>";
            metaText.fontStyle = FontStyles.Bold;
        }

        if (button != null)
            button.interactable = snapshot.CanPurchase;

        if (backgroundImage != null)
        {
            if (snapshot.IsOwned)
            {
                backgroundImage.color = ownedColor;
            }
            else if (snapshot.CanPurchase)
            {
                backgroundImage.color = snapshot.Definition.IsStarterOffer
                    ? starterAvailableColor
                    : coinPackAvailableColor;
            }
            else
            {
                backgroundImage.color = unavailableColor;
            }
        }
    }

    void OnPressed()
    {
        if (shopManager != null)
            shopManager.BuyOfferCard(offerId);
    }
}
