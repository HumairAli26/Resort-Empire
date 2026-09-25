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
        EconomyManager.Instance.OnBalanceChanged += UpdateAffordability;
        UpdateAffordability(EconomyManager.Instance.CurrentBalance);
    }

    void OnDestroy()
    {
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnBalanceChanged -= UpdateAffordability;
    }

    void UpdateAffordability(int currentBalance)
    {
        button.interactable = currentBalance >= itemData.cost;
    }

    void SelectItem()
    {
        PlacementManager.SetPreviewObject(itemData);
    }
}