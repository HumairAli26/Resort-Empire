using UnityEngine;
using UnityEngine.UI;

public class ShopItemButton : MonoBehaviour
{
    public ShopItemData itemData;
    public CrossPlatformPlacement PlacementManager;
    public Button button;

    void Start()
    {
        button.onClick.AddListener(SelectItem);
    }

    void SelectItem()
    {
        PlacementManager.SetPreviewObject(itemData);
    }
}