using System;
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

    public float sidePadding = 20f;
    public float topPadding = 154f;
    public float bottomPadding = 92f;
    public float rowSpacing = 14f;
    public float offerItemHeight = 214f;
    public float itemHeight = 214f;
    public float sectionHeaderHeight = 34f;
    public float sectionSpacing = 10f;
    public float sectionGap = 18f;
    public float cardHorizontalInset = 20f;
    public float cardMaxWidth = 780f;
    public float headerHorizontalInset = 18f;

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
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        BuildShopButtons();
        shouldSnapScrollToTop = true;
        RefreshUI();
    }

    void OnEnable()
    {
        ResolveMonetizationManager(true);
        SubscribeMonetizationEvents();
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
        SafeAreaUtility.NormalizeCanvas(canvas);

        if (uiRootRect == null && totalCoinsText != null)
            uiRootRect = totalCoinsText.rectTransform.parent as RectTransform;

        SafeAreaUtility.ApplySafeArea(uiRootRect);
        MenuBackdropUtility.EnsureBackdrop(uiRootRect, CaveThemeLibrary.GetMenuTheme(), "ShopBackdrop");
        ApplySceneTheme();

        Image rootImage = uiRootRect != null ? uiRootRect.GetComponent<Image>() : null;

        if (rootImage != null)
            rootImage.color = new Color(0.02f, 0.03f, 0.05f, 0.08f);
    }

    void ApplySceneTheme()
    {
        RuntimeCaveTheme theme = CaveThemeLibrary.GetMenuTheme();
        Camera mainCamera = Camera.main;

        if (mainCamera != null)
            mainCamera.backgroundColor = Color.Lerp(theme.BackgroundBottom, theme.FogColor, 0.3f);
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
        feedbackText.color = new Color(0.89f, 0.93f, 0.96f, 1f);

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
            StudioUiTheme.ApplyPanel(viewportImage, StudioPanelStyle.Scrim, 0.42f);
        }

        if (viewportRect.GetComponent<RectMask2D>() == null)
            viewportRect.gameObject.AddComponent<RectMask2D>();

        Image existingViewportImage = viewportRect.GetComponent<Image>();

        if (existingViewportImage != null)
            StudioUiTheme.ApplyPanel(existingViewportImage, StudioPanelStyle.Scrim, 0.42f);

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
            "Treasure Caches");
        consumablesHeaderText = FindOrCreateSectionHeader(
            ConsumablesHeaderObjectName,
            "Run Supplies");
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
        text.color = new Color(0.9f, 0.95f, 0.98f, 0.96f);

        if (runtimeFont != null && text.font == null)
            text.font = runtimeFont;

        return text;
    }

    void LayoutStaticElements()
    {
        if (uiRootRect == null)
            return;

        RuntimeCaveTheme theme = CaveThemeLibrary.GetMenuTheme();
        Canvas.ForceUpdateCanvases();
        float contentWidth = GetCenteredContentWidth();

        if (shopTitleRect != null)
        {
            shopTitleRect.anchorMin = new Vector2(0.5f, 1f);
            shopTitleRect.anchorMax = new Vector2(0.5f, 1f);
            shopTitleRect.pivot = new Vector2(0.5f, 1f);
            shopTitleRect.sizeDelta = new Vector2(contentWidth, 46f);
            shopTitleRect.anchoredPosition = new Vector2(0f, -34f);
        }

        if (shopTitleText != null)
        {
            shopTitleText.text = "Supply Cache";
            shopTitleText.alignment = TextAlignmentOptions.Center;
            shopTitleText.enableAutoSizing = true;
            shopTitleText.fontSizeMin = 24;
            shopTitleText.fontSizeMax = 38;
            shopTitleText.fontStyle = FontStyles.Bold;
            shopTitleText.color = StudioUiTheme.Text;
        }

        if (totalCoinsText != null)
        {
            RectTransform coinsRect = totalCoinsText.rectTransform;
            coinsRect.anchorMin = new Vector2(0.5f, 1f);
            coinsRect.anchorMax = new Vector2(0.5f, 1f);
            coinsRect.pivot = new Vector2(0.5f, 1f);
            coinsRect.sizeDelta = new Vector2(contentWidth, 28f);
            coinsRect.anchoredPosition = new Vector2(0f, -78f);
            totalCoinsText.alignment = TextAlignmentOptions.Center;
            totalCoinsText.enableAutoSizing = true;
            totalCoinsText.fontSizeMin = 15;
            totalCoinsText.fontSizeMax = 22;
            totalCoinsText.color = StudioUiTheme.Gold;
        }

        if (feedbackText != null)
        {
            RectTransform feedbackRect = feedbackText.rectTransform;
            feedbackRect.anchorMin = new Vector2(0.5f, 1f);
            feedbackRect.anchorMax = new Vector2(0.5f, 1f);
            feedbackRect.pivot = new Vector2(0.5f, 1f);
            feedbackRect.sizeDelta = new Vector2(contentWidth, 28f);
            feedbackRect.anchoredPosition = new Vector2(0f, -108f);
            feedbackText.fontSizeMin = 13;
            feedbackText.fontSizeMax = 19;
            feedbackText.color = StudioUiTheme.MutedText;
        }

        if (backButtonRect != null)
        {
            backButtonRect.anchorMin = new Vector2(0.5f, 0f);
            backButtonRect.anchorMax = new Vector2(0.5f, 0f);
            backButtonRect.pivot = new Vector2(0.5f, 0f);
            backButtonRect.sizeDelta = new Vector2(230f, 56f);
            backButtonRect.anchoredPosition = new Vector2(0f, 16f);
        }

        if (backButtonText != null)
        {
            backButtonText.text = "Back to Camp";
            backButtonText.enableAutoSizing = true;
            backButtonText.fontSizeMin = 16;
            backButtonText.fontSizeMax = 24;
            backButtonText.color = StudioUiTheme.Text;
        }

        if (backButtonRect != null)
        {
            Image backButtonImage = backButtonRect.GetComponent<Image>();

            if (backButtonImage != null)
                StudioUiTheme.ApplyButton(backButtonRect.GetComponent<Button>(), StudioButtonStyle.Quiet, backButtonText);
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
        StudioUiTheme.ApplyPanel(buttonImage, StudioPanelStyle.Surface);

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
        StudioUiTheme.ApplyPanel(offerImage, StudioPanelStyle.Elevated);

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

        float headerWidth = GetScrollableCardWidth() - (headerHorizontalInset * 2f);

        headerRect.anchorMin = new Vector2(0.5f, 1f);
        headerRect.anchorMax = new Vector2(0.5f, 1f);
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.sizeDelta = new Vector2(headerWidth, sectionHeaderHeight);
        headerRect.anchoredPosition = new Vector2(0f, -yOffset);
    }

    void LayoutCard(RectTransform cardRect, float height, float yOffset)
    {
        float cardWidth = GetScrollableCardWidth();

        cardRect.anchorMin = new Vector2(0.5f, 1f);
        cardRect.anchorMax = new Vector2(0.5f, 1f);
        cardRect.pivot = new Vector2(0.5f, 1f);
        cardRect.sizeDelta = new Vector2(cardWidth, height);
        cardRect.anchoredPosition = new Vector2(0f, -yOffset);
    }

    float GetCenteredContentWidth()
    {
        if (uiRootRect == null)
            return cardMaxWidth;

        float availableWidth = Mathf.Max(320f, uiRootRect.rect.width - (sidePadding * 2f));
        return Mathf.Min(availableWidth, cardMaxWidth);
    }

    float GetScrollableCardWidth()
    {
        if (viewportRect == null)
            return cardMaxWidth;

        float availableWidth = Mathf.Max(320f, viewportRect.rect.width - (cardHorizontalInset * 2f));
        return Mathf.Min(availableWidth, cardMaxWidth);
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

        if (feedbackText != null &&
            (string.IsNullOrEmpty(feedbackText.text) ||
             feedbackText.text == "Tap a card to buy coins or consumables" ||
             feedbackText.text == "Tap a card to buy" ||
             feedbackText.text == PlayerProgressionSystem.GetShopRecommendation()))
        {
            feedbackText.text = "Treasure packs up top, run buffs below.";
        }

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
                totalCoins >= cost,
                totalCoins);
        }
    }

    void RefreshOfferCards()
    {
        if (monetizationHeaderText != null)
            monetizationHeaderText.text = "Treasure Caches";

        if (consumablesHeaderText != null)
            consumablesHeaderText.text = "Run Supplies";

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
                return "Absorb one collision";
            case UpgradeType.SpeedBoost:
                return "Burst through tight lanes";
            case UpgradeType.ExtraLife:
                return "Recover from one bad hit";
            case UpgradeType.CoinMagnet:
                return "Pull nearby coins inward";
            case UpgradeType.DoubleCoins:
                return "Double run coin value";
            case UpgradeType.SlowTime:
                return "Briefly calm the swarm";
            case UpgradeType.SmallerPlayer:
                return "Shrink your dodge profile";
            case UpgradeType.ScoreBooster:
                return "Multiply score gain";
            case UpgradeType.Bomb:
                return "Blast the lane clear";
            case UpgradeType.RareCoinBoost:
                return "Spawn richer coin trails";
            default:
                return "Pack a run consumable";
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
    private TextMeshProUGUI accentText;
    private Image backgroundImage;
    private Image accentBarImage;
    private Button button;

    private readonly Color affordableColor = StudioUiTheme.Surface;
    private readonly Color expensiveColor = new Color(0.085f, 0.105f, 0.095f, 0.92f);
    private readonly Color titleColor = StudioUiTheme.Text;
    private readonly Color bodyColor = StudioUiTheme.MutedText;

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
        ApplyCardChrome();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnPressed);
        }
    }

    void CreateLabels(TMP_FontAsset runtimeFont)
    {
        accentBarImage = FindOrCreateBarImage("AccentBar", runtimeFont, out accentText);

        titleText = FindOrCreateStretchText(
            "Title",
            new Vector2(22f, 122f),
            new Vector2(-22f, -24f),
            TextAlignmentOptions.TopLeft,
            26f,
            38f,
            runtimeFont);

        descriptionText = FindOrCreateStretchText(
            "Description",
            new Vector2(22f, 64f),
            new Vector2(-22f, -108f),
            TextAlignmentOptions.TopLeft,
            19f,
            28f,
            runtimeFont);

        metaText = FindOrCreateStretchText(
            "Meta",
            new Vector2(22f, 16f),
            new Vector2(-22f, -176f),
            TextAlignmentOptions.BottomLeft,
            18f,
            24f,
            runtimeFont);
    }

    Image FindOrCreateBarImage(string objectName, TMP_FontAsset runtimeFont, out TextMeshProUGUI label)
    {
        Transform existing = transform.Find(objectName);
        GameObject barObject;

        if (existing != null)
        {
            barObject = existing.gameObject;
        }
        else
        {
            barObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            barObject.transform.SetParent(transform, false);
        }

        RectTransform barRect = barObject.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0f, 1f);
        barRect.anchorMax = new Vector2(1f, 1f);
        barRect.pivot = new Vector2(0.5f, 1f);
        barRect.sizeDelta = new Vector2(0f, 34f);
        barRect.anchoredPosition = Vector2.zero;

        Image barImage = barObject.GetComponent<Image>();
        StudioUiTheme.ApplyPanel(barImage, StudioPanelStyle.Muted, 0.92f);

        Transform labelTransform = barObject.transform.Find("Label");
        GameObject labelObject;

        if (labelTransform != null)
        {
            labelObject = labelTransform.gameObject;
        }
        else
        {
            labelObject = new GameObject("Label", typeof(RectTransform));
            labelObject.transform.SetParent(barObject.transform, false);
        }

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(16f, 0f);
        labelRect.offsetMax = new Vector2(-16f, 0f);

        label = labelObject.GetComponent<TextMeshProUGUI>();

        if (label == null)
            label = labelObject.AddComponent<TextMeshProUGUI>();

        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.enableAutoSizing = true;
        label.fontSizeMin = 16f;
        label.fontSizeMax = 22f;
        label.color = new Color(0.075f, 0.085f, 0.075f, 1f);
        label.fontStyle = FontStyles.Bold;

        if (runtimeFont != null && label.font == null)
            label.font = runtimeFont;

        return barImage;
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
        text.margin = new Vector4(2f, 0f, 2f, 0f);

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

    void ApplyCardChrome()
    {
        if (backgroundImage == null)
            return;

        StudioUiTheme.ApplyPanel(backgroundImage, StudioPanelStyle.Surface);
    }

    Color GetAccentColor()
    {
        switch (upgradeType)
        {
            case UpgradeType.Shield:
                return new Color(0.92f, 0.71f, 0.32f, 1f);
            case UpgradeType.SpeedBoost:
                return new Color(0.51f, 0.84f, 0.94f, 1f);
            case UpgradeType.ExtraLife:
                return new Color(0.45f, 0.88f, 0.65f, 1f);
            case UpgradeType.CoinMagnet:
                return new Color(0.99f, 0.85f, 0.41f, 1f);
            case UpgradeType.DoubleCoins:
                return new Color(1f, 0.76f, 0.3f, 1f);
            case UpgradeType.SlowTime:
                return new Color(0.55f, 0.72f, 1f, 1f);
            case UpgradeType.SmallerPlayer:
                return new Color(0.56f, 0.92f, 0.85f, 1f);
            case UpgradeType.ScoreBooster:
                return new Color(1f, 0.58f, 0.42f, 1f);
            case UpgradeType.Bomb:
                return new Color(1f, 0.47f, 0.38f, 1f);
            case UpgradeType.RareCoinBoost:
                return new Color(0.76f, 0.66f, 0.98f, 1f);
            default:
                return new Color(0.78f, 0.84f, 0.92f, 1f);
        }
    }

    public void RefreshView(string displayName, string description, int ownedAmount, int cost, bool canAfford, int currentTotalCoins)
    {
        Color accentColor = GetAccentColor();

        if (accentBarImage != null)
            accentBarImage.color = accentColor;

        if (accentText != null)
            accentText.text = canAfford ? "RUN SUPPLY READY" : "NEEDS " + Mathf.Max(0, cost - currentTotalCoins) + " MORE COINS";

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
            descriptionText.lineSpacing = 6f;
        }

        if (metaText != null)
        {
            string stateText = canAfford ? "Tap to stash" : "More coins needed";
            string priceColorHex = canAfford ? "FFD47A" : "7E8B98";
            metaText.text =
                "Owned " + ownedAmount +
                "\n<color=#" + priceColorHex + ">" + cost + " coins</color>   |   " + stateText;
            metaText.fontStyle = FontStyles.Bold;
            metaText.color = new Color(0.9f, 0.95f, 0.99f, 1f);
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = canAfford
                ? Color.Lerp(affordableColor, accentColor, 0.14f)
                : Color.Lerp(expensiveColor, accentColor, 0.08f);
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
    private TextMeshProUGUI accentText;
    private Image backgroundImage;
    private Image accentBarImage;
    private Button button;

    private readonly Color starterAvailableColor = new Color(0.28f, 0.22f, 0.13f, 0.98f);
    private readonly Color coinPackAvailableColor = StudioUiTheme.Surface;
    private readonly Color ownedColor = new Color(0.13f, 0.23f, 0.16f, 0.98f);
    private readonly Color unavailableColor = new Color(0.085f, 0.105f, 0.095f, 0.92f);
    private readonly Color titleColor = StudioUiTheme.Text;
    private readonly Color bodyColor = StudioUiTheme.MutedText;

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
        ApplyCardChrome();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnPressed);
        }
    }

    void CreateLabels(TMP_FontAsset runtimeFont)
    {
        accentBarImage = FindOrCreateBarImage("AccentBar", runtimeFont, out accentText);

        titleText = FindOrCreateStretchText(
            "Title",
            new Vector2(22f, 118f),
            new Vector2(-22f, -26f),
            TextAlignmentOptions.TopLeft,
            22f,
            36f,
            runtimeFont);

        descriptionText = FindOrCreateStretchText(
            "Description",
            new Vector2(22f, 62f),
            new Vector2(-22f, -100f),
            TextAlignmentOptions.TopLeft,
            18f,
            26f,
            runtimeFont);

        metaText = FindOrCreateStretchText(
            "Meta",
            new Vector2(22f, 16f),
            new Vector2(-22f, -176f),
            TextAlignmentOptions.BottomLeft,
            16f,
            24f,
            runtimeFont);
    }

    Image FindOrCreateBarImage(string objectName, TMP_FontAsset runtimeFont, out TextMeshProUGUI label)
    {
        Transform existing = transform.Find(objectName);
        GameObject barObject;

        if (existing != null)
        {
            barObject = existing.gameObject;
        }
        else
        {
            barObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            barObject.transform.SetParent(transform, false);
        }

        RectTransform barRect = barObject.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0f, 1f);
        barRect.anchorMax = new Vector2(1f, 1f);
        barRect.pivot = new Vector2(0.5f, 1f);
        barRect.sizeDelta = new Vector2(0f, 34f);
        barRect.anchoredPosition = Vector2.zero;

        Image barImage = barObject.GetComponent<Image>();
        StudioUiTheme.ApplyPanel(barImage, StudioPanelStyle.Muted, 0.92f);

        Transform labelTransform = barObject.transform.Find("Label");
        GameObject labelObject;

        if (labelTransform != null)
        {
            labelObject = labelTransform.gameObject;
        }
        else
        {
            labelObject = new GameObject("Label", typeof(RectTransform));
            labelObject.transform.SetParent(barObject.transform, false);
        }

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(16f, 0f);
        labelRect.offsetMax = new Vector2(-16f, 0f);

        label = labelObject.GetComponent<TextMeshProUGUI>();

        if (label == null)
            label = labelObject.AddComponent<TextMeshProUGUI>();

        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.enableAutoSizing = true;
        label.fontSizeMin = 15f;
        label.fontSizeMax = 22f;
        label.color = new Color(0.075f, 0.085f, 0.075f, 1f);
        label.fontStyle = FontStyles.Bold;

        if (runtimeFont != null && label.font == null)
            label.font = runtimeFont;

        return barImage;
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
        text.margin = new Vector4(2f, 0f, 2f, 0f);

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

    void ApplyCardChrome()
    {
        if (backgroundImage == null)
            return;

        StudioUiTheme.ApplyPanel(backgroundImage, StudioPanelStyle.Elevated);
    }

    Color GetAccentColor(MonetizationOfferSnapshot snapshot)
    {
        if (snapshot.IsOwned)
            return new Color(0.48f, 0.86f, 0.63f, 1f);

        if (snapshot.Definition.IsStarterOffer)
            return new Color(0.98f, 0.76f, 0.33f, 1f);

        return new Color(0.46f, 0.87f, 0.92f, 1f);
    }

    public void RefreshView(MonetizationOfferSnapshot snapshot, string valueSummary)
    {
        Color accentColor = GetAccentColor(snapshot);

        if (accentBarImage != null)
            accentBarImage.color = accentColor;

        if (accentText != null)
        {
            accentText.text = snapshot.IsOwned
                ? "CACHE CLAIMED"
                : snapshot.Definition.IsStarterOffer
                    ? "STARTER CACHE"
                    : "CRYSTAL PACK";
        }

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
                    : "\n<color=#9CE9F7>" + snapshot.Definition.HighlightLabel + "</color>");
            descriptionText.color = bodyColor;
            descriptionText.lineSpacing = 6f;
        }

        if (metaText != null)
        {
            string priceColorHex = snapshot.CanPurchase ? "FFD47A" : "8A8FA0";
            string statusLabel = snapshot.StatusLabel;

            if (string.Equals(statusLabel, "Loading offers", StringComparison.OrdinalIgnoreCase))
                statusLabel = snapshot.CanPurchase ? "Tap to claim" : "Offer syncing";

            string statusColorHex = snapshot.IsOwned
                ? "7FF0A6"
                : (snapshot.CanPurchase ? "9CE9F7" : "8A8FA0");

            metaText.text =
                "<color=#" + priceColorHex + ">" + snapshot.PriceLabel + "</color>" +
                "\n<color=#" + statusColorHex + ">" + statusLabel + "</color>";
            metaText.fontStyle = FontStyles.Bold;
            metaText.color = new Color(0.92f, 0.96f, 0.99f, 1f);
        }

        if (button != null)
            button.interactable = snapshot.CanPurchase;

        if (backgroundImage != null)
        {
            if (snapshot.IsOwned)
            {
                backgroundImage.color = Color.Lerp(ownedColor, accentColor, 0.12f);
            }
            else if (snapshot.CanPurchase)
            {
                Color baseColor = snapshot.Definition.IsStarterOffer
                    ? starterAvailableColor
                    : coinPackAvailableColor;
                backgroundImage.color = Color.Lerp(baseColor, accentColor, 0.12f);
            }
            else
            {
                backgroundImage.color = Color.Lerp(unavailableColor, accentColor, 0.06f);
            }
        }
    }

    void OnPressed()
    {
        if (shopManager != null)
            shopManager.BuyOfferCard(offerId);
    }
}
