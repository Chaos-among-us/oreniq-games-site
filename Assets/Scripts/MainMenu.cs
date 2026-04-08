using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private const string ExpectedSceneName = "MainMenu";
    public string gameSceneName = "Game";
    public string shopSceneName = "Shop";
    public string inventorySceneName = "Inventory";
    public bool createInventoryButtonAtRuntime = true;
    public bool createDailyRewardPanelAtRuntime = true;
    public string playButtonObjectName = "PlayButton";
    public string shopButtonObjectName = "ShopButton";
    public string exitButtonObjectName = "ExitButton";
    public string inventoryButtonObjectName = "InventoryButton";
    public string titleObjectName = "TitleText";
    public float menuButtonSpacing = 150f;

    private TMP_FontAsset runtimeFont;
    private RectTransform menuRootRect;
    private TextMeshProUGUI profileStatsText;

    private GameObject dailyRewardPanel;
    private TextMeshProUGUI dailyRewardTitleText;
    private TextMeshProUGUI dailyRewardBodyText;
    private TextMeshProUGUI dailyRewardStatusText;
    private Button claimRewardButton;
    private TextMeshProUGUI claimRewardButtonText;

    private GameObject missionSummaryPanel;
    private TextMeshProUGUI missionSummaryTitleText;
    private TextMeshProUGUI missionSummaryStatusText;
    private Button missionSummaryOpenButton;
    private TextMeshProUGUI missionSummaryOpenButtonText;

    private GameObject missionOverlayRoot;
    private GameObject missionOverlayPanel;
    private TextMeshProUGUI missionOverlayTitleText;
    private readonly Image[] missionCardImages = new Image[3];
    private readonly TextMeshProUGUI[] missionCardTitleTexts = new TextMeshProUGUI[3];
    private readonly TextMeshProUGUI[] missionCardProgressTexts = new TextMeshProUGUI[3];
    private readonly TextMeshProUGUI[] missionCardRewardTexts = new TextMeshProUGUI[3];
    private TextMeshProUGUI missionOverlayStatusText;
    private Button claimMissionButton;
    private TextMeshProUGUI claimMissionButtonText;
    private Button closeMissionButton;
    private TextMeshProUGUI closeMissionButtonText;
    private string missionClaimFeedback = string.Empty;

    private const string ProfileStatsObjectName = "ProfileStatsText";
    private const string DailyRewardPanelObjectName = "DailyRewardPanel";
    private const string MissionSummaryPanelObjectName = "DailyMissionSummaryPanel";
    private const string MissionOverlayRootObjectName = "DailyMissionOverlayRoot";

    void Start()
    {
        if (SceneManager.GetActiveScene().name != ExpectedSceneName)
        {
            enabled = false;
            return;
        }

        runtimeFont = ResolveRuntimeFont();
        menuRootRect = GetMenuRoot();

        if (createInventoryButtonAtRuntime)
            EnsureInventoryButtonExists();

        EnsureProfileStatsLabel();

        if (createDailyRewardPanelAtRuntime)
            EnsureDailyRewardPanel();

        EnsureMissionSummaryPanel();
        EnsureMissionOverlayPanel();
        NormalizeMenuLayout();
        RefreshMenuHud();
        SetMissionOverlayVisible(false);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenShop()
    {
        SceneManager.LoadScene(shopSceneName);
    }

    public void OpenInventory()
    {
        SceneManager.LoadScene(inventorySceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    public void ClaimDailyReward()
    {
        if (DailyRewardSystem.TryClaimReward(out DailyRewardPackage rewardPackage))
        {
            GameSettings.TriggerHaptic();
            RefreshMenuHud();

            if (dailyRewardStatusText != null)
                dailyRewardStatusText.text = "Claimed today";
        }
        else
        {
            RefreshDailyRewardPanel();
        }
    }

    public void OpenMissionOverlay()
    {
        RefreshDailyMissionPanel();
        SetMissionOverlayVisible(true);
    }

    public void CloseMissionOverlay()
    {
        SetMissionOverlayVisible(false);
    }

    public void ClaimMissionRewards()
    {
        if (DailyMissionSystem.ClaimCompletedRewards(out int coinsGranted, out List<string> upgradesGranted))
        {
            GameSettings.TriggerHaptic();
            missionClaimFeedback = "Claimed " + coinsGranted + " coins";

            if (upgradesGranted.Count > 0)
                missionClaimFeedback += " + " + upgradesGranted.Count + " buff" + (upgradesGranted.Count == 1 ? "" : "s");

            RefreshMenuHud();
        }
        else
        {
            RefreshDailyMissionPanel();
        }
    }

    void EnsureInventoryButtonExists()
    {
        if (GameObject.Find(inventoryButtonObjectName) != null)
            return;

        Button shopButton = FindButton(shopButtonObjectName);

        if (shopButton == null)
            return;

        Button inventoryButton = Instantiate(shopButton, shopButton.transform.parent);
        inventoryButton.gameObject.name = inventoryButtonObjectName;

        RectTransform inventoryRect = inventoryButton.GetComponent<RectTransform>();
        RectTransform shopRect = shopButton.GetComponent<RectTransform>();

        if (inventoryRect != null && shopRect != null)
        {
            inventoryRect.anchoredPosition = new Vector2(
                shopRect.anchoredPosition.x,
                shopRect.anchoredPosition.y - menuButtonSpacing);
        }

        Button exitButton = FindButton(exitButtonObjectName);

        if (exitButton != null)
        {
            RectTransform exitRect = exitButton.GetComponent<RectTransform>();

            if (exitRect != null && inventoryRect != null)
            {
                exitRect.anchoredPosition = new Vector2(
                    exitRect.anchoredPosition.x,
                    inventoryRect.anchoredPosition.y - menuButtonSpacing);
            }
        }

        TMP_Text label = inventoryButton.GetComponentInChildren<TMP_Text>(true);

        if (label != null)
            label.text = "Inventory";

        inventoryButton.onClick.RemoveAllListeners();
        inventoryButton.onClick.AddListener(OpenInventory);
    }

    void EnsureProfileStatsLabel()
    {
        if (menuRootRect == null)
            return;

        Transform existing = menuRootRect.Find(ProfileStatsObjectName);

        if (existing != null)
        {
            profileStatsText = existing.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            GameObject statsObject = new GameObject(ProfileStatsObjectName, typeof(RectTransform));
            statsObject.transform.SetParent(menuRootRect, false);
            profileStatsText = statsObject.AddComponent<TextMeshProUGUI>();
        }

        RectTransform statsRect = profileStatsText.rectTransform;
        statsRect.anchorMin = new Vector2(0.5f, 1f);
        statsRect.anchorMax = new Vector2(0.5f, 1f);
        statsRect.pivot = new Vector2(0.5f, 1f);
        statsRect.sizeDelta = new Vector2(760f, 60f);
        statsRect.anchoredPosition = new Vector2(0f, -340f);

        profileStatsText.alignment = TextAlignmentOptions.Center;
        profileStatsText.enableAutoSizing = true;
        profileStatsText.fontSizeMin = 22;
        profileStatsText.fontSizeMax = 32;
        profileStatsText.color = new Color(0.92f, 0.96f, 1f, 1f);

        if (runtimeFont != null)
            profileStatsText.font = runtimeFont;
    }

    void EnsureDailyRewardPanel()
    {
        if (menuRootRect == null)
            return;

        Transform existing = menuRootRect.Find(DailyRewardPanelObjectName);

        if (existing != null)
        {
            dailyRewardPanel = existing.gameObject;
            CacheDailyRewardPanelReferences();
            return;
        }

        dailyRewardPanel = new GameObject(DailyRewardPanelObjectName, typeof(RectTransform), typeof(Image));
        dailyRewardPanel.transform.SetParent(menuRootRect, false);

        RectTransform panelRect = dailyRewardPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.sizeDelta = new Vector2(760f, 300f);
        panelRect.anchoredPosition = new Vector2(0f, -440f);

        Image panelImage = dailyRewardPanel.GetComponent<Image>();
        panelImage.color = new Color(0.1f, 0.18f, 0.32f, 0.96f);

        dailyRewardTitleText = CreatePanelText(
            dailyRewardPanel.transform,
            "DailyRewardTitle",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(680f, 44f),
            new Vector2(0f, -22f),
            TextAlignmentOptions.Center,
            28f,
            38f,
            new Color(1f, 0.94f, 0.72f, 1f));

        dailyRewardBodyText = CreatePanelText(
            dailyRewardPanel.transform,
            "DailyRewardBody",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(680f, 68f),
            new Vector2(0f, -102f),
            TextAlignmentOptions.Center,
            20f,
            28f,
            new Color(0.94f, 0.97f, 1f, 1f));

        dailyRewardStatusText = CreatePanelText(
            dailyRewardPanel.transform,
            "DailyRewardStatus",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(680f, 28f),
            new Vector2(0f, 102f),
            TextAlignmentOptions.Center,
            17f,
            24f,
            new Color(0.79f, 0.88f, 1f, 1f));

        claimRewardButton = CreatePanelButton(
            dailyRewardPanel.transform,
            "ClaimRewardButton",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(340f, 66f),
            new Vector2(0f, 18f),
            "Claim Reward",
            out claimRewardButtonText);

        if (claimRewardButton != null)
        {
            claimRewardButton.onClick.RemoveAllListeners();
            claimRewardButton.onClick.AddListener(ClaimDailyReward);
        }
    }

    void EnsureMissionSummaryPanel()
    {
        if (menuRootRect == null)
            return;

        Transform existing = menuRootRect.Find(MissionSummaryPanelObjectName);

        if (existing != null)
        {
            missionSummaryPanel = existing.gameObject;
            CacheMissionSummaryReferences();
            return;
        }

        missionSummaryPanel = new GameObject(MissionSummaryPanelObjectName, typeof(RectTransform), typeof(Image));
        missionSummaryPanel.transform.SetParent(menuRootRect, false);

        RectTransform panelRect = missionSummaryPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(760f, 200f);
        panelRect.anchoredPosition = new Vector2(0f, 18f);

        Image panelImage = missionSummaryPanel.GetComponent<Image>();
        panelImage.color = new Color(0.09f, 0.16f, 0.28f, 0.96f);

        missionSummaryTitleText = CreatePanelText(
            missionSummaryPanel.transform,
            "MissionSummaryTitle",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(680f, 40f),
            new Vector2(0f, -20f),
            TextAlignmentOptions.Center,
            24f,
            34f,
            new Color(1f, 0.94f, 0.72f, 1f));

        missionSummaryStatusText = CreatePanelText(
            missionSummaryPanel.transform,
            "MissionSummaryStatus",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(680f, 54f),
            new Vector2(0f, -80f),
            TextAlignmentOptions.Center,
            20f,
            28f,
            new Color(0.93f, 0.97f, 1f, 1f));

        missionSummaryOpenButton = CreatePanelButton(
            missionSummaryPanel.transform,
            "MissionSummaryOpenButton",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(360f, 60f),
            new Vector2(0f, 20f),
            "View Missions",
            out missionSummaryOpenButtonText);

        if (missionSummaryOpenButton != null)
        {
            missionSummaryOpenButton.onClick.RemoveAllListeners();
            missionSummaryOpenButton.onClick.AddListener(OpenMissionOverlay);
        }
    }

    void EnsureMissionOverlayPanel()
    {
        if (menuRootRect == null)
            return;

        Transform existing = menuRootRect.Find(MissionOverlayRootObjectName);

        if (existing != null)
        {
            missionOverlayRoot = existing.gameObject;
            CacheMissionOverlayReferences();
            return;
        }

        missionOverlayRoot = new GameObject(MissionOverlayRootObjectName, typeof(RectTransform), typeof(Image));
        missionOverlayRoot.transform.SetParent(menuRootRect, false);

        RectTransform rootRect = missionOverlayRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image rootImage = missionOverlayRoot.GetComponent<Image>();
        rootImage.color = new Color(0.02f, 0.04f, 0.08f, 0.86f);

        missionOverlayPanel = new GameObject("MissionOverlayPanel", typeof(RectTransform), typeof(Image));
        missionOverlayPanel.transform.SetParent(missionOverlayRoot.transform, false);

        RectTransform panelRect = missionOverlayPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(860f, 980f);
        panelRect.anchoredPosition = new Vector2(0f, -20f);

        Image panelImage = missionOverlayPanel.GetComponent<Image>();
        panelImage.color = new Color(0.11f, 0.18f, 0.31f, 1f);

        missionOverlayTitleText = CreatePanelText(
            missionOverlayPanel.transform,
            "MissionOverlayTitle",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(720f, 54f),
            new Vector2(0f, -28f),
            TextAlignmentOptions.Center,
            34f,
            44f,
            Color.white);

        for (int i = 0; i < 3; i++)
            CreateMissionCard(i);

        missionOverlayStatusText = CreatePanelText(
            missionOverlayPanel.transform,
            "MissionOverlayStatus",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(720f, 40f),
            new Vector2(0f, 132f),
            TextAlignmentOptions.Center,
            22f,
            30f,
            new Color(0.84f, 0.92f, 1f, 1f));

        claimMissionButton = CreatePanelButton(
            missionOverlayPanel.transform,
            "ClaimMissionButton",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(1f, 0f),
            new Vector2(320f, 72f),
            new Vector2(-20f, 34f),
            "Claim Rewards",
            out claimMissionButtonText);

        if (claimMissionButton != null)
        {
            claimMissionButton.onClick.RemoveAllListeners();
            claimMissionButton.onClick.AddListener(ClaimMissionRewards);
        }

        closeMissionButton = CreatePanelButton(
            missionOverlayPanel.transform,
            "CloseMissionButton",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 0f),
            new Vector2(220f, 72f),
            new Vector2(20f, 34f),
            "Close",
            out closeMissionButtonText);

        if (closeMissionButton != null)
        {
            closeMissionButton.onClick.RemoveAllListeners();
            closeMissionButton.onClick.AddListener(CloseMissionOverlay);

            Image closeImage = closeMissionButton.GetComponent<Image>();

            if (closeImage != null)
                closeImage.color = new Color(0.32f, 0.42f, 0.56f, 1f);
        }
    }

    void CreateMissionCard(int index)
    {
        GameObject cardObject = new GameObject("MissionCard" + index, typeof(RectTransform), typeof(Image));
        cardObject.transform.SetParent(missionOverlayPanel.transform, false);

        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 1f);
        cardRect.anchorMax = new Vector2(0.5f, 1f);
        cardRect.pivot = new Vector2(0.5f, 1f);
        cardRect.sizeDelta = new Vector2(760f, 170f);
        cardRect.anchoredPosition = new Vector2(0f, -116f - (index * 186f));

        Image cardImage = cardObject.GetComponent<Image>();
        cardImage.color = new Color(0.18f, 0.27f, 0.43f, 1f);
        missionCardImages[index] = cardImage;

        missionCardTitleTexts[index] = CreatePanelText(
            cardObject.transform,
            "Title",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(680f, 48f),
            new Vector2(0f, -20f),
            TextAlignmentOptions.Center,
            26f,
            34f,
            Color.white);

        missionCardProgressTexts[index] = CreatePanelText(
            cardObject.transform,
            "Progress",
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(330f, 36f),
            new Vector2(28f, 28f),
            TextAlignmentOptions.Left,
            22f,
            28f,
            new Color(0.9f, 0.96f, 1f, 1f));

        missionCardRewardTexts[index] = CreatePanelText(
            cardObject.transform,
            "Reward",
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(330f, 36f),
            new Vector2(-28f, 28f),
            TextAlignmentOptions.Right,
            22f,
            28f,
            new Color(1f, 0.9f, 0.7f, 1f));
    }

    void CacheDailyRewardPanelReferences()
    {
        if (dailyRewardPanel == null)
            return;

        dailyRewardTitleText = FindTextInParent(dailyRewardPanel.transform, "DailyRewardTitle");
        dailyRewardBodyText = FindTextInParent(dailyRewardPanel.transform, "DailyRewardBody");
        dailyRewardStatusText = FindTextInParent(dailyRewardPanel.transform, "DailyRewardStatus");

        Transform buttonTransform = dailyRewardPanel.transform.Find("ClaimRewardButton");

        if (buttonTransform != null)
        {
            claimRewardButton = buttonTransform.GetComponent<Button>();
            claimRewardButtonText = buttonTransform.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    void CacheMissionSummaryReferences()
    {
        if (missionSummaryPanel == null)
            return;

        missionSummaryTitleText = FindTextInParent(missionSummaryPanel.transform, "MissionSummaryTitle");
        missionSummaryStatusText = FindTextInParent(missionSummaryPanel.transform, "MissionSummaryStatus");

        Transform buttonTransform = missionSummaryPanel.transform.Find("MissionSummaryOpenButton");

        if (buttonTransform != null)
        {
            missionSummaryOpenButton = buttonTransform.GetComponent<Button>();
            missionSummaryOpenButtonText = buttonTransform.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    void CacheMissionOverlayReferences()
    {
        if (missionOverlayRoot == null)
            return;

        Transform panelTransform = missionOverlayRoot.transform.Find("MissionOverlayPanel");

        if (panelTransform != null)
            missionOverlayPanel = panelTransform.gameObject;

        if (missionOverlayPanel == null)
            return;

        missionOverlayTitleText = FindTextInParent(missionOverlayPanel.transform, "MissionOverlayTitle");
        missionOverlayStatusText = FindTextInParent(missionOverlayPanel.transform, "MissionOverlayStatus");

        for (int i = 0; i < 3; i++)
        {
            Transform cardTransform = missionOverlayPanel.transform.Find("MissionCard" + i);

            if (cardTransform == null)
                continue;

            missionCardImages[i] = cardTransform.GetComponent<Image>();
            missionCardTitleTexts[i] = FindTextInParent(cardTransform, "Title");
            missionCardProgressTexts[i] = FindTextInParent(cardTransform, "Progress");
            missionCardRewardTexts[i] = FindTextInParent(cardTransform, "Reward");
        }

        Transform claimTransform = missionOverlayPanel.transform.Find("ClaimMissionButton");

        if (claimTransform != null)
        {
            claimMissionButton = claimTransform.GetComponent<Button>();
            claimMissionButtonText = claimTransform.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        Transform closeTransform = missionOverlayPanel.transform.Find("CloseMissionButton");

        if (closeTransform != null)
        {
            closeMissionButton = closeTransform.GetComponent<Button>();
            closeMissionButtonText = closeTransform.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    TextMeshProUGUI FindTextInParent(Transform parent, string objectName)
    {
        if (parent == null)
            return null;

        Transform textTransform = parent.Find(objectName);

        if (textTransform == null)
            return null;

        return textTransform.GetComponent<TextMeshProUGUI>();
    }

    TextMeshProUGUI CreatePanelText(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 size,
        Vector2 anchoredPosition,
        TextAlignmentOptions alignment,
        float minSize,
        float maxSize,
        Color color)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = anchorMin;
        textRect.anchorMax = anchorMax;
        textRect.pivot = pivot;
        textRect.sizeDelta = size;
        textRect.anchoredPosition = anchoredPosition;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.alignment = alignment;
        text.enableAutoSizing = true;
        text.fontSizeMin = minSize;
        text.fontSizeMax = maxSize;
        text.color = color;
        text.lineSpacing = 0f;

        if (runtimeFont != null)
            text.font = runtimeFont;

        return text;
    }

    Button CreatePanelButton(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 size,
        Vector2 anchoredPosition,
        string labelText,
        out TextMeshProUGUI label)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = anchorMin;
        buttonRect.anchorMax = anchorMax;
        buttonRect.pivot = pivot;
        buttonRect.sizeDelta = size;
        buttonRect.anchoredPosition = anchoredPosition;

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.93f, 0.73f, 0.24f, 1f);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = labelText;
        label.alignment = TextAlignmentOptions.Center;
        label.enableAutoSizing = true;
        label.fontSizeMin = 20;
        label.fontSizeMax = 28;
        label.color = new Color(0.15f, 0.18f, 0.22f, 1f);

        if (runtimeFont != null)
            label.font = runtimeFont;

        return buttonObject.GetComponent<Button>();
    }

    void RefreshMenuHud()
    {
        RefreshProfileStats();
        RefreshDailyRewardPanel();
        RefreshMissionSummaryPanel();
        RefreshDailyMissionPanel();
    }

    void RefreshProfileStats()
    {
        if (profileStatsText == null)
            return;

        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        int streak = DailyRewardSystem.GetCurrentStreak();

        profileStatsText.text =
            "Coins: " + totalCoins +
            "   |   Best: " + bestScore +
            "   |   Streak: " + streak;
    }

    void RefreshDailyRewardPanel()
    {
        if (dailyRewardPanel == null)
            return;

        DailyRewardPackage rewardPackage = DailyRewardSystem.GetPreviewReward();
        bool canClaimToday = DailyRewardSystem.CanClaimToday();
        int currentStreak = DailyRewardSystem.GetCurrentStreak();
        Image panelImage = dailyRewardPanel.GetComponent<Image>();
        Image rewardButtonImage = claimRewardButton != null ? claimRewardButton.GetComponent<Image>() : null;

        if (panelImage != null)
        {
            panelImage.color = canClaimToday
                ? new Color(0.1f, 0.18f, 0.32f, 0.96f)
                : new Color(0.1f, 0.18f, 0.32f, 0.74f);
        }

        if (dailyRewardTitleText != null)
        {
            dailyRewardTitleText.text = canClaimToday ? "Reward Ready" : "Reward Claimed";
            dailyRewardTitleText.color = canClaimToday
                ? new Color(1f, 0.94f, 0.72f, 1f)
                : new Color(0.86f, 0.92f, 1f, 0.9f);
        }

        if (dailyRewardBodyText != null)
        {
            dailyRewardBodyText.text = canClaimToday
                ? rewardPackage.coins + " Coins" +
                  "\n+ " + rewardPackage.bonusAmount + " " +
                  UpgradeInventory.GetDisplayName(rewardPackage.bonusUpgrade)
                : "Today's reward collected";

            dailyRewardBodyText.color = canClaimToday
                ? new Color(0.94f, 0.97f, 1f, 1f)
                : new Color(0.9f, 0.95f, 1f, 0.84f);
        }

        if (dailyRewardStatusText != null)
        {
            dailyRewardStatusText.text = canClaimToday
                ? "Day " + rewardPackage.rewardDay + "   |   Streak " + currentStreak
                : "Streak " + currentStreak + "   |   Next " + DailyRewardSystem.GetNextClaimCountdownText();

            dailyRewardStatusText.color = canClaimToday
                ? new Color(0.9f, 0.96f, 1f, 1f)
                : new Color(0.84f, 0.9f, 0.98f, 0.8f);
        }

        if (claimRewardButton != null)
            claimRewardButton.interactable = canClaimToday;

        if (claimRewardButtonText != null)
        {
            claimRewardButtonText.text = canClaimToday ? "Claim Reward" : "Come Back";
            claimRewardButtonText.color = canClaimToday
                ? new Color(0.15f, 0.18f, 0.22f, 1f)
                : new Color(0.2f, 0.24f, 0.32f, 0.76f);
        }

        if (rewardButtonImage != null)
        {
            rewardButtonImage.color = canClaimToday
                ? new Color(0.93f, 0.73f, 0.24f, 1f)
                : new Color(0.7f, 0.58f, 0.33f, 0.6f);
        }
    }

    void RefreshMissionSummaryPanel()
    {
        if (missionSummaryPanel == null)
            return;

        int completedCount = DailyMissionSystem.GetCompletedCount();
        int claimableCount = DailyMissionSystem.GetClaimableCount();

        if (missionSummaryTitleText != null)
            missionSummaryTitleText.text = "Daily Missions";

        if (missionSummaryStatusText != null)
        {
            missionSummaryStatusText.text = claimableCount > 0
                ? claimableCount + (claimableCount == 1 ? " reward ready" : " rewards ready")
                : completedCount + "/3 complete today";
        }

        if (missionSummaryOpenButtonText != null)
            missionSummaryOpenButtonText.text = claimableCount > 0 ? "Open + Claim" : "Open Missions";
    }

    void RefreshDailyMissionPanel()
    {
        if (missionOverlayPanel == null)
            return;

        DailyMissionData[] missions = DailyMissionSystem.GetMissions();
        int claimableCount = DailyMissionSystem.GetClaimableCount();
        int completedCount = DailyMissionSystem.GetCompletedCount();

        if (missionOverlayTitleText != null)
            missionOverlayTitleText.text = "Daily Missions";

        for (int i = 0; i < 3; i++)
        {
            if (i >= missions.Length)
                continue;

            DailyMissionData mission = missions[i];
            Color cardColor;
            string progressLabel;

            if (mission.claimed)
            {
                cardColor = new Color(0.26f, 0.33f, 0.43f, 1f);
                progressLabel = "Claimed";
            }
            else if (mission.progress >= mission.target)
            {
                cardColor = new Color(0.2f, 0.42f, 0.28f, 1f);
                progressLabel = "Ready to claim";
            }
            else
            {
                cardColor = new Color(0.18f, 0.27f, 0.43f, 1f);
                progressLabel = DailyMissionSystem.GetMissionProgressLabel(mission);
            }

            if (missionCardImages[i] != null)
                missionCardImages[i].color = cardColor;

            if (missionCardTitleTexts[i] != null)
                missionCardTitleTexts[i].text = DailyMissionSystem.GetMissionSummary(mission);

            if (missionCardProgressTexts[i] != null)
                missionCardProgressTexts[i].text = progressLabel;

            if (missionCardRewardTexts[i] != null)
                missionCardRewardTexts[i].text = "Reward: " + DailyMissionSystem.GetMissionRewardLabel(mission);
        }

        if (missionOverlayStatusText != null)
        {
            if (!string.IsNullOrEmpty(missionClaimFeedback) && claimableCount == 0)
            {
                missionOverlayStatusText.text = missionClaimFeedback;
            }
            else if (claimableCount > 0)
            {
                missionOverlayStatusText.text =
                    claimableCount +
                    (claimableCount == 1 ? " reward ready to claim" : " rewards ready to claim");
            }
            else
            {
                missionOverlayStatusText.text = completedCount + "/3 completed today";
            }
        }

        if (claimMissionButton != null)
            claimMissionButton.interactable = claimableCount > 0;

        if (claimMissionButtonText != null)
            claimMissionButtonText.text = claimableCount > 0 ? "Claim Rewards" : "No Rewards Yet";

        if (closeMissionButtonText != null)
            closeMissionButtonText.text = "Close";
    }

    void SetMissionOverlayVisible(bool isVisible)
    {
        if (missionOverlayRoot != null)
            missionOverlayRoot.SetActive(isVisible);
    }

    TMP_FontAsset ResolveRuntimeFont()
    {
        TMP_Text titleText = FindText(titleObjectName);

        if (titleText != null)
            return titleText.font;

        Button playButton = FindButton(playButtonObjectName);

        if (playButton != null)
        {
            TMP_Text label = playButton.GetComponentInChildren<TMP_Text>(true);

            if (label != null)
                return label.font;
        }

        return null;
    }

    void NormalizeMenuLayout()
    {
        NormalizeDailyRewardPanel();
        NormalizeMissionSummaryPanel();
        NormalizeMissionOverlayPanel();
        NormalizeButtons();
    }

    void NormalizeDailyRewardPanel()
    {
        if (dailyRewardPanel == null)
            return;

        RectTransform panelRect = dailyRewardPanel.GetComponent<RectTransform>();

        if (panelRect == null)
            return;

        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.sizeDelta = new Vector2(760f, 260f);
        panelRect.anchoredPosition = new Vector2(0f, -440f);

        if (dailyRewardTitleText != null)
        {
            RectTransform titleRect = dailyRewardTitleText.rectTransform;
            titleRect.sizeDelta = new Vector2(680f, 42f);
            titleRect.anchoredPosition = new Vector2(0f, -18f);
        }

        if (dailyRewardBodyText != null)
        {
            RectTransform bodyRect = dailyRewardBodyText.rectTransform;
            bodyRect.sizeDelta = new Vector2(680f, 64f);
            bodyRect.anchoredPosition = new Vector2(0f, -92f);
            dailyRewardBodyText.lineSpacing = -4f;
        }

        if (dailyRewardStatusText != null)
        {
            RectTransform statusRect = dailyRewardStatusText.rectTransform;
            statusRect.sizeDelta = new Vector2(680f, 24f);
            statusRect.anchoredPosition = new Vector2(0f, 90f);
        }

        if (claimRewardButton != null)
        {
            RectTransform buttonRect = claimRewardButton.GetComponent<RectTransform>();

            if (buttonRect != null)
            {
                buttonRect.sizeDelta = new Vector2(320f, 56f);
                buttonRect.anchoredPosition = new Vector2(0f, 16f);
            }
        }
    }

    void NormalizeMissionSummaryPanel()
    {
        if (missionSummaryPanel == null)
            return;

        RectTransform panelRect = missionSummaryPanel.GetComponent<RectTransform>();

        if (panelRect == null)
            return;

        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(760f, 200f);
        panelRect.anchoredPosition = new Vector2(0f, 18f);

        if (missionSummaryTitleText != null)
        {
            RectTransform titleRect = missionSummaryTitleText.rectTransform;
            titleRect.sizeDelta = new Vector2(680f, 40f);
            titleRect.anchoredPosition = new Vector2(0f, -20f);
        }

        if (missionSummaryStatusText != null)
        {
            RectTransform statusRect = missionSummaryStatusText.rectTransform;
            statusRect.sizeDelta = new Vector2(680f, 40f);
            statusRect.anchoredPosition = new Vector2(0f, -82f);
            missionSummaryStatusText.fontSizeMin = 24f;
            missionSummaryStatusText.fontSizeMax = 32f;
            missionSummaryStatusText.lineSpacing = 0f;
        }

        if (missionSummaryOpenButton != null)
        {
            RectTransform buttonRect = missionSummaryOpenButton.GetComponent<RectTransform>();

            if (buttonRect != null)
            {
                buttonRect.sizeDelta = new Vector2(380f, 64f);
                buttonRect.anchoredPosition = new Vector2(0f, 18f);
            }
        }

        if (missionSummaryOpenButtonText != null)
        {
            missionSummaryOpenButtonText.fontSizeMin = 22f;
            missionSummaryOpenButtonText.fontSizeMax = 30f;
        }
    }

    void NormalizeMissionOverlayPanel()
    {
        if (missionOverlayRoot == null || missionOverlayPanel == null)
            return;

        RectTransform rootRect = missionOverlayRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        RectTransform panelRect = missionOverlayPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(860f, 980f);
        panelRect.anchoredPosition = new Vector2(0f, -20f);

        if (missionOverlayTitleText != null)
        {
            RectTransform titleRect = missionOverlayTitleText.rectTransform;
            titleRect.sizeDelta = new Vector2(720f, 54f);
            titleRect.anchoredPosition = new Vector2(0f, -28f);
        }

        for (int i = 0; i < 3; i++)
        {
            if (missionCardImages[i] == null)
                continue;

            RectTransform cardRect = missionCardImages[i].rectTransform;
            cardRect.sizeDelta = new Vector2(760f, 170f);
            cardRect.anchoredPosition = new Vector2(0f, -116f - (i * 186f));

            if (missionCardTitleTexts[i] != null)
            {
                RectTransform titleRect = missionCardTitleTexts[i].rectTransform;
                titleRect.sizeDelta = new Vector2(680f, 48f);
                titleRect.anchoredPosition = new Vector2(0f, -20f);
            }

            if (missionCardProgressTexts[i] != null)
            {
                RectTransform progressRect = missionCardProgressTexts[i].rectTransform;
                progressRect.sizeDelta = new Vector2(330f, 36f);
                progressRect.anchoredPosition = new Vector2(28f, 28f);
            }

            if (missionCardRewardTexts[i] != null)
            {
                RectTransform rewardRect = missionCardRewardTexts[i].rectTransform;
                rewardRect.sizeDelta = new Vector2(330f, 36f);
                rewardRect.anchoredPosition = new Vector2(-28f, 28f);
            }
        }

        if (missionOverlayStatusText != null)
        {
            RectTransform statusRect = missionOverlayStatusText.rectTransform;
            statusRect.sizeDelta = new Vector2(720f, 40f);
            statusRect.anchoredPosition = new Vector2(0f, 132f);
        }

        if (claimMissionButton != null)
        {
            RectTransform buttonRect = claimMissionButton.GetComponent<RectTransform>();

            if (buttonRect != null)
            {
                buttonRect.sizeDelta = new Vector2(320f, 72f);
                buttonRect.anchoredPosition = new Vector2(-20f, 34f);
            }
        }

        if (closeMissionButton != null)
        {
            RectTransform buttonRect = closeMissionButton.GetComponent<RectTransform>();

            if (buttonRect != null)
            {
                buttonRect.sizeDelta = new Vector2(220f, 72f);
                buttonRect.anchoredPosition = new Vector2(20f, 34f);
            }
        }
    }

    void NormalizeButtons()
    {
        SetButtonLayout(playButtonObjectName, 50f);
        SetButtonLayout(shopButtonObjectName, -88f);
        SetButtonLayout(inventoryButtonObjectName, -226f);
        SetButtonLayout(exitButtonObjectName, -364f);
    }

    void SetButtonLayout(string objectName, float anchoredY)
    {
        Button button = FindButton(objectName);

        if (button == null)
            return;

        RectTransform buttonRect = button.GetComponent<RectTransform>();

        if (buttonRect != null)
        {
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(500f, 128f);
            buttonRect.anchoredPosition = new Vector2(0f, anchoredY);
        }

        Image buttonImage = button.GetComponent<Image>();

        if (buttonImage != null)
            buttonImage.color = new Color(0.96f, 0.97f, 1f, 0.96f);

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);

        if (label != null)
        {
            label.enableAutoSizing = true;
            label.fontSizeMin = 24;
            label.fontSizeMax = 34;
            label.color = new Color(0.18f, 0.22f, 0.3f, 1f);
        }
    }

    TMP_Text FindText(string objectName)
    {
        GameObject textObject = GameObject.Find(objectName);

        if (textObject == null)
            return null;

        return textObject.GetComponent<TMP_Text>();
    }

    RectTransform GetMenuRoot()
    {
        Button playButton = FindButton(playButtonObjectName);

        if (playButton != null && playButton.transform.parent != null)
            return playButton.transform.parent as RectTransform;

        Canvas canvas = FindAnyObjectByType<Canvas>();

        if (canvas != null)
            return canvas.GetComponent<RectTransform>();

        return null;
    }

    Button FindButton(string objectName)
    {
        GameObject buttonObject = GameObject.Find(objectName);

        if (buttonObject == null)
            return null;

        return buttonObject.GetComponent<Button>();
    }
}
