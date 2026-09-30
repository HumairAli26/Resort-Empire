using UnityEngine;

public class PlaceableItem : MonoBehaviour
{
    public string itemName;
    public Vector2Int size = new Vector2Int(1, 1); 

    [Header("Tycoon Stats")]
    public int cost;
    public int incomeGeneration;
    public int maintenanceCost;
    public int employeeRate;
    public string itemtype; // e.g. "Room", "Plant", "Facility"

    [HideInInspector] public Vector2Int gridPosition; 

    public void Initialize(ShopItemData data, Vector2Int placedAtCell)
    {
        itemName = data.itemName;
        cost = data.cost;
        size = data.size;
        
        // 1. FORCE GET TYPE FROM SCRIPTABLE OBJECT DATA
        itemtype = data.itemtype; 

        gridPosition = placedAtCell;

        // 2. RUN INCREMENT
        IncrementStatsBasedOnType();
    }

    private void IncrementStatsBasedOnType()
    {
        if (GameStatsManager.Instance == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(itemtype))
        {
            return;
        }

        string cleanType = itemtype.Trim().ToLower();

        switch (cleanType)
        {
            case "room":
                GameStatsManager.Instance.AddRoom();
                break;
            case "plant":
                GameStatsManager.Instance.AddPlant();
                break;
            case "facility":
                GameStatsManager.Instance.AddFacility();
                break;
            default:
                break;
        }
    }

    /*private void OnDestroy()
    {
        // Don't trigger if stopping the game
        if (GameStatsManager.Instance == null || string.IsNullOrEmpty(itemtype)) return;

        string cleanType = itemtype.Trim().ToLower();
        switch (cleanType)
        {
            case "room": GameStatsManager.Instance.RemoveRoom(); break;
            case "plant": GameStatsManager.Instance.RemovePlant(); break;
            case "facility": GameStatsManager.Instance.RemoveFacility(); break;
        }
    }*/
}
