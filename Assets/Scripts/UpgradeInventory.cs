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

public enum EquipToggleResult
{
    Equipped,
    Unequipped,
    NotOwned,
    LoadoutFull
}

public class UpgradeInventory : MonoBehaviour
{
    public static UpgradeInventory Instance;
    public const int MaxEquippedUpgrades = 3;

    private Dictionary<UpgradeType, int> ownedUpgrades = new Dictionary<UpgradeType, int>();
    private List<UpgradeType> equippedUpgrades = new List<UpgradeType>();

    private const string EquippedCountKey = "EquippedUpgradeCount";
    private const string EquippedKeyPrefix = "EquippedUpgrade_";

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
            LoadEquippedUpgrades();
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

    public bool IsEquipped(UpgradeType type)
    {
        return equippedUpgrades.Contains(type);
    }

    public int GetEquippedCount()
    {
        return equippedUpgrades.Count;
    }

    public List<UpgradeType> GetEquippedUpgrades()
    {
        List<UpgradeType> result = new List<UpgradeType>();

        for (int i = 0; i < equippedUpgrades.Count && i < MaxEquippedUpgrades; i++)
        {
            result.Add(equippedUpgrades[i]);
        }

        return result;
    }

    public EquipToggleResult ToggleEquippedUpgrade(UpgradeType type)
    {
        if (IsEquipped(type))
        {
            equippedUpgrades.Remove(type);
            SaveEquippedUpgrades();
            return EquipToggleResult.Unequipped;
        }

        if (GetAmount(type) <= 0)
            return EquipToggleResult.NotOwned;

        if (equippedUpgrades.Count >= MaxEquippedUpgrades)
            return EquipToggleResult.LoadoutFull;

        equippedUpgrades.Add(type);
        SaveEquippedUpgrades();
        return EquipToggleResult.Equipped;
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

    void SaveEquippedUpgrades()
    {
        int previousCount = PlayerPrefs.GetInt(EquippedCountKey, 0);

        PlayerPrefs.SetInt(EquippedCountKey, equippedUpgrades.Count);

        for (int i = 0; i < equippedUpgrades.Count; i++)
        {
            PlayerPrefs.SetString(EquippedKeyPrefix + i, equippedUpgrades[i].ToString());
        }

        for (int i = equippedUpgrades.Count; i < previousCount; i++)
        {
            PlayerPrefs.DeleteKey(EquippedKeyPrefix + i);
        }

        PlayerPrefs.Save();
    }

    void LoadEquippedUpgrades()
    {
        equippedUpgrades.Clear();

        int equippedCount = PlayerPrefs.GetInt(EquippedCountKey, 0);

        for (int i = 0; i < equippedCount; i++)
        {
            string savedValue = PlayerPrefs.GetString(EquippedKeyPrefix + i, "");

            if (string.IsNullOrEmpty(savedValue))
                continue;

            if (System.Enum.TryParse(savedValue, out UpgradeType loadedType) &&
                !equippedUpgrades.Contains(loadedType) &&
                equippedUpgrades.Count < MaxEquippedUpgrades)
            {
                equippedUpgrades.Add(loadedType);
            }
        }
    }

    public static string GetDisplayName(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.SpeedBoost:
                return "Speed Boost";
            case UpgradeType.ExtraLife:
                return "Extra Life";
            case UpgradeType.CoinMagnet:
                return "Coin Magnet";
            case UpgradeType.DoubleCoins:
                return "Double Coins";
            case UpgradeType.SlowTime:
                return "Slow Time";
            case UpgradeType.SmallerPlayer:
                return "Smaller Player";
            case UpgradeType.ScoreBooster:
                return "Score Booster";
            case UpgradeType.RareCoinBoost:
                return "Rare Coin Boost";
            default:
                return type.ToString();
        }
    }
}
