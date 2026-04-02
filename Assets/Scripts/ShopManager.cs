using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    public TextMeshProUGUI totalCoinsText;

    public TextMeshProUGUI speedUpgradeText;
    public TextMeshProUGUI coinUpgradeText;

    private int totalCoins;

    private int speedLevel;
    private int speedCost;

    private int coinLevel;
    private int coinCost;

    void Start()
    {
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        speedLevel = PlayerPrefs.GetInt("SpeedLevel", 0);
        coinLevel = PlayerPrefs.GetInt("CoinLevel", 0);

        UpdateUI();
    }

    void UpdateUI()
    {
        speedCost = 10 + (speedLevel * 5);
        coinCost = 15 + (coinLevel * 10);

        if (totalCoinsText != null)
            totalCoinsText.text = "Total Coins: " + totalCoins;

        if (speedUpgradeText != null)
            speedUpgradeText.text = "Speed Lv." + speedLevel + "\nCost: " + speedCost;

        if (coinUpgradeText != null)
            coinUpgradeText.text = "Coin Bonus Lv." + coinLevel + "\nCost: " + coinCost;
    }

    public void BuySpeedUpgrade()
    {
        if (totalCoins < speedCost)
            return;

        totalCoins -= speedCost;
        speedLevel++;

        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.SetInt("SpeedLevel", speedLevel);

        UpdateUI();
    }

    public void BuyCoinUpgrade()
    {
        if (totalCoins < coinCost)
            return;

        totalCoins -= coinCost;
        coinLevel++;

        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.SetInt("CoinLevel", coinLevel);

        UpdateUI();
    }

    public void GoBack()
    {
        SceneManager.LoadScene("MainMenu");
    }
}