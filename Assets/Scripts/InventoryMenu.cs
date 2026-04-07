using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryMenu : MonoBehaviour
{
    public TextMeshProUGUI equippedCountText;
    public TextMeshProUGUI feedbackText;
    public InventorySlotUI[] slotUIs;
    public InventorySlotUI slotTemplate;
    public RectTransform slotParent;

    public int columnCount = 1;
    public float sidePadding = 18f;
    public float topPadding = 118f;
    public float bottomPadding = 76f;
    public float columnSpacing = 18f;
    public float rowSpacing = 10f;
    public float slotHeight = 72f;

    private RectTransform titleRect;
    private TextMeshProUGUI titleText;
    private RectTransform backButtonRect;
    private RectTransform slotViewport;
    private RectTransform slotContent;
    private ScrollRect slotScrollRect;

    private readonly UpgradeType[] slotOrder =
    {
        UpgradeType.Shield,
        UpgradeType.SpeedBoost,
        UpgradeType.ExtraLife,
        UpgradeType.CoinMagnet,
        UpgradeType.DoubleCoins,
        UpgradeType.SlowTime,
        UpgradeType.SmallerPlayer,
        UpgradeType.ScoreBooster,
        UpgradeType.Bomb,
        UpgradeType.RareCoinBoost
    };

    void Awake()
    {
        if (slotParent == null)
            slotParent = transform as RectTransform;

        if (slotTemplate == null)
            slotTemplate = GetComponentInChildren<InventorySlotUI>(true);

        FindStaticReferences();
        EnsureRuntimeLabels();
        EnsureScrollArea();
        RebuildSlotArray();
    }

    void Start()
    {
        BuildSlotsFromTemplate();
        RefreshUI();
    }

    void OnEnable()
    {
        RefreshUI();
    }

    void BuildSlotsFromTemplate()
    {
        if (slotTemplate == null)
            return;

        if (slotParent == null)
            slotParent = transform as RectTransform;

        EnsureScrollArea();

        if (slotTemplate.transform.parent != slotContent)
            slotTemplate.transform.SetParent(slotContent, false);

        RebuildSlotArray();

        while (slotUIs.Length < slotOrder.Length)
        {
            InventorySlotUI newSlot = Instantiate(slotTemplate, slotContent);
            newSlot.gameObject.SetActive(true);
            RebuildSlotArray();
        }

        Canvas.ForceUpdateCanvases();
        LayoutStaticElements();
        Canvas.ForceUpdateCanvases();
        LayoutSlotList();
        Canvas.ForceUpdateCanvases();

        for (int i = 0; i < slotOrder.Length; i++)
        {
            InventorySlotUI slot = slotUIs[i];

            if (slot == null)
                continue;

            slot.Initialize(this, slotOrder[i]);
            slot.gameObject.name = UpgradeInventory.GetDisplayName(slotOrder[i]).Replace(" ", "") + "Slot";
            PositionSlot(slot.GetComponent<RectTransform>(), i);
        }
    }

    void PositionSlot(RectTransform slotRect, int index)
    {
        if (slotRect == null || slotViewport == null || slotContent == null)
            return;

        float y = -(index * (slotHeight + rowSpacing));

        slotRect.anchorMin = new Vector2(0f, 1f);
        slotRect.anchorMax = new Vector2(1f, 1f);
        slotRect.pivot = new Vector2(0.5f, 1f);
        slotRect.sizeDelta = new Vector2(0f, slotHeight);
        slotRect.anchoredPosition = new Vector2(0f, y);
    }

    void LayoutSlotList()
    {
        if (slotViewport == null || slotContent == null)
            return;

        int rowCount = slotOrder.Length;
        float contentHeight = (rowCount * slotHeight) + ((rowCount - 1) * rowSpacing);
        float viewportHeight = slotViewport.rect.height;

        if (contentHeight < viewportHeight)
            contentHeight = viewportHeight;

        slotContent.anchorMin = new Vector2(0f, 1f);
        slotContent.anchorMax = new Vector2(1f, 1f);
        slotContent.pivot = new Vector2(0.5f, 1f);
        slotContent.sizeDelta = new Vector2(0f, contentHeight);
        slotContent.anchoredPosition = Vector2.zero;
    }

    void FindStaticReferences()
    {
        if (titleRect == null)
        {
            Transform titleTransform = transform.Find("InventoryTitle");

            if (titleTransform != null)
            {
                titleRect = titleTransform as RectTransform;
                titleText = titleTransform.GetComponent<TextMeshProUGUI>();
            }
        }

        if (backButtonRect == null)
        {
            Transform backTransform = transform.Find("BackButton");

            if (backTransform != null)
                backButtonRect = backTransform as RectTransform;
        }
    }

    void EnsureRuntimeLabels()
    {
        if (equippedCountText == null)
            equippedCountText = CreateRuntimeLabel("EquippedCountText");

        if (feedbackText == null)
            feedbackText = CreateRuntimeLabel("FeedbackText");
    }

    TextMeshProUGUI CreateRuntimeLabel(string objectName)
    {
        Transform existing = transform.Find(objectName);

        if (existing != null)
            return existing.GetComponent<TextMeshProUGUI>();

        GameObject labelObject = new GameObject(objectName, typeof(RectTransform));
        labelObject.transform.SetParent(transform, false);

        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.fontSize = 20;
        label.enableAutoSizing = true;
        label.fontSizeMin = 14;
        label.fontSizeMax = 20;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(0.16f, 0.18f, 0.24f, 1f);

        return label;
    }

    void EnsureScrollArea()
    {
        if (slotParent == null)
            return;

        if (slotViewport == null)
        {
            Transform viewportTransform = transform.Find("SlotViewport");

            if (viewportTransform != null)
            {
                slotViewport = viewportTransform as RectTransform;
            }
            else
            {
                GameObject viewportObject = new GameObject("SlotViewport", typeof(RectTransform), typeof(RectMask2D));
                viewportObject.transform.SetParent(transform, false);
                slotViewport = viewportObject.GetComponent<RectTransform>();
            }
        }

        if (slotContent == null)
        {
            Transform contentTransform = slotViewport.Find("SlotContent");

            if (contentTransform != null)
            {
                slotContent = contentTransform as RectTransform;
            }
            else
            {
                GameObject contentObject = new GameObject("SlotContent", typeof(RectTransform));
                contentObject.transform.SetParent(slotViewport, false);
                slotContent = contentObject.GetComponent<RectTransform>();
            }
        }

        if (slotScrollRect == null)
        {
            slotScrollRect = GetComponent<ScrollRect>();

            if (slotScrollRect == null)
                slotScrollRect = gameObject.AddComponent<ScrollRect>();
        }

        slotScrollRect.viewport = slotViewport;
        slotScrollRect.content = slotContent;
        slotScrollRect.horizontal = false;
        slotScrollRect.vertical = true;
        slotScrollRect.movementType = ScrollRect.MovementType.Clamped;
        slotScrollRect.scrollSensitivity = 30f;
    }

    void LayoutStaticElements()
    {
        FindStaticReferences();

        float panelWidth = slotParent.rect.width;
        float panelHeight = slotParent.rect.height;

        if (titleRect != null)
        {
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(panelWidth - (sidePadding * 2f), 54f);
            titleRect.anchoredPosition = new Vector2(0f, -20f);
        }

        if (titleText != null)
        {
            titleText.text = "Choose 3 Upgrades";
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 18;
            titleText.fontSizeMax = 30;
            titleText.alignment = TextAlignmentOptions.Center;
        }

        if (equippedCountText != null)
        {
            RectTransform countRect = equippedCountText.rectTransform;
            countRect.anchorMin = new Vector2(0.5f, 1f);
            countRect.anchorMax = new Vector2(0.5f, 1f);
            countRect.pivot = new Vector2(0.5f, 1f);
            countRect.sizeDelta = new Vector2(panelWidth - (sidePadding * 2f), 28f);
            countRect.anchoredPosition = new Vector2(0f, -68f);
        }

        if (feedbackText != null)
        {
            RectTransform feedbackRect = feedbackText.rectTransform;
            feedbackRect.anchorMin = new Vector2(0.5f, 1f);
            feedbackRect.anchorMax = new Vector2(0.5f, 1f);
            feedbackRect.pivot = new Vector2(0.5f, 1f);
            feedbackRect.sizeDelta = new Vector2(panelWidth - (sidePadding * 2f), 24f);
            feedbackRect.anchoredPosition = new Vector2(0f, -96f);
        }

        if (backButtonRect != null)
        {
            backButtonRect.anchorMin = new Vector2(0.5f, 0f);
            backButtonRect.anchorMax = new Vector2(0.5f, 0f);
            backButtonRect.pivot = new Vector2(0.5f, 0f);
            backButtonRect.sizeDelta = new Vector2(180f, 46f);
            backButtonRect.anchoredPosition = new Vector2(0f, 20f);
        }

        if (slotViewport != null)
        {
            slotViewport.anchorMin = new Vector2(0f, 0f);
            slotViewport.anchorMax = new Vector2(1f, 1f);
            slotViewport.pivot = new Vector2(0.5f, 0.5f);
            slotViewport.offsetMin = new Vector2(sidePadding, bottomPadding);
            slotViewport.offsetMax = new Vector2(-sidePadding, -topPadding);
        }
    }

    void RebuildSlotArray()
    {
        if (slotContent != null)
            slotUIs = slotContent.GetComponentsInChildren<InventorySlotUI>(true);
        else
            slotUIs = GetComponentsInChildren<InventorySlotUI>(true);
    }

    public void OnSlotPressed(UpgradeType type)
    {
        if (UpgradeInventory.Instance == null)
            return;

        EquipToggleResult result = UpgradeInventory.Instance.ToggleEquippedUpgrade(type);
        UpdateFeedback(result, type);
        RefreshUI();
    }

    public void RefreshUI()
    {
        int equippedCount = 0;

        if (UpgradeInventory.Instance != null)
            equippedCount = UpgradeInventory.Instance.GetEquippedCount();

        if (equippedCountText != null)
            equippedCountText.text = "Equipped: " + equippedCount + "/" + UpgradeInventory.MaxEquippedUpgrades;

        if (feedbackText != null && string.IsNullOrEmpty(feedbackText.text))
            feedbackText.text = "Tap a card to select it";

        RebuildSlotArray();
        Canvas.ForceUpdateCanvases();
        LayoutStaticElements();
        Canvas.ForceUpdateCanvases();
        LayoutSlotList();
        Canvas.ForceUpdateCanvases();

        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (slotUIs[i] != null)
                PositionSlot(slotUIs[i].GetComponent<RectTransform>(), i);
        }

        foreach (InventorySlotUI slot in slotUIs)
        {
            if (slot != null)
                slot.Refresh();
        }
    }

    void UpdateFeedback(EquipToggleResult result, UpgradeType type)
    {
        if (feedbackText == null)
            return;

        string upgradeName = UpgradeInventory.GetDisplayName(type);

        switch (result)
        {
            case EquipToggleResult.Equipped:
                feedbackText.text = upgradeName + " selected";
                break;
            case EquipToggleResult.Unequipped:
                feedbackText.text = upgradeName + " removed";
                break;
            case EquipToggleResult.NotOwned:
                feedbackText.text = "Buy " + upgradeName + " first";
                break;
            case EquipToggleResult.LoadoutFull:
                feedbackText.text = "Only 3 upgrades can be selected";
                break;
        }
    }
}
