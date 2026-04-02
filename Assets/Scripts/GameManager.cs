using System.Collections;
using UnityEngine;
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

    private int speedLevel = 0;
    private int coinLevel = 0;

    private float runTime = 0f;
    public float difficultyStepTime = 15f;

    private int currentLevel = 1;

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
        speedLevel = PlayerPrefs.GetInt("SpeedLevel", 0);
        coinLevel = PlayerPrefs.GetInt("CoinLevel", 0);

        if (totalCoinsText != null)
            totalCoinsText.text = "Coins: " + totalCoins;

        if (player != null)
            player.moveSpeed = 8f + speedLevel;

        UpdateUpgradeText();

        currentLevel = GetDifficultyLevel();
        UpdateLevelText();
    }

    void Update()
    {
        if (gameEnded) return;

        runTime += Time.deltaTime;
        score += Time.deltaTime;

        if (scoreText != null)
            scoreText.text = "Score: " + Mathf.FloorToInt(score);

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
        if (upgradeText != null)
            upgradeText.text = "Speed Lv. " + speedLevel + "\nCoin Lv. " + coinLevel;
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
        int coinValue = 1 + coinLevel;

        totalCoins += coinValue;
        PlayerPrefs.SetInt("TotalCoins", totalCoins);

        if (totalCoinsText != null)
            totalCoinsText.text = "Coins: " + totalCoins;
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
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}