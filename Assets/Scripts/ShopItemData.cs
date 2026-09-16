using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Tycoon/Shop Item")]
public class ShopItemData : ScriptableObject
{
    public string itemName;
    public int cost;
    public Sprite itemIcon;
    public GameObject itemPrefab; // The actual game object built on the grid
}
