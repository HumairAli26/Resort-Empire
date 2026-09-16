using UnityEngine;

public class ShopTabController : MonoBehaviour
{
    [Header("Sub Panels")]
    public GameObject[] subPanels; // Drag HousesPanel, FacilityPanel, etc. here

    private void Start()
    {
        // Keep the menu clean by hiding the sub-bar entirely when the game starts
        gameObject.SetActive(false);
    }

    public void OpenCategoryTab(int panelIndex)
    {
        // 1. Make sure the main sub-bar container is turned on
        gameObject.SetActive(true);

        // 2. Turn off all individual sub-panels first
        for (int i = 0; i < subPanels.Length; i++)
        {
            if (subPanels[i] != null)
            {
                subPanels[i].SetActive(false);
            }
        }

        // 3. Turn on only the one the player clicked
        if (panelIndex >= 0 && panelIndex < subPanels.Length)
        {
            subPanels[panelIndex].SetActive(true);
        }
    }

    // Call this if the player clicks a "Close" button or clicks back onto the main game screen
    public void CloseShopMenu()
    {
        gameObject.SetActive(false);
    }
}
