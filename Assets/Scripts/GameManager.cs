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
    private bool gameEnded = false;

    private float runTime = 0f;
    public float difficultyStepTime = 15f;

    private int currentLevel = 1;
    public int activeShields = 0;
    public int armedExtraLives = 0;

    private readonly List<UpgradeType> equippedUpgrades = new List<UpgradeType>();
    private readonly Dictionary<UpgradeType, float> activeUpgradeEndTimes = new Dictionary<UpgradeType, float>();
    private readonly Dictionary<UpgradeType, RunUpgradeButtonUI> runUpgradeButtons = new Dictionary<UpgradeType, RunUpgradeButtonUI>();

    private RectTransform runUpgradePanel;
    private TMP_FontAsset runtimeFont;

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

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (gameOverText != null)
            gameOverText.SetActive(false);

        if (restartButton != null)
            restartButton.SetActive(false);

        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        if (totalCoinsText != null)
            totalCoinsText.text = "Coins: " + totalCoins;

        runtimeFont = GetRuntimeFont();
        SyncLoadoutFromInventory(true);
        ApplyContinuousEffects();
        UpdateUpgradeText();

        currentLevel = GetDifficultyLevel();
        UpdateLevelText();
    }

    void Update()
    {
        SyncLoadoutFromInventory(false);

        if (gameEnded) return;

        runTime += Time.deltaTime;
        score += Time.deltaTime * GetScoreMultiplier();

        if (scoreText != null)
            scoreText.text = "Score: " + Mathf.FloorToInt(score);

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
        totalCoins += GetCoinValue();
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.Save();

        if (totalCoinsText != null)
            totalCoinsText.text = "Coins: " + totalCoins;
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

            return;
        }

        if (armedExtraLives > 0)
        {
            armedExtraLives -= 1;

            if (obstacle != null)
                Destroy(obstacle);

            StartCoroutine(RevivePlayerRoutine());
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

        switch (type)
        {
            case UpgradeType.Shield:
                ActivateShield();
                break;
            case UpgradeType.ExtraLife:
                ArmExtraLife();
                break;
            case UpgradeType.SpeedBoost:
                ActivateTimedUpgrade(type, SpeedBoostDuration);
                break;
            case UpgradeType.CoinMagnet:
                ActivateTimedUpgrade(type, CoinMagnetDuration);
                break;
            case UpgradeType.DoubleCoins:
                ActivateTimedUpgrade(type, DoubleCoinsDuration);
                break;
            case UpgradeType.SlowTime:
                ActivateTimedUpgrade(type, SlowTimeDuration);
                break;
            case UpgradeType.SmallerPlayer:
                ActivateTimedUpgrade(type, SmallerPlayerDuration);
                break;
            case UpgradeType.ScoreBooster:
                ActivateTimedUpgrade(type, ScoreBoosterDuration);
                break;
            case UpgradeType.Bomb:
                ActivateBomb();
                break;
            case UpgradeType.RareCoinBoost:
                ActivateTimedUpgrade(type, RareCoinBoostDuration);
                break;
        }

        RefreshRunUpgradeButtons();
        UpdateUpgradeText();
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
        runUpgradePanel.anchoredPosition = new Vector2(-20f, 20f);
        runUpgradePanel.sizeDelta = new Vector2(170f, 0f);

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
        buttonRect.sizeDelta = new Vector2(170f, 60f);

        LayoutElement layoutElement = buttonObject.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = 60f;
        layoutElement.preferredWidth = 170f;

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
        label.fontSizeMin = 11f;
        label.fontSizeMax = 18f;
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
            bool isActive = IsUpgradeShownAsActive(type);
            bool canUse = !gameEnded && GetUpgradeOwnedCount(type) > 0;

            buttonUI.RefreshView(GetUpgradeOwnedCount(type), stateText, isActive, canUse);
        }
    }

    string BuildButtonStateText(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Shield:
                return "Active " + activeShields;
            case UpgradeType.ExtraLife:
                return "Armed " + armedExtraLives;
            case UpgradeType.Bomb:
                return "Clear screen";
            default:
                if (IsUpgradeActive(type))
                    return Mathf.CeilToInt(GetUpgradeRemainingTime(type)) + "s active";

                return "Tap to use";
        }
    }

    bool IsUpgradeShownAsActive(UpgradeType type)
    {
        if (type == UpgradeType.Shield)
            return activeShields > 0;

        if (type == UpgradeType.ExtraLife)
            return armedExtraLives > 0;

        return IsUpgradeActive(type);
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
        if (IsUpgradeActive(UpgradeType.SlowTime))
            return 0.55f;

        return 1f;
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
        if (IsUpgradeActive(UpgradeType.RareCoinBoost))
            return Mathf.Clamp01(baseChance + 0.3f);

        return baseChance;
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

        if (spawner != null)
            spawner.StopSpawning();

        if (player != null)
            player.Die();

        if (gameOverText != null)
            gameOverText.SetActive(true);

        if (restartButton != null)
            restartButton.SetActive(true);

        RefreshRunUpgradeButtons();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
    private readonly Color activeColor = new Color(0.16f, 0.5f, 0.33f, 0.98f);
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

    public void RefreshView(int ownedAmount, string stateText, bool isActive, bool canUse)
    {
        if (label != null)
        {
            label.text =
                UpgradeInventory.GetDisplayName(upgradeType) +
                "\n<size=72%>" + stateText + "  |  Owned " + ownedAmount + "</size>";
        }

        if (backgroundImage != null)
        {
            if (isActive)
                backgroundImage.color = activeColor;
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
