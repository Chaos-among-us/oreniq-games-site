using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public UpgradeType upgradeType;
    public InventoryMenu inventoryMenu;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI ownedText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI combinedText;
    public Image backgroundImage;

    public Color normalColor = new Color(0.97f, 0.98f, 1f, 0.96f);
    public Color selectedColor = new Color(0.78f, 0.93f, 0.82f, 0.98f);
    public Color unavailableColor = new Color(0.84f, 0.86f, 0.9f, 0.82f);

    private Button button;
    private bool clickBound = false;

    void Awake()
    {
        CacheReferences();
    }

    void Start()
    {
        BindButton();
        Refresh();
    }

    void OnEnable()
    {
        Refresh();
    }

    public void Initialize(InventoryMenu menu, UpgradeType type)
    {
        inventoryMenu = menu;
        upgradeType = type;
        CacheReferences();
        BindButton();
        Refresh();
    }

    void CacheReferences()
    {
        if (inventoryMenu == null)
            inventoryMenu = GetComponentInParent<InventoryMenu>();

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (button == null)
            button = GetComponent<Button>();

        if (combinedText == null)
        {
            Transform existing = transform.Find("CombinedText");

            if (existing != null)
                combinedText = existing.GetComponent<TextMeshProUGUI>();
            else if (nameText == null && ownedText == null && statusText == null)
                combinedText = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        EnsureCardText();
    }

    void BindButton()
    {
        if (button == null || clickBound)
            return;

        button.onClick.RemoveListener(OnSlotPressed);
        button.onClick.AddListener(OnSlotPressed);
        clickBound = true;
    }

    public void OnSlotPressed()
    {
        if (inventoryMenu != null)
            inventoryMenu.OnSlotPressed(upgradeType);
    }

    public void Refresh()
    {
        CacheReferences();

        int ownedAmount = 0;
        bool isEquipped = false;

        if (UpgradeInventory.Instance != null)
        {
            ownedAmount = UpgradeInventory.Instance.GetAmount(upgradeType);
            isEquipped = UpgradeInventory.Instance.IsEquipped(upgradeType);
        }

        if (combinedText != null)
        {
            string statusLine;
            string statusColor;

            if (isEquipped)
            {
                statusLine = "Owned: " + ownedAmount + "   |   Equipped now";
                statusColor = "#246641";
            }
            else if (ownedAmount > 0)
            {
                statusLine = "Owned: " + ownedAmount + "   |   Tap to equip";
                statusColor = "#425676";
            }
            else
            {
                statusLine = "Owned: 0   |   Buy in shop";
                statusColor = "#6D7380";
            }

            combinedText.gameObject.SetActive(true);
            combinedText.text =
                "<size=125%><b>" + UpgradeInventory.GetDisplayName(upgradeType) + "</b></size>" +
                "\n<size=88%>" + GetUpgradeDescription(upgradeType) + "</size>" +
                "\n<size=74%><color=" + statusColor + ">" + statusLine + "</color></size>";
        }

        if (backgroundImage != null)
        {
            if (isEquipped)
                backgroundImage.color = selectedColor;
            else if (ownedAmount > 0)
                backgroundImage.color = normalColor;
            else
                backgroundImage.color = unavailableColor;
        }
    }

    void EnsureCardText()
    {
        TMP_FontAsset runtimeFont = ResolveRuntimeFont();

        if (combinedText == null)
        {
            GameObject textObject = new GameObject("CombinedText", typeof(RectTransform));
            textObject.transform.SetParent(transform, false);
            combinedText = textObject.AddComponent<TextMeshProUGUI>();
        }

        RectTransform textRect = combinedText.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(26f, 22f);
        textRect.offsetMax = new Vector2(-26f, -22f);

        combinedText.enableAutoSizing = true;
        combinedText.fontSizeMin = 18f;
        combinedText.fontSizeMax = 28f;
        combinedText.alignment = TextAlignmentOptions.Center;
        combinedText.margin = new Vector4(4f, 0f, 4f, 0f);
        combinedText.lineSpacing = 8f;
        combinedText.color = new Color(0.18f, 0.22f, 0.3f, 1f);

        if (runtimeFont != null && combinedText.font == null)
            combinedText.font = runtimeFont;

        SetOptionalTextVisible(nameText, false);
        SetOptionalTextVisible(ownedText, false);
        SetOptionalTextVisible(statusText, false);
    }

    TMP_FontAsset ResolveRuntimeFont()
    {
        if (nameText != null && nameText.font != null)
            return nameText.font;

        if (ownedText != null && ownedText.font != null)
            return ownedText.font;

        if (statusText != null && statusText.font != null)
            return statusText.font;

        if (combinedText != null && combinedText.font != null)
            return combinedText.font;

        TextMeshProUGUI fallbackText = GetComponentInParent<TextMeshProUGUI>(true);
        return fallbackText != null ? fallbackText.font : null;
    }

    void SetOptionalTextVisible(TextMeshProUGUI text, bool isVisible)
    {
        if (text != null && text != combinedText)
            text.gameObject.SetActive(isVisible);
    }

    string GetUpgradeDescription(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Shield:
                return "Block one hit";
            case UpgradeType.SpeedBoost:
                return "Move faster";
            case UpgradeType.ExtraLife:
                return "Revive once";
            case UpgradeType.CoinMagnet:
                return "Pull in coins";
            case UpgradeType.DoubleCoins:
                return "Double coin value";
            case UpgradeType.SlowTime:
                return "Slow obstacles";
            case UpgradeType.SmallerPlayer:
                return "Shrink your hitbox";
            case UpgradeType.ScoreBooster:
                return "Double score gain";
            case UpgradeType.Bomb:
                return "Clear the screen";
            case UpgradeType.RareCoinBoost:
                return "More coin spawns";
            default:
                return "Consumable upgrade";
        }
    }
}
