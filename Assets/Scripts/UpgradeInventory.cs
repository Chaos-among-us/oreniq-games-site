using System.Collections.Generic;
using UnityEngine;

public enum UpgradeType
{
    SpeedBoost,
    Shield,
    ExtraLife,
    CoinMagnet,
    DoubleCoins,
    SlowTime,
    SmallerPlayer,
    ScoreBooster,
    Bomb,
    RareCoinBoost
}

public class UpgradeInventory : MonoBehaviour
{
    public static UpgradeInventory Instance;

    private Dictionary<UpgradeType, int> ownedUpgrades = new Dictionary<UpgradeType, int>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoCreate()
    {
        if (Instance == null)
        {
            GameObject obj = new GameObject("UpgradeInventory");
            obj.AddComponent<UpgradeInventory>();
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeInventory();
            LoadInventory();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void InitializeInventory()
    {
        foreach (UpgradeType type in System.Enum.GetValues(typeof(UpgradeType)))
        {
            if (!ownedUpgrades.ContainsKey(type))
            {
                ownedUpgrades[type] = 0;
            }
        }
    }

    public int GetAmount(UpgradeType type)
    {
        if (ownedUpgrades.ContainsKey(type))
            return ownedUpgrades[type];

        return 0;
    }

    public void AddUpgrade(UpgradeType type, int amount)
    {
        if (!ownedUpgrades.ContainsKey(type))
            ownedUpgrades[type] = 0;

        ownedUpgrades[type] += amount;
        SaveInventory();
    }

    public bool UseUpgrade(UpgradeType type, int amount = 1)
    {
        if (GetAmount(type) >= amount)
        {
            ownedUpgrades[type] -= amount;
            SaveInventory();
            return true;
        }

        return false;
    }

    public void SaveInventory()
    {
        foreach (UpgradeType type in System.Enum.GetValues(typeof(UpgradeType)))
        {
            PlayerPrefs.SetInt("Upgrade_" + type.ToString(), GetAmount(type));
        }

        PlayerPrefs.Save();
    }

    public void LoadInventory()
    {
        foreach (UpgradeType type in System.Enum.GetValues(typeof(UpgradeType)))
        {
            ownedUpgrades[type] = PlayerPrefs.GetInt("Upgrade_" + type.ToString(), 0);
        }
    }
}