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

    public Color normalColor = new Color(1f, 1f, 1f, 0.92f);
    public Color selectedColor = new Color(0.58f, 0.9f, 0.68f, 0.98f);
    public Color unavailableColor = new Color(0.82f, 0.82f, 0.84f, 0.78f);

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

        if (combinedText == null && nameText == null && ownedText == null && statusText == null)
            combinedText = GetComponentInChildren<TextMeshProUGUI>(true);
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
            nameText.text = UpgradeInventory.GetDisplayName(upgradeType);

        if (ownedText != null)
            ownedText.text = "Owned: " + ownedAmount;

        if (statusText != null)
        {
            if (isEquipped)
                statusText.text = "Selected";
            else if (ownedAmount > 0)
                statusText.text = "Tap to Select";
            else
                statusText.text = "Not Owned";
        }

        if (combinedText != null)
        {
            RectTransform textRect = combinedText.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(18f, 10f);
            textRect.offsetMax = new Vector2(-18f, -10f);

            combinedText.enableAutoSizing = true;
            combinedText.fontSizeMin = 16;
            combinedText.fontSizeMax = 24;
            combinedText.alignment = TextAlignmentOptions.CenterGeoAligned;
            combinedText.color = new Color(0.16f, 0.18f, 0.24f, 1f);

            string statusLine;

            if (isEquipped)
                statusLine = "Selected";
            else if (ownedAmount > 0)
                statusLine = "Ready";
            else
                statusLine = "Not Owned";

            combinedText.text =
                UpgradeInventory.GetDisplayName(upgradeType) +
                "\n<size=76%>Owned: " + ownedAmount + "   " + statusLine + "</size>";
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
}
