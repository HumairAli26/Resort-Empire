using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopUIManager : MonoBehaviour
{
    public Transform contentPanel;                // Drag the 'Content' object from your ScrollView here
    public GameObject shopItemButtonPrefab;       // Drag your 'ShopItemPrefab' from your folders here
    public CrossPlatformPlacement placementSystem; // Drag your placement controller here
    
    public List<ShopItemData> availableItems;     // Drop your created ScriptableObjects into this list

    void Start()
    {
        PopulateShop();
    }

    void PopulateShop()
    {
        foreach (ShopItemData item in availableItems)
        {
            // Spawn a button template inside the scrollable content view
            GameObject newButton = Instantiate(shopItemButtonPrefab, contentPanel);

            // Update the UI text fields and artwork icons dynamically
            newButton.GetComponentInChildren<TMP_Text>().text = $"{item.itemName}\n${item.cost}";
            
            Image iconImage = newButton.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage != null && item.itemIcon != null)
            {
                iconImage.sprite = item.itemIcon;
            }

            // Bind a touch click event to tell the grid system to spawn the target item
            Button btn = newButton.GetComponent<Button>();
            btn.onClick.AddListener(() => OnItemButtonClicked(item));
        }
    }

    void OnItemButtonClicked(ShopItemData item)
    {
        Debug.Log($"Selected to build: {item.itemName}");
        
        // Pass the actual asset prefab over to your placement controller
        placementSystem.SetPreviewObject(item.itemPrefab);
    }
}