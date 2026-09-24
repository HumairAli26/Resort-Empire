using UnityEngine;

public class ItemPlacer : MonoBehaviour
{
    [SerializeField] private Grid grid;

    private GameObject selectedPrefab;

    private void Update()
    {
        // If no item has been selected from the shop
        if (selectedPrefab == null)
            return;

        // Left click = try to place
        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceItem();
        }

        // Right click = cancel placement
        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
        }
    }

    // Called by the Shop
    public void SelectItem(GameObject prefab)
    {
        selectedPrefab = prefab;

        Debug.Log("Selected: " + prefab.name);
    }

    private void TryPlaceItem()
    {
        Vector3 mousePosition =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector3Int cell =
            grid.WorldToCell(mousePosition);

        Vector2Int gridPosition =
            new Vector2Int(cell.x, cell.y);

        PlaceableItem item =
            selectedPrefab.GetComponent<PlaceableItem>();

        if (CanPlaceItem(gridPosition, item.size))
        {
            PlaceItem(gridPosition, item);

            // Clear selection after successful purchase
            selectedPrefab = null;
        }
        else
        {
            Debug.Log("Cannot place here!");
        }
    }

    private bool CanPlaceItem(
        Vector2Int startPosition,
        Vector2Int size)
    {
        GridManager gridManager =
            FindFirstObjectByType<GridManager>();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int position =
                    new Vector2Int(
                        startPosition.x + x,
                        startPosition.y + y
                    );

                if (!gridManager.IsTileEmpty(position))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void PlaceItem(
        Vector2Int gridPosition,
        PlaceableItem item)
    {
        Vector3Int cellPosition =
            new Vector3Int(
                gridPosition.x,
                gridPosition.y,
                0
            );

        Vector3 worldPosition =
            grid.GetCellCenterWorld(cellPosition);

        GameObject newItem =
            Instantiate(
                selectedPrefab,
                worldPosition,
                Quaternion.identity
            );

        PlaceableItem placedItem =
            newItem.GetComponent<PlaceableItem>();

        GridManager gridManager =
            FindFirstObjectByType<GridManager>();

        for (int x = 0; x < placedItem.size.x; x++)
        {
            for (int y = 0; y < placedItem.size.y; y++)
            {
                Vector2Int position =
                    new Vector2Int(
                        gridPosition.x + x,
                        gridPosition.y + y
                    );

                gridManager.OccupyTile(position);
            }
        }

        Debug.Log("Purchased and placed: " + placedItem.itemName);
    }

    private void CancelPlacement()
    {
        selectedPrefab = null;

        Debug.Log("Placement cancelled");
    }
}