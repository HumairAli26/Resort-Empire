using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class CrossPlatformPlacement : MonoBehaviour
{
    [Header("References")]
    public Grid grid;
    public GridManager gridManager;
    public Camera mainCamera;

    [Header("Visual Tuning")]
    public Vector3 previewVisualOffset = Vector3.zero; // tweak in Inspector until it lines up with your cursor

    private GameObject previewObject;
    private GameObject selectedPrefab;
    private ShopItemData selectedItemData;
    private SpriteRenderer previewRenderer;

    private void Update()
    {
        // Nothing selected
        if (previewObject == null)
            return;

        // No pointer available
        if (Pointer.current == null)
            return;

        // Right-click cancels placement
        if (Mouse.current != null &&
            Mouse.current.rightButton.wasPressedThisFrame)
        {
            CancelPlacement();
            return;
        }

        // Get mouse/touch position on screen
        Vector2 screenPosition =
            Pointer.current.position.ReadValue();

        // Convert screen position to world position
        // using a ray from the camera
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        // Ground plane at Z = 0
        Plane groundPlane =
            new Plane(Vector3.forward, Vector3.zero);

        if (!groundPlane.Raycast(ray, out float distance))
            return;

        Vector3 worldPosition =
            ray.GetPoint(distance);

        // Make sure Z stays at 0
        worldPosition.z = 0f;

        // Convert world position to grid cell
        Vector3Int cellPosition =
            grid.WorldToCell(worldPosition);

        // Get exact center of the grid cell
        Vector3 snappedPosition =
            grid.GetCellCenterWorld(cellPosition);

        // Move preview (with visual offset applied so art lines up with cursor)
        previewObject.transform.position =
            snappedPosition + previewVisualOffset;

        // Convert cell to GridManager coordinates
        Vector2Int gridPosition =
            new Vector2Int(
                cellPosition.x,
                cellPosition.y
            );

        // Check if the tile is available
        bool canPlace =
            gridManager.IsTileEmpty(gridPosition);

        // Change preview color
        if (previewRenderer != null)
        {
            if (canPlace)
            {
                previewRenderer.color =
                    new Color(1f, 1f, 1f, 0.5f);
            }
            else
            {
                previewRenderer.color =
                    new Color(1f, 0.2f, 0.2f, 0.5f);
            }
        }

        // Check for click/tap
        if (Pointer.current.press.wasPressedThisFrame)
        {
            // Don't place if clicking UI
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (canPlace)
            {
                ConfirmPlacement(gridPosition);
            }
            else
            {
                Debug.Log("Cannot build here!");
            }
        }
    }

    public void SetPreviewObject(ShopItemData data)
    {
        // Remove previous preview
        if (previewObject != null)
        {
            Destroy(previewObject);
        }

        if (data == null || data.itemPrefab == null)
        {
            Debug.LogError("ShopItemData or its prefab is NULL!");
            return;
        }

        // Store selected data and prefab
        selectedItemData = data;
        selectedPrefab = data.itemPrefab;

        // Create preview
        previewObject =
            Instantiate(selectedPrefab);

        // Get SpriteRenderer
        previewRenderer =
            previewObject.GetComponentInChildren<SpriteRenderer>();

        // Make preview transparent
        if (previewRenderer != null)
        {
            previewRenderer.color =
                new Color(1f, 1f, 1f, 0.5f);
        }

        Debug.Log(
            "Selected: " +
            selectedItemData.itemName
        );
    }

    private void ConfirmPlacement(Vector2Int targetCell)
    {
        if (!gridManager.IsTileEmpty(targetCell))
        {
            Debug.Log("Tile is already occupied!");
            return;
        }

        // NEW: check and deduct funds before placing
        if (!EconomyManager.Instance.SpendMoney(selectedItemData.cost))
        {
            Debug.Log("Cannot afford this item!");
            return; // stop here — don't place, don't destroy preview yet
        }

        Vector3Int cellPosition = new Vector3Int(targetCell.x, targetCell.y, 0);
        Vector3 worldPosition = grid.GetCellCenterWorld(cellPosition) + previewVisualOffset;

        GameObject placedBuilding = Instantiate(
            selectedPrefab,
            worldPosition,
            previewObject.transform.rotation
        );

        PlaceableItem placeable = placedBuilding.GetComponent<PlaceableItem>();
        if (placeable != null)
        {
            placeable.Initialize(selectedItemData, targetCell);
        }

        gridManager.OccupyTile(targetCell);

        Debug.Log("Placed: " + placedBuilding.name + " at " + worldPosition);

        Destroy(previewObject);
        previewObject = null;
        selectedPrefab = null;
        selectedItemData = null;
        previewRenderer = null;
    }

    private void CancelPlacement()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
        }

        previewObject = null;
        selectedPrefab = null;
        selectedItemData = null;
        previewRenderer = null;

        Debug.Log("Placement cancelled.");
    }
}