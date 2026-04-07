using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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

    private int totalCoins;

    void Start()
    {
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (totalCoinsText != null)
            totalCoinsText.text = "Total Coins: " + totalCoins;

        UpdateUpgradeText(speedUpgradeText, UpgradeType.SpeedBoost, 10, "Speed Boost");
        UpdateUpgradeText(shieldUpgradeText, UpgradeType.Shield, 15, "Shield");
        UpdateUpgradeText(extraLifeUpgradeText, UpgradeType.ExtraLife, 25, "Extra Life");
        UpdateUpgradeText(coinMagnetUpgradeText, UpgradeType.CoinMagnet, 20, "Coin Magnet");
        UpdateUpgradeText(doubleCoinsUpgradeText, UpgradeType.DoubleCoins, 25, "Double Coins");
        UpdateUpgradeText(slowTimeUpgradeText, UpgradeType.SlowTime, 20, "Slow Time");
        UpdateUpgradeText(smallerPlayerUpgradeText, UpgradeType.SmallerPlayer, 18, "Smaller Player");
        UpdateUpgradeText(scoreBoosterUpgradeText, UpgradeType.ScoreBooster, 15, "Score Booster");
        UpdateUpgradeText(bombUpgradeText, UpgradeType.Bomb, 30, "Bomb");
        UpdateUpgradeText(rareCoinBoostUpgradeText, UpgradeType.RareCoinBoost, 22, "Rare Coin Boost");
    }

    void UpdateUpgradeText(TextMeshProUGUI textBox, UpgradeType type, int cost, string itemName)
    {
        if (textBox == null)
            return;

        int owned = 0;

        if (UpgradeInventory.Instance != null)
            owned = UpgradeInventory.Instance.GetAmount(type);

        textBox.text = itemName + "\nOwned: " + owned + "\nCost: " + cost;
    }

    bool BuyUpgrade(UpgradeType type, int cost)
    {
        if (totalCoins < cost)
            return false;

        totalCoins -= cost;
        PlayerPrefs.SetInt("TotalCoins", totalCoins);

        if (UpgradeInventory.Instance != null)
            UpgradeInventory.Instance.AddUpgrade(type, 1);

        UpdateUI();
        return true;
    }

    public void BuySpeedUpgrade()
    {
        BuyUpgrade(UpgradeType.SpeedBoost, 10);
    }

    public void BuyShieldUpgrade()
    {
        BuyUpgrade(UpgradeType.Shield, 15);
    }

    public void BuyExtraLifeUpgrade()
    {
        BuyUpgrade(UpgradeType.ExtraLife, 25);
    }

    public void BuyCoinMagnetUpgrade()
    {
        BuyUpgrade(UpgradeType.CoinMagnet, 20);
    }

    public void BuyDoubleCoinsUpgrade()
    {
        BuyUpgrade(UpgradeType.DoubleCoins, 25);
    }

    public void BuySlowTimeUpgrade()
    {
        BuyUpgrade(UpgradeType.SlowTime, 20);
    }

    public void BuySmallerPlayerUpgrade()
    {
        BuyUpgrade(UpgradeType.SmallerPlayer, 18);
    }

    public void BuyScoreBoosterUpgrade()
    {
        BuyUpgrade(UpgradeType.ScoreBooster, 15);
    }

    public void BuyBombUpgrade()
    {
        BuyUpgrade(UpgradeType.Bomb, 30);
    }

    public void BuyRareCoinBoostUpgrade()
    {
        BuyUpgrade(UpgradeType.RareCoinBoost, 22);
    }

    public void GoBack()
    {
        SceneManager.LoadScene("MainMenu");
    }
}