using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Tycoon/Shop Item")]
public class ShopItemData : ScriptableObject
{
    [Header("Grid Size")]
    public int width = 1;  // Number of tiles horizontally
    public int height = 1; // Number of tiles vertically
    public string itemName;
    public int cost;
    public Sprite itemIcon;
    public GameObject itemPrefab; // The actual game object built on the grid
    public Vector2Int size = new Vector2Int(1, 1); // Tile footprint (1x1, 2x2, 3x3, etc.)
    public string itemtype; // e.g. "Room", "Restaurant", "Attraction", etc.
    public bool isWalkable = false; // Can characters walk on this tile?
}