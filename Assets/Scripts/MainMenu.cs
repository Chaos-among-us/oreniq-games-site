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
    private TMP_Text titleText;
    private RectTransform titleRect;
    private TextMeshProUGUI profileStatsText;
    private bool profileStatsSceneOwned;

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

    private GameObject challengeSummaryPanel;
    private TextMeshProUGUI challengeSummaryTitleText;
    private TextMeshProUGUI challengeSummaryBodyText;
    private TextMeshProUGUI challengeSummaryStatusText;
    private Button challengeSummaryActionButton;
    private TextMeshProUGUI challengeSummaryActionButtonText;

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

    private Button masteryRoadmapButton;
    private TextMeshProUGUI masteryRoadmapButtonText;
    private GameObject masteryOverlayRoot;
    private GameObject masteryOverlayPanel;
    private TextMeshProUGUI masteryOverlayTitleText;
    private TextMeshProUGUI masteryOverlayStatusText;
    private readonly Image[] masteryTitleCardImages = new Image[3];
    private readonly TextMeshProUGUI[] masteryTitleCardTitleTexts = new TextMeshProUGUI[3];
    private readonly TextMeshProUGUI[] masteryTitleCardRequirementTexts = new TextMeshProUGUI[3];
    private readonly TextMeshProUGUI[] masteryTitleCardProgressTexts = new TextMeshProUGUI[3];
    private readonly Image[] masteryMilestoneCardImages = new Image[4];
    private readonly TextMeshProUGUI[] masteryMilestoneTitleTexts = new TextMeshProUGUI[4];
    private readonly TextMeshProUGUI[] masteryMilestoneRequirementTexts = new TextMeshProUGUI[4];
    private readonly TextMeshProUGUI[] masteryMilestoneProgressTexts = new TextMeshProUGUI[4];
    private readonly TextMeshProUGUI[] masteryMilestoneRewardTexts = new TextMeshProUGUI[4];
    private readonly Image[] masteryMilestoneProgressFills = new Image[4];
    private TextMeshProUGUI masteryOverlayFooterText;
    private Button closeMasteryButton;
    private TextMeshProUGUI closeMasteryButtonText;
    private Button qaModeButton;
    private TextMeshProUGUI qaModeButtonText;
    private GameObject qaOverlayRoot;
    private GameObject qaOverlayPanel;
    private TextMeshProUGUI qaOverlayTitleText;
    private TextMeshProUGUI qaOverlayBodyText;
    private TextMeshProUGUI qaOverlayStatusText;
    private Button qaOverlayToggleButton;
    private TextMeshProUGUI qaOverlayToggleButtonText;
    private Button qaOverlayPracticeButton;
    private TextMeshProUGUI qaOverlayPracticeButtonText;
    private Button qaOverlayClearButton;
    private TextMeshProUGUI qaOverlayClearButtonText;
    private Button qaOverlayCloseButton;
    private TextMeshProUGUI qaOverlayCloseButtonText;

    private const string ProfileStatsObjectName = "ProfileStatsText";
    private const string DailyRewardPanelObjectName = "DailyRewardPanel";
    private const string ChallengeSummaryPanelObjectName = "DailyChallengeSummaryPanel";
    private const string MissionSummaryPanelObjectName = "DailyMissionSummaryPanel";
    private const string MissionOverlayRootObjectName = "DailyMissionOverlayRoot";
    private const string MasteryRoadmapButtonObjectName = "MasteryRoadmapButton";
    private const string MasteryOverlayRootObjectName = "MasteryOverlayRoot";
    private const string QaModeButtonObjectName = "QaModeButton";
    private const string QaOverlayRootObjectName = "QaOverlayRoot";

    void Start()
    {
        if (SceneManager.GetActiveScene().name != ExpectedSceneName)
        {
            enabled = false;
            return;
        }

        DailyRewardSystem.GetPreviewReward();
        DailyMissionSystem.EnsureInitializedForToday();
        DailyChallengeSystem.EnsureInitializedForToday();

        runtimeFont = ResolveRuntimeFont();
        menuRootRect = GetMenuRoot();
        titleText = FindText(titleObjectName);
        titleRect = titleText != null ? titleText.rectTransform : null;
        QaTestingSystem.EnsureRuntime();

        if (createInventoryButtonAtRuntime)
            EnsureInventoryButtonExists();

        EnsureProfileStatsLabel();
        EnsureMasteryRoadmapButton();
        EnsureQaModeButton();

        if (createDailyRewardPanelAtRuntime)
            EnsureDailyRewardPanel();

        EnsureChallengeSummaryPanel();
        EnsureMissionSummaryPanel();
        EnsureMissionOverlayPanel();
        EnsureMasteryOverlayPanel();
        EnsureQaOverlayPanel();
        NormalizeMenuLayout();
        RefreshMenuHud();
        SetMissionOverlayVisible(false);
        SetMasteryOverlayVisible(false);
        SetQaOverlayVisible(false);
    }

    void OnRectTransformDimensionsChange()
    {
        if (!isActiveAndEnabled || SceneManager.GetActiveScene().name != ExpectedSceneName)
            return;

        NormalizeMenuLayout();
        RefreshMenuHud();
    }

    public void PlayGame()
    {
        DailyChallengeSystem.ClearActiveRun();
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

    public void HandleDailyChallengeAction()
    {
        DailyChallengeData challenge = DailyChallengeSystem.GetTodayChallenge();

        if (challenge.rewardClaimed)
        {
            RefreshMenuHud();
            return;
        }

        if (DailyChallengeSystem.TryClaimReward(out int coinsGranted, out UpgradeType rewardUpgrade, out int rewardAmount))
        {
            GameSettings.TriggerHaptic();
            RefreshMenuHud();

            if (challengeSummaryStatusText != null)
            {
                challengeSummaryStatusText.text = "Reward claimed for today";
            }

            return;
        }

        DailyChallengeSystem.BeginTodayChallengeRun();
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenMissionOverlay()
    {
        RefreshDailyMissionPanel();
        SetMasteryOverlayVisible(false);
        SetMissionOverlayVisible(true);
    }

    public void CloseMissionOverlay()
    {
        SetMissionOverlayVisible(false);
    }

    public void OpenMasteryOverlay()
    {
        RefreshMasteryOverlay();
        SetMissionOverlayVisible(false);
        SetMasteryOverlayVisible(true);
    }

    public void CloseMasteryOverlay()
    {
        SetMasteryOverlayVisible(false);
    }

    public void OpenQaOverlay()
    {
        RefreshQaMenu();
        SetMissionOverlayVisible(false);
        SetMasteryOverlayVisible(false);
        SetQaOverlayVisible(true);
    }

    public void CloseQaOverlay()
    {
        SetQaOverlayVisible(false);
    }

    public void ToggleQaMode()
    {
        bool enableQaMode = !QaTestingSystem.IsQaModeEnabled();

        if (enableQaMode)
            QaTestingSystem.MarkNoticeAccepted();

        QaTestingSystem.SetQaModeEnabled(enableQaMode);
        GameSettings.TriggerHaptic();
        RefreshQaMenu();
    }

    public void DeleteQaArtifacts()
    {
        QaTestingSystem.DeleteAllArtifacts();
        GameSettings.TriggerHaptic();
        RefreshQaMenu();
    }

    public void StartQaPracticeRun()
    {
        QaTestingSystem.RequestPracticeRun();
        GameSettings.TriggerHaptic();
        DailyChallengeSystem.ClearActiveRun();
        SceneManager.LoadScene(gameSceneName);
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
            profileStatsSceneOwned = true;
            profileStatsText = existing.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            profileStatsSceneOwned = false;
            GameObject statsObject = new GameObject(ProfileStatsObjectName, typeof(RectTransform));
            statsObject.transform.SetParent(menuRootRect, false);
            profileStatsText = statsObject.AddComponent<TextMeshProUGUI>();
        }

        if (!profileStatsSceneOwned)
        {
            RectTransform statsRect = profileStatsText.rectTransform;
            statsRect.anchorMin = new Vector2(0.5f, 1f);
            statsRect.anchorMax = new Vector2(0.5f, 1f);
            statsRect.pivot = new Vector2(0.5f, 1f);
            statsRect.sizeDelta = new Vector2(760f, 94f);
            statsRect.anchoredPosition = new Vector2(0f, -316f);
        }

        profileStatsText.alignment = TextAlignmentOptions.Center;
        profileStatsText.enableAutoSizing = true;
        profileStatsText.fontSizeMin = 18;
        profileStatsText.fontSizeMax = 30;
        profileStatsText.lineSpacing = 4f;
        profileStatsText.color = new Color(0.92f, 0.96f, 1f, 1f);

        if (runtimeFont != null)
            profileStatsText.font = runtimeFont;
    }

    void EnsureMasteryRoadmapButton()
    {
        if (menuRootRect == null)
            return;

        Transform existing = menuRootRect.Find(MasteryRoadmapButtonObjectName);

        if (existing != null)
        {
            masteryRoadmapButton = existing.GetComponent<Button>();
            masteryRoadmapButtonText = existing.GetComponentInChildren<TextMeshProUGUI>(true);
        }
        else
        {
            masteryRoadmapButton = CreatePanelButton(
                menuRootRect,
                MasteryRoadmapButtonObjectName,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(400f, 58f),
                new Vector2(0f, -384f),
                "Roadmap & Titles",
                out masteryRoadmapButtonText);
        }

        if (masteryRoadmapButton != null)
        {
            masteryRoadmapButton.onClick.RemoveAllListeners();
            masteryRoadmapButton.onClick.AddListener(OpenMasteryOverlay);
        }
    }

    void EnsureQaModeButton()
    {
        if (menuRootRect == null)
            return;

        Transform existing = menuRootRect.Find(QaModeButtonObjectName);

        if (existing != null)
        {
            qaModeButton = existing.GetComponent<Button>();
            qaModeButtonText = existing.GetComponentInChildren<TextMeshProUGUI>(true);
        }
        else
        {
            qaModeButton = CreatePanelButton(
                menuRootRect,
                QaModeButtonObjectName,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(260f, 74f),
                new Vector2(20f, 20f),
                "QA Mode",
                out qaModeButtonText);
        }

        if (qaModeButton != null)
        {
            qaModeButton.onClick.RemoveAllListeners();
            qaModeButton.onClick.AddListener(OpenQaOverlay);
        }
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

    void EnsureChallengeSummaryPanel()
    {
        if (menuRootRect == null)
            return;

        Transform existing = menuRootRect.Find(ChallengeSummaryPanelObjectName);

        if (existing != null)
        {
            challengeSummaryPanel = existing.gameObject;
            CacheChallengeSummaryReferences();
            return;
        }

        challengeSummaryPanel = new GameObject(ChallengeSummaryPanelObjectName, typeof(RectTransform), typeof(Image));
        challengeSummaryPanel.transform.SetParent(menuRootRect, false);

        RectTransform panelRect = challengeSummaryPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(760f, 220f);
        panelRect.anchoredPosition = new Vector2(0f, 244f);

        Image panelImage = challengeSummaryPanel.GetComponent<Image>();
        panelImage.color = new Color(0.1f, 0.19f, 0.34f, 0.97f);

        challengeSummaryTitleText = CreatePanelText(
            challengeSummaryPanel.transform,
            "ChallengeSummaryTitle",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(680f, 40f),
            new Vector2(0f, -18f),
            TextAlignmentOptions.Center,
            24f,
            34f,
            new Color(1f, 0.94f, 0.72f, 1f));

        challengeSummaryBodyText = CreatePanelText(
            challengeSummaryPanel.transform,
            "ChallengeSummaryBody",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(680f, 64f),
            new Vector2(0f, -78f),
            TextAlignmentOptions.Center,
            20f,
            28f,
            new Color(0.94f, 0.97f, 1f, 1f));

        challengeSummaryStatusText = CreatePanelText(
            challengeSummaryPanel.transform,
            "ChallengeSummaryStatus",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(680f, 34f),
            new Vector2(0f, -138f),
            TextAlignmentOptions.Center,
            18f,
            24f,
            new Color(0.86f, 0.93f, 1f, 1f));

        challengeSummaryActionButton = CreatePanelButton(
            challengeSummaryPanel.transform,
            "ChallengeSummaryActionButton",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(420f, 60f),
            new Vector2(0f, 18f),
            "Play Challenge",
            out challengeSummaryActionButtonText);

        if (challengeSummaryActionButton != null)
        {
            challengeSummaryActionButton.onClick.RemoveAllListeners();
            challengeSummaryActionButton.onClick.AddListener(HandleDailyChallengeAction);
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

    void EnsureMasteryOverlayPanel()
    {
        if (menuRootRect == null)
            return;

        Transform existing = menuRootRect.Find(MasteryOverlayRootObjectName);

        if (existing != null)
        {
            masteryOverlayRoot = existing.gameObject;
            CacheMasteryOverlayReferences();
            return;
        }

        masteryOverlayRoot = new GameObject(MasteryOverlayRootObjectName, typeof(RectTransform), typeof(Image));
        masteryOverlayRoot.transform.SetParent(menuRootRect, false);

        RectTransform rootRect = masteryOverlayRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image rootImage = masteryOverlayRoot.GetComponent<Image>();
        rootImage.color = new Color(0.02f, 0.04f, 0.08f, 0.88f);

        masteryOverlayPanel = new GameObject("MasteryOverlayPanel", typeof(RectTransform), typeof(Image));
        masteryOverlayPanel.transform.SetParent(masteryOverlayRoot.transform, false);

        RectTransform panelRect = masteryOverlayPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(900f, 1080f);
        panelRect.anchoredPosition = new Vector2(0f, -20f);

        Image panelImage = masteryOverlayPanel.GetComponent<Image>();
        panelImage.color = new Color(0.11f, 0.18f, 0.31f, 1f);

        masteryOverlayTitleText = CreatePanelText(
            masteryOverlayPanel.transform,
            "MasteryOverlayTitle",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(760f, 54f),
            new Vector2(0f, -28f),
            TextAlignmentOptions.Center,
            34f,
            44f,
            Color.white);

        masteryOverlayStatusText = CreatePanelText(
            masteryOverlayPanel.transform,
            "MasteryOverlayStatus",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(760f, 48f),
            new Vector2(0f, -86f),
            TextAlignmentOptions.Center,
            18f,
            28f,
            new Color(0.84f, 0.92f, 1f, 1f));

        for (int i = 0; i < masteryTitleCardImages.Length; i++)
            CreateMasteryTitleCard(i);

        for (int i = 0; i < masteryMilestoneCardImages.Length; i++)
            CreateMasteryMilestoneCard(i);

        masteryOverlayFooterText = CreatePanelText(
            masteryOverlayPanel.transform,
            "MasteryOverlayFooter",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(760f, 74f),
            new Vector2(0f, 130f),
            TextAlignmentOptions.Center,
            18f,
            26f,
            new Color(0.8f, 0.88f, 0.95f, 1f));

        closeMasteryButton = CreatePanelButton(
            masteryOverlayPanel.transform,
            "CloseMasteryButton",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(260f, 68f),
            new Vector2(0f, 34f),
            "Close",
            out closeMasteryButtonText);

        if (closeMasteryButton != null)
        {
            closeMasteryButton.onClick.RemoveAllListeners();
            closeMasteryButton.onClick.AddListener(CloseMasteryOverlay);

            Image closeImage = closeMasteryButton.GetComponent<Image>();

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

    void CreateMasteryTitleCard(int index)
    {
        GameObject cardObject = new GameObject("TitleCard" + index, typeof(RectTransform), typeof(Image));
        cardObject.transform.SetParent(masteryOverlayPanel.transform, false);

        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 1f);
        cardRect.anchorMax = new Vector2(0.5f, 1f);
        cardRect.pivot = new Vector2(0.5f, 1f);
        cardRect.sizeDelta = new Vector2(246f, 138f);
        cardRect.anchoredPosition = new Vector2(-262f + (index * 262f), -164f);

        Image cardImage = cardObject.GetComponent<Image>();
        cardImage.color = new Color(0.22f, 0.3f, 0.46f, 1f);
        masteryTitleCardImages[index] = cardImage;

        masteryTitleCardTitleTexts[index] = CreatePanelText(
            cardObject.transform,
            "Title",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(220f, 40f),
            new Vector2(0f, -16f),
            TextAlignmentOptions.Center,
            24f,
            32f,
            Color.white);

        masteryTitleCardRequirementTexts[index] = CreatePanelText(
            cardObject.transform,
            "Requirement",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(220f, 36f),
            new Vector2(0f, -62f),
            TextAlignmentOptions.Center,
            16f,
            22f,
            new Color(0.84f, 0.92f, 1f, 1f));

        masteryTitleCardProgressTexts[index] = CreatePanelText(
            cardObject.transform,
            "Progress",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(220f, 30f),
            new Vector2(0f, 16f),
            TextAlignmentOptions.Center,
            16f,
            22f,
            new Color(1f, 0.94f, 0.72f, 1f));
    }

    void CreateMasteryMilestoneCard(int index)
    {
        GameObject cardObject = new GameObject("MilestoneCard" + index, typeof(RectTransform), typeof(Image));
        cardObject.transform.SetParent(masteryOverlayPanel.transform, false);

        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 1f);
        cardRect.anchorMax = new Vector2(0.5f, 1f);
        cardRect.pivot = new Vector2(0.5f, 1f);
        cardRect.sizeDelta = new Vector2(812f, 132f);
        cardRect.anchoredPosition = new Vector2(0f, -342f - (index * 140f));

        Image cardImage = cardObject.GetComponent<Image>();
        cardImage.color = new Color(0.18f, 0.27f, 0.43f, 1f);
        masteryMilestoneCardImages[index] = cardImage;

        masteryMilestoneTitleTexts[index] = CreatePanelText(
            cardObject.transform,
            "Title",
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(520f, 30f),
            new Vector2(26f, -14f),
            TextAlignmentOptions.Left,
            20f,
            28f,
            Color.white);

        masteryMilestoneRequirementTexts[index] = CreatePanelText(
            cardObject.transform,
            "Requirement",
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(720f, 24f),
            new Vector2(26f, -42f),
            TextAlignmentOptions.Left,
            16f,
            22f,
            new Color(0.84f, 0.92f, 1f, 1f));

        masteryMilestoneProgressTexts[index] = CreatePanelText(
            cardObject.transform,
            "Progress",
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(320f, 22f),
            new Vector2(26f, 8f),
            TextAlignmentOptions.Left,
            16f,
            22f,
            new Color(0.88f, 0.96f, 1f, 1f));

        masteryMilestoneRewardTexts[index] = CreatePanelText(
            cardObject.transform,
            "Reward",
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(320f, 22f),
            new Vector2(-26f, 8f),
            TextAlignmentOptions.Right,
            16f,
            22f,
            new Color(1f, 0.9f, 0.7f, 1f));

        GameObject progressTrackObject = new GameObject("ProgressTrack", typeof(RectTransform), typeof(Image));
        progressTrackObject.transform.SetParent(cardObject.transform, false);

        RectTransform trackRect = progressTrackObject.GetComponent<RectTransform>();
        trackRect.anchorMin = new Vector2(0.5f, 0f);
        trackRect.anchorMax = new Vector2(0.5f, 0f);
        trackRect.pivot = new Vector2(0.5f, 0f);
        trackRect.sizeDelta = new Vector2(760f, 14f);
        trackRect.anchoredPosition = new Vector2(0f, 38f);

        Image trackImage = progressTrackObject.GetComponent<Image>();
        trackImage.color = new Color(0.08f, 0.12f, 0.18f, 0.92f);

        GameObject progressFillObject = new GameObject("ProgressFill", typeof(RectTransform), typeof(Image));
        progressFillObject.transform.SetParent(progressTrackObject.transform, false);

        RectTransform fillRect = progressFillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fillImage = progressFillObject.GetComponent<Image>();
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;
        fillImage.fillAmount = 0f;
        fillImage.color = new Color(0.28f, 0.7f, 0.58f, 1f);
        masteryMilestoneProgressFills[index] = fillImage;
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

    void CacheChallengeSummaryReferences()
    {
        if (challengeSummaryPanel == null)
            return;

        challengeSummaryTitleText = FindTextInParent(challengeSummaryPanel.transform, "ChallengeSummaryTitle");
        challengeSummaryBodyText = FindTextInParent(challengeSummaryPanel.transform, "ChallengeSummaryBody");
        challengeSummaryStatusText = FindTextInParent(challengeSummaryPanel.transform, "ChallengeSummaryStatus");

        Transform buttonTransform = challengeSummaryPanel.transform.Find("ChallengeSummaryActionButton");

        if (buttonTransform != null)
        {
            challengeSummaryActionButton = buttonTransform.GetComponent<Button>();
            challengeSummaryActionButtonText = buttonTransform.GetComponentInChildren<TextMeshProUGUI>(true);
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

    void CacheMasteryOverlayReferences()
    {
        if (masteryOverlayRoot == null)
            return;

        Transform panelTransform = masteryOverlayRoot.transform.Find("MasteryOverlayPanel");

        if (panelTransform != null)
            masteryOverlayPanel = panelTransform.gameObject;

        if (masteryOverlayPanel == null)
            return;

        masteryOverlayTitleText = FindTextInParent(masteryOverlayPanel.transform, "MasteryOverlayTitle");
        masteryOverlayStatusText = FindTextInParent(masteryOverlayPanel.transform, "MasteryOverlayStatus");
        masteryOverlayFooterText = FindTextInParent(masteryOverlayPanel.transform, "MasteryOverlayFooter");

        for (int i = 0; i < masteryTitleCardImages.Length; i++)
        {
            Transform cardTransform = masteryOverlayPanel.transform.Find("TitleCard" + i);

            if (cardTransform == null)
                continue;

            masteryTitleCardImages[i] = cardTransform.GetComponent<Image>();
            masteryTitleCardTitleTexts[i] = FindTextInParent(cardTransform, "Title");
            masteryTitleCardRequirementTexts[i] = FindTextInParent(cardTransform, "Requirement");
            masteryTitleCardProgressTexts[i] = FindTextInParent(cardTransform, "Progress");
        }

        for (int i = 0; i < masteryMilestoneCardImages.Length; i++)
        {
            Transform cardTransform = masteryOverlayPanel.transform.Find("MilestoneCard" + i);

            if (cardTransform == null)
                continue;

            masteryMilestoneCardImages[i] = cardTransform.GetComponent<Image>();
            masteryMilestoneTitleTexts[i] = FindTextInParent(cardTransform, "Title");
            masteryMilestoneRequirementTexts[i] = FindTextInParent(cardTransform, "Requirement");
            masteryMilestoneProgressTexts[i] = FindTextInParent(cardTransform, "Progress");
            masteryMilestoneRewardTexts[i] = FindTextInParent(cardTransform, "Reward");

            Transform trackTransform = cardTransform.Find("ProgressTrack");

            if (trackTransform != null)
            {
                Transform fillTransform = trackTransform.Find("ProgressFill");

                if (fillTransform != null)
                    masteryMilestoneProgressFills[i] = fillTransform.GetComponent<Image>();
            }
        }

        Transform closeTransform = masteryOverlayPanel.transform.Find("CloseMasteryButton");

        if (closeTransform != null)
        {
            closeMasteryButton = closeTransform.GetComponent<Button>();
            closeMasteryButtonText = closeTransform.GetComponentInChildren<TextMeshProUGUI>(true);
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

    void EnsureQaOverlayPanel()
    {
        if (menuRootRect == null)
            return;

        Transform existingRoot = menuRootRect.Find(QaOverlayRootObjectName);

        if (existingRoot != null)
        {
            qaOverlayRoot = existingRoot.gameObject;
            qaOverlayPanel = existingRoot.Find("QaOverlayPanel")?.gameObject;
            Transform qaPanelTransform = existingRoot.Find("QaOverlayPanel");
            qaOverlayTitleText = FindTextInParent(qaPanelTransform, "QaOverlayTitle");
            qaOverlayBodyText = FindTextInParent(qaPanelTransform, "QaOverlayBody");
            qaOverlayStatusText = FindTextInParent(qaPanelTransform, "QaOverlayStatus");

            Transform toggleTransform = existingRoot.Find("QaOverlayPanel/QaOverlayToggleButton");

            if (toggleTransform != null)
            {
                qaOverlayToggleButton = toggleTransform.GetComponent<Button>();
                qaOverlayToggleButtonText = toggleTransform.GetComponentInChildren<TextMeshProUGUI>(true);
            }

            Transform practiceTransform = existingRoot.Find("QaOverlayPanel/QaOverlayPracticeButton");

            if (practiceTransform != null)
            {
                qaOverlayPracticeButton = practiceTransform.GetComponent<Button>();
                qaOverlayPracticeButtonText = practiceTransform.GetComponentInChildren<TextMeshProUGUI>(true);
            }

            Transform clearTransform = existingRoot.Find("QaOverlayPanel/QaOverlayClearButton");

            if (clearTransform != null)
            {
                qaOverlayClearButton = clearTransform.GetComponent<Button>();
                qaOverlayClearButtonText = clearTransform.GetComponentInChildren<TextMeshProUGUI>(true);
            }

            Transform closeTransform = existingRoot.Find("QaOverlayPanel/QaOverlayCloseButton");

            if (closeTransform != null)
            {
                qaOverlayCloseButton = closeTransform.GetComponent<Button>();
                qaOverlayCloseButtonText = closeTransform.GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }
        else
        {
            qaOverlayRoot = new GameObject(QaOverlayRootObjectName, typeof(RectTransform), typeof(Image));
            qaOverlayRoot.transform.SetParent(menuRootRect, false);

            RectTransform rootRect = qaOverlayRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            Image rootImage = qaOverlayRoot.GetComponent<Image>();
            rootImage.color = new Color(0.01f, 0.02f, 0.04f, 0.9f);

            qaOverlayPanel = new GameObject("QaOverlayPanel", typeof(RectTransform), typeof(Image));
            qaOverlayPanel.transform.SetParent(qaOverlayRoot.transform, false);

            RectTransform panelRect = qaOverlayPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(860f, 920f);
            panelRect.anchoredPosition = new Vector2(0f, -20f);

            Image panelImage = qaOverlayPanel.GetComponent<Image>();
            panelImage.color = new Color(0.11f, 0.16f, 0.24f, 0.98f);

            qaOverlayTitleText = CreatePanelText(
                qaOverlayPanel.transform,
                "QaOverlayTitle",
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(700f, 64f),
                new Vector2(0f, -28f),
                TextAlignmentOptions.Center,
                32f,
                44f,
                Color.white);

            qaOverlayBodyText = CreatePanelText(
                qaOverlayPanel.transform,
                "QaOverlayBody",
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(720f, 490f),
                new Vector2(0f, -110f),
                TextAlignmentOptions.TopLeft,
                20f,
                28f,
                new Color(0.9f, 0.95f, 1f, 1f));

            qaOverlayStatusText = CreatePanelText(
                qaOverlayPanel.transform,
                "QaOverlayStatus",
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(720f, 124f),
                new Vector2(0f, -628f),
                TextAlignmentOptions.TopLeft,
                18f,
                24f,
                new Color(0.82f, 0.9f, 0.98f, 1f));

            qaOverlayToggleButton = CreatePanelButton(
                qaOverlayPanel.transform,
                "QaOverlayToggleButton",
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(420f, 72f),
                new Vector2(0f, 262f),
                "Enable QA Mode",
                out qaOverlayToggleButtonText);

            qaOverlayPracticeButton = CreatePanelButton(
                qaOverlayPanel.transform,
                "QaOverlayPracticeButton",
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(420f, 72f),
                new Vector2(0f, 178f),
                "Practice Tutorial Run",
                out qaOverlayPracticeButtonText);

            qaOverlayClearButton = CreatePanelButton(
                qaOverlayPanel.transform,
                "QaOverlayClearButton",
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(420f, 72f),
                new Vector2(0f, 94f),
                "Delete QA Files",
                out qaOverlayClearButtonText);

            qaOverlayCloseButton = CreatePanelButton(
                qaOverlayPanel.transform,
                "QaOverlayCloseButton",
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(220f, 64f),
                new Vector2(0f, 18f),
                "Close",
                out qaOverlayCloseButtonText);
        }

        if (qaOverlayToggleButton != null)
        {
            qaOverlayToggleButton.onClick.RemoveAllListeners();
            qaOverlayToggleButton.onClick.AddListener(ToggleQaMode);
        }

        if (qaOverlayPracticeButton != null)
        {
            qaOverlayPracticeButton.onClick.RemoveAllListeners();
            qaOverlayPracticeButton.onClick.AddListener(StartQaPracticeRun);
        }

        if (qaOverlayClearButton != null)
        {
            qaOverlayClearButton.onClick.RemoveAllListeners();
            qaOverlayClearButton.onClick.AddListener(DeleteQaArtifacts);
        }

        if (qaOverlayCloseButton != null)
        {
            qaOverlayCloseButton.onClick.RemoveAllListeners();
            qaOverlayCloseButton.onClick.AddListener(CloseQaOverlay);
        }

        if (qaOverlayRoot != null)
            qaOverlayRoot.SetActive(false);
    }

    void RefreshMenuHud()
    {
        RefreshProfileStats();
        RefreshMasteryRoadmapButton();
        RefreshQaMenu();
        RefreshDailyRewardPanel();
        RefreshDailyChallengePanel();
        RefreshMissionSummaryPanel();
        RefreshDailyMissionPanel();
        RefreshMasteryOverlay();
    }

    void RefreshQaMenu()
    {
        RuntimeCaveTheme theme = CaveThemeLibrary.GetMenuTheme();
        bool qaModeEnabled = QaTestingSystem.IsQaModeEnabled();

        if (qaModeButtonText != null)
            qaModeButtonText.text = QaTestingSystem.GetMenuButtonLabel();

        if (qaModeButton != null)
        {
            Image buttonImage = qaModeButton.GetComponent<Image>();

            if (buttonImage != null)
            {
                buttonImage.color = qaModeEnabled
                    ? Color.Lerp(theme.WallColor, theme.AccentColor, 0.62f)
                    : Color.Lerp(theme.WallColor, theme.FogColor, 0.4f);
            }
        }

        if (qaOverlayTitleText != null)
            qaOverlayTitleText.text = "QA Capture Mode";

        if (qaOverlayBodyText != null)
        {
            qaOverlayBodyText.text = QaTestingSystem.GetNoticeBody();
            qaOverlayBodyText.lineSpacing = 6f;
        }

        if (qaOverlayStatusText != null)
            qaOverlayStatusText.text = QaTestingSystem.GetMenuStatusText();

        if (qaOverlayToggleButtonText != null)
            qaOverlayToggleButtonText.text = qaModeEnabled ? "Disable QA Mode" : "Enable QA Mode";

        if (qaOverlayPracticeButtonText != null)
            qaOverlayPracticeButtonText.text = qaModeEnabled ? "Practice Tutorial Run" : "Practice Run First";

        if (qaOverlayClearButtonText != null)
            qaOverlayClearButtonText.text = "Delete QA Files";

        if (qaOverlayClearButton != null)
            qaOverlayClearButton.interactable = QaTestingSystem.GetStoredArtifactCount() > 0;
    }

    void RefreshProfileStats()
    {
        if (profileStatsText == null)
            return;

        PlayerProfileSnapshot profile = PlayerProgressionSystem.GetProfileSnapshot();
        int streak = DailyRewardSystem.GetCurrentStreak();
        string nextGoal = profile.FeaturedGoals != null && profile.FeaturedGoals.Length > 0
            ? profile.FeaturedGoals[0]
            : "Keep pushing for the next mastery reward";

        string dangerLabel = profile.BestDangerCombo > 0 ? "Peak x" + profile.BestDangerCombo : "Peak x0";

        profileStatsText.text =
            "<color=#FFF1B9>" + profile.RankTitle + " Lv " + profile.Level + "</color>" +
            "\nXP " + profile.ExperienceIntoLevel + "/" + profile.ExperienceForNextLevel + "   Best " + profile.BestScore +
            "\nCoins " + PlayerPrefs.GetInt("TotalCoins", 0) + "   Streak " + streak + "   " + dangerLabel +
            "\nGoal: " + nextGoal;
    }

    void RefreshMasteryRoadmapButton()
    {
        if (masteryRoadmapButtonText == null)
            return;

        ProgressionTitleSnapshot[] titles = PlayerProgressionSystem.GetTitleRoadmap(2);
        string detailLine = titles.Length > 1
            ? "Next: " + titles[1].Title + " Lv " + titles[1].UnlockLevel
            : "Mastery track complete";

        masteryRoadmapButtonText.text = "Roadmap & Titles\n" + detailLine;
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
                : new Color(0.1f, 0.18f, 0.32f, 0.56f);
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
                : "Come back tomorrow";

            dailyRewardBodyText.color = canClaimToday
                ? new Color(0.94f, 0.97f, 1f, 1f)
                : new Color(0.9f, 0.95f, 1f, 0.84f);
        }

        if (dailyRewardStatusText != null)
        {
            dailyRewardStatusText.text = canClaimToday
                ? "Day " + rewardPackage.rewardDay + "   |   Streak " + currentStreak
                : "Next " + DailyRewardSystem.GetNextClaimCountdownText();

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

    void RefreshDailyChallengePanel()
    {
        if (challengeSummaryPanel == null)
            return;

        DailyChallengeData challenge = DailyChallengeSystem.GetTodayChallenge();
        bool canClaimReward = DailyChallengeSystem.CanClaimReward();
        bool rewardClaimed = challenge.rewardClaimed;
        Image panelImage = challengeSummaryPanel.GetComponent<Image>();
        Image buttonImage = challengeSummaryActionButton != null ? challengeSummaryActionButton.GetComponent<Image>() : null;

        if (panelImage != null)
        {
            panelImage.color = rewardClaimed
                ? new Color(0.14f, 0.2f, 0.29f, 0.82f)
                : canClaimReward
                    ? new Color(0.15f, 0.3f, 0.23f, 0.97f)
                    : new Color(0.1f, 0.19f, 0.34f, 0.97f);
        }

        if (challengeSummaryTitleText != null)
            challengeSummaryTitleText.text = "Daily Challenge";

        if (challengeSummaryBodyText != null)
        {
            challengeSummaryBodyText.text = challenge.title;
        }

        if (challengeSummaryStatusText != null)
        {
            if (rewardClaimed)
            {
                challengeSummaryStatusText.text = "Reward claimed";
            }
            else if (canClaimReward)
            {
                challengeSummaryStatusText.text = "Reward ready";
            }
            else if (challenge.bestScore > 0)
            {
                string unitLabel = challenge.goalType == DailyChallengeGoalType.CollectCoins ? "coins" : "score";
                challengeSummaryStatusText.text = "Best: " + challenge.bestScore + "/" + challenge.targetScore + " " + unitLabel;
            }
            else
            {
                challengeSummaryStatusText.text = challenge.goalType == DailyChallengeGoalType.CollectCoins
                    ? "Goal: " + challenge.targetScore + " coins"
                    : "Goal: " + challenge.targetScore + " score";
            }
        }

        if (challengeSummaryActionButton != null)
            challengeSummaryActionButton.interactable = !rewardClaimed;

        if (challengeSummaryActionButtonText != null)
        {
            challengeSummaryActionButtonText.text = rewardClaimed
                ? "Done"
                : canClaimReward
                    ? "Claim"
                    : "Play";
        }

        if (buttonImage != null)
        {
            buttonImage.color = rewardClaimed
                ? new Color(0.42f, 0.46f, 0.5f, 0.72f)
                : canClaimReward
                    ? new Color(0.3f, 0.75f, 0.46f, 1f)
                    : new Color(0.93f, 0.73f, 0.24f, 1f);
        }
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

    void RefreshMasteryOverlay()
    {
        if (masteryOverlayPanel == null)
            return;

        PlayerProfileSnapshot profile = PlayerProgressionSystem.GetProfileSnapshot();
        ProgressionTitleSnapshot[] titles = PlayerProgressionSystem.GetTitleRoadmap(masteryTitleCardImages.Length);
        ProgressionMilestoneSnapshot[] milestones =
            PlayerProgressionSystem.GetMilestoneRoadmap(masteryMilestoneCardImages.Length);

        if (masteryOverlayTitleText != null)
            masteryOverlayTitleText.text = "Mastery Roadmap";

        if (masteryOverlayStatusText != null)
        {
            masteryOverlayStatusText.text =
                "Current: " +
                profile.RankTitle +
                " Lv " +
                profile.Level +
                "   |   XP " +
                profile.ExperienceIntoLevel +
                "/" +
                profile.ExperienceForNextLevel +
                "\n" +
                PlayerProgressionSystem.GetPrimaryGoalSummary();
        }

        for (int i = 0; i < masteryTitleCardImages.Length; i++)
        {
            if (masteryTitleCardImages[i] == null)
                continue;

            bool hasSnapshot = i < titles.Length;
            masteryTitleCardImages[i].gameObject.SetActive(hasSnapshot);

            if (!hasSnapshot)
                continue;

            ProgressionTitleSnapshot snapshot = titles[i];
            masteryTitleCardImages[i].color = snapshot.IsCurrent
                ? new Color(0.44f, 0.34f, 0.18f, 1f)
                : snapshot.IsUnlocked
                    ? new Color(0.23f, 0.38f, 0.28f, 1f)
                    : new Color(0.2f, 0.29f, 0.44f, 1f);

            if (masteryTitleCardTitleTexts[i] != null)
                masteryTitleCardTitleTexts[i].text = snapshot.Title;

            if (masteryTitleCardRequirementTexts[i] != null)
                masteryTitleCardRequirementTexts[i].text = snapshot.RequirementLabel;

            if (masteryTitleCardProgressTexts[i] != null)
            {
                masteryTitleCardProgressTexts[i].text = snapshot.StatusLabel + "  |  " + snapshot.ProgressLabel;
                masteryTitleCardProgressTexts[i].color = snapshot.IsCurrent
                    ? new Color(1f, 0.94f, 0.72f, 1f)
                    : snapshot.IsUnlocked
                        ? new Color(0.82f, 0.98f, 0.84f, 1f)
                        : new Color(0.84f, 0.92f, 1f, 1f);
            }
        }

        for (int i = 0; i < masteryMilestoneCardImages.Length; i++)
        {
            if (masteryMilestoneCardImages[i] == null)
                continue;

            bool hasSnapshot = i < milestones.Length;
            masteryMilestoneCardImages[i].gameObject.SetActive(hasSnapshot);

            if (!hasSnapshot)
                continue;

            ProgressionMilestoneSnapshot snapshot = milestones[i];
            Color accentColor = GetMasteryAccentColor(snapshot.StatType);
            masteryMilestoneCardImages[i].color = snapshot.IsClaimed
                ? new Color(0.23f, 0.34f, 0.27f, 1f)
                : snapshot.IsReadyToClaim
                    ? new Color(0.33f, 0.29f, 0.16f, 1f)
                    : Color.Lerp(new Color(0.14f, 0.2f, 0.31f, 1f), accentColor, 0.28f);

            if (masteryMilestoneTitleTexts[i] != null)
                masteryMilestoneTitleTexts[i].text = snapshot.Title;

            if (masteryMilestoneRequirementTexts[i] != null)
                masteryMilestoneRequirementTexts[i].text = snapshot.RequirementLabel;

            if (masteryMilestoneProgressTexts[i] != null)
                masteryMilestoneProgressTexts[i].text = snapshot.ProgressLabel;

            if (masteryMilestoneRewardTexts[i] != null)
                masteryMilestoneRewardTexts[i].text = snapshot.RewardLabel;

            if (masteryMilestoneProgressFills[i] != null)
            {
                masteryMilestoneProgressFills[i].color = snapshot.IsClaimed
                    ? new Color(0.4f, 0.86f, 0.58f, 1f)
                    : accentColor;
                masteryMilestoneProgressFills[i].fillAmount = snapshot.IsClaimed
                    ? 1f
                    : Mathf.Clamp01(snapshot.Completion01);
            }
        }

        if (masteryOverlayFooterText != null)
        {
            masteryOverlayFooterText.text =
                "Lifetime: " +
                profile.CompletedRuns +
                " runs   |   " +
                profile.LifetimeCoinsCollected +
                " coins   |   " +
                profile.LifetimeNearMisses +
                " near misses" +
                "\nChallenges " +
                profile.DailyChallengesCleared +
                "   |   Shares " +
                profile.LifetimeShares +
                "   |   Peak x" +
                profile.BestDangerCombo;
        }

        if (closeMasteryButtonText != null)
            closeMasteryButtonText.text = "Close";
    }

    void SetMissionOverlayVisible(bool isVisible)
    {
        if (missionOverlayRoot != null)
        {
            missionOverlayRoot.SetActive(isVisible);
            if (isVisible)
                missionOverlayRoot.transform.SetAsLastSibling();
        }
    }

    void SetMasteryOverlayVisible(bool isVisible)
    {
        if (masteryOverlayRoot != null)
        {
            masteryOverlayRoot.SetActive(isVisible);
            if (isVisible)
                masteryOverlayRoot.transform.SetAsLastSibling();
        }
    }

    Color GetMasteryAccentColor(ProgressionMilestoneStat statType)
    {
        switch (statType)
        {
            case ProgressionMilestoneStat.BestScore:
                return new Color(0.97f, 0.72f, 0.3f, 1f);
            case ProgressionMilestoneStat.LifetimeCoinsCollected:
                return new Color(0.98f, 0.88f, 0.4f, 1f);
            case ProgressionMilestoneStat.CompletedRuns:
                return new Color(0.44f, 0.83f, 0.55f, 1f);
            case ProgressionMilestoneStat.NearMisses:
                return new Color(0.5f, 0.76f, 0.96f, 1f);
            case ProgressionMilestoneStat.DailyChallengeClears:
                return new Color(0.78f, 0.62f, 0.98f, 1f);
            case ProgressionMilestoneStat.LifetimeShares:
                return new Color(0.42f, 0.9f, 0.82f, 1f);
            case ProgressionMilestoneStat.BestDangerCombo:
                return new Color(1f, 0.58f, 0.42f, 1f);
            default:
                return new Color(0.42f, 0.78f, 0.9f, 1f);
        }
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
        NormalizeMenuRoot();
        NormalizeHeaderLayout();
        NormalizeMasteryRoadmapButton();
        NormalizeQaModeButton();
        NormalizeDailyRewardPanel();
        NormalizeChallengeSummaryPanel();
        NormalizeMissionSummaryPanel();
        NormalizeMissionOverlayPanel();
        NormalizeMasteryOverlayPanel();
        NormalizeQaOverlayPanel();
        NormalizeButtons();
    }

    void NormalizeMenuRoot()
    {
        if (menuRootRect == null)
            menuRootRect = GetMenuRoot();

        Canvas canvas = FindAnyObjectByType<Canvas>();
        SafeAreaUtility.NormalizeCanvas(canvas);
        SafeAreaUtility.ApplySafeArea(menuRootRect);

        RuntimeCaveTheme theme = CaveThemeLibrary.GetMenuTheme();
        MenuBackdropUtility.EnsureBackdrop(menuRootRect, theme, "MainMenuBackdrop");
        Camera mainCamera = Camera.main;

        if (mainCamera != null)
            mainCamera.backgroundColor = Color.Lerp(theme.BackgroundBottom, theme.FogColor, 0.24f);

        Image rootImage = menuRootRect != null ? menuRootRect.GetComponent<Image>() : null;

        if (rootImage != null)
            rootImage.color = new Color(0.02f, 0.03f, 0.05f, 0.08f);
    }

    void NormalizeHeaderLayout()
    {
        float contentWidth = SafeAreaUtility.GetContentWidth(menuRootRect, 760f, 72f);

        if (titleText == null)
            titleText = FindText(titleObjectName);

        if (titleText != null)
        {
            if (titleRect == null)
                titleRect = titleText.rectTransform;

            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(contentWidth, 240f);
            titleRect.anchoredPosition = new Vector2(0f, -44f);

            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 52f;
            titleText.fontSizeMax = 100f;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.lineSpacing = -10f;
            titleText.color = new Color(0.94f, 0.97f, 0.99f, 1f);
        }

        if (profileStatsText != null)
        {
            RectTransform statsRect = profileStatsText.rectTransform;
            statsRect.anchorMin = new Vector2(0.5f, 1f);
            statsRect.anchorMax = new Vector2(0.5f, 1f);
            statsRect.pivot = new Vector2(0.5f, 1f);
            statsRect.sizeDelta = new Vector2(contentWidth, 156f);
            statsRect.anchoredPosition = new Vector2(0f, -304f);
            profileStatsText.fontSizeMin = 16f;
            profileStatsText.fontSizeMax = 24f;
            profileStatsText.lineSpacing = 8f;
            profileStatsText.color = new Color(0.8f, 0.88f, 0.94f, 1f);
        }
    }

    void NormalizeMasteryRoadmapButton()
    {
        if (masteryRoadmapButton == null)
            return;

        RuntimeCaveTheme theme = CaveThemeLibrary.GetMenuTheme();
        float contentWidth = SafeAreaUtility.GetContentWidth(menuRootRect, 760f, 72f);
        RectTransform buttonRect = masteryRoadmapButton.GetComponent<RectTransform>();

        if (buttonRect != null)
        {
            buttonRect.anchorMin = new Vector2(0.5f, 1f);
            buttonRect.anchorMax = new Vector2(0.5f, 1f);
            buttonRect.pivot = new Vector2(0.5f, 1f);
            buttonRect.sizeDelta = new Vector2(Mathf.Min(430f, contentWidth), 58f);
            buttonRect.anchoredPosition = new Vector2(0f, -472f);
        }

        if (masteryRoadmapButtonText != null)
        {
            masteryRoadmapButtonText.enableAutoSizing = true;
            masteryRoadmapButtonText.fontSizeMin = 16f;
            masteryRoadmapButtonText.fontSizeMax = 24f;
            masteryRoadmapButtonText.alignment = TextAlignmentOptions.Center;
            masteryRoadmapButtonText.lineSpacing = -4f;
            masteryRoadmapButtonText.color = new Color(0.95f, 0.98f, 1f, 1f);
        }

        Image buttonImage = masteryRoadmapButton.GetComponent<Image>();

        if (buttonImage != null)
            buttonImage.color = Color.Lerp(theme.WallColor, theme.AccentColor, 0.54f);
    }

    void NormalizeQaModeButton()
    {
        if (qaModeButton == null)
            return;

        RuntimeCaveTheme theme = CaveThemeLibrary.GetMenuTheme();
        RectTransform buttonRect = qaModeButton.GetComponent<RectTransform>();

        if (buttonRect != null)
        {
            buttonRect.anchorMin = new Vector2(0f, 0f);
            buttonRect.anchorMax = new Vector2(0f, 0f);
            buttonRect.pivot = new Vector2(0f, 0f);
            buttonRect.sizeDelta = new Vector2(274f, 76f);
            buttonRect.anchoredPosition = new Vector2(18f, 18f);
        }

        if (qaModeButtonText != null)
        {
            qaModeButtonText.enableAutoSizing = true;
            qaModeButtonText.fontSizeMin = 16f;
            qaModeButtonText.fontSizeMax = 24f;
            qaModeButtonText.alignment = TextAlignmentOptions.Center;
            qaModeButtonText.lineSpacing = -4f;
            qaModeButtonText.color = new Color(0.95f, 0.98f, 1f, 1f);
        }

        Image buttonImage = qaModeButton.GetComponent<Image>();

        if (buttonImage != null && !QaTestingSystem.IsQaModeEnabled())
            buttonImage.color = Color.Lerp(theme.WallColor, theme.FogColor, 0.4f);
    }

    void NormalizeDailyRewardPanel()
    {
        if (dailyRewardPanel == null)
            return;

        RectTransform panelRect = dailyRewardPanel.GetComponent<RectTransform>();

        if (panelRect == null)
            return;

        float panelWidth = SafeAreaUtility.GetContentWidth(menuRootRect, 760f, 72f);
        float textWidth = panelWidth - 72f;
        RuntimeCaveTheme theme = CaveThemeLibrary.GetMenuTheme();

        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.sizeDelta = new Vector2(panelWidth, 292f);
        panelRect.anchoredPosition = new Vector2(0f, -532f);

        Image panelImage = dailyRewardPanel.GetComponent<Image>();

        if (panelImage != null)
            panelImage.color = Color.Lerp(theme.WallColor, theme.AccentColor, 0.26f);

        if (dailyRewardTitleText != null)
        {
            RectTransform titleRect = dailyRewardTitleText.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(textWidth, 42f);
            titleRect.anchoredPosition = new Vector2(0f, -22f);
            dailyRewardTitleText.fontSizeMin = 24f;
            dailyRewardTitleText.fontSizeMax = 34f;
        }

        if (dailyRewardBodyText != null)
        {
            RectTransform bodyRect = dailyRewardBodyText.rectTransform;
            bodyRect.anchorMin = new Vector2(0.5f, 1f);
            bodyRect.anchorMax = new Vector2(0.5f, 1f);
            bodyRect.pivot = new Vector2(0.5f, 1f);
            bodyRect.sizeDelta = new Vector2(textWidth, 82f);
            bodyRect.anchoredPosition = new Vector2(0f, -84f);
            dailyRewardBodyText.lineSpacing = 4f;
            dailyRewardBodyText.fontSizeMin = 18f;
            dailyRewardBodyText.fontSizeMax = 28f;
        }

        if (dailyRewardStatusText != null)
        {
            RectTransform statusRect = dailyRewardStatusText.rectTransform;
            statusRect.anchorMin = new Vector2(0.5f, 1f);
            statusRect.anchorMax = new Vector2(0.5f, 1f);
            statusRect.pivot = new Vector2(0.5f, 1f);
            statusRect.sizeDelta = new Vector2(textWidth, 28f);
            statusRect.anchoredPosition = new Vector2(0f, -176f);
            dailyRewardStatusText.fontSizeMin = 18f;
            dailyRewardStatusText.fontSizeMax = 24f;
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

        if (claimRewardButtonText != null)
        {
            claimRewardButtonText.fontSizeMin = 20f;
            claimRewardButtonText.fontSizeMax = 28f;
        }
    }

    void NormalizeMissionSummaryPanel()
    {
        if (missionSummaryPanel == null)
            return;

        RectTransform panelRect = missionSummaryPanel.GetComponent<RectTransform>();

        if (panelRect == null)
            return;

        float panelWidth = SafeAreaUtility.GetContentWidth(menuRootRect, 760f, 72f);
        float textWidth = panelWidth - 72f;
        RuntimeCaveTheme theme = CaveThemeLibrary.GetMenuTheme();

        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(panelWidth, 212f);
        panelRect.anchoredPosition = new Vector2(0f, 18f);

        Image panelImage = missionSummaryPanel.GetComponent<Image>();

        if (panelImage != null)
            panelImage.color = Color.Lerp(theme.WallColor, theme.FogColor, 0.18f);

        if (missionSummaryTitleText != null)
        {
            RectTransform titleRect = missionSummaryTitleText.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(textWidth, 40f);
            titleRect.anchoredPosition = new Vector2(0f, -20f);
        }

        if (missionSummaryStatusText != null)
        {
            RectTransform statusRect = missionSummaryStatusText.rectTransform;
            statusRect.anchorMin = new Vector2(0.5f, 1f);
            statusRect.anchorMax = new Vector2(0.5f, 1f);
            statusRect.pivot = new Vector2(0.5f, 1f);
            statusRect.sizeDelta = new Vector2(textWidth, 36f);
            statusRect.anchoredPosition = new Vector2(0f, -78f);
            missionSummaryStatusText.fontSizeMin = 20f;
            missionSummaryStatusText.fontSizeMax = 28f;
            missionSummaryStatusText.lineSpacing = 2f;
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

    void NormalizeChallengeSummaryPanel()
    {
        if (challengeSummaryPanel == null)
            return;

        RectTransform panelRect = challengeSummaryPanel.GetComponent<RectTransform>();

        if (panelRect == null)
            return;

        float panelWidth = SafeAreaUtility.GetContentWidth(menuRootRect, 760f, 72f);
        float textWidth = panelWidth - 60f;
        RuntimeCaveTheme theme = CaveThemeLibrary.GetMenuTheme();

        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(panelWidth, 344f);
        panelRect.anchoredPosition = new Vector2(0f, 244f);

        Image panelImage = challengeSummaryPanel.GetComponent<Image>();

        if (panelImage != null)
            panelImage.color = Color.Lerp(theme.WallColor, theme.AccentColor, 0.22f);

        if (challengeSummaryTitleText != null)
        {
            RectTransform titleRect = challengeSummaryTitleText.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(textWidth, 46f);
            titleRect.anchoredPosition = new Vector2(0f, -20f);
            challengeSummaryTitleText.fontSizeMin = 26f;
            challengeSummaryTitleText.fontSizeMax = 36f;
        }

        if (challengeSummaryBodyText != null)
        {
            RectTransform bodyRect = challengeSummaryBodyText.rectTransform;
            bodyRect.anchorMin = new Vector2(0.5f, 1f);
            bodyRect.anchorMax = new Vector2(0.5f, 1f);
            bodyRect.pivot = new Vector2(0.5f, 1f);
            bodyRect.sizeDelta = new Vector2(textWidth, 52f);
            bodyRect.anchoredPosition = new Vector2(0f, -84f);
            challengeSummaryBodyText.fontSizeMin = 24f;
            challengeSummaryBodyText.fontSizeMax = 34f;
            challengeSummaryBodyText.lineSpacing = 2f;
        }

        if (challengeSummaryStatusText != null)
        {
            RectTransform statusRect = challengeSummaryStatusText.rectTransform;
            statusRect.anchorMin = new Vector2(0.5f, 1f);
            statusRect.anchorMax = new Vector2(0.5f, 1f);
            statusRect.pivot = new Vector2(0.5f, 1f);
            statusRect.sizeDelta = new Vector2(textWidth, 50f);
            statusRect.anchoredPosition = new Vector2(0f, -146f);
            challengeSummaryStatusText.fontSizeMin = 20f;
            challengeSummaryStatusText.fontSizeMax = 28f;
            challengeSummaryStatusText.lineSpacing = 2f;
        }

        if (challengeSummaryActionButton != null)
        {
            RectTransform buttonRect = challengeSummaryActionButton.GetComponent<RectTransform>();

            if (buttonRect != null)
            {
                buttonRect.sizeDelta = new Vector2(440f, 62f);
                buttonRect.anchoredPosition = new Vector2(0f, 22f);
            }
        }

        if (challengeSummaryActionButtonText != null)
        {
            challengeSummaryActionButtonText.fontSizeMin = 24f;
            challengeSummaryActionButtonText.fontSizeMax = 34f;
        }
    }

    void NormalizeMissionOverlayPanel()
    {
        if (missionOverlayRoot == null || missionOverlayPanel == null)
            return;

        RuntimeCaveTheme theme = CaveThemeLibrary.GetMenuTheme();

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

        Image rootImage = missionOverlayRoot.GetComponent<Image>();

        if (rootImage != null)
            rootImage.color = new Color(0.01f, 0.02f, 0.04f, 0.88f);

        Image panelImage = missionOverlayPanel.GetComponent<Image>();

        if (panelImage != null)
            panelImage.color = Color.Lerp(theme.WallColor, theme.AccentColor, 0.24f);

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

    void NormalizeMasteryOverlayPanel()
    {
        if (masteryOverlayRoot == null || masteryOverlayPanel == null)
            return;

        RuntimeCaveTheme theme = CaveThemeLibrary.GetMenuTheme();

        RectTransform rootRect = masteryOverlayRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        RectTransform panelRect = masteryOverlayPanel.GetComponent<RectTransform>();
        float safeWidth = Screen.safeArea.width > 0f ? Screen.safeArea.width : Screen.width;
        float safeHeight = Screen.safeArea.height > 0f ? Screen.safeArea.height : Screen.height;
        float panelWidth = Mathf.Clamp(safeWidth - 56f, 760f, 920f);
        float panelHeight = Mathf.Clamp(safeHeight - 120f, 960f, 1120f);
        float innerWidth = panelWidth - 96f;
        float titleGap = 16f;
        float titleCardWidth = (panelWidth - 112f - (titleGap * 2f)) / 3f;
        float titleGroupWidth = (titleCardWidth * 3f) + (titleGap * 2f);
        float titleStartX = -((titleGroupWidth - titleCardWidth) * 0.5f);
        float milestoneWidth = panelWidth - 88f;

        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
        panelRect.anchoredPosition = new Vector2(0f, -20f);

        Image rootImage = masteryOverlayRoot.GetComponent<Image>();

        if (rootImage != null)
            rootImage.color = new Color(0.01f, 0.02f, 0.04f, 0.88f);

        Image panelImage = masteryOverlayPanel.GetComponent<Image>();

        if (panelImage != null)
            panelImage.color = Color.Lerp(theme.WallColor, theme.AccentColor, 0.24f);

        if (masteryOverlayTitleText != null)
        {
            RectTransform titleRect = masteryOverlayTitleText.rectTransform;
            titleRect.sizeDelta = new Vector2(innerWidth, 54f);
            titleRect.anchoredPosition = new Vector2(0f, -28f);
        }

        if (masteryOverlayStatusText != null)
        {
            RectTransform statusRect = masteryOverlayStatusText.rectTransform;
            statusRect.sizeDelta = new Vector2(innerWidth, 82f);
            statusRect.anchoredPosition = new Vector2(0f, -96f);
        }

        for (int i = 0; i < masteryTitleCardImages.Length; i++)
        {
            if (masteryTitleCardImages[i] == null)
                continue;

            RectTransform cardRect = masteryTitleCardImages[i].rectTransform;
            cardRect.sizeDelta = new Vector2(titleCardWidth, 138f);
            cardRect.anchoredPosition = new Vector2(titleStartX + (i * (titleCardWidth + titleGap)), -164f);

            if (masteryTitleCardTitleTexts[i] != null)
            {
                RectTransform titleRect = masteryTitleCardTitleTexts[i].rectTransform;
                titleRect.sizeDelta = new Vector2(titleCardWidth - 28f, 40f);
                titleRect.anchoredPosition = new Vector2(0f, -16f);
            }

            if (masteryTitleCardRequirementTexts[i] != null)
            {
                RectTransform requirementRect = masteryTitleCardRequirementTexts[i].rectTransform;
                requirementRect.sizeDelta = new Vector2(titleCardWidth - 30f, 36f);
                requirementRect.anchoredPosition = new Vector2(0f, -62f);
            }

            if (masteryTitleCardProgressTexts[i] != null)
            {
                RectTransform progressRect = masteryTitleCardProgressTexts[i].rectTransform;
                progressRect.sizeDelta = new Vector2(titleCardWidth - 28f, 30f);
                progressRect.anchoredPosition = new Vector2(0f, 16f);
            }
        }

        for (int i = 0; i < masteryMilestoneCardImages.Length; i++)
        {
            if (masteryMilestoneCardImages[i] == null)
                continue;

            RectTransform cardRect = masteryMilestoneCardImages[i].rectTransform;
            cardRect.sizeDelta = new Vector2(milestoneWidth, 132f);
            cardRect.anchoredPosition = new Vector2(0f, -342f - (i * 140f));

            if (masteryMilestoneTitleTexts[i] != null)
            {
                RectTransform titleRect = masteryMilestoneTitleTexts[i].rectTransform;
                titleRect.sizeDelta = new Vector2(milestoneWidth - 300f, 30f);
                titleRect.anchoredPosition = new Vector2(26f, -14f);
            }

            if (masteryMilestoneRequirementTexts[i] != null)
            {
                RectTransform requirementRect = masteryMilestoneRequirementTexts[i].rectTransform;
                requirementRect.sizeDelta = new Vector2(milestoneWidth - 52f, 24f);
                requirementRect.anchoredPosition = new Vector2(26f, -42f);
            }

            if (masteryMilestoneProgressTexts[i] != null)
            {
                RectTransform progressRect = masteryMilestoneProgressTexts[i].rectTransform;
                progressRect.sizeDelta = new Vector2(320f, 22f);
                progressRect.anchoredPosition = new Vector2(26f, 8f);
            }

            if (masteryMilestoneRewardTexts[i] != null)
            {
                RectTransform rewardRect = masteryMilestoneRewardTexts[i].rectTransform;
                rewardRect.sizeDelta = new Vector2(320f, 22f);
                rewardRect.anchoredPosition = new Vector2(-26f, 8f);
            }

            Transform trackTransform = masteryMilestoneCardImages[i].transform.Find("ProgressTrack");

            if (trackTransform != null)
            {
                RectTransform trackRect = trackTransform as RectTransform;

                if (trackRect != null)
                {
                    trackRect.sizeDelta = new Vector2(milestoneWidth - 52f, 14f);
                    trackRect.anchoredPosition = new Vector2(0f, 38f);
                }
            }
        }

        if (masteryOverlayFooterText != null)
        {
            RectTransform footerRect = masteryOverlayFooterText.rectTransform;
            footerRect.sizeDelta = new Vector2(innerWidth, 82f);
            footerRect.anchoredPosition = new Vector2(0f, 130f);
        }

        if (closeMasteryButton != null)
        {
            RectTransform buttonRect = closeMasteryButton.GetComponent<RectTransform>();

            if (buttonRect != null)
            {
                buttonRect.sizeDelta = new Vector2(260f, 68f);
                buttonRect.anchoredPosition = new Vector2(0f, 34f);
            }

            Image buttonImage = closeMasteryButton.GetComponent<Image>();

            if (buttonImage != null)
                buttonImage.color = Color.Lerp(theme.WallColor, theme.FogColor, 0.52f);
        }
    }

    void NormalizeQaOverlayPanel()
    {
        if (qaOverlayRoot == null || qaOverlayPanel == null)
            return;

        RuntimeCaveTheme theme = CaveThemeLibrary.GetMenuTheme();
        RectTransform rootRect = qaOverlayRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        RectTransform panelRect = qaOverlayPanel.GetComponent<RectTransform>();
        float safeWidth = Screen.safeArea.width > 0f ? Screen.safeArea.width : Screen.width;
        float safeHeight = Screen.safeArea.height > 0f ? Screen.safeArea.height : Screen.height;
        float panelWidth = Mathf.Clamp(safeWidth - 56f, 780f, 940f);
        float panelHeight = Mathf.Clamp(safeHeight - 120f, 900f, 1080f);
        float textWidth = panelWidth - 120f;

        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
        panelRect.anchoredPosition = new Vector2(0f, -20f);

        Image rootImage = qaOverlayRoot.GetComponent<Image>();

        if (rootImage != null)
            rootImage.color = new Color(0.01f, 0.02f, 0.04f, 0.88f);

        Image panelImage = qaOverlayPanel.GetComponent<Image>();

        if (panelImage != null)
            panelImage.color = Color.Lerp(theme.WallColor, theme.AccentColor, 0.22f);

        if (qaOverlayTitleText != null)
        {
            RectTransform titleRect = qaOverlayTitleText.rectTransform;
            titleRect.sizeDelta = new Vector2(textWidth, 64f);
            titleRect.anchoredPosition = new Vector2(0f, -28f);
        }

        if (qaOverlayBodyText != null)
        {
            RectTransform bodyRect = qaOverlayBodyText.rectTransform;
            bodyRect.sizeDelta = new Vector2(textWidth, panelHeight - 650f);
            bodyRect.anchoredPosition = new Vector2(0f, -112f);
            qaOverlayBodyText.fontSizeMin = 15f;
            qaOverlayBodyText.fontSizeMax = 20f;
            qaOverlayBodyText.lineSpacing = 2f;
        }

        if (qaOverlayStatusText != null)
        {
            RectTransform statusRect = qaOverlayStatusText.rectTransform;
            statusRect.sizeDelta = new Vector2(textWidth, 120f);
            statusRect.anchoredPosition = new Vector2(0f, -(panelHeight - 390f));
            qaOverlayStatusText.fontSizeMin = 16f;
            qaOverlayStatusText.fontSizeMax = 22f;
            qaOverlayStatusText.lineSpacing = 2f;
        }

        NormalizeOverlayButton(qaOverlayToggleButton, qaOverlayToggleButtonText, new Vector2(430f, 74f), new Vector2(0f, 262f));
        NormalizeOverlayButton(qaOverlayPracticeButton, qaOverlayPracticeButtonText, new Vector2(430f, 74f), new Vector2(0f, 178f));
        NormalizeOverlayButton(qaOverlayClearButton, qaOverlayClearButtonText, new Vector2(430f, 74f), new Vector2(0f, 94f));
        NormalizeOverlayButton(qaOverlayCloseButton, qaOverlayCloseButtonText, new Vector2(230f, 64f), new Vector2(0f, 18f));
    }

    void NormalizeOverlayButton(Button button, TextMeshProUGUI label, Vector2 size, Vector2 anchoredPosition)
    {
        if (button == null)
            return;

        RectTransform buttonRect = button.GetComponent<RectTransform>();

        if (buttonRect != null)
        {
            buttonRect.anchorMin = new Vector2(0.5f, 0f);
            buttonRect.anchorMax = new Vector2(0.5f, 0f);
            buttonRect.pivot = new Vector2(0.5f, 0f);
            buttonRect.sizeDelta = size;
            buttonRect.anchoredPosition = anchoredPosition;
        }

        if (label != null)
        {
            label.enableAutoSizing = true;
            label.fontSizeMin = 18f;
            label.fontSizeMax = 28f;
            label.color = new Color(0.14f, 0.18f, 0.22f, 1f);
        }
    }

    void NormalizeButtons()
    {
        SetButtonLayout(playButtonObjectName, 202f);
        SetButtonLayout(shopButtonObjectName, 60f);
        SetButtonLayout(inventoryButtonObjectName, -82f);
        SetButtonLayout(exitButtonObjectName, -224f);
    }

    void SetButtonLayout(string objectName, float anchoredY)
    {
        Button button = FindButton(objectName);

        if (button == null)
            return;

        RectTransform buttonRect = button.GetComponent<RectTransform>();

        if (buttonRect != null)
        {
            Vector2 anchoredPosition = buttonRect.anchoredPosition;
            buttonRect.anchoredPosition = new Vector2(anchoredPosition.x, anchoredY);
        }

        Image buttonImage = button.GetComponent<Image>();
        RuntimeCaveTheme theme = CaveThemeLibrary.GetMenuTheme();

        if (buttonImage != null)
            buttonImage.color = Color.Lerp(theme.WallColor, theme.AccentColor, 0.46f);

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);

        if (label != null)
        {
            label.enableAutoSizing = true;
            label.fontSizeMin = 24;
            label.fontSizeMax = 34;
            label.color = new Color(0.96f, 0.98f, 1f, 1f);
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

    void SetQaOverlayVisible(bool isVisible)
    {
        if (qaOverlayRoot != null)
            qaOverlayRoot.SetActive(isVisible);
    }
}

public static class SafeAreaUtility
{
    private static readonly Vector2 DefaultReferenceResolution = new Vector2(1080f, 1920f);

    public static void NormalizeCanvas(Canvas canvas, float matchWidthOrHeight = 0.5f)
    {
        if (canvas == null)
            return;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        if (canvasRect != null)
        {
            canvasRect.localScale = Vector3.one;
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.zero;
            canvasRect.sizeDelta = Vector2.zero;
        }

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();

        if (scaler != null)
        {
            scaler.referenceResolution = DefaultReferenceResolution;
            scaler.matchWidthOrHeight = matchWidthOrHeight;
        }
    }

    public static void ApplySafeArea(RectTransform targetRect)
    {
        if (targetRect == null || Screen.width <= 0 || Screen.height <= 0)
            return;

        Rect safeArea = Screen.safeArea;
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        targetRect.localScale = Vector3.one;
        targetRect.anchorMin = anchorMin;
        targetRect.anchorMax = anchorMax;
        targetRect.offsetMin = Vector2.zero;
        targetRect.offsetMax = Vector2.zero;
        targetRect.anchoredPosition = Vector2.zero;
        targetRect.sizeDelta = Vector2.zero;
    }

    public static float GetContentWidth(RectTransform rootRect, float maxWidth, float horizontalPadding)
    {
        if (rootRect == null)
            return maxWidth;

        float availableWidth = Mathf.Max(320f, rootRect.rect.width - (horizontalPadding * 2f));
        return Mathf.Min(availableWidth, maxWidth);
    }
}

public static class MenuBackdropUtility
{
    private const int BackdropWidth = 512;
    private const int BackdropHeight = 1024;
    private static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    public static void EnsureBackdrop(RectTransform rootRect, RuntimeCaveTheme theme, string objectName)
    {
        if (rootRect == null)
            return;

        Transform existing = rootRect.Find(objectName);
        GameObject backdropObject;

        if (existing != null)
        {
            backdropObject = existing.gameObject;
        }
        else
        {
            backdropObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            backdropObject.transform.SetParent(rootRect, false);
            backdropObject.transform.SetAsFirstSibling();
        }

        RectTransform backdropRect = backdropObject.GetComponent<RectTransform>();
        backdropRect.anchorMin = Vector2.zero;
        backdropRect.anchorMax = Vector2.one;
        backdropRect.offsetMin = Vector2.zero;
        backdropRect.offsetMax = Vector2.zero;
        backdropRect.localScale = Vector3.one;

        Image backdropImage = backdropObject.GetComponent<Image>();
        backdropImage.raycastTarget = false;
        backdropImage.preserveAspect = false;
        backdropImage.color = Color.white;
        backdropImage.sprite = GetOrCreateBackdropSprite(theme);
    }

    private static Sprite GetOrCreateBackdropSprite(RuntimeCaveTheme theme)
    {
        string key = "MenuBackdrop_" + theme.BiomeIndex;

        if (spriteCache.TryGetValue(key, out Sprite cachedSprite) && cachedSprite != null)
            return cachedSprite;

        Texture2D texture = new Texture2D(BackdropWidth, BackdropHeight, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[BackdropWidth * BackdropHeight];
        float seed = (theme.Level * 71.19f) + (theme.BiomeIndex * 113.7f);
        Color upperGlow = Color.Lerp(theme.BackgroundTop, theme.FogColor, 0.18f);
        Color lowerGlow = Color.Lerp(theme.BackgroundBottom, theme.WallColor, 0.28f);
        Color tunnelColor = Color.Lerp(theme.BackgroundBottom, theme.FogColor, 0.1f);

        for (int y = 0; y < BackdropHeight; y++)
        {
            float y01 = y / (BackdropHeight - 1f);
            float tunnelCenter = 0.5f + (Mathf.PerlinNoise(seed * 0.012f, y01 * 1.3f + seed * 0.003f) - 0.5f) * 0.08f;
            float tunnelHalfWidth = 0.2f + Mathf.Sin((y01 * 2.1f) + seed * 0.04f) * 0.04f;
            tunnelHalfWidth += (Mathf.PerlinNoise(seed * 0.021f, y01 * 3.6f + seed * 0.008f) - 0.5f) * 0.1f;
            tunnelHalfWidth = Mathf.Clamp(tunnelHalfWidth, 0.15f, 0.32f);

            float topSpikeDepth = 0.08f + Mathf.PerlinNoise(seed * 0.032f, y01 * 0.2f) * 0.06f;
            float bottomSpikeDepth = 0.07f + Mathf.PerlinNoise(seed * 0.041f, y01 * 0.24f) * 0.05f;

            for (int x = 0; x < BackdropWidth; x++)
            {
                float x01 = x / (BackdropWidth - 1f);
                float xDistance = Mathf.Abs(x01 - tunnelCenter);
                float caveNoise = (Mathf.PerlinNoise((x + seed * 2.4f) * 0.018f, (y + seed * 1.7f) * 0.014f) - 0.5f) * 0.08f;
                bool inWall = xDistance > tunnelHalfWidth + caveNoise;
                inWall |= y01 > 1f - topSpikeDepth - Mathf.Abs(Mathf.Sin((x01 * 7.8f) + seed * 0.03f)) * 0.12f;
                inWall |= y01 < bottomSpikeDepth + Mathf.Abs(Mathf.Sin((x01 * 9.1f) + seed * 0.06f)) * 0.11f;

                Color pixel = Color.Lerp(lowerGlow, upperGlow, Mathf.Pow(y01, 1.08f));

                if (inWall)
                {
                    float rockNoise = Mathf.PerlinNoise((x + seed * 0.9f) * 0.046f, (y + seed * 0.5f) * 0.038f);
                    float edgeHighlight = Mathf.Clamp01((xDistance - tunnelHalfWidth) / 0.035f);
                    pixel = Color.Lerp(theme.WallColor, theme.AccentColor, rockNoise * 0.08f + edgeHighlight * 0.16f);
                    pixel = Color.Lerp(pixel, theme.CrystalColor, Mathf.SmoothStep(0.9f, 1f, rockNoise) * 0.12f);
                }
                else
                {
                    float fogNoise = Mathf.PerlinNoise((x + seed * 3.1f) * 0.028f, (y + seed * 2.2f) * 0.022f);
                    float shaft = Mathf.Exp(-Mathf.Pow((x01 - tunnelCenter) * 4.6f, 2f)) * Mathf.SmoothStep(0.25f, 0.95f, y01) * 0.18f;
                    pixel = Color.Lerp(pixel, tunnelColor, 0.36f);
                    pixel = Color.Lerp(pixel, theme.FogColor, fogNoise * 0.03f + shaft);
                }

                pixel.a = 1f;
                pixels[y * BackdropWidth + x] = pixel;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, BackdropWidth, BackdropHeight),
            new Vector2(0.5f, 0.5f),
            100f);
        sprite.name = key;
        spriteCache[key] = sprite;
        return sprite;
    }
}
