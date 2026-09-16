using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem; // Make sure to include this!

public class CrossPlatformPlacement : MonoBehaviour
{
    public Grid grid;                   // Drag your Grid here
    public GameObject previewObject;    // The object/item prefab being placed
    public GridManager gridManager;     // Drag your GridManager here

    private Camera mainCamera;
    private SpriteRenderer previewRenderer;

    void Start()
    {
        mainCamera = Camera.main;
    }

    public void SetPreviewObject(GameObject newPreview)
    {
        previewObject = Instantiate(newPreview);
        previewRenderer = previewObject.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (previewObject == null) return;

        Vector2 screenPosition = Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPos.z = 0;

        Vector3Int cellPos = grid.WorldToCell(worldPos);
        previewObject.transform.position = grid.GetCellCenterWorld(cellPos);

        bool canPlace = gridManager.IsTileEmpty(new Vector2Int(cellPos.x, cellPos.y));
        
        if (previewRenderer != null)
        {
            previewRenderer.color = canPlace ? Color.white : new Color(1f, 0.3f, 0.3f, 0.8f);
        }
        if (Pointer.current.press.wasPressedThisFrame && canPlace)
        {
            ConfirmPlacement(new Vector2Int(cellPos.x, cellPos.y));
        }
    }

    void ConfirmPlacement(Vector2Int targetCell)
    {
        Vector2Int checkPos = new Vector2Int(targetCell.x, targetCell.y);

        // Ask the dictionary if the spot is clear
        if (gridManager.IsTileEmpty(checkPos))
        {
            // 1. Tell the logbook this spot is now taken!
            gridManager.OccupyTile(checkPos);

            // 2. Spawn the actual furniture item
            Instantiate(previewObject, transform.position, Quaternion.identity);
            
            Debug.Log("Item placed successfully!");
        }
        else
        {
            // Play an error sound or turn the preview red!
            Debug.Log("Cannot build here! Tile is already full.");
        }
    }
}