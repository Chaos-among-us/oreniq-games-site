using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public UpgradeType upgradeType;
    public InventoryMenu inventoryMenu;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI ownedText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI combinedText;
    public Image backgroundImage;

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
            else if (nameText == null && descriptionText == null && ownedText == null && statusText == null)
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

        if (nameText != null)
        {
            nameText.text = UpgradeInventory.GetDisplayName(upgradeType);
            nameText.color = StudioUiTheme.Text;
            nameText.fontStyle = FontStyles.Bold;
        }

        if (descriptionText != null)
        {
            descriptionText.text = GetUpgradeDescription(upgradeType);
            descriptionText.color = StudioUiTheme.MutedText;
        }

        if (ownedText != null)
        {
            ownedText.text = "Owned: " + ownedAmount;
            ownedText.color = ownedAmount > 0
                ? StudioUiTheme.Text
                : StudioUiTheme.WithAlpha(StudioUiTheme.MutedText, 0.74f);
        }

        if (statusText != null)
        {
            if (isEquipped)
            {
                statusText.text = "Selected";
                statusText.color = StudioUiTheme.Gold;
            }
            else if (ownedAmount > 0)
            {
                statusText.text = "Tap to use";
                statusText.color = StudioUiTheme.Blue;
            }
            else
            {
                statusText.text = "Shop first";
                statusText.color = StudioUiTheme.WithAlpha(StudioUiTheme.Gold, 0.78f);
            }
        }

        if (combinedText != null)
            combinedText.gameObject.SetActive(false);

        if (backgroundImage != null)
        {
            RuntimeCaveTheme theme = CaveThemeLibrary.GetMenuTheme();
            StudioUiTheme.ApplyPanel(backgroundImage, isEquipped ? StudioPanelStyle.Accent : StudioPanelStyle.Surface);
            Color normalColor = StudioUiTheme.Surface;
            normalColor.a = 0.96f;
            Color selectedColor = new Color(0.18f, 0.34f, 0.24f, 0.97f);
            selectedColor.a = 0.97f;
            Color unavailableColor = StudioUiTheme.Dim(StudioUiTheme.Surface, 0.42f);
            unavailableColor.a = 0.9f;

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

        nameText = EnsureTopBandLabel(
            "NameText",
            nameText,
            runtimeFont,
            20f,
            42f,
            26f,
            150f,
            TextAlignmentOptions.TopLeft,
            21f,
            32f,
            FontStyles.Bold);

        descriptionText = EnsureTopBandLabel(
            "DescriptionText",
            descriptionText,
            runtimeFont,
            68f,
            56f,
            26f,
            26f,
            TextAlignmentOptions.TopLeft,
            14f,
            19f,
            FontStyles.Normal);

        ownedText = EnsureBottomHalfLabel(
            "OwnedText",
            ownedText,
            runtimeFont,
            true,
            18f,
            32f,
            26f,
            TextAlignmentOptions.BottomLeft,
            14f,
            20f,
            FontStyles.Normal);

        statusText = EnsureBottomHalfLabel(
            "StatusText",
            statusText,
            runtimeFont,
            false,
            18f,
            32f,
            26f,
            TextAlignmentOptions.BottomRight,
            14f,
            20f,
            FontStyles.Bold);

        if (combinedText != null)
            combinedText.gameObject.SetActive(false);
    }

    TextMeshProUGUI EnsureTopBandLabel(
        string objectName,
        TextMeshProUGUI label,
        TMP_FontAsset runtimeFont,
        float topOffset,
        float height,
        float leftInset,
        float rightInset,
        TextAlignmentOptions alignment,
        float minSize,
        float maxSize,
        FontStyles fontStyle)
    {
        if (label == null)
        {
            Transform existing = transform.Find(objectName);

            if (existing != null)
                label = existing.GetComponent<TextMeshProUGUI>();
        }

        if (label == null)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform));
            textObject.transform.SetParent(transform, false);
            label = textObject.AddComponent<TextMeshProUGUI>();
        }

        RectTransform textRect = label.rectTransform;
        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.offsetMin = new Vector2(leftInset, -(topOffset + height));
        textRect.offsetMax = new Vector2(-rightInset, -topOffset);

        label.gameObject.SetActive(true);
        label.enableAutoSizing = true;
        label.fontSizeMin = minSize;
        label.fontSizeMax = maxSize;
        label.alignment = alignment;
        label.margin = new Vector4(2f, 0f, 2f, 0f);
        label.lineSpacing = 2f;
        label.fontStyle = fontStyle;

        if (runtimeFont != null && label.font == null)
            label.font = runtimeFont;

        return label;
    }

    TextMeshProUGUI EnsureBottomHalfLabel(
        string objectName,
        TextMeshProUGUI label,
        TMP_FontAsset runtimeFont,
        bool alignLeftHalf,
        float bottomOffset,
        float height,
        float inset,
        TextAlignmentOptions alignment,
        float minSize,
        float maxSize,
        FontStyles fontStyle)
    {
        if (label == null)
        {
            Transform existing = transform.Find(objectName);

            if (existing != null)
                label = existing.GetComponent<TextMeshProUGUI>();
        }

        if (label == null)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform));
            textObject.transform.SetParent(transform, false);
            label = textObject.AddComponent<TextMeshProUGUI>();
        }

        RectTransform textRect = label.rectTransform;
        textRect.anchorMin = alignLeftHalf ? new Vector2(0f, 0f) : new Vector2(0.5f, 0f);
        textRect.anchorMax = alignLeftHalf ? new Vector2(0.5f, 0f) : new Vector2(1f, 0f);
        textRect.pivot = new Vector2(0.5f, 0f);
        textRect.offsetMin = new Vector2(inset, bottomOffset);
        textRect.offsetMax = new Vector2(-inset, bottomOffset + height);

        label.gameObject.SetActive(true);
        label.enableAutoSizing = true;
        label.fontSizeMin = minSize;
        label.fontSizeMax = maxSize;
        label.alignment = alignment;
        label.margin = new Vector4(0f, 0f, 0f, 0f);
        label.lineSpacing = 0f;
        label.fontStyle = fontStyle;

        if (runtimeFont != null && label.font == null)
            label.font = runtimeFont;

        return label;
    }

    TMP_FontAsset ResolveRuntimeFont()
    {
        if (nameText != null && nameText.font != null)
            return nameText.font;

        if (descriptionText != null && descriptionText.font != null)
            return descriptionText.font;

        if (ownedText != null && ownedText.font != null)
            return ownedText.font;

        if (statusText != null && statusText.font != null)
            return statusText.font;

        if (combinedText != null && combinedText.font != null)
            return combinedText.font;

        TextMeshProUGUI fallbackText = GetComponentInParent<TextMeshProUGUI>(true);
        return fallbackText != null ? fallbackText.font : null;
    }

    string GetUpgradeDescription(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Shield:
                return "One-hit barrier";
            case UpgradeType.SpeedBoost:
                return "Faster movement";
            case UpgradeType.ExtraLife:
                return "One revive";
            case UpgradeType.CoinMagnet:
                return "Pull coins in";
            case UpgradeType.DoubleCoins:
                return "2x coin value";
            case UpgradeType.SlowTime:
                return "Slow hazards";
            case UpgradeType.SmallerPlayer:
                return "Smaller hitbox";
            case UpgradeType.ScoreBooster:
                return "2x score gain";
            case UpgradeType.Bomb:
                return "Clear screen";
            case UpgradeType.RareCoinBoost:
                return "More coin spawns";
            default:
                return "Consumable upgrade";
        }
    }
}
