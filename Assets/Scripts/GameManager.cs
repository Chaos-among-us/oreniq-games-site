using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI totalCoinsText;
    public TextMeshProUGUI upgradeText;
    public TextMeshProUGUI levelText;

    public GameObject gameOverText;
    public GameObject restartButton;
    public Spawner spawner;
    public PlayerController player;

    private float score = 0f;
    private int totalCoins = 0;
    private int runCoinsEarned = 0;
    private int bestScore = 0;
    private bool gameEnded = false;
    private bool newBestScore = false;

    private float runTime = 0f;
    public float difficultyStepTime = 15f;

    private int currentLevel = 1;
    public int activeShields = 0;
    public int armedExtraLives = 0;
    private bool isDailyChallengeRun = false;
    private DailyChallengeData activeDailyChallenge;

    private readonly List<UpgradeType> equippedUpgrades = new List<UpgradeType>();
    private readonly Dictionary<UpgradeType, float> activeUpgradeEndTimes = new Dictionary<UpgradeType, float>();
    private readonly Dictionary<UpgradeType, RunUpgradeButtonUI> runUpgradeButtons = new Dictionary<UpgradeType, RunUpgradeButtonUI>();
    private readonly HashSet<UpgradeType> autoEnabledUpgrades = new HashSet<UpgradeType>();

    private RectTransform runUpgradePanel;
    private TMP_FontAsset runtimeFont;
    private CaveBackgroundController caveBackgroundController;
    private EndlessDodgeAudioDirector audioDirector;
    private PlayerPowerupVisuals playerPowerupVisuals;
    private PlayfieldBorderController playfieldBorderController;
    private GameObject postRunPanel;
    private TextMeshProUGUI postRunSummaryText;
    private Button postRunDoubleCoinsButton;
    private TextMeshProUGUI postRunDoubleCoinsButtonText;
    private Button postRunShareButton;
    private TextMeshProUGUI postRunShareButtonText;
    private Button postRunReviewButton;
    private TextMeshProUGUI postRunReviewButtonText;
    private TextMeshProUGUI bestScoreHudText;
    private Button pauseButton;
    private TextMeshProUGUI pauseButtonText;
    private GameObject pauseOverlayRoot;
    private TextMeshProUGUI pauseOverlayTitleText;
    private TextMeshProUGUI hapticsStatusText;
    private Button hapticsToggleButton;
    private TextMeshProUGUI hapticsToggleButtonText;
    private GameObject revivePromptRoot;
    private TextMeshProUGUI revivePromptBodyText;
    private Button revivePromptAcceptButton;
    private TextMeshProUGUI revivePromptAcceptButtonText;
    private Button revivePromptDeclineButton;
    private TextMeshProUGUI revivePromptDeclineButtonText;
    private GameObject tutorialOverlayRoot;
    private TextMeshProUGUI tutorialBodyText;
    private Button tutorialPrimaryButton;
    private TextMeshProUGUI tutorialPrimaryButtonText;
    private GameObject qaCaptureOverlayRoot;
    private TextMeshProUGUI qaCaptureOverlayTitleText;
    private TextMeshProUGUI qaCaptureOverlayBodyText;
    private Button qaCaptureOverlayContinueButton;
    private TextMeshProUGUI qaCaptureOverlayContinueButtonText;
    private GameObject qaSurveyPanel;
    private TextMeshProUGUI qaSurveyStatusText;
    private Button qaFairnessButton;
    private TextMeshProUGUI qaFairnessButtonText;
    private Button qaDifficultyButton;
    private TextMeshProUGUI qaDifficultyButtonText;
    private Button qaRewardButton;
    private TextMeshProUGUI qaRewardButtonText;
    private Button qaReplayButton;
    private TextMeshProUGUI qaReplayButtonText;
    private Button qaPriceButton;
    private TextMeshProUGUI qaPriceButtonText;
    private Button qaShopValueButton;
    private TextMeshProUGUI qaShopValueButtonText;
    private Button qaAdButton;
    private TextMeshProUGUI qaAdButtonText;
    private Button qaDangerButton;
    private TextMeshProUGUI qaDangerButtonText;
    private Button qaFeatureButton;
    private TextMeshProUGUI qaFeatureButtonText;
    private TMP_InputField qaTesterNotesInput;
    private Button qaPlayAgainButton;
    private TextMeshProUGUI qaPlayAgainButtonText;
    private Button qaSavePackageButton;
    private TextMeshProUGUI qaSavePackageButtonText;
    private Button qaSharePackageButton;
    private TextMeshProUGUI qaSharePackageButtonText;
    private Button qaDeletePackageButton;
    private TextMeshProUGUI qaDeletePackageButtonText;
    private bool markTutorialSeenOnClose;
    private bool postRunDoubleCoinsClaimed;
    private bool rewardedReviveUsed;
    private bool isAwaitingRewardedRevive;
    private bool showPostRunReviewButton;
    private int completedRunsCount;
    private int lastFinishedScore;
    private int lastFinishedLevel;
    private float dangerComboDecayTimer;
    private int dangerComboCount;
    private int bestDangerComboCount;
    private int runNearMissCount;
    private PlayerProgressionRunResult lastProgressionResult;
    private string lastPerformanceRewardSummary;
    private string lastShareRewardSummary;
    private bool openingGraceShieldAvailable;
    private bool qaCaptureRequestedThisScene;
    private bool qaCaptureNoticePending;
    private bool qaPracticeRunActive;

    private const float RunUpgradeButtonWidth = 216f;
    private const float RunUpgradeButtonHeight = 66f;
    private const float ShieldProtectionDuration = 0.35f;
    private const float ExtraLifeReviveDelay = 0.2f;
    private const float ExtraLifeInvulnerabilityDuration = 1.4f;
    private const float SpeedBoostDuration = 10f;
    private const float CoinMagnetDuration = 12f;
    private const float DoubleCoinsDuration = 12f;
    private const float SlowTimeDuration = 10f;
    private const float SmallerPlayerDuration = 12f;
    private const float ScoreBoosterDuration = 12f;
    private const float RareCoinBoostDuration = 15f;
    private const float DangerComboGraceDuration = 4.25f;
    private const float DangerComboDecayStepDuration = 1.15f;
    private const float OpeningGraceShieldWindow = 11f;
    private const float OpeningGraceInvulnerabilityDuration = 1.05f;
    private const string BestScoreKey = "BestScore";

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        DailyChallengeSystem.EnsureInitializedForToday();
        DailyMissionSystem.EnsureInitializedForToday();
        isDailyChallengeRun = DailyChallengeSystem.IsDailyChallengeRunActive();

        if (isDailyChallengeRun)
            activeDailyChallenge = DailyChallengeSystem.GetTodayChallenge();

        if (gameOverText != null)
            gameOverText.SetActive(false);

        if (restartButton != null)
            restartButton.SetActive(false);

        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        postRunDoubleCoinsClaimed = false;
        rewardedReviveUsed = false;
        isAwaitingRewardedRevive = false;
        showPostRunReviewButton = false;
        completedRunsCount = GameSettings.GetCompletedRunCount();
        lastFinishedScore = 0;
        lastFinishedLevel = 1;
        dangerComboDecayTimer = 0f;
        dangerComboCount = 0;
        bestDangerComboCount = 0;
        runNearMissCount = 0;
        lastProgressionResult = default;
        lastPerformanceRewardSummary = string.Empty;
        lastShareRewardSummary = string.Empty;
        openingGraceShieldAvailable = !isDailyChallengeRun;
        qaCaptureRequestedThisScene = false;
        qaCaptureNoticePending = false;
        qaPracticeRunActive = QaTestingSystem.ConsumePracticeRunRequest();
        QaTestingSystem.EnsureRuntime();

        if (totalCoinsText != null)
            totalCoinsText.text = "Coins " + totalCoins;

        if (isDailyChallengeRun)
            UpdateUpgradeText();

        DynamicBackgroundController.EnsureExists();
        runtimeFont = GetRuntimeFont();
        EnsureRuntimeFeedbackUI();
        RefreshPauseOverlayState();
        UpdateBestScoreHud();
        SyncLoadoutFromInventory(true);
        ApplyContinuousEffects();
        UpdateUpgradeText();
        ShowTutorialIfNeeded();

        currentLevel = GetDifficultyLevel();
        UpdateLevelText();
        EnsurePresentationSystems();
        LaunchAnalytics.RecordRunStarted(isDailyChallengeRun, equippedUpgrades.Count);
    }

    void Update()
    {
        RefreshQaFeedbackUi();

        if (!qaCaptureRequestedThisScene &&
            QaTestingSystem.IsQaModeEnabled() &&
            !qaPracticeRunActive &&
            !IsPauseOverlayVisible() &&
            !IsTutorialOverlayVisible() &&
            !IsRevivePromptVisible())
        {
            qaCaptureRequestedThisScene = true;
            qaCaptureNoticePending = true;
            RefreshQaFeedbackUi();
        }

        EnsurePresentationSystems();
        SyncLoadoutFromInventory(false);

        if (gameEnded) return;

        ProcessAutoUpgrades();
        UpdateDangerComboState();
        runTime += Time.deltaTime;
        score += Time.deltaTime * GetScoreMultiplier();

        if (scoreText != null)
            scoreText.text = "Score " + Mathf.FloorToInt(score);

        if (isDailyChallengeRun)
            UpdateUpgradeText();

        UpdateBestScoreHud();

        ApplyContinuousEffects();
        RefreshRunUpgradeButtons();
        openingGraceShieldAvailable = openingGraceShieldAvailable && runTime < OpeningGraceShieldWindow;
        int newLevel = GetDifficultyLevel();

        if (newLevel != currentLevel)
        {
            currentLevel = newLevel;
            audioDirector?.PlayLevelAdvanceCue();
            UpdateLevelText();
            StartCoroutine(LevelFlash());
        }
    }

    void UpdateUpgradeText()
    {
        if (upgradeText == null)
            return;

        if (isDailyChallengeRun)
        {
            upgradeText.text =
                activeDailyChallenge.title +
                "\n" + DailyChallengeSystem.GetObjectiveLabel(activeDailyChallenge);
            return;
        }

        if (equippedUpgrades.Count == 0)
        {
            upgradeText.text = "No loadout";
            return;
        }

        List<string> lines = new List<string>();

        for (int i = 0; i < equippedUpgrades.Count; i++)
        {
            lines.Add(BuildUpgradeSummary(equippedUpgrades[i]));
        }

        upgradeText.text = string.Join("\n", lines.ToArray());
    }

    void UpdateLevelText()
    {
        if (levelText != null)
            levelText.text = "Level " + currentLevel;
    }

    IEnumerator LevelFlash()
    {
        if (levelText == null)
            yield break;

        Color originalColor = levelText.color;
        levelText.color = Color.yellow;
        yield return new WaitForSeconds(0.3f);
        levelText.color = originalColor;
    }

    public int GetDifficultyLevel()
    {
        return Mathf.FloorToInt(runTime / difficultyStepTime) + 1;
    }

    public float GetContinuousDifficultyLevel()
    {
        if (difficultyStepTime <= 0.001f)
            return Mathf.Max(1f, currentLevel);

        return 1f + Mathf.Max(0f, runTime) / difficultyStepTime;
    }

    public float GetLevelProgress01()
    {
        if (difficultyStepTime <= 0.001f)
            return 1f;

        return Mathf.Clamp01(Mathf.Repeat(runTime, difficultyStepTime) / difficultyStepTime);
    }

    public float GetLevelTransitionBlend01()
    {
        return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.24f, 0.94f, GetLevelProgress01()));
    }

    public float GetObstacleSpeedRampMultiplier()
    {
        float difficulty = Mathf.Max(0f, GetContinuousDifficultyLevel() - 1f);
        float earlyRamp = Mathf.Min(difficulty, 2.5f) * 0.08f;
        float midRamp = Mathf.Clamp(difficulty - 2.5f, 0f, 4f) * 0.06f;
        float lateRamp = Mathf.Max(0f, difficulty - 6.5f) * 0.04f;
        return 1f + earlyRamp + midRamp + lateRamp;
    }

    public void AddCoin()
    {
        int coinValue = GetCoinValue();
        totalCoins += coinValue;
        runCoinsEarned += coinValue;
        DailyMissionSystem.RegisterCoinsCollected(coinValue);
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.Save();

        if (totalCoinsText != null)
            totalCoinsText.text = "Coins " + totalCoins;

        if (isDailyChallengeRun)
        {
            UpdateUpgradeText();
            UpdateBestScoreHud();
        }

        audioDirector?.PlayCoinPickup();
    }

    public void ActivateShield()
    {
        if (UpgradeInventory.Instance == null)
            return;

        bool used = UpgradeInventory.Instance.UseUpgrade(UpgradeType.Shield, 1);

        if (used)
        {
            activeShields += 1;
            audioDirector?.PlayShieldActivated();
            playerPowerupVisuals?.PlayUpgradeBurst(UpgradeType.Shield);
            RefreshRunUpgradeButtons();
            UpdateUpgradeText();
        }
    }

    public bool ConsumeShieldIfAvailable()
    {
        if (activeShields > 0)
        {
            activeShields -= 1;
            RefreshRunUpgradeButtons();
            UpdateUpgradeText();
            return true;
        }

        return false;
    }

    void SyncLoadoutFromInventory(bool forceRebuild)
    {
        if (isDailyChallengeRun)
        {
            bool shouldClear = forceRebuild || equippedUpgrades.Count > 0 || runUpgradeButtons.Count > 0;

            if (shouldClear)
            {
                equippedUpgrades.Clear();
                autoEnabledUpgrades.Clear();
                activeUpgradeEndTimes.Clear();
                activeShields = 0;
                armedExtraLives = 0;
                RebuildRunUpgradeButtons();
                UpdateUpgradeText();
            }

            return;
        }

        if (UpgradeInventory.Instance == null)
            return;

        List<UpgradeType> savedUpgrades = UpgradeInventory.Instance.GetEquippedUpgrades();
        bool loadoutChanged = forceRebuild || HaveEquippedUpgradesChanged(savedUpgrades);

        if (!loadoutChanged)
            return;

        equippedUpgrades.Clear();

        for (int i = 0; i < savedUpgrades.Count; i++)
        {
            equippedUpgrades.Add(savedUpgrades[i]);
        }

        RemoveUnavailableAutoUpgrades();
        RebuildRunUpgradeButtons();
        UpdateUpgradeText();
    }

    bool HaveEquippedUpgradesChanged(List<UpgradeType> savedUpgrades)
    {
        if (savedUpgrades.Count != equippedUpgrades.Count)
            return true;

        for (int i = 0; i < savedUpgrades.Count; i++)
        {
            if (savedUpgrades[i] != equippedUpgrades[i])
                return true;
        }

        return false;
    }

    void RebuildRunUpgradeButtons()
    {
        if (runUpgradePanel != null)
            Destroy(runUpgradePanel.gameObject);

        runUpgradePanel = null;
        runUpgradeButtons.Clear();
        BuildRunUpgradeButtons();
    }

    void RemoveUnavailableAutoUpgrades()
    {
        List<UpgradeType> autoTypes = new List<UpgradeType>(autoEnabledUpgrades);

        for (int i = 0; i < autoTypes.Count; i++)
        {
            UpgradeType type = autoTypes[i];

            if (!equippedUpgrades.Contains(type) || !IsAutoToggleUpgrade(type))
                autoEnabledUpgrades.Remove(type);
        }
    }

    void ProcessAutoUpgrades()
    {
        if (UpgradeInventory.Instance == null)
            return;

        for (int i = 0; i < equippedUpgrades.Count; i++)
        {
            UpgradeType type = equippedUpgrades[i];

            if (!autoEnabledUpgrades.Contains(type) || !IsAutoToggleUpgrade(type))
                continue;

            switch (type)
            {
                case UpgradeType.Shield:
                    SustainShield();
                    break;
                case UpgradeType.ExtraLife:
                    SustainExtraLife();
                    break;
                case UpgradeType.SpeedBoost:
                    SustainTimedUpgrade(type, SpeedBoostDuration);
                    break;
                case UpgradeType.CoinMagnet:
                    SustainTimedUpgrade(type, CoinMagnetDuration);
                    break;
                case UpgradeType.DoubleCoins:
                    SustainTimedUpgrade(type, DoubleCoinsDuration);
                    break;
                case UpgradeType.SlowTime:
                    SustainTimedUpgrade(type, SlowTimeDuration);
                    break;
                case UpgradeType.SmallerPlayer:
                    SustainTimedUpgrade(type, SmallerPlayerDuration);
                    break;
                case UpgradeType.ScoreBooster:
                    SustainTimedUpgrade(type, ScoreBoosterDuration);
                    break;
                case UpgradeType.RareCoinBoost:
                    SustainTimedUpgrade(type, RareCoinBoostDuration);
                    break;
            }
        }
    }

    void SustainShield()
    {
        if (activeShields > 0)
            return;

        if (GetUpgradeOwnedCount(UpgradeType.Shield) > 0)
            ActivateShield();
        else
            autoEnabledUpgrades.Remove(UpgradeType.Shield);
    }

    void SustainExtraLife()
    {
        if (armedExtraLives > 0)
            return;

        if (GetUpgradeOwnedCount(UpgradeType.ExtraLife) > 0)
            ArmExtraLife();
        else
            autoEnabledUpgrades.Remove(UpgradeType.ExtraLife);
    }

    void SustainTimedUpgrade(UpgradeType type, float duration)
    {
        if (IsUpgradeActive(type))
            return;

        if (GetUpgradeOwnedCount(type) > 0)
            ActivateTimedUpgrade(type, duration);
        else
            autoEnabledUpgrades.Remove(type);
    }

    public void HandlePlayerHit(GameObject obstacle)
    {
        if (gameEnded)
            return;

        if (player != null && player.IsInvulnerable())
        {
            if (obstacle != null)
                Destroy(obstacle);

            audioDirector?.PlayShieldBlocked();
            playerPowerupVisuals?.PlayShieldBlockBurst();
            return;
        }

        if (TryConsumeOpeningGraceShield(obstacle))
            return;

        if (ConsumeShieldIfAvailable())
        {
            if (obstacle != null)
                Destroy(obstacle);

            if (player != null)
                player.TriggerInvulnerability(ShieldProtectionDuration);

            audioDirector?.PlayShieldBlocked();
            playerPowerupVisuals?.PlayShieldBlockBurst();
            GameSettings.TriggerHaptic();

            ProcessAutoUpgrades();
            return;
        }

        if (armedExtraLives > 0)
        {
            armedExtraLives -= 1;

            if (obstacle != null)
                Destroy(obstacle);

            GameSettings.TriggerHaptic();
            StartCoroutine(RevivePlayerRoutine());
            ProcessAutoUpgrades();
            RefreshRunUpgradeButtons();
            UpdateUpgradeText();
            return;
        }

        audioDirector?.PlayObstacleHit();
        Debug.Log(
            "Player hit by " +
            (obstacle != null ? obstacle.name : "Unknown") +
            " | score " + Mathf.FloorToInt(score) +
            " | level " + currentLevel +
            " | runTime " + runTime.ToString("0.0"));

        if (CanOfferRewardedRevive())
        {
            if (obstacle != null)
                Destroy(obstacle);

            ShowRewardedRevivePrompt();
            return;
        }

        GameOver();
    }

    bool TryConsumeOpeningGraceShield(GameObject obstacle)
    {
        if (!openingGraceShieldAvailable || runTime > OpeningGraceShieldWindow)
            return false;

        openingGraceShieldAvailable = false;

        if (obstacle != null)
            Destroy(obstacle);

        if (player != null)
            player.TriggerInvulnerability(Mathf.Max(ShieldProtectionDuration, OpeningGraceInvulnerabilityDuration));

        audioDirector?.PlayShieldBlocked();
        playerPowerupVisuals?.PlayShieldBlockBurst();
        GameSettings.TriggerHaptic();
        Debug.Log(
            "Opening grace shield saved the run | score " + Mathf.FloorToInt(score) +
            " | level " + currentLevel +
            " | runTime " + runTime.ToString("0.0"));
        return true;
    }

    IEnumerator RevivePlayerRoutine()
    {
        if (player != null)
            player.Die();

        yield return new WaitForSeconds(ExtraLifeReviveDelay);

        if (player != null)
            player.Revive(ExtraLifeInvulnerabilityDuration);

        audioDirector?.PlayExtraLifeRevive();
        playerPowerupVisuals?.PlayUpgradeBurst(UpgradeType.ExtraLife);
    }

    public void UseRunUpgrade(UpgradeType type)
    {
        if (gameEnded || !equippedUpgrades.Contains(type) || UpgradeInventory.Instance == null)
            return;

        if (IsAutoToggleUpgrade(type))
        {
            ToggleAutoUpgrade(type);
            ProcessAutoUpgrades();
        }
        else if (type == UpgradeType.Bomb)
        {
            ActivateBomb();
        }

        RefreshRunUpgradeButtons();
        UpdateUpgradeText();
    }

    void ToggleAutoUpgrade(UpgradeType type)
    {
        if (autoEnabledUpgrades.Contains(type))
        {
            autoEnabledUpgrades.Remove(type);
            return;
        }

        if (GetUpgradeOwnedCount(type) > 0 || HasBufferedUpgradeState(type))
            autoEnabledUpgrades.Add(type);
    }

    bool IsAutoToggleUpgrade(UpgradeType type)
    {
        return type != UpgradeType.Bomb;
    }

    bool IsUpgradeAutoEnabled(UpgradeType type)
    {
        return autoEnabledUpgrades.Contains(type);
    }

    bool HasBufferedUpgradeState(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Shield:
                return activeShields > 0;
            case UpgradeType.ExtraLife:
                return armedExtraLives > 0;
            case UpgradeType.Bomb:
                return false;
            default:
                return IsUpgradeActive(type);
        }
    }

    void ArmExtraLife()
    {
        if (UpgradeInventory.Instance.UseUpgrade(UpgradeType.ExtraLife, 1))
        {
            armedExtraLives += 1;
            audioDirector?.PlayExtraLifeArmed();
            playerPowerupVisuals?.PlayUpgradeBurst(UpgradeType.ExtraLife);
        }
    }

    void ActivateBomb()
    {
        if (!UpgradeInventory.Instance.UseUpgrade(UpgradeType.Bomb, 1))
            return;

        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        for (int i = 0; i < obstacles.Length; i++)
        {
            Destroy(obstacles[i]);
        }

        audioDirector?.PlayBombBlast();
        playerPowerupVisuals?.PlayBombBurst();
    }

    void ActivateTimedUpgrade(UpgradeType type, float duration)
    {
        if (!UpgradeInventory.Instance.UseUpgrade(type, 1))
            return;

        float startTime = Time.time;

        if (IsUpgradeActive(type))
            startTime = activeUpgradeEndTimes[type];

        activeUpgradeEndTimes[type] = startTime + duration;
        audioDirector?.PlayUpgradeActivated(type);
        playerPowerupVisuals?.PlayUpgradeBurst(type);
    }

    void LoadEquippedUpgrades()
    {
        equippedUpgrades.Clear();

        if (UpgradeInventory.Instance == null)
            return;

        List<UpgradeType> savedUpgrades = UpgradeInventory.Instance.GetEquippedUpgrades();

        for (int i = 0; i < savedUpgrades.Count; i++)
        {
            equippedUpgrades.Add(savedUpgrades[i]);
        }
    }

    void BuildRunUpgradeButtons()
    {
        if (equippedUpgrades.Count == 0)
            return;

        RectTransform parentRect = GetRuntimeUIParent();

        if (parentRect == null)
            return;

        GameObject panelObject = new GameObject(
            "RunUpgradePanel",
            typeof(RectTransform),
            typeof(VerticalLayoutGroup),
            typeof(ContentSizeFitter));

        panelObject.transform.SetParent(parentRect, false);
        runUpgradePanel = panelObject.GetComponent<RectTransform>();
        runUpgradePanel.anchorMin = new Vector2(1f, 0f);
        runUpgradePanel.anchorMax = new Vector2(1f, 0f);
        runUpgradePanel.pivot = new Vector2(1f, 0f);
        runUpgradePanel.anchoredPosition = new Vector2(-20f, 22f);
        runUpgradePanel.sizeDelta = new Vector2(RunUpgradeButtonWidth, 0f);

        VerticalLayoutGroup layoutGroup = panelObject.GetComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 8f;
        layoutGroup.childAlignment = TextAnchor.LowerRight;
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;

        ContentSizeFitter fitter = panelObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        for (int i = 0; i < equippedUpgrades.Count; i++)
        {
            UpgradeType type = equippedUpgrades[i];
            RunUpgradeButtonUI buttonUI = CreateRunUpgradeButton(type);

            if (buttonUI != null)
                runUpgradeButtons[type] = buttonUI;
        }
    }

    RunUpgradeButtonUI CreateRunUpgradeButton(UpgradeType type)
    {
        if (runUpgradePanel == null)
            return null;

        GameObject buttonObject = new GameObject(
            UpgradeInventory.GetDisplayName(type) + "Button",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button),
            typeof(LayoutElement),
            typeof(RunUpgradeButtonUI));

        buttonObject.transform.SetParent(runUpgradePanel, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(RunUpgradeButtonWidth, RunUpgradeButtonHeight);

        LayoutElement layoutElement = buttonObject.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = RunUpgradeButtonHeight;
        layoutElement.preferredWidth = RunUpgradeButtonWidth;

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = StudioUiTheme.WithAlpha(StudioUiTheme.Surface, 0.95f);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(8f, 6f);
        labelRect.offsetMax = new Vector2(-8f, -6f);

        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.enableAutoSizing = true;
        label.fontSizeMin = 12f;
        label.fontSizeMax = 20f;
        label.color = StudioUiTheme.Text;
        label.lineSpacing = -8f;

        if (runtimeFont != null)
            label.font = runtimeFont;

        RunUpgradeButtonUI buttonUI = buttonObject.GetComponent<RunUpgradeButtonUI>();
        buttonUI.Initialize(this, type, label, buttonImage, buttonObject.GetComponent<Button>());

        return buttonUI;
    }

    RectTransform GetRuntimeUIParent()
    {
        if (scoreText != null && scoreText.rectTransform.parent != null)
            return scoreText.rectTransform.parent as RectTransform;

        Canvas canvas = FindAnyObjectByType<Canvas>();

        if (canvas != null)
            return canvas.GetComponent<RectTransform>();

        return null;
    }

    TMP_FontAsset GetRuntimeFont()
    {
        if (scoreText != null)
            return scoreText.font;

        if (upgradeText != null)
            return upgradeText.font;

        return null;
    }

    void RefreshRunUpgradeButtons()
    {
        if (runUpgradePanel != null)
            runUpgradePanel.gameObject.SetActive(!gameEnded && !IsRevivePromptVisible());

        if (gameEnded || IsRevivePromptVisible())
            return;

        if (runUpgradeButtons.Count == 0)
            return;

        List<UpgradeType> buttonTypes = new List<UpgradeType>(runUpgradeButtons.Keys);

        for (int i = 0; i < buttonTypes.Count; i++)
        {
            UpgradeType type = buttonTypes[i];
            RunUpgradeButtonUI buttonUI = runUpgradeButtons[type];

            if (buttonUI == null)
                continue;

            string stateText = BuildButtonStateText(type);
            string modeText = BuildButtonModeText(type);
            bool hasEffect = HasBufferedUpgradeState(type);
            bool isSelected = IsUpgradeAutoEnabled(type);
            bool canUse = CanUseRunUpgradeButton(type);

            buttonUI.RefreshView(
                GetUpgradeOwnedCount(type),
                stateText,
                modeText,
                isSelected,
                hasEffect,
                canUse,
                !IsAutoToggleUpgrade(type));
        }
    }

    string BuildButtonStateText(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Shield:
                if (activeShields > 0)
                    return "Shield ready";

                if (GetUpgradeOwnedCount(type) > 0)
                    return "Waiting to arm";

                return "Out";
            case UpgradeType.ExtraLife:
                if (armedExtraLives > 0)
                    return "Life armed";

                if (GetUpgradeOwnedCount(type) > 0)
                    return "Waiting to arm";

                return "Out";
            case UpgradeType.Bomb:
                return "Clear screen";
            default:
                if (IsUpgradeActive(type))
                    return Mathf.CeilToInt(GetUpgradeRemainingTime(type)) + "s active";

                if (GetUpgradeOwnedCount(type) > 0)
                    return "Ready";

                return "Out";
        }
    }

    string BuildButtonModeText(UpgradeType type)
    {
        if (!IsAutoToggleUpgrade(type))
            return "Tap to use";

        return IsUpgradeAutoEnabled(type) ? "Auto ON" : "Auto OFF";
    }

    bool CanUseRunUpgradeButton(UpgradeType type)
    {
        if (gameEnded)
            return false;

        if (IsAutoToggleUpgrade(type))
            return GetUpgradeOwnedCount(type) > 0 || HasBufferedUpgradeState(type) || IsUpgradeAutoEnabled(type);

        return GetUpgradeOwnedCount(type) > 0;
    }

    int GetUpgradeOwnedCount(UpgradeType type)
    {
        if (UpgradeInventory.Instance == null)
            return 0;

        return UpgradeInventory.Instance.GetAmount(type);
    }

    string BuildUpgradeSummary(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Shield:
                return "Shield " + activeShields + "/" + GetUpgradeOwnedCount(type);
            case UpgradeType.ExtraLife:
                return "Life " + armedExtraLives + "/" + GetUpgradeOwnedCount(type);
            case UpgradeType.Bomb:
                return "Bomb " + GetUpgradeOwnedCount(type);
            default:
                if (IsUpgradeActive(type))
                {
                    return GetCompactUpgradeName(type) + " " +
                           Mathf.CeilToInt(GetUpgradeRemainingTime(type)) + "s";
                }

                return GetCompactUpgradeName(type) + " x" + GetUpgradeOwnedCount(type);
        }
    }

    string GetCompactUpgradeName(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.SpeedBoost:
                return "Speed";
            case UpgradeType.CoinMagnet:
                return "Magnet";
            case UpgradeType.DoubleCoins:
                return "2x Coins";
            case UpgradeType.SlowTime:
                return "Slow";
            case UpgradeType.SmallerPlayer:
                return "Small";
            case UpgradeType.ScoreBooster:
                return "Score";
            case UpgradeType.RareCoinBoost:
                return "Rare";
            default:
                return UpgradeInventory.GetDisplayName(type);
        }
    }

    void ApplyContinuousEffects()
    {
        if (player == null)
            return;

        player.SetSizeMultiplier(GetPlayerSizeMultiplier());
        UpdateUpgradeText();
    }

    public bool IsUpgradeActive(UpgradeType type)
    {
        if (!activeUpgradeEndTimes.ContainsKey(type))
            return false;

        return activeUpgradeEndTimes[type] > Time.time;
    }

    public float GetUpgradeRemainingTime(UpgradeType type)
    {
        if (!activeUpgradeEndTimes.ContainsKey(type))
            return 0f;

        return Mathf.Max(0f, activeUpgradeEndTimes[type] - Time.time);
    }

    public float GetWorldSpeedMultiplier()
    {
        float multiplier = 1f;

        if (isDailyChallengeRun)
            multiplier *= activeDailyChallenge.worldSpeedMultiplier;

        if (IsUpgradeActive(UpgradeType.SlowTime))
            multiplier *= 0.55f;

        return multiplier;
    }

    public float GetSpawnIntervalMultiplier()
    {
        float difficulty = Mathf.Max(0f, GetContinuousDifficultyLevel() - 1f);
        float earlyBreather = Mathf.Lerp(1.34f, 1.04f, Mathf.Clamp01(runTime / 18f));
        float laterCompression = Mathf.Lerp(1f, 0.87f, Mathf.Clamp01((difficulty - 2f) / 6f));
        return earlyBreather * laterCompression;
    }

    public float GetPlayerMoveSpeedMultiplier()
    {
        float multiplier = 1.14f + (Mathf.Clamp01((GetContinuousDifficultyLevel() - 1f) / 8f) * 0.08f);

        if (IsUpgradeActive(UpgradeType.SpeedBoost))
            multiplier *= 1.42f;

        return multiplier;
    }

    public float GetPlayerSizeMultiplier()
    {
        if (IsUpgradeActive(UpgradeType.SmallerPlayer))
            return 0.68f;

        return 1f;
    }

    public float GetScoreMultiplier()
    {
        float multiplier = 1f;

        if (IsUpgradeActive(UpgradeType.ScoreBooster))
            multiplier *= 2f;

        multiplier *= GetDangerScoreMultiplier();
        return multiplier;
    }

    public int GetCoinValue()
    {
        if (IsUpgradeActive(UpgradeType.DoubleCoins))
            return 2;

        return 1;
    }

    public float GetCurrentCoinSpawnChance(float baseChance)
    {
        float spawnChance = baseChance;
        float difficulty = Mathf.Max(0f, GetContinuousDifficultyLevel() - 1f);

        if (isDailyChallengeRun)
            spawnChance *= activeDailyChallenge.coinSpawnMultiplier;

        if (IsUpgradeActive(UpgradeType.RareCoinBoost))
            spawnChance += 0.3f;

        if (runTime < 14f)
            spawnChance += 0.12f;
        else if (runTime < 24f)
            spawnChance += 0.04f;

        spawnChance += Mathf.Clamp01(difficulty / 8f) * 0.05f;
        return Mathf.Clamp01(spawnChance);
    }

    public Transform GetCoinMagnetTarget(Vector3 coinPosition)
    {
        if (!IsUpgradeActive(UpgradeType.CoinMagnet) || player == null)
            return null;

        float magnetRadius = 2.8f;

        if (Vector3.Distance(coinPosition, player.transform.position) <= magnetRadius)
            return player.transform;

        return null;
    }

    public void GameOver()
    {
        if (gameEnded) return;

        isAwaitingRewardedRevive = false;
        SetRevivePromptVisible(false);
        gameEnded = true;
        int finalScore = Mathf.FloorToInt(score);
        DailyMissionSystem.RegisterRunFinished(finalScore, currentLevel);

        if (isDailyChallengeRun)
            activeDailyChallenge = DailyChallengeSystem.RegisterRunResult(finalScore, runCoinsEarned);

        SetPauseOverlayVisible(false);
        SetTutorialOverlayVisible(false);
        GameSettings.TriggerHaptic();

        if (finalScore > bestScore)
        {
            bestScore = finalScore;
            newBestScore = true;
            PlayerPrefs.SetInt(BestScoreKey, bestScore);
            PlayerPrefs.Save();
        }
        else
        {
            newBestScore = false;
        }

        postRunDoubleCoinsClaimed = false;
        lastFinishedScore = finalScore;
        lastFinishedLevel = currentLevel;
        lastPerformanceRewardSummary = string.Empty;
        lastShareRewardSummary = string.Empty;
        completedRunsCount = GameSettings.RegisterCompletedRun();
        bool clearedDailyChallenge = isDailyChallengeRun &&
                                     activeDailyChallenge.bestScore >= activeDailyChallenge.targetScore;
        lastProgressionResult = PlayerProgressionSystem.RegisterRunCompleted(
            finalScore,
            runCoinsEarned,
            currentLevel,
            isDailyChallengeRun,
            newBestScore,
            runNearMissCount,
            bestDangerComboCount,
            clearedDailyChallenge);
        totalCoins = PlayerPrefs.GetInt("TotalCoins", totalCoins);
        lastPerformanceRewardSummary = AwardRunPerformanceRewards(finalScore, currentLevel);
        totalCoins = PlayerPrefs.GetInt("TotalCoins", totalCoins);

        if (totalCoinsText != null)
            totalCoinsText.text = "Coins " + totalCoins;

        dangerComboCount = 0;
        dangerComboDecayTimer = 0f;
        showPostRunReviewButton = GameSettings.ShouldShowReviewPrompt(
            completedRunsCount,
            finalScore,
            newBestScore,
            isDailyChallengeRun);

        if (showPostRunReviewButton)
        {
            GameSettings.DeferReviewPrompt(3);
            LaunchAnalytics.RecordReviewPromptShown(
                "post_run",
                completedRunsCount,
                finalScore,
                newBestScore);
        }

        LaunchAnalytics.RecordRunFinished(
            finalScore,
            runCoinsEarned,
            currentLevel,
            isDailyChallengeRun,
            newBestScore);
        if (!qaPracticeRunActive)
        {
            QaTestingSystem.RegisterRunFinished(
                finalScore,
                currentLevel,
                runCoinsEarned,
                runNearMissCount,
                bestDangerComboCount,
                newBestScore,
                isDailyChallengeRun);
        }
        Debug.Log(
            "Run ended | score " + finalScore +
            " | level " + currentLevel +
            " | coins " + runCoinsEarned +
            " | nearMisses " + runNearMissCount +
            " | dangerPeak " + bestDangerComboCount);

        if (spawner != null)
            spawner.StopSpawning();

        if (player != null)
            player.Die();

        if (gameOverText != null)
            gameOverText.SetActive(true);

        if (restartButton != null)
            restartButton.SetActive(true);

        ShowPostRunSummary(finalScore);
        UpdateBestScoreHud();
        RefreshRunUpgradeButtons();
        RefreshQaFeedbackUi();
    }

    public void RestartGame()
    {
        QaTestingSystem.StopCapture("restart_run");
        Time.timeScale = 1f;

        if (isDailyChallengeRun)
            DailyChallengeSystem.BeginTodayChallengeRun();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void EnsureRuntimeFeedbackUI()
    {
        RectTransform parentRect = GetRuntimeUIParent();

        if (parentRect == null)
            return;

        if (bestScoreHudText == null)
        {
            bestScoreHudText = TryBindSceneText(parentRect, "BestScoreText");

            if (bestScoreHudText == null)
            {
                bestScoreHudText = CreateRuntimeLabel(
                    parentRect,
                    "BestScoreText",
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(360f, 42f),
                    new Vector2(0f, -118f),
                    TextAlignmentOptions.Center,
                    20,
                    32,
                    new Color(0.9f, 0.94f, 1f, 1f));
            }
        }

        if (postRunPanel == null || postRunSummaryText == null)
        {
            if (!TryBindScenePostRunPanel(parentRect))
                CreatePostRunPanel(parentRect);
        }

        ConfigurePostRunSummaryText(postRunSummaryText);
        EnsurePostRunActionButtons();

        if (pauseButton == null)
        {
            TryBindScenePauseButton(parentRect);

            if (pauseButton == null)
                CreatePauseButton(parentRect);
        }

        if (pauseOverlayRoot == null)
            CreatePauseOverlay(parentRect);

        if (tutorialOverlayRoot == null)
            CreateTutorialOverlay(parentRect);

        if (revivePromptRoot == null)
            CreateRevivePrompt(parentRect);

        if (qaCaptureOverlayRoot == null)
            CreateQaCaptureOverlay(parentRect);

        NormalizeHudLayout(parentRect);
    }

    void NormalizeHudLayout(RectTransform parentRect)
    {
        if (parentRect == null)
            return;

        Canvas canvas = FindAnyObjectByType<Canvas>();
        SafeAreaUtility.NormalizeCanvas(canvas);
        SafeAreaUtility.ApplySafeArea(parentRect);

        if (pauseButton != null)
        {
            RectTransform pauseRect = pauseButton.GetComponent<RectTransform>();

            if (pauseRect != null)
            {
                pauseRect.anchorMin = new Vector2(0f, 1f);
                pauseRect.anchorMax = new Vector2(0f, 1f);
                pauseRect.pivot = new Vector2(0f, 1f);
                pauseRect.sizeDelta = new Vector2(162f, 58f);
                pauseRect.anchoredPosition = new Vector2(20f, -28f);
            }

            if (pauseButtonText != null)
            {
                pauseButtonText.enableAutoSizing = true;
                pauseButtonText.fontSizeMin = 18f;
                pauseButtonText.fontSizeMax = 28f;
            }

            ApplyGameplayButtonStyle(pauseButton, StudioButtonStyle.Secondary);
        }

        StyleSceneGameplayButton(parentRect, "MenuButton", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(162f, 58f), new Vector2(-20f, -28f), StudioButtonStyle.Quiet);
        StyleSceneGameplayButton(parentRect, "RestartButton", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300f, 72f), new Vector2(0f, -86f), StudioButtonStyle.Primary);

        ConfigureHudText(
            totalCoinsText,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(240f, 40f),
            new Vector2(20f, -98f),
            TextAlignmentOptions.Left,
            18f,
            28f);

        ConfigureHudText(
            scoreText,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(330f, 54f),
            new Vector2(0f, -30f),
            TextAlignmentOptions.Center,
            30f,
            44f);

        ConfigureHudText(
            bestScoreHudText,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(300f, 28f),
            new Vector2(0f, -82f),
            TextAlignmentOptions.Center,
            16f,
            24f);

        ConfigureHudText(
            levelText,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(180f, 28f),
            new Vector2(0f, -112f),
            TextAlignmentOptions.Center,
            16f,
            24f);

        ConfigureHudText(
            upgradeText,
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(220f, 98f),
            new Vector2(-20f, -96f),
            TextAlignmentOptions.TopRight,
            14f,
            20f);

        if (upgradeText != null)
            upgradeText.lineSpacing = 2f;

        ApplyHudTextStyle(scoreText, StudioUiTheme.Text, FontStyles.Bold);
        ApplyHudTextStyle(bestScoreHudText, StudioUiTheme.MutedText, FontStyles.Normal);
        ApplyHudTextStyle(levelText, StudioUiTheme.Gold, FontStyles.Bold);
        ApplyHudTextStyle(totalCoinsText, StudioUiTheme.Text, FontStyles.Bold);
        ApplyHudTextStyle(upgradeText, StudioUiTheme.MutedText, FontStyles.Normal);

        ConfigureHudBackplate(scoreText, "ScoreHudBackplate", new Vector2(38f, 18f), StudioPanelStyle.Scrim, 0.38f);
        ConfigureHudBackplate(totalCoinsText, "CoinsHudBackplate", new Vector2(28f, 14f), StudioPanelStyle.Scrim, 0.34f);
        ConfigureHudBackplate(upgradeText, "LoadoutHudBackplate", new Vector2(26f, 18f), StudioPanelStyle.Scrim, 0.3f);

        if (runUpgradePanel != null)
            runUpgradePanel.anchoredPosition = new Vector2(-20f, 22f);
    }

    void ConfigureHudText(
        TextMeshProUGUI text,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 size,
        Vector2 anchoredPosition,
        TextAlignmentOptions alignment,
        float minFontSize,
        float maxFontSize)
    {
        if (text == null)
            return;

        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = anchorMin;
        textRect.anchorMax = anchorMax;
        textRect.pivot = pivot;
        textRect.sizeDelta = size;
        textRect.anchoredPosition = anchoredPosition;

        text.alignment = alignment;
        text.enableAutoSizing = true;
        text.fontSizeMin = minFontSize;
        text.fontSizeMax = maxFontSize;
        text.raycastTarget = false;
    }

    void ApplyHudTextStyle(TextMeshProUGUI text, Color color, FontStyles style)
    {
        if (text == null)
            return;

        text.color = color;
        text.fontStyle = style;

        Shadow shadow = text.GetComponent<Shadow>();

        if (shadow == null)
            shadow = text.gameObject.AddComponent<Shadow>();

        shadow.effectColor = new Color(0f, 0f, 0f, 0.52f);
        shadow.effectDistance = new Vector2(0f, -2.5f);
        shadow.useGraphicAlpha = true;
    }

    void ConfigureHudBackplate(TextMeshProUGUI text, string objectName, Vector2 padding, StudioPanelStyle style, float alpha)
    {
        if (text == null || text.rectTransform.parent == null)
            return;

        RectTransform parentRect = text.rectTransform.parent as RectTransform;

        if (parentRect == null)
            return;

        Transform existing = parentRect.Find(objectName);
        GameObject plateObject = existing != null ? existing.gameObject : new GameObject(objectName, typeof(RectTransform), typeof(Image));

        if (existing == null)
            plateObject.transform.SetParent(parentRect, false);

        RectTransform plateRect = plateObject.GetComponent<RectTransform>();
        plateRect.anchorMin = text.rectTransform.anchorMin;
        plateRect.anchorMax = text.rectTransform.anchorMax;
        plateRect.pivot = text.rectTransform.pivot;
        plateRect.sizeDelta = text.rectTransform.sizeDelta + padding;
        plateRect.anchoredPosition = text.rectTransform.anchoredPosition;

        Image plateImage = plateObject.GetComponent<Image>();
        StudioUiTheme.ApplyPanel(plateImage, style, alpha);
        plateImage.raycastTarget = false;

        int textIndex = text.transform.GetSiblingIndex();
        plateObject.transform.SetSiblingIndex(Mathf.Max(0, textIndex));
        text.transform.SetSiblingIndex(Mathf.Min(parentRect.childCount - 1, plateObject.transform.GetSiblingIndex() + 1));
    }

    void StyleSceneGameplayButton(
        RectTransform parentRect,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 size,
        Vector2 anchoredPosition,
        StudioButtonStyle style)
    {
        if (parentRect == null)
            return;

        Transform buttonTransform = parentRect.Find(objectName);

        if (buttonTransform == null)
            return;

        RectTransform buttonRect = buttonTransform as RectTransform;

        if (buttonRect != null)
        {
            buttonRect.anchorMin = anchorMin;
            buttonRect.anchorMax = anchorMax;
            buttonRect.pivot = pivot;
            buttonRect.sizeDelta = size;
            buttonRect.anchoredPosition = anchoredPosition;
        }

        ApplyGameplayButtonStyle(buttonTransform.GetComponent<Button>(), style);
    }

    void ApplyGameplayButtonStyle(Button button, StudioButtonStyle style)
    {
        if (button == null)
            return;

        TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
        StudioUiTheme.ApplyButton(button, style, label);

        if (label != null)
        {
            label.fontSizeMin = 16f;
            label.fontSizeMax = 28f;
        }
    }

    TextMeshProUGUI TryBindSceneText(RectTransform parentRect, string objectName)
    {
        if (parentRect == null)
            return null;

        Transform childTransform = parentRect.Find(objectName);

        if (childTransform == null)
            return null;

        return childTransform.GetComponent<TextMeshProUGUI>();
    }

    void TryBindScenePauseButton(RectTransform parentRect)
    {
        if (parentRect == null)
            return;

        Transform pauseTransform = parentRect.Find("PauseButton");

        if (pauseTransform == null)
            return;

        pauseButton = pauseTransform.GetComponent<Button>();
        pauseButtonText = pauseTransform.GetComponentInChildren<TextMeshProUGUI>(true);

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(TogglePause);
        }
    }

    bool TryBindScenePostRunPanel(RectTransform parentRect)
    {
        if (parentRect == null)
            return false;

        Transform panelTransform = parentRect.Find("PostRunSummaryPanel");

        if (panelTransform == null)
            return false;

        postRunPanel = panelTransform.gameObject;

        Transform summaryTransform = panelTransform.Find("PostRunSummaryText");

        if (summaryTransform != null)
            postRunSummaryText = summaryTransform.GetComponent<TextMeshProUGUI>();

        Transform buttonTransform = panelTransform.Find("DoubleCoinsButton");

        if (buttonTransform != null)
        {
            postRunDoubleCoinsButton = buttonTransform.GetComponent<Button>();
            postRunDoubleCoinsButtonText = buttonTransform.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        Transform shareButtonTransform = panelTransform.Find("ShareRunButton");

        if (shareButtonTransform != null)
        {
            postRunShareButton = shareButtonTransform.GetComponent<Button>();
            postRunShareButtonText = shareButtonTransform.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        Transform reviewButtonTransform = panelTransform.Find("RateGameButton");

        if (reviewButtonTransform != null)
        {
            postRunReviewButton = reviewButtonTransform.GetComponent<Button>();
            postRunReviewButtonText = reviewButtonTransform.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (postRunPanel != null)
            postRunPanel.SetActive(false);

        return postRunPanel != null && postRunSummaryText != null;
    }

    void ConfigurePostRunSummaryText(TextMeshProUGUI summaryText)
    {
        if (summaryText == null)
            return;

        summaryText.enableAutoSizing = true;
        summaryText.fontSizeMin = 26f;
        summaryText.fontSizeMax = 44f;
        summaryText.lineSpacing = 10f;
        summaryText.alignment = TextAlignmentOptions.Center;
        summaryText.color = new Color(0.96f, 0.98f, 1f, 1f);

        if (runtimeFont != null)
            summaryText.font = runtimeFont;
    }

    void EnsurePostRunActionButtons()
    {
        if (postRunPanel == null)
            return;

        RectTransform panelRect = postRunPanel.GetComponent<RectTransform>();

        if (panelRect == null)
            return;

        bool qaModeActive = QaTestingSystem.IsQaModeEnabled() && !qaPracticeRunActive;
        panelRect.sizeDelta = qaModeActive ? new Vector2(700f, 1280f) : new Vector2(620f, 430f);
        panelRect.anchoredPosition = qaModeActive ? new Vector2(0f, -120f) : new Vector2(0f, -250f);

        if (postRunDoubleCoinsButton == null)
        {
            postRunDoubleCoinsButton = CreateRuntimeButton(
                panelRect,
                "DoubleCoinsButton",
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(420f, 52f),
                qaModeActive ? new Vector2(0f, 224f) : new Vector2(0f, 82f),
                "Watch Ad: 2x Coins",
                new Color(0.94f, 0.73f, 0.23f, 1f),
                out postRunDoubleCoinsButtonText);
        }

        if (postRunShareButton == null)
        {
            postRunShareButton = CreateRuntimeButton(
                panelRect,
                "ShareRunButton",
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(420f, 52f),
                qaModeActive ? new Vector2(0f, 164f) : new Vector2(0f, 22f),
                "Share Result",
                new Color(0.23f, 0.5f, 0.66f, 1f),
                out postRunShareButtonText);
        }

        if (postRunReviewButton == null)
        {
            postRunReviewButton = CreateRuntimeButton(
                panelRect,
                "RateGameButton",
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(420f, 52f),
                qaModeActive ? new Vector2(0f, 284f) : new Vector2(0f, 142f),
                "Rate on Google Play",
                new Color(0.26f, 0.58f, 0.34f, 1f),
                out postRunReviewButtonText);
        }

        if (postRunDoubleCoinsButton != null)
        {
            postRunDoubleCoinsButton.onClick.RemoveAllListeners();
            postRunDoubleCoinsButton.onClick.AddListener(ClaimPostRunDoubleCoins);
        }

        if (postRunShareButton != null)
        {
            postRunShareButton.onClick.RemoveAllListeners();
            postRunShareButton.onClick.AddListener(SharePostRunResult);
        }

        if (postRunReviewButton != null)
        {
            postRunReviewButton.onClick.RemoveAllListeners();
            postRunReviewButton.onClick.AddListener(OpenStoreReviewPage);
        }

        ConfigurePostRunActionButton(
            postRunDoubleCoinsButton,
            new Vector2(420f, 52f),
            qaModeActive ? new Vector2(0f, 224f) : new Vector2(0f, 82f));
        ConfigurePostRunActionButton(
            postRunShareButton,
            new Vector2(420f, 52f),
            qaModeActive ? new Vector2(0f, 164f) : new Vector2(0f, 22f));
        ConfigurePostRunActionButton(
            postRunReviewButton,
            new Vector2(420f, 52f),
            qaModeActive ? new Vector2(0f, 284f) : new Vector2(0f, 142f));
        ConfigurePostRunActionButtonText(postRunDoubleCoinsButtonText);
        ConfigurePostRunActionButtonText(postRunShareButtonText);
        ConfigurePostRunActionButtonText(postRunReviewButtonText);

        if (postRunSummaryText != null)
        {
            RectTransform summaryRect = postRunSummaryText.rectTransform;
            summaryRect.offsetMin = qaModeActive ? new Vector2(40f, 760f) : new Vector2(28f, 204f);
            summaryRect.offsetMax = new Vector2(-28f, -24f);
        }

        if (qaModeActive && qaSurveyPanel == null)
            CreateQaSurveyPanel(panelRect);

        RefreshPostRunActionButtons();
        RefreshQaSurveyUi();
    }

    void ConfigurePostRunActionButton(Button actionButton, Vector2 size, Vector2 anchoredPosition)
    {
        if (actionButton == null)
            return;

        RectTransform buttonRect = actionButton.GetComponent<RectTransform>();

        if (buttonRect == null)
            return;

        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.sizeDelta = size;
        buttonRect.anchoredPosition = anchoredPosition;
    }

    void ConfigurePostRunActionButtonText(TextMeshProUGUI actionButtonText)
    {
        if (actionButtonText == null)
            return;

        actionButtonText.enableAutoSizing = true;
        actionButtonText.fontSizeMin = 18f;
        actionButtonText.fontSizeMax = 28f;
    }

    TextMeshProUGUI CreateRuntimeLabel(
        RectTransform parent,
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
        GameObject labelObject = new GameObject(objectName, typeof(RectTransform));
        labelObject.transform.SetParent(parent, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = anchorMin;
        labelRect.anchorMax = anchorMax;
        labelRect.pivot = pivot;
        labelRect.sizeDelta = size;
        labelRect.anchoredPosition = anchoredPosition;

        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.alignment = alignment;
        label.enableAutoSizing = true;
        label.fontSizeMin = minSize;
        label.fontSizeMax = maxSize;
        label.color = color;

        if (runtimeFont != null)
            label.font = runtimeFont;

        return label;
    }

    void CreatePostRunPanel(RectTransform parentRect)
    {
        postRunPanel = new GameObject("PostRunPanel", typeof(RectTransform), typeof(Image));
        postRunPanel.transform.SetParent(parentRect, false);

        RectTransform panelRect = postRunPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(620f, 430f);
        panelRect.anchoredPosition = new Vector2(0f, -250f);

        Image panelImage = postRunPanel.GetComponent<Image>();
        StudioUiTheme.ApplyPanel(panelImage, StudioPanelStyle.Elevated, 0.96f);

        postRunSummaryText = CreateRuntimeLabel(
            panelRect,
            "PostRunSummaryText",
            Vector2.zero,
            Vector2.one,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            TextAlignmentOptions.Center,
            26f,
            44f,
            new Color(0.97f, 0.98f, 1f, 1f));

        ConfigurePostRunSummaryText(postRunSummaryText);

        if (postRunSummaryText != null)
        {
            RectTransform summaryRect = postRunSummaryText.rectTransform;
            summaryRect.offsetMin = new Vector2(28f, 204f);
            summaryRect.offsetMax = new Vector2(-28f, -24f);
        }

        EnsurePostRunActionButtons();
        postRunPanel.SetActive(false);
    }

    void CreateQaCaptureOverlay(RectTransform parentRect)
    {
        qaCaptureOverlayRoot = new GameObject("QaCaptureOverlayRoot", typeof(RectTransform), typeof(Image));
        qaCaptureOverlayRoot.transform.SetParent(parentRect, false);

        RectTransform rootRect = qaCaptureOverlayRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image overlayImage = qaCaptureOverlayRoot.GetComponent<Image>();
        StudioUiTheme.ApplyPanel(overlayImage, StudioPanelStyle.Scrim);

        GameObject panelObject = new GameObject("QaCaptureOverlayPanel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(qaCaptureOverlayRoot.transform, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(720f, 500f);
        panelRect.anchoredPosition = new Vector2(0f, -10f);

        Image panelImage = panelObject.GetComponent<Image>();
        StudioUiTheme.ApplyPanel(panelImage, StudioPanelStyle.Elevated);

        qaCaptureOverlayTitleText = CreateRuntimeLabel(
            panelRect,
            "QaCaptureOverlayTitle",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(560f, 62f),
            new Vector2(0f, -36f),
            TextAlignmentOptions.Center,
            28f,
            40f,
            Color.white);

        qaCaptureOverlayBodyText = CreateRuntimeLabel(
            panelRect,
            "QaCaptureOverlayBody",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(620f, 250f),
            new Vector2(0f, -126f),
            TextAlignmentOptions.Center,
            20f,
            28f,
            new Color(0.92f, 0.97f, 1f, 1f));

        if (qaCaptureOverlayBodyText != null)
            qaCaptureOverlayBodyText.lineSpacing = 4f;

        qaCaptureOverlayContinueButton = CreateRuntimeButton(
            panelRect,
            "QaCaptureOverlayContinueButton",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(460f, 76f),
            new Vector2(0f, 36f),
            "I Agree - Open Android Prompt",
            StudioUiTheme.Gold,
            out qaCaptureOverlayContinueButtonText);

        if (qaCaptureOverlayContinueButton != null)
        {
            qaCaptureOverlayContinueButton.onClick.RemoveAllListeners();
            qaCaptureOverlayContinueButton.onClick.AddListener(ConfirmQaCaptureNotice);
        }

        qaCaptureOverlayRoot.SetActive(false);
    }

    void CreateQaSurveyPanel(RectTransform parentRect)
    {
        qaSurveyPanel = new GameObject("QaSurveyPanel", typeof(RectTransform), typeof(Image));
        qaSurveyPanel.transform.SetParent(parentRect, false);

        RectTransform panelRect = qaSurveyPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(580f, 720f);
        panelRect.anchoredPosition = new Vector2(0f, 18f);

        Image panelImage = qaSurveyPanel.GetComponent<Image>();
        StudioUiTheme.ApplyPanel(panelImage, StudioPanelStyle.Elevated);

        CreateRuntimeLabel(
            panelRect,
            "QaSurveyTitle",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(500f, 36f),
            new Vector2(0f, -14f),
            TextAlignmentOptions.Center,
            22f,
            30f,
            Color.white).text = "QA Check-In";

        qaSurveyStatusText = CreateRuntimeLabel(
            panelRect,
            "QaSurveyStatus",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(520f, 56f),
            new Vector2(0f, -54f),
            TextAlignmentOptions.Center,
            14f,
            19f,
            new Color(0.84f, 0.91f, 0.98f, 1f));

        if (qaSurveyStatusText != null)
            qaSurveyStatusText.lineSpacing = 2f;

        qaFairnessButton = CreateRuntimeButton(
            panelRect,
            "QaFairnessButton",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(250f, 44f),
            new Vector2(-135f, -116f),
            "Collision Feel",
            new Color(0.29f, 0.47f, 0.66f, 1f),
            out qaFairnessButtonText);

        qaDifficultyButton = CreateRuntimeButton(
            panelRect,
            "QaDifficultyButton",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(250f, 44f),
            new Vector2(135f, -116f),
            "Difficulty Curve",
            new Color(0.34f, 0.53f, 0.34f, 1f),
            out qaDifficultyButtonText);

        qaRewardButton = CreateRuntimeButton(
            panelRect,
            "QaRewardButton",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(250f, 44f),
            new Vector2(-135f, -168f),
            "Reward Feel",
            new Color(0.66f, 0.48f, 0.21f, 1f),
            out qaRewardButtonText);

        qaReplayButton = CreateRuntimeButton(
            panelRect,
            "QaReplayButton",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(250f, 44f),
            new Vector2(135f, -168f),
            "One-More-Run Pull",
            new Color(0.54f, 0.33f, 0.62f, 1f),
            out qaReplayButtonText);

        qaPriceButton = CreateRuntimeButton(
            panelRect,
            "QaPriceButton",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(250f, 44f),
            new Vector2(-135f, -220f),
            "Shop Prices",
            new Color(0.31f, 0.5f, 0.58f, 1f),
            out qaPriceButtonText);

        qaShopValueButton = CreateRuntimeButton(
            panelRect,
            "QaShopValueButton",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(250f, 44f),
            new Vector2(135f, -220f),
            "Shop Rewards",
            new Color(0.53f, 0.42f, 0.23f, 1f),
            out qaShopValueButtonText);

        qaAdButton = CreateRuntimeButton(
            panelRect,
            "QaAdButton",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(250f, 44f),
            new Vector2(-135f, -272f),
            "Ad Offers",
            new Color(0.27f, 0.46f, 0.43f, 1f),
            out qaAdButtonText);

        qaDangerButton = CreateRuntimeButton(
            panelRect,
            "QaDangerButton",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(250f, 44f),
            new Vector2(135f, -272f),
            "Danger Multiplier",
            new Color(0.48f, 0.24f, 0.27f, 1f),
            out qaDangerButtonText);

        qaFeatureButton = CreateRuntimeButton(
            panelRect,
            "QaFeatureButton",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(520f, 44f),
            new Vector2(0f, -324f),
            "Feature Clarity",
            new Color(0.29f, 0.35f, 0.54f, 1f),
            out qaFeatureButtonText);

        qaTesterNotesInput = CreateRuntimeInputField(
            panelRect,
            "QaTesterNotesInput",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(520f, 92f),
            new Vector2(0f, -378f),
            "Optional notes: danger multiplier, shop prices, ads, confusing moments...",
            true);

        qaPlayAgainButton = CreateRuntimeButton(
            panelRect,
            "QaPlayAgainButton",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(520f, 48f),
            new Vector2(0f, 138f),
            "Play Again",
            new Color(0.55f, 0.39f, 0.18f, 1f),
            out qaPlayAgainButtonText);

        qaSavePackageButton = CreateRuntimeButton(
            panelRect,
            "QaSavePackageButton",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(210f, 44f),
            new Vector2(-154f, 22f),
            "Save Local Copy",
            new Color(0.17f, 0.46f, 0.55f, 1f),
            out qaSavePackageButtonText);

        qaSharePackageButton = CreateRuntimeButton(
            panelRect,
            "QaSharePackageButton",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(520f, 58f),
            new Vector2(0f, 76f),
            "Send QA Package",
            new Color(0.22f, 0.56f, 0.46f, 1f),
            out qaSharePackageButtonText);

        qaDeletePackageButton = CreateRuntimeButton(
            panelRect,
            "QaDeletePackageButton",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(210f, 44f),
            new Vector2(154f, 22f),
            "Delete Local",
            new Color(0.42f, 0.28f, 0.22f, 1f),
            out qaDeletePackageButtonText);

        if (qaFairnessButton != null)
        {
            qaFairnessButton.onClick.RemoveAllListeners();
            qaFairnessButton.onClick.AddListener(CycleQaFairness);
        }

        if (qaDifficultyButton != null)
        {
            qaDifficultyButton.onClick.RemoveAllListeners();
            qaDifficultyButton.onClick.AddListener(CycleQaDifficulty);
        }

        if (qaRewardButton != null)
        {
            qaRewardButton.onClick.RemoveAllListeners();
            qaRewardButton.onClick.AddListener(CycleQaRewardFeel);
        }

        if (qaReplayButton != null)
        {
            qaReplayButton.onClick.RemoveAllListeners();
            qaReplayButton.onClick.AddListener(CycleQaReplayPull);
        }

        if (qaPriceButton != null)
        {
            qaPriceButton.onClick.RemoveAllListeners();
            qaPriceButton.onClick.AddListener(CycleQaPriceFeel);
        }

        if (qaShopValueButton != null)
        {
            qaShopValueButton.onClick.RemoveAllListeners();
            qaShopValueButton.onClick.AddListener(CycleQaShopValueFeel);
        }

        if (qaAdButton != null)
        {
            qaAdButton.onClick.RemoveAllListeners();
            qaAdButton.onClick.AddListener(CycleQaAdFeel);
        }

        if (qaDangerButton != null)
        {
            qaDangerButton.onClick.RemoveAllListeners();
            qaDangerButton.onClick.AddListener(CycleQaDangerFeel);
        }

        if (qaFeatureButton != null)
        {
            qaFeatureButton.onClick.RemoveAllListeners();
            qaFeatureButton.onClick.AddListener(CycleQaFeatureFeel);
        }

        if (qaTesterNotesInput != null)
        {
            qaTesterNotesInput.onEndEdit.RemoveAllListeners();
            qaTesterNotesInput.onEndEdit.AddListener(_ => QaTestingSystem.SetTesterNotes(qaTesterNotesInput.text));
        }

        if (qaPlayAgainButton != null)
        {
            qaPlayAgainButton.onClick.RemoveAllListeners();
            qaPlayAgainButton.onClick.AddListener(RestartGame);
        }

        if (qaSavePackageButton != null)
        {
            qaSavePackageButton.onClick.RemoveAllListeners();
            qaSavePackageButton.onClick.AddListener(SaveQaPackage);
        }

        if (qaSharePackageButton != null)
        {
            qaSharePackageButton.onClick.RemoveAllListeners();
            qaSharePackageButton.onClick.AddListener(ShareQaPackage);
        }

        if (qaDeletePackageButton != null)
        {
            qaDeletePackageButton.onClick.RemoveAllListeners();
            qaDeletePackageButton.onClick.AddListener(DeleteQaPackage);
        }

        ConfigureQaSurveyQuestionText(qaFairnessButtonText);
        ConfigureQaSurveyQuestionText(qaDifficultyButtonText);
        ConfigureQaSurveyQuestionText(qaRewardButtonText);
        ConfigureQaSurveyQuestionText(qaReplayButtonText);
        ConfigureQaSurveyQuestionText(qaPriceButtonText);
        ConfigureQaSurveyQuestionText(qaShopValueButtonText);
        ConfigureQaSurveyQuestionText(qaAdButtonText);
        ConfigureQaSurveyQuestionText(qaDangerButtonText);
        ConfigureQaSurveyQuestionText(qaFeatureButtonText);
        ConfigureQaSurveyActionText(qaPlayAgainButtonText, 18f, 28f);
        ConfigureQaSurveyActionText(qaSavePackageButtonText, 16f, 24f);
        ConfigureQaSurveyActionText(qaSharePackageButtonText, 22f, 32f);
        ConfigureQaSurveyActionText(qaDeletePackageButtonText, 16f, 24f);

        qaSurveyPanel.SetActive(false);
    }

    void ConfigureQaSurveyQuestionText(TextMeshProUGUI label)
    {
        if (label == null)
            return;

        label.enableAutoSizing = true;
        label.fontSizeMin = 11f;
        label.fontSizeMax = 18f;
        label.lineSpacing = -8f;
        label.alignment = TextAlignmentOptions.Center;
    }

    void ConfigureQaSurveyActionText(TextMeshProUGUI label, float minSize, float maxSize)
    {
        if (label == null)
            return;

        label.enableAutoSizing = true;
        label.fontSizeMin = minSize;
        label.fontSizeMax = maxSize;
        label.lineSpacing = -2f;
        label.alignment = TextAlignmentOptions.Center;
    }

    void CreateRevivePrompt(RectTransform parentRect)
    {
        revivePromptRoot = new GameObject("RevivePromptRoot", typeof(RectTransform), typeof(Image));
        revivePromptRoot.transform.SetParent(parentRect, false);

        RectTransform rootRect = revivePromptRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image overlayImage = revivePromptRoot.GetComponent<Image>();
        overlayImage.color = new Color(0.03f, 0.05f, 0.08f, 0.84f);

        GameObject panelObject = new GameObject("RevivePromptPanel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(revivePromptRoot.transform, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(660f, 360f);
        panelRect.anchoredPosition = new Vector2(0f, -10f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.11f, 0.17f, 0.28f, 0.98f);

        CreateRuntimeLabel(
            panelRect,
            "RevivePromptTitle",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(520f, 58f),
            new Vector2(0f, -30f),
            TextAlignmentOptions.Center,
            28f,
            40f,
            Color.white).text = "Continue This Run?";

        revivePromptBodyText = CreateRuntimeLabel(
            panelRect,
            "RevivePromptBody",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(560f, 132f),
            new Vector2(0f, -122f),
            TextAlignmentOptions.Center,
            22f,
            32f,
            new Color(0.93f, 0.97f, 1f, 1f));

        if (revivePromptBodyText != null)
        {
            revivePromptBodyText.lineSpacing = 8f;
            revivePromptBodyText.text = "Watch an ad to revive once and keep your coin run going.";
        }

        revivePromptAcceptButton = CreateRuntimeButton(
            panelRect,
            "ReviveAcceptButton",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(450f, 72f),
            new Vector2(0f, 106f),
            "Watch Ad: Revive",
            new Color(0.94f, 0.73f, 0.23f, 1f),
            out revivePromptAcceptButtonText);

        if (revivePromptAcceptButton != null)
        {
            revivePromptAcceptButton.onClick.RemoveAllListeners();
            revivePromptAcceptButton.onClick.AddListener(ClaimRewardedRevive);
        }

        if (revivePromptAcceptButtonText != null)
        {
            revivePromptAcceptButtonText.enableAutoSizing = true;
            revivePromptAcceptButtonText.fontSizeMin = 20f;
            revivePromptAcceptButtonText.fontSizeMax = 30f;
        }

        revivePromptDeclineButton = CreateRuntimeButton(
            panelRect,
            "ReviveDeclineButton",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(450f, 64f),
            new Vector2(0f, 24f),
            "End Run",
            new Color(0.34f, 0.4f, 0.48f, 1f),
            out revivePromptDeclineButtonText);

        if (revivePromptDeclineButton != null)
        {
            revivePromptDeclineButton.onClick.RemoveAllListeners();
            revivePromptDeclineButton.onClick.AddListener(SkipRewardedRevive);
        }

        revivePromptRoot.SetActive(false);
    }

    void CreatePauseButton(RectTransform parentRect)
    {
        pauseButton = CreateRuntimeButton(
            parentRect,
            "PauseButton",
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(190f, 78f),
            new Vector2(28f, -28f),
            "Pause",
            new Color(0.18f, 0.26f, 0.42f, 0.96f),
            out pauseButtonText);

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(TogglePause);
        }
    }

    void CreatePauseOverlay(RectTransform parentRect)
    {
        pauseOverlayRoot = new GameObject("PauseOverlayRoot", typeof(RectTransform), typeof(Image));
        pauseOverlayRoot.transform.SetParent(parentRect, false);

        RectTransform rootRect = pauseOverlayRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image overlayImage = pauseOverlayRoot.GetComponent<Image>();
        overlayImage.color = new Color(0.03f, 0.05f, 0.09f, 0.82f);

        GameObject panelObject = new GameObject("PausePanel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(pauseOverlayRoot.transform, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(720f, 700f);
        panelRect.anchoredPosition = new Vector2(0f, -10f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.11f, 0.17f, 0.29f, 1f);

        pauseOverlayTitleText = CreateRuntimeLabel(
            panelRect,
            "PauseTitle",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(520f, 60f),
            new Vector2(0f, -28f),
            TextAlignmentOptions.Center,
            30f,
            40f,
            Color.white);

        Button resumeButton = CreateRuntimeButton(
            panelRect,
            "ResumeButton",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(420f, 72f),
            new Vector2(0f, -122f),
            "Resume",
            new Color(0.25f, 0.53f, 0.34f, 1f),
            out _);

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(ResumeGameplay);
        }

        hapticsStatusText = CreateRuntimeLabel(
            panelRect,
            "HapticsStatus",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(520f, 44f),
            new Vector2(0f, -220f),
            TextAlignmentOptions.Center,
            20f,
            28f,
            new Color(0.88f, 0.94f, 1f, 1f));

        hapticsToggleButton = CreateRuntimeButton(
            panelRect,
            "HapticsToggleButton",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(420f, 68f),
            new Vector2(0f, -282f),
            "Haptics",
            new Color(0.93f, 0.73f, 0.24f, 1f),
            out hapticsToggleButtonText);

        if (hapticsToggleButton != null)
        {
            hapticsToggleButton.onClick.RemoveAllListeners();
            hapticsToggleButton.onClick.AddListener(ToggleHaptics);
        }

        Button tutorialButton = CreateRuntimeButton(
            panelRect,
            "ReplayTutorialButton",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(420f, 68f),
            new Vector2(0f, -364f),
            "How To Play",
            new Color(0.24f, 0.36f, 0.56f, 1f),
            out _);

        if (tutorialButton != null)
        {
            tutorialButton.onClick.RemoveAllListeners();
            tutorialButton.onClick.AddListener(ReplayTutorial);
        }

        Button restartButtonOverlay = CreateRuntimeButton(
            panelRect,
            "RestartFromPauseButton",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(420f, 68f),
            new Vector2(0f, -446f),
            "Restart Run",
            new Color(0.72f, 0.45f, 0.19f, 1f),
            out _);

        if (restartButtonOverlay != null)
        {
            restartButtonOverlay.onClick.RemoveAllListeners();
            restartButtonOverlay.onClick.AddListener(RestartGame);
        }

        Button menuButtonOverlay = CreateRuntimeButton(
            panelRect,
            "MenuFromPauseButton",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(420f, 68f),
            new Vector2(0f, -528f),
            "Main Menu",
            new Color(0.32f, 0.42f, 0.56f, 1f),
            out _);

        if (menuButtonOverlay != null)
        {
            menuButtonOverlay.onClick.RemoveAllListeners();
            menuButtonOverlay.onClick.AddListener(ReturnToMainMenu);
        }

        pauseOverlayRoot.SetActive(false);
    }

    void CreateTutorialOverlay(RectTransform parentRect)
    {
        tutorialOverlayRoot = new GameObject("TutorialOverlayRoot", typeof(RectTransform), typeof(Image));
        tutorialOverlayRoot.transform.SetParent(parentRect, false);

        RectTransform rootRect = tutorialOverlayRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image overlayImage = tutorialOverlayRoot.GetComponent<Image>();
        overlayImage.color = new Color(0.02f, 0.05f, 0.1f, 0.88f);

        GameObject panelObject = new GameObject("TutorialPanel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(tutorialOverlayRoot.transform, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(860f, 900f);
        panelRect.anchoredPosition = new Vector2(0f, -10f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.1f, 0.18f, 0.31f, 1f);

        CreateRuntimeLabel(
            panelRect,
            "TutorialTitle",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(700f, 86f),
            new Vector2(0f, -34f),
            TextAlignmentOptions.Center,
            38f,
            56f,
            Color.white).text = "How To Play";

        tutorialBodyText = CreateRuntimeLabel(
            panelRect,
            "TutorialBody",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(740f, 460f),
            new Vector2(0f, -170f),
            TextAlignmentOptions.Center,
            34f,
            48f,
            new Color(0.93f, 0.97f, 1f, 1f));

        if (tutorialBodyText != null)
        {
            RefreshTutorialOverlayContent(false);
        }

        tutorialPrimaryButton = CreateRuntimeButton(
            panelRect,
            "TutorialPrimaryButton",
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(420f, 92f),
            new Vector2(0f, 42f),
            "Start Run",
            new Color(0.25f, 0.53f, 0.34f, 1f),
            out tutorialPrimaryButtonText);

        if (tutorialPrimaryButton != null)
        {
            tutorialPrimaryButton.onClick.RemoveAllListeners();
            tutorialPrimaryButton.onClick.AddListener(CloseTutorialOverlay);
        }

        tutorialOverlayRoot.SetActive(false);
    }

    Button CreateRuntimeButton(
        RectTransform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 size,
        Vector2 anchoredPosition,
        string text,
        Color backgroundColor,
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
        buttonImage.color = backgroundColor;

        GameObject labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(10f, 8f);
        labelRect.offsetMax = new Vector2(-10f, -8f);

        label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.alignment = TextAlignmentOptions.Center;
        label.enableAutoSizing = true;
        label.fontSizeMin = 28f;
        label.fontSizeMax = 40f;
        label.color = Color.white;

        if (runtimeFont != null)
            label.font = runtimeFont;

        Button button = buttonObject.GetComponent<Button>();
        Color labelColor = backgroundColor.r > 0.75f && backgroundColor.g > 0.5f
            ? new Color(0.08f, 0.095f, 0.09f, 1f)
            : StudioUiTheme.Text;
        StudioUiTheme.ApplyButtonChrome(button, backgroundColor, label, labelColor);
        return button;
    }

    TMP_InputField CreateRuntimeInputField(
        RectTransform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 size,
        Vector2 anchoredPosition,
        string placeholderText,
        bool multiLine)
    {
        GameObject inputObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        inputObject.transform.SetParent(parent, false);

        RectTransform inputRect = inputObject.GetComponent<RectTransform>();
        inputRect.anchorMin = anchorMin;
        inputRect.anchorMax = anchorMax;
        inputRect.pivot = pivot;
        inputRect.sizeDelta = size;
        inputRect.anchoredPosition = anchoredPosition;

        Image inputImage = inputObject.GetComponent<Image>();
        inputImage.color = new Color(0.07f, 0.1f, 0.15f, 0.96f);

        GameObject viewportObject = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        viewportObject.transform.SetParent(inputObject.transform, false);

        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(14f, 8f);
        viewportRect.offsetMax = new Vector2(-14f, -8f);

        GameObject textObject = new GameObject("Text", typeof(RectTransform));
        textObject.transform.SetParent(viewportObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.TopLeft;
        text.enableAutoSizing = true;
        text.fontSizeMin = 14f;
        text.fontSizeMax = 20f;
        text.color = Color.white;

        if (runtimeFont != null)
            text.font = runtimeFont;

        GameObject placeholderObject = new GameObject("Placeholder", typeof(RectTransform));
        placeholderObject.transform.SetParent(viewportObject.transform, false);

        RectTransform placeholderRect = placeholderObject.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;

        TextMeshProUGUI placeholder = placeholderObject.AddComponent<TextMeshProUGUI>();
        placeholder.text = placeholderText;
        placeholder.alignment = TextAlignmentOptions.TopLeft;
        placeholder.enableAutoSizing = true;
        placeholder.fontSizeMin = 14f;
        placeholder.fontSizeMax = 20f;
        placeholder.color = new Color(0.74f, 0.8f, 0.86f, 1f);

        if (runtimeFont != null)
            placeholder.font = runtimeFont;

        TMP_InputField input = inputObject.GetComponent<TMP_InputField>();
        input.textComponent = text;
        input.placeholder = placeholder;
        input.lineType = multiLine
            ? TMP_InputField.LineType.MultiLineNewline
            : TMP_InputField.LineType.SingleLine;
        input.characterLimit = multiLine ? 600 : 48;
        input.text = QaTestingSystem.GetTesterNotes();
        StudioUiTheme.ApplyInput(input);
        return input;
    }

    void RefreshPauseOverlayState()
    {
        if (pauseOverlayTitleText != null)
            pauseOverlayTitleText.text = "Paused";

        if (hapticsStatusText != null)
            hapticsStatusText.text = "Haptics: " + (GameSettings.IsHapticsEnabled() ? "On" : "Off");

        if (hapticsToggleButtonText != null)
            hapticsToggleButtonText.text = GameSettings.IsHapticsEnabled() ? "Turn Haptics Off" : "Turn Haptics On";

        if (pauseButtonText != null)
            pauseButtonText.text = IsPauseOverlayVisible() ? "Paused" : "Pause";
    }

    void UpdateBestScoreHud()
    {
        if (bestScoreHudText == null)
            return;

        string dangerLabel = BuildDangerHudLabel();

        if (isDailyChallengeRun)
        {
            string challengeLabel = gameEnded
                ? DailyChallengeSystem.GetBestProgressLabel(activeDailyChallenge)
                : DailyChallengeSystem.GetCurrentRunLabel(activeDailyChallenge, Mathf.FloorToInt(score), runCoinsEarned);

            bestScoreHudText.text = string.IsNullOrEmpty(dangerLabel)
                ? challengeLabel
                : challengeLabel + "\n" + dangerLabel;
            return;
        }

        bestScoreHudText.text = string.IsNullOrEmpty(dangerLabel)
            ? "Best: " + bestScore
            : "Best: " + bestScore + "\n" + dangerLabel;
    }

    void ShowPostRunSummary(int finalScore)
    {
        if (postRunPanel == null || postRunSummaryText == null)
            return;

        int completedMissionCount = DailyMissionSystem.GetCompletedCount();
        int claimableMissionCount = DailyMissionSystem.GetClaimableCount();

        string summaryText;

        if (isDailyChallengeRun)
        {
            summaryText =
                activeDailyChallenge.title +
                "\n" + DailyChallengeSystem.GetCurrentRunLabel(activeDailyChallenge, finalScore, runCoinsEarned) +
                "\n" + DailyChallengeSystem.GetBestProgressLabel(activeDailyChallenge);

            if (DailyChallengeSystem.CanClaimReward())
            {
                summaryText += "\n<color=#FFD876>Challenge reward ready in menu</color>";
            }
            else if (activeDailyChallenge.rewardClaimed)
            {
                summaryText += "\n<color=#7FF0A6>Reward already claimed today</color>";
            }
            else if (activeDailyChallenge.bestScore >= activeDailyChallenge.targetScore)
            {
                summaryText += "\n<color=#7FF0A6>Challenge cleared</color>";
            }
            else
            {
                summaryText += "\n" + DailyChallengeSystem.GetNeedsMoreLabel(activeDailyChallenge);
            }
        }
        else
        {
            int displayedCoinGain = runCoinsEarned * (postRunDoubleCoinsClaimed ? 2 : 1);
            summaryText =
                "Run Score " + finalScore +
                "\nBest " + bestScore + "   Coins +" + displayedCoinGain +
                "\nLevel " + currentLevel + "   Missions " + completedMissionCount + "/3";

            if (bestDangerComboCount > 1 && lastProgressionResult.ExperienceGained > 0)
            {
                summaryText +=
                    "\nDanger x" + bestDangerComboCount +
                    "   XP +" + lastProgressionResult.ExperienceGained;
            }
            else if (bestDangerComboCount > 1)
            {
                summaryText += "\nDanger x" + bestDangerComboCount;
            }
            else if (lastProgressionResult.ExperienceGained > 0)
            {
                summaryText += "\nXP +" + lastProgressionResult.ExperienceGained;
            }

            if (claimableMissionCount > 0)
            {
                summaryText +=
                    "\n<color=#FFD876>" +
                    claimableMissionCount +
                    (claimableMissionCount == 1 ? " reward ready in menu" : " rewards ready in menu") +
                    "</color>";
            }

        if (postRunDoubleCoinsClaimed)
            {
                summaryText += "\n<color=#7FF0A6>Double coins claimed</color>";
            }
            else if (CanOfferPostRunDoubleCoins())
            {
                summaryText += "\n<color=#FFD876>Watch an ad for +" + runCoinsEarned + " bonus coins</color>";
            }
        }

        if (newBestScore)
            summaryText += "\n<color=#7FF0A6>New Best!</color>";

        if (lastProgressionResult.LeveledUp)
            summaryText += "\n<color=#A9F3BC>" + lastProgressionResult.LevelUpLabel + "</color>";

        if (lastProgressionResult.HasRewardBundle)
            summaryText += "\n<color=#FFD876>" + lastProgressionResult.RewardBundleLabel + "</color>";

        if (lastProgressionResult.HasMilestoneSummary)
            summaryText += "\n<color=#9EDAFF>" + lastProgressionResult.MilestoneSummaryLine + "</color>";

        if (!string.IsNullOrEmpty(lastProgressionResult.PrimaryGoalSummary))
            summaryText += "\n" + lastProgressionResult.PrimaryGoalSummary;

        if (!string.IsNullOrEmpty(lastPerformanceRewardSummary))
            summaryText += "\n<color=#FFD876>" + lastPerformanceRewardSummary + "</color>";

        if (showPostRunReviewButton)
            summaryText += "\n<color=#A9F3BC>Enjoying the run? Rate it on Google Play.</color>";

        if (!string.IsNullOrEmpty(lastShareRewardSummary))
            summaryText += "\n<color=#8FF2C5>" + lastShareRewardSummary + "</color>";
        else
            summaryText += "\n" + GetAvailableShareRewardLine();

        postRunSummaryText.text = summaryText;
        postRunPanel.SetActive(true);
        RefreshPostRunActionButtons();
    }

    bool CanOfferRewardedRevive()
    {
        return !gameEnded &&
               !isDailyChallengeRun &&
               !rewardedReviveUsed &&
               !isAwaitingRewardedRevive &&
               MonetizationManager.Instance != null &&
               MonetizationManager.Instance.CanShowRewardedAd(RewardedOfferType.MidRunRevive);
    }

    void ShowRewardedRevivePrompt()
    {
        if (revivePromptRoot == null)
        {
            GameOver();
            return;
        }

        isAwaitingRewardedRevive = true;
        SetPauseOverlayVisible(false);
        SetTutorialOverlayVisible(false);

        if (revivePromptBodyText != null)
            revivePromptBodyText.text = "Watch an ad to revive once and continue from this run.";

        if (revivePromptAcceptButton != null)
            revivePromptAcceptButton.interactable = true;

        if (revivePromptAcceptButtonText != null)
            revivePromptAcceptButtonText.text = "Watch Ad: Revive";

        if (revivePromptDeclineButton != null)
            revivePromptDeclineButton.interactable = true;

        if (revivePromptDeclineButtonText != null)
            revivePromptDeclineButtonText.text = "End Run";

        SetRevivePromptVisible(true);
        GameSettings.TriggerHaptic();
    }

    bool CanOfferPostRunDoubleCoins()
    {
        return gameEnded &&
               !isDailyChallengeRun &&
               runCoinsEarned > 0 &&
               !postRunDoubleCoinsClaimed &&
               MonetizationManager.Instance != null &&
               MonetizationManager.Instance.CanShowRewardedAd(RewardedOfferType.PostRunDoubleCoins);
    }

    void RefreshPostRunDoubleCoinsButton()
    {
        if (postRunDoubleCoinsButton == null)
            return;

        bool canOffer = CanOfferPostRunDoubleCoins();
        bool showClaimedState = gameEnded && postRunDoubleCoinsClaimed;
        postRunDoubleCoinsButton.gameObject.SetActive(canOffer || showClaimedState);
        postRunDoubleCoinsButton.interactable = canOffer;

        if (postRunDoubleCoinsButtonText != null)
        {
            if (postRunDoubleCoinsClaimed)
                postRunDoubleCoinsButtonText.text = "Bonus Claimed";
            else
                postRunDoubleCoinsButtonText.text = "Watch Ad: +" + runCoinsEarned + " Coins";
        }
    }

    void RefreshPostRunShareButton()
    {
        if (postRunShareButton == null)
            return;

        bool canShare = gameEnded;
        postRunShareButton.gameObject.SetActive(canShare);
        postRunShareButton.interactable = canShare;

        if (postRunShareButtonText != null)
        {
            if (canShare && ShareGrowthSystem.CanClaimToday())
            {
                int shareCoins = ShareGrowthSystem.GetPotentialRewardCoins(
                    isDailyChallengeRun,
                    newBestScore,
                    ShareGrowthSystem.ShouldGrantDepthBonus(lastFinishedScore, lastFinishedLevel));
                postRunShareButtonText.text = isDailyChallengeRun
                    ? "Share Challenge +" + shareCoins
                    : "Share +" + shareCoins + " Coins";
            }
            else if (canShare)
            {
                postRunShareButtonText.text = "Shared Today";
            }
            else
            {
                postRunShareButtonText.text = isDailyChallengeRun ? "Share Challenge Run" : "Share Result";
            }
        }
    }

    void RefreshPostRunReviewButton()
    {
        if (postRunReviewButton == null)
            return;

        postRunReviewButton.gameObject.SetActive(gameEnded && showPostRunReviewButton);
        postRunReviewButton.interactable = showPostRunReviewButton;

        if (postRunReviewButtonText != null)
            postRunReviewButtonText.text = "Rate on Google Play";
    }

    void RefreshPostRunActionButtons()
    {
        RefreshPostRunDoubleCoinsButton();
        RefreshPostRunShareButton();
        RefreshPostRunReviewButton();

        if (QaTestingSystem.IsQaModeEnabled() && !qaPracticeRunActive && gameEnded)
        {
            if (postRunDoubleCoinsButton != null)
                postRunDoubleCoinsButton.gameObject.SetActive(false);

            if (postRunShareButton != null)
                postRunShareButton.gameObject.SetActive(false);

            if (postRunReviewButton != null)
                postRunReviewButton.gameObject.SetActive(false);
        }

        RefreshQaSurveyUi();
    }

    void RefreshQaFeedbackUi()
    {
        bool showCaptureOverlay = qaCaptureNoticePending || QaTestingSystem.IsAwaitingConsent();
        SetQaCaptureOverlayVisible(showCaptureOverlay);

        if (qaCaptureOverlayTitleText != null)
            qaCaptureOverlayTitleText.text = qaCaptureNoticePending
                ? "Before Recording Starts"
                : "QA Capture Permission";

        if (qaCaptureOverlayBodyText != null)
        {
            qaCaptureOverlayBodyText.text = qaCaptureNoticePending
                ? "This temporary QA build records gameplay, in-game menus, and the post-run survey after Android permission.\n\n" +
                  "Do not enter private information while recording is active.\nMicrophone audio is not captured.\n\n" +
                  "Tap below when ready. If Android shows Share one app, switch it to Share full screen, then approve."
                : "Android is showing or preparing its screen-capture prompt for this run.\n" +
                  "Choose Cavern Veerfall if Android offers that option, or share the full screen only for this QA run.\n\n" +
                  QaTestingSystem.GetLiveCaptureStatus();
        }

        if (qaCaptureOverlayContinueButton != null)
        {
            qaCaptureOverlayContinueButton.gameObject.SetActive(qaCaptureNoticePending);
            qaCaptureOverlayContinueButton.interactable = qaCaptureNoticePending;
        }

        if (qaCaptureOverlayContinueButtonText != null)
        {
            qaCaptureOverlayContinueButtonText.text = "I Agree - Open Android Prompt";
        }

        RefreshQaSurveyUi();
    }

    void RefreshQaSurveyUi()
    {
        if (qaSurveyPanel == null)
            return;

        bool showSurvey = QaTestingSystem.IsQaModeEnabled() && gameEnded && !qaPracticeRunActive;
        qaSurveyPanel.SetActive(showSurvey);

        if (restartButton != null)
            restartButton.SetActive(gameEnded && !showSurvey);

        if (!showSurvey)
            return;

        if (qaSurveyStatusText != null)
            qaSurveyStatusText.text = QaTestingSystem.GetSurveyStatusText();

        if (qaFairnessButtonText != null)
            qaFairnessButtonText.text = QaTestingSystem.GetFairnessLabel();

        if (qaDifficultyButtonText != null)
            qaDifficultyButtonText.text = QaTestingSystem.GetDifficultyLabel();

        if (qaRewardButtonText != null)
            qaRewardButtonText.text = QaTestingSystem.GetRewardLabel();

        if (qaReplayButtonText != null)
            qaReplayButtonText.text = QaTestingSystem.GetReplayLabel();

        if (qaPriceButtonText != null)
            qaPriceButtonText.text = QaTestingSystem.GetPriceLabel();

        if (qaShopValueButtonText != null)
            qaShopValueButtonText.text = QaTestingSystem.GetShopValueLabel();

        if (qaAdButtonText != null)
            qaAdButtonText.text = QaTestingSystem.GetAdLabel();

        if (qaDangerButtonText != null)
            qaDangerButtonText.text = QaTestingSystem.GetDangerLabel();

        if (qaFeatureButtonText != null)
            qaFeatureButtonText.text = QaTestingSystem.GetFeatureLabel();

        if (qaTesterNotesInput != null && !qaTesterNotesInput.isFocused)
            qaTesterNotesInput.SetTextWithoutNotify(QaTestingSystem.GetTesterNotes());

        if (qaPlayAgainButton != null)
        {
            qaPlayAgainButton.gameObject.SetActive(true);
            qaPlayAgainButton.interactable = gameEnded;
        }

        bool uploadConfigured = QaTestingSystem.HasUploadTargetConfigured();

        if (qaSavePackageButton != null)
            qaSavePackageButton.gameObject.SetActive(!uploadConfigured);

        if (qaDeletePackageButton != null)
            qaDeletePackageButton.gameObject.SetActive(!uploadConfigured);

        if (qaSavePackageButtonText != null)
            qaSavePackageButtonText.text = QaTestingSystem.GetSaveButtonLabel();

        if (qaSharePackageButtonText != null)
            qaSharePackageButtonText.text = QaTestingSystem.GetShareButtonLabel();

        if (qaDeletePackageButtonText != null)
            qaDeletePackageButtonText.text = "Delete Local";

        if (qaSavePackageButton != null)
            qaSavePackageButton.interactable = !uploadConfigured && QaTestingSystem.CanSaveCurrentRun();

        if (qaSharePackageButton != null)
            qaSharePackageButton.interactable = QaTestingSystem.CanShareCurrentRun();

        if (qaDeletePackageButton != null)
            qaDeletePackageButton.interactable = !uploadConfigured && QaTestingSystem.HasLastPackage();
    }

    public void CycleQaFairness()
    {
        QaTestingSystem.CycleFairness();
        GameSettings.TriggerHaptic();
        RefreshQaSurveyUi();
    }

    public void CycleQaDifficulty()
    {
        QaTestingSystem.CycleDifficulty();
        GameSettings.TriggerHaptic();
        RefreshQaSurveyUi();
    }

    public void CycleQaRewardFeel()
    {
        QaTestingSystem.CycleRewardFeel();
        GameSettings.TriggerHaptic();
        RefreshQaSurveyUi();
    }

    public void CycleQaReplayPull()
    {
        QaTestingSystem.CycleReplayPull();
        GameSettings.TriggerHaptic();
        RefreshQaSurveyUi();
    }

    public void CycleQaPriceFeel()
    {
        QaTestingSystem.CyclePriceFeel();
        GameSettings.TriggerHaptic();
        RefreshQaSurveyUi();
    }

    public void CycleQaShopValueFeel()
    {
        QaTestingSystem.CycleShopValueFeel();
        GameSettings.TriggerHaptic();
        RefreshQaSurveyUi();
    }

    public void CycleQaAdFeel()
    {
        QaTestingSystem.CycleAdFeel();
        GameSettings.TriggerHaptic();
        RefreshQaSurveyUi();
    }

    public void CycleQaDangerFeel()
    {
        QaTestingSystem.CycleDangerFeel();
        GameSettings.TriggerHaptic();
        RefreshQaSurveyUi();
    }

    public void CycleQaFeatureFeel()
    {
        QaTestingSystem.CycleFeatureFeel();
        GameSettings.TriggerHaptic();
        RefreshQaSurveyUi();
    }

    public void SaveQaPackage()
    {
        if (!QaTestingSystem.CanSaveCurrentRun())
            return;

        SaveQaTesterNotes();
        QaTestingSystem.SaveCurrentRunToDownloads();
        GameSettings.TriggerHaptic();
        RefreshQaSurveyUi();
    }

    public void ShareQaPackage()
    {
        if (!QaTestingSystem.CanShareCurrentRun())
            return;

        SaveQaTesterNotes();
        QaTestingSystem.ShareCurrentRun();
        GameSettings.TriggerHaptic();
        RefreshQaSurveyUi();
    }

    public void ConfirmQaCaptureNotice()
    {
        if (!qaCaptureNoticePending)
            return;

        qaCaptureNoticePending = false;
        QaTestingSystem.BeginGameplayCapture();
        GameSettings.TriggerHaptic();
        RefreshQaFeedbackUi();
    }

    public void HandleQaPermissionPromptResolved()
    {
        qaCaptureNoticePending = false;
        RefreshQaFeedbackUi();
        UpdateOverlayTimeScale();
        Debug.Log(
            "[QA-UI] Prompt resolved | overlay=" +
            IsQaCaptureOverlayVisible() +
            " | awaitingConsent=" +
            QaTestingSystem.IsAwaitingConsent() +
            " | timeScale=" +
            Time.timeScale.ToString("0.00"));
    }

    public void DeleteQaPackage()
    {
        QaTestingSystem.DeleteLastPackage();
        GameSettings.TriggerHaptic();
        RefreshQaSurveyUi();
    }

    void SaveQaTesterNotes()
    {
        if (qaTesterNotesInput != null)
            QaTestingSystem.SetTesterNotes(qaTesterNotesInput.text);
    }

    public void SharePostRunResult()
    {
        if (!gameEnded)
            return;

        LaunchAnalytics.RecordShareTapped("post_run", isDailyChallengeRun, lastFinishedScore, lastFinishedLevel);
        bool launchedShareSheet = MobileGrowthActions.ShareText(
            "Share your run",
            BuildShareSubject(),
            BuildShareBody());
        GameSettings.TriggerHaptic();

        if (ShareGrowthSystem.TryClaimShareReward(
            isDailyChallengeRun,
            newBestScore,
            lastFinishedScore,
            lastFinishedLevel,
            out ShareGrowthReward reward))
        {
            totalCoins = PlayerPrefs.GetInt("TotalCoins", totalCoins);
            lastShareRewardSummary = ShareGrowthSystem.GetRewardBreakdownLabel(reward);

            if (totalCoinsText != null)
                totalCoinsText.text = "Coins " + totalCoins;

            if (postRunShareButtonText != null)
                postRunShareButtonText.text = "Echo Cache Claimed";

            ShowPostRunSummary(lastFinishedScore);
            return;
        }

        if (postRunShareButtonText != null)
            postRunShareButtonText.text = launchedShareSheet ? "Shared Today" : "Copied Result";
    }

    public void OpenStoreReviewPage()
    {
        if (!showPostRunReviewButton)
            return;

        bool launchedStoreListing = MobileGrowthActions.OpenStoreListing();
        LaunchAnalytics.RecordReviewTapped("post_run", launchedStoreListing);

        if (launchedStoreListing)
        {
            GameSettings.MarkReviewPromptCompleted();
            showPostRunReviewButton = false;
            GameSettings.TriggerHaptic();
            ShowPostRunSummary(lastFinishedScore);
        }
    }

    string BuildShareSubject()
    {
        if (isDailyChallengeRun)
            return "Can you beat today's Cavern Veerfall cave route?";

        if (newBestScore)
            return "I just set a new Cavern Veerfall cave record";

        return "Can you beat my Cavern Veerfall cave run?";
    }

    string BuildShareBody()
    {
        string storeUrl = MobileGrowthActions.GetProductionStoreUrl();
        PlayerProfileSnapshot profileSnapshot = PlayerProgressionSystem.GetProfileSnapshot();
        string dangerSuffix = bestDangerComboCount > 1 ? " | Danger x" + bestDangerComboCount : string.Empty;

        if (isDailyChallengeRun)
        {
            return
                "I just finished today's \"" + activeDailyChallenge.title + "\" route in Cavern Veerfall.\n" +
                "Score " + lastFinishedScore + " | Coins +" + GetDisplayedRunCoinGain() + dangerSuffix + "\n" +
                profileSnapshot.RankTitle + " Lv " + profileSnapshot.Level + "\n" +
                "Download Cavern Veerfall and take the same cave challenge.\n" +
                storeUrl;
        }

        string openingLine = newBestScore
            ? "New cave record in Cavern Veerfall."
            : "Just finished another deep run in Cavern Veerfall.";

        return
            openingLine + "\n" +
            "Score " + lastFinishedScore + " | Depth " + lastFinishedLevel + " | Coins +" + GetDisplayedRunCoinGain() + dangerSuffix + "\n" +
            profileSnapshot.RankTitle + " Lv " + profileSnapshot.Level + "\n" +
            "Download Cavern Veerfall and see if you can beat it.\n" +
            storeUrl;
    }

    void UpdateDangerComboState()
    {
        if (dangerComboCount <= 0)
            return;

        dangerComboDecayTimer -= Time.deltaTime;

        if (dangerComboDecayTimer > 0f)
            return;

        dangerComboCount = Mathf.Max(0, dangerComboCount - 1);
        dangerComboDecayTimer = dangerComboCount > 0 ? DangerComboDecayStepDuration : 0f;
    }

    public void RegisterNearMiss(float closeness01)
    {
        if (gameEnded)
            return;

        runNearMissCount += 1;
        dangerComboCount = Mathf.Min(dangerComboCount + 1, 12);
        bestDangerComboCount = Mathf.Max(bestDangerComboCount, dangerComboCount);
        dangerComboDecayTimer = DangerComboGraceDuration;

        int scoreBonus = Mathf.RoundToInt((5f + (dangerComboCount * 2.5f)) * Mathf.Lerp(0.85f, 1.35f, Mathf.Clamp01(closeness01)));
        score += Mathf.Max(3, scoreBonus);

        int bonusCoins = 0;

        if (dangerComboCount == 3 || dangerComboCount == 6 || dangerComboCount == 9)
            bonusCoins = 1;
        else if (dangerComboCount >= 10 && dangerComboCount % 2 == 0)
            bonusCoins = 2;

        if (bonusCoins > 0)
        {
            totalCoins += bonusCoins;
            runCoinsEarned += bonusCoins;
            DailyMissionSystem.RegisterCoinsCollected(bonusCoins);
            PlayerPrefs.SetInt("TotalCoins", totalCoins);
            PlayerPrefs.Save();

            if (totalCoinsText != null)
                totalCoinsText.text = "Coins " + totalCoins;

            if (isDailyChallengeRun)
                UpdateUpgradeText();

            audioDirector?.PlayCoinPickup();
        }

        if (dangerComboCount >= 3)
            GameSettings.TriggerHaptic();

        UpdateBestScoreHud();
    }

    float GetDangerScoreMultiplier()
    {
        if (dangerComboCount <= 1)
            return 1f;

        return 1f + (Mathf.Min(dangerComboCount, 8) * 0.08f);
    }

    string BuildDangerHudLabel()
    {
        if (!gameEnded && dangerComboCount > 1)
            return "Danger x" + dangerComboCount + "   Score x" + GetDangerScoreMultiplier().ToString("0.00");

        if (gameEnded && bestDangerComboCount > 1)
            return "Peak Danger x" + bestDangerComboCount;

        return string.Empty;
    }

    int GetDisplayedRunCoinGain()
    {
        return runCoinsEarned * (postRunDoubleCoinsClaimed ? 2 : 1);
    }

    string AwardRunPerformanceRewards(int finalScore, int levelReached)
    {
        if (isDailyChallengeRun)
            return string.Empty;

        int bonusCoins = 3;
        bonusCoins += Mathf.Max(0, levelReached - 1) * 4;
        bonusCoins += Mathf.Clamp(finalScore / 18, 0, 20);
        bonusCoins += Mathf.Clamp(runNearMissCount / 3, 0, 8);

        if (bestDangerComboCount >= 4)
            bonusCoins += 4 + ((Mathf.Min(bestDangerComboCount, 10) - 4) * 2);

        if (newBestScore)
            bonusCoins += 10;

        UpgradeType bonusUpgrade = UpgradeType.Shield;
        int bonusUpgradeAmount = 0;

        if (levelReached >= 10)
        {
            bonusUpgrade = UpgradeType.Bomb;
            bonusUpgradeAmount = 1;
        }
        else if (levelReached >= 7)
        {
            bonusUpgrade = UpgradeType.ExtraLife;
            bonusUpgradeAmount = 1;
        }
        else if (levelReached >= 4)
        {
            bonusUpgrade = UpgradeType.Shield;
            bonusUpgradeAmount = 1;
        }

        if (bonusCoins <= 0 && bonusUpgradeAmount <= 0)
            return string.Empty;

        totalCoins += bonusCoins;
        runCoinsEarned += bonusCoins;
        PlayerPrefs.SetInt("TotalCoins", totalCoins);

        if (UpgradeInventory.Instance != null && bonusUpgradeAmount > 0)
            UpgradeInventory.Instance.AddUpgrade(bonusUpgrade, bonusUpgradeAmount);

        PlayerPrefs.Save();

        List<string> rewardParts = new List<string>();

        if (bonusCoins > 0)
            rewardParts.Add("+" + bonusCoins + " coins");

        if (bonusUpgradeAmount > 0)
            rewardParts.Add("+" + bonusUpgradeAmount + " " + UpgradeInventory.GetDisplayName(bonusUpgrade));

        return "Expedition cache: " + string.Join("  |  ", rewardParts.ToArray());
    }

    string GetAvailableShareRewardLine()
    {
        if (!ShareGrowthSystem.CanClaimToday())
            return "<color=#8EDCFB>Echo Cache claimed today</color>";

        ShareGrowthReward previewReward = ShareGrowthSystem.GetPreviewReward();
        int totalPotentialCoins = ShareGrowthSystem.GetPotentialRewardCoins(
            isDailyChallengeRun,
            newBestScore,
            ShareGrowthSystem.ShouldGrantDepthBonus(lastFinishedScore, lastFinishedLevel));

        return
            "<color=#8FF2C5>Share this run for Day " +
            previewReward.rewardDay +
            " Echo Cache: +" +
            totalPotentialCoins +
            " coins + " +
            previewReward.bonusAmount +
            " " +
            UpgradeInventory.GetDisplayName(previewReward.bonusUpgrade) +
            "</color>";
    }

    public void ClaimPostRunDoubleCoins()
    {
        if (!CanOfferPostRunDoubleCoins())
            return;

        if (postRunDoubleCoinsButton != null)
            postRunDoubleCoinsButton.interactable = false;

        if (postRunDoubleCoinsButtonText != null)
            postRunDoubleCoinsButtonText.text = "Loading Ad...";

        LaunchAnalytics.RecordRewardedOfferRequested("post_run_double_coins", runCoinsEarned);
        MonetizationManager.Instance.ShowRewardedAd(
            RewardedOfferType.PostRunDoubleCoins,
            HandlePostRunDoubleCoinsResult);
    }

    public void ClaimRewardedRevive()
    {
        if (!CanOfferRewardedRevive())
            return;

        if (revivePromptAcceptButton != null)
            revivePromptAcceptButton.interactable = false;

        if (revivePromptDeclineButton != null)
            revivePromptDeclineButton.interactable = false;

        if (revivePromptAcceptButtonText != null)
            revivePromptAcceptButtonText.text = "Loading Ad...";

        LaunchAnalytics.RecordRewardedOfferRequested("mid_run_revive", 1);
        MonetizationManager.Instance.ShowRewardedAd(
            RewardedOfferType.MidRunRevive,
            HandleRewardedReviveResult);
    }

    public void SkipRewardedRevive()
    {
        if (!isAwaitingRewardedRevive)
            return;

        isAwaitingRewardedRevive = false;
        SetRevivePromptVisible(false);
        GameOver();
    }

    void HandleRewardedReviveResult(bool rewarded)
    {
        LaunchAnalytics.RecordRewardedOfferResult("mid_run_revive", rewarded, 1);
        isAwaitingRewardedRevive = false;

        if (!rewarded)
        {
            SetRevivePromptVisible(false);
            GameOver();
            return;
        }

        rewardedReviveUsed = true;
        SetRevivePromptVisible(false);
        StartCoroutine(RevivePlayerRoutine());
        RefreshRunUpgradeButtons();
        UpdateUpgradeText();
    }

    void HandlePostRunDoubleCoinsResult(bool rewarded)
    {
        LaunchAnalytics.RecordRewardedOfferResult("post_run_double_coins", rewarded, runCoinsEarned);

        if (!rewarded)
        {
            RefreshPostRunDoubleCoinsButton();
            return;
        }

        if (postRunDoubleCoinsClaimed)
            return;

        postRunDoubleCoinsClaimed = true;
        totalCoins += runCoinsEarned;
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.Save();

        if (totalCoinsText != null)
            totalCoinsText.text = "Coins " + totalCoins;

        GameSettings.TriggerHaptic();
        ShowPostRunSummary(Mathf.FloorToInt(score));
    }

    void ShowTutorialIfNeeded()
    {
        bool shouldShowPracticeTutorial = qaPracticeRunActive;

        if (GameSettings.HasSeenTutorial() && !shouldShowPracticeTutorial)
            return;

        markTutorialSeenOnClose = !GameSettings.HasSeenTutorial();
        RefreshTutorialOverlayContent(shouldShowPracticeTutorial || QaTestingSystem.IsQaModeEnabled());
        SetTutorialOverlayVisible(true);
    }

    public void TogglePause()
    {
        if (gameEnded || IsTutorialOverlayVisible() || IsRevivePromptVisible() || IsQaCaptureOverlayVisible())
            return;

        SetPauseOverlayVisible(!IsPauseOverlayVisible());
    }

    public void ResumeGameplay()
    {
        SetPauseOverlayVisible(false);
    }

    public void ReturnToMainMenu()
    {
        QaTestingSystem.StopCapture("return_to_menu");
        Time.timeScale = 1f;

        if (isDailyChallengeRun)
            DailyChallengeSystem.ClearActiveRun();

        SceneManager.LoadScene("MainMenu");
    }

    public void ReplayTutorial()
    {
        markTutorialSeenOnClose = false;
        RefreshTutorialOverlayContent(QaTestingSystem.IsQaModeEnabled() || qaPracticeRunActive);
        SetTutorialOverlayVisible(true);
    }

    public void CloseTutorialOverlay()
    {
        if (markTutorialSeenOnClose)
        {
            GameSettings.MarkTutorialSeen();
            markTutorialSeenOnClose = false;
        }

        SetTutorialOverlayVisible(false);
    }

    void RefreshTutorialOverlayContent(bool includeQaChecklist)
    {
        if (tutorialBodyText == null)
            return;

        tutorialBodyText.text =
            "Avoid every obstacle, no matter the color." +
            "\nCollect the gold coins to grow your stash." +
            "\nEquip up to 3 upgrades in Inventory before each run." +
            "\nTap a run upgrade once to keep shields, buffs, and extra lives auto-armed.";

        if (includeQaChecklist)
        {
            tutorialBodyText.text +=
                "\n\nTester checklist:" +
                "\n- this first practice run is for learning the flow only" +
                "\n- your real QA runs auto-record after Android capture consent" +
                "\n- after each QA run, save the bundle or share it from the post-run screen";
            tutorialBodyText.lineSpacing = 14f;
        }
        else
        {
            tutorialBodyText.lineSpacing = 18f;
        }

        if (tutorialPrimaryButtonText != null)
            tutorialPrimaryButtonText.text = includeQaChecklist ? "Start Practice Run" : "Start Run";
    }

    public void ToggleHaptics()
    {
        GameSettings.SetHapticsEnabled(!GameSettings.IsHapticsEnabled());
        RefreshPauseOverlayState();
        GameSettings.TriggerHaptic();
    }

    void SetPauseOverlayVisible(bool isVisible)
    {
        if (pauseOverlayRoot != null)
            pauseOverlayRoot.SetActive(isVisible);

        RefreshPauseOverlayState();
        UpdateOverlayTimeScale();
    }

    void SetTutorialOverlayVisible(bool isVisible)
    {
        if (tutorialOverlayRoot != null)
            tutorialOverlayRoot.SetActive(isVisible);

        UpdateOverlayTimeScale();
    }

    void SetRevivePromptVisible(bool isVisible)
    {
        if (revivePromptRoot != null)
            revivePromptRoot.SetActive(isVisible);

        UpdateOverlayTimeScale();
    }

    void SetQaCaptureOverlayVisible(bool isVisible)
    {
        if (qaCaptureOverlayRoot != null)
            qaCaptureOverlayRoot.SetActive(isVisible);

        UpdateOverlayTimeScale();
    }

    bool IsPauseOverlayVisible()
    {
        return pauseOverlayRoot != null && pauseOverlayRoot.activeSelf;
    }

    bool IsTutorialOverlayVisible()
    {
        return tutorialOverlayRoot != null && tutorialOverlayRoot.activeSelf;
    }

    bool IsRevivePromptVisible()
    {
        return revivePromptRoot != null && revivePromptRoot.activeSelf;
    }

    bool IsQaCaptureOverlayVisible()
    {
        return qaCaptureOverlayRoot != null && qaCaptureOverlayRoot.activeSelf;
    }

    void UpdateOverlayTimeScale()
    {
        Time.timeScale = (IsPauseOverlayVisible() ||
            IsTutorialOverlayVisible() ||
            IsRevivePromptVisible() ||
            IsQaCaptureOverlayVisible()) ? 0f : 1f;
    }

    void OnApplicationPause(bool paused)
    {
        QaTestingSystem.HandleApplicationFocusChanged(!paused);
        RefreshQaFeedbackUi();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        QaTestingSystem.HandleApplicationFocusChanged(hasFocus);
        RefreshQaFeedbackUi();
    }

    void OnDestroy()
    {
        QaTestingSystem.StopCapture("game_scene_destroyed");
    }

    void EnsurePresentationSystems()
    {
        if (caveBackgroundController == null)
        {
            caveBackgroundController = FindAnyObjectByType<CaveBackgroundController>();

            if (caveBackgroundController == null)
            {
                GameObject backgroundRoot = new GameObject("RuntimeCaveBackground");
                caveBackgroundController = backgroundRoot.AddComponent<CaveBackgroundController>();
            }
        }

        if (audioDirector == null)
            audioDirector = EndlessDodgeAudioDirector.EnsureExists();

        if (playfieldBorderController == null)
        {
            playfieldBorderController = FindAnyObjectByType<PlayfieldBorderController>();

            if (playfieldBorderController == null)
            {
                GameObject bordersRoot = GameObject.Find("Borders");

                if (bordersRoot == null)
                    bordersRoot = new GameObject("Borders");

                playfieldBorderController = bordersRoot.GetComponent<PlayfieldBorderController>();

                if (playfieldBorderController == null)
                    playfieldBorderController = bordersRoot.AddComponent<PlayfieldBorderController>();
            }
        }

        if (player != null && playerPowerupVisuals == null)
        {
            playerPowerupVisuals = player.GetComponent<PlayerPowerupVisuals>();

            if (playerPowerupVisuals == null)
                playerPowerupVisuals = player.gameObject.AddComponent<PlayerPowerupVisuals>();
        }
    }
}

public class RunUpgradeButtonUI : MonoBehaviour
{
    private GameManager gameManager;
    private UpgradeType upgradeType;
    private TextMeshProUGUI label;
    private Image backgroundImage;
    private Button button;

    private readonly Color readyColor = new Color(0.2f, 0.32f, 0.28f, 0.96f);
    private readonly Color selectedColor = new Color(0.18f, 0.46f, 0.42f, 0.98f);
    private readonly Color activeColor = new Color(0.24f, 0.6f, 0.42f, 0.98f);
    private readonly Color manualColor = new Color(0.74f, 0.5f, 0.18f, 0.98f);
    private readonly Color disabledColor = new Color(0.14f, 0.18f, 0.16f, 0.86f);

    public void Initialize(
        GameManager manager,
        UpgradeType type,
        TextMeshProUGUI labelText,
        Image image,
        Button sourceButton)
    {
        gameManager = manager;
        upgradeType = type;
        label = labelText;
        backgroundImage = image;
        button = sourceButton;

        if (button != null)
        {
            button.onClick.RemoveListener(OnPressed);
            button.onClick.AddListener(OnPressed);
            StudioUiTheme.ApplyButtonChrome(button, new Color(0.2f, 0.28f, 0.24f, 0.96f), label, StudioUiTheme.Text);
        }
    }

    public void RefreshView(
        int ownedAmount,
        string stateText,
        string modeText,
        bool isSelected,
        bool hasEffect,
        bool canUse,
        bool isManualUse)
    {
        if (label != null)
        {
            label.text =
                UpgradeInventory.GetDisplayName(upgradeType) +
                "\n<size=72%>" + modeText + "  |  " + stateText + "  |  x" + ownedAmount + "</size>";
        }

        if (backgroundImage != null)
        {
            if (hasEffect)
                backgroundImage.color = activeColor;
            else if (isSelected)
                backgroundImage.color = selectedColor;
            else if (isManualUse && canUse)
                backgroundImage.color = manualColor;
            else if (canUse)
                backgroundImage.color = readyColor;
            else
                backgroundImage.color = disabledColor;
        }

        if (button != null)
            button.interactable = canUse;
    }

    void OnPressed()
    {
        if (gameManager != null)
            gameManager.UseRunUpgrade(upgradeType);
    }
}
