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
    private GameObject postRunPanel;
    private TextMeshProUGUI postRunSummaryText;
    private TextMeshProUGUI bestScoreHudText;
    private Button pauseButton;
    private TextMeshProUGUI pauseButtonText;
    private GameObject pauseOverlayRoot;
    private TextMeshProUGUI pauseOverlayTitleText;
    private TextMeshProUGUI hapticsStatusText;
    private Button hapticsToggleButton;
    private TextMeshProUGUI hapticsToggleButtonText;
    private GameObject tutorialOverlayRoot;
    private TextMeshProUGUI tutorialBodyText;
    private Button tutorialPrimaryButton;
    private TextMeshProUGUI tutorialPrimaryButtonText;
    private bool markTutorialSeenOnClose;

    private const float RunUpgradeButtonWidth = 230f;
    private const float RunUpgradeButtonHeight = 78f;
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

        if (totalCoinsText != null)
            totalCoinsText.text = "Coins: " + totalCoins;

        if (isDailyChallengeRun)
            UpdateUpgradeText();

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
    }

    void Update()
    {
        SyncLoadoutFromInventory(false);

        if (gameEnded) return;

        ProcessAutoUpgrades();
        runTime += Time.deltaTime;
        score += Time.deltaTime * GetScoreMultiplier();

        if (scoreText != null)
            scoreText.text = "Score: " + Mathf.FloorToInt(score);

        if (isDailyChallengeRun)
        {
            UpdateUpgradeText();
            UpdateBestScoreHud();
        }

        ApplyContinuousEffects();
        RefreshRunUpgradeButtons();
        int newLevel = GetDifficultyLevel();

        if (newLevel != currentLevel)
        {
            currentLevel = newLevel;
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
            upgradeText.text = "No loadout selected";
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

    public void AddCoin()
    {
        int coinValue = GetCoinValue();
        totalCoins += coinValue;
        runCoinsEarned += coinValue;
        DailyMissionSystem.RegisterCoinsCollected(coinValue);
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.Save();

        if (totalCoinsText != null)
            totalCoinsText.text = "Coins: " + totalCoins;

        if (isDailyChallengeRun)
        {
            UpdateUpgradeText();
            UpdateBestScoreHud();
        }
    }

    public void ActivateShield()
    {
        if (UpgradeInventory.Instance == null)
            return;

        bool used = UpgradeInventory.Instance.UseUpgrade(UpgradeType.Shield, 1);

        if (used)
        {
            activeShields += 1;
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

            return;
        }

        if (ConsumeShieldIfAvailable())
        {
            if (obstacle != null)
                Destroy(obstacle);

            if (player != null)
                player.TriggerInvulnerability(ShieldProtectionDuration);

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

        GameOver();
    }

    IEnumerator RevivePlayerRoutine()
    {
        if (player != null)
            player.Die();

        yield return new WaitForSeconds(ExtraLifeReviveDelay);

        if (player != null)
            player.Revive(ExtraLifeInvulnerabilityDuration);
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
    }

    void ActivateTimedUpgrade(UpgradeType type, float duration)
    {
        if (!UpgradeInventory.Instance.UseUpgrade(type, 1))
            return;

        float startTime = Time.time;

        if (IsUpgradeActive(type))
            startTime = activeUpgradeEndTimes[type];

        activeUpgradeEndTimes[type] = startTime + duration;
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
        runUpgradePanel.anchoredPosition = new Vector2(-18f, 26f);
        runUpgradePanel.sizeDelta = new Vector2(RunUpgradeButtonWidth, 0f);

        VerticalLayoutGroup layoutGroup = panelObject.GetComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 10f;
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
        buttonImage.color = new Color(0.16f, 0.2f, 0.3f, 0.92f);

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
        label.fontSizeMax = 22f;
        label.color = Color.white;

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
                return "Shield: " + activeShields + " active / " + GetUpgradeOwnedCount(type) + " owned";
            case UpgradeType.ExtraLife:
                return "Extra Life: " + armedExtraLives + " armed / " + GetUpgradeOwnedCount(type) + " owned";
            case UpgradeType.Bomb:
                return "Bomb: " + GetUpgradeOwnedCount(type) + " ready";
            default:
                if (IsUpgradeActive(type))
                {
                    return UpgradeInventory.GetDisplayName(type) + ": " +
                           Mathf.CeilToInt(GetUpgradeRemainingTime(type)) + "s";
                }

                return UpgradeInventory.GetDisplayName(type) + ": " + GetUpgradeOwnedCount(type) + " owned";
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

    public float GetPlayerMoveSpeedMultiplier()
    {
        if (IsUpgradeActive(UpgradeType.SpeedBoost))
            return 1.55f;

        return 1f;
    }

    public float GetPlayerSizeMultiplier()
    {
        if (IsUpgradeActive(UpgradeType.SmallerPlayer))
            return 0.68f;

        return 1f;
    }

    public float GetScoreMultiplier()
    {
        if (IsUpgradeActive(UpgradeType.ScoreBooster))
            return 2f;

        return 1f;
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

        if (isDailyChallengeRun)
            spawnChance *= activeDailyChallenge.coinSpawnMultiplier;

        if (IsUpgradeActive(UpgradeType.RareCoinBoost))
            spawnChance += 0.3f;

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
    }

    public void RestartGame()
    {
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
        panelRect.sizeDelta = new Vector2(560f, 250f);
        panelRect.anchoredPosition = new Vector2(0f, -290f);

        Image panelImage = postRunPanel.GetComponent<Image>();
        panelImage.color = new Color(0.12f, 0.18f, 0.3f, 0.9f);

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
            summaryRect.offsetMin = new Vector2(28f, 24f);
            summaryRect.offsetMax = new Vector2(-28f, -24f);
        }

        postRunPanel.SetActive(false);
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
            tutorialBodyText.text =
                "Avoid every obstacle, no matter the color." +
                "\nCollect the gold coins to buy consumables." +
                "\nEquip up to 3 upgrades in Inventory before each run." +
                "\nTap a run upgrade once to keep shields, buffs, and extra lives auto-armed.";
            tutorialBodyText.lineSpacing = 18f;
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

        return buttonObject.GetComponent<Button>();
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
        if (bestScoreHudText != null)
        {
            if (isDailyChallengeRun)
            {
                bestScoreHudText.text = gameEnded
                    ? DailyChallengeSystem.GetBestProgressLabel(activeDailyChallenge)
                    : DailyChallengeSystem.GetCurrentRunLabel(activeDailyChallenge, Mathf.FloorToInt(score), runCoinsEarned);
            }
            else
                bestScoreHudText.text = "Best: " + bestScore;
        }
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
            summaryText =
                "Run Score " + finalScore +
                "\nBest " + bestScore + "   Coins +" + runCoinsEarned +
                "\nLevel " + currentLevel + "   Missions " + completedMissionCount + "/3";

            if (claimableMissionCount > 0)
            {
                summaryText +=
                    "\n<color=#FFD876>" +
                    claimableMissionCount +
                    (claimableMissionCount == 1 ? " reward ready in menu" : " rewards ready in menu") +
                    "</color>";
            }
        }

        if (newBestScore)
            summaryText += "\n<color=#7FF0A6>New Best!</color>";

        postRunSummaryText.text = summaryText;
        postRunPanel.SetActive(true);
    }

    void ShowTutorialIfNeeded()
    {
        if (GameSettings.HasSeenTutorial())
            return;

        markTutorialSeenOnClose = true;
        SetTutorialOverlayVisible(true);
    }

    public void TogglePause()
    {
        if (gameEnded || IsTutorialOverlayVisible())
            return;

        SetPauseOverlayVisible(!IsPauseOverlayVisible());
    }

    public void ResumeGameplay()
    {
        SetPauseOverlayVisible(false);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        if (isDailyChallengeRun)
            DailyChallengeSystem.ClearActiveRun();

        SceneManager.LoadScene("MainMenu");
    }

    public void ReplayTutorial()
    {
        markTutorialSeenOnClose = false;
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

    bool IsPauseOverlayVisible()
    {
        return pauseOverlayRoot != null && pauseOverlayRoot.activeSelf;
    }

    bool IsTutorialOverlayVisible()
    {
        return tutorialOverlayRoot != null && tutorialOverlayRoot.activeSelf;
    }

    void UpdateOverlayTimeScale()
    {
        Time.timeScale = (IsPauseOverlayVisible() || IsTutorialOverlayVisible()) ? 0f : 1f;
    }
}

public class RunUpgradeButtonUI : MonoBehaviour
{
    private GameManager gameManager;
    private UpgradeType upgradeType;
    private TextMeshProUGUI label;
    private Image backgroundImage;
    private Button button;

    private readonly Color readyColor = new Color(0.18f, 0.33f, 0.56f, 0.96f);
    private readonly Color selectedColor = new Color(0.17f, 0.46f, 0.52f, 0.98f);
    private readonly Color activeColor = new Color(0.18f, 0.58f, 0.38f, 0.98f);
    private readonly Color manualColor = new Color(0.6f, 0.36f, 0.13f, 0.98f);
    private readonly Color disabledColor = new Color(0.3f, 0.32f, 0.37f, 0.88f);

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
                "\n<size=80%>" + modeText + "</size>" +
                "\n<size=68%>" + stateText + "  |  Owned " + ownedAmount + "</size>";
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
