using UnityEngine;

public class PlaceableItem : MonoBehaviour
{
    public string itemName;
    public Vector2Int size = new Vector2Int(1, 1); // Most items are 1x1, but rooms could be 3x3

    // Add other tycoon stats here later!
    public int cost;
    public int incomeGeneration;
    public int maintenanceCost;
    public int employeeRate;

    [HideInInspector] public Vector2Int gridPosition; // which cell it was placed at

    public void Initialize(ShopItemData data, Vector2Int placedAtCell)
    {
        itemName = data.itemName;
        cost = data.cost;
        size = data.size;
        // incomeGeneration / maintenanceCost / employeeRate here too,
        // once you add matching fields to ShopItemData

        gridPosition = placedAtCell;
    }
}