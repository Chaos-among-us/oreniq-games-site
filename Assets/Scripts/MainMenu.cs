using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public string gameSceneName = "Game";
    public string shopSceneName = "Shop";
    public string inventorySceneName = "Inventory";
    public bool createInventoryButtonAtRuntime = true;
    public string shopButtonObjectName = "ShopButton";
    public string exitButtonObjectName = "ExitButton";
    public string inventoryButtonObjectName = "InventoryButton";
    public float menuButtonSpacing = 150f;

    void Start()
    {
        if (createInventoryButtonAtRuntime)
            EnsureInventoryButtonExists();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenShop()
    {
        SceneManager.LoadScene(shopSceneName);
    }

    public void OpenInventory()
    {
        SceneManager.LoadScene(inventorySceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    void EnsureInventoryButtonExists()
    {
        if (GameObject.Find(inventoryButtonObjectName) != null)
            return;

        Button shopButton = FindButton(shopButtonObjectName);

        if (shopButton == null)
            return;

        Button inventoryButton = Instantiate(shopButton, shopButton.transform.parent);
        inventoryButton.gameObject.name = inventoryButtonObjectName;

        RectTransform inventoryRect = inventoryButton.GetComponent<RectTransform>();
        RectTransform shopRect = shopButton.GetComponent<RectTransform>();

        if (inventoryRect != null && shopRect != null)
        {
            inventoryRect.anchoredPosition = new Vector2(
                shopRect.anchoredPosition.x,
                shopRect.anchoredPosition.y - menuButtonSpacing);
        }

        Button exitButton = FindButton(exitButtonObjectName);

        if (exitButton != null)
        {
            RectTransform exitRect = exitButton.GetComponent<RectTransform>();

            if (exitRect != null && inventoryRect != null)
            {
                exitRect.anchoredPosition = new Vector2(
                    exitRect.anchoredPosition.x,
                    inventoryRect.anchoredPosition.y - menuButtonSpacing);
            }
        }

        TMP_Text label = inventoryButton.GetComponentInChildren<TMP_Text>(true);

        if (label != null)
            label.text = "Inventory";

        inventoryButton.onClick.RemoveAllListeners();
        inventoryButton.onClick.AddListener(OpenInventory);
    }

    Button FindButton(string objectName)
    {
        GameObject buttonObject = GameObject.Find(objectName);

        if (buttonObject == null)
            return null;

        return buttonObject.GetComponent<Button>();
    }
}
