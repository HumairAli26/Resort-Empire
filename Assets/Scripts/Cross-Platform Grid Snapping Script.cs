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
    public Vector3 previewVisualOffset = Vector3.zero;

    [Header("Collider Settings")]
    [Tooltip("Layers containing objects that should block placement.")]
    public LayerMask placementBlockingLayers = ~0;

    [Tooltip("If enabled, trigger colliders also block placement.")]
    public bool blockTriggerColliders = true;

    private GameObject previewObject;
    private GameObject selectedPrefab;
    private ShopItemData selectedItemData;
    private SpriteRenderer previewRenderer;

    private Collider2D[] previewColliders;

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // Nothing selected
        if (previewObject == null)
            return;

        // No pointer
        if (Pointer.current == null)
            return;

        // -----------------------------------------------------
        // RIGHT CLICK = CANCEL
        // -----------------------------------------------------

        if (Mouse.current != null &&
            Mouse.current.rightButton.wasPressedThisFrame)
        {
            CancelPlacement();
            return;
        }

        // -----------------------------------------------------
        // GET POINTER POSITION
        // -----------------------------------------------------

        Vector2 screenPosition =
            Pointer.current.position.ReadValue();

        // -----------------------------------------------------
        // SCREEN → WORLD
        // -----------------------------------------------------

        Ray ray =
            mainCamera.ScreenPointToRay(screenPosition);

        Plane groundPlane =
            new Plane(Vector3.forward, Vector3.zero);

        if (!groundPlane.Raycast(
                ray,
                out float distance))
        {
            return;
        }

        Vector3 worldPosition =
            ray.GetPoint(distance);

        worldPosition.z = 0f;

        // -----------------------------------------------------
        // WORLD → GRID CELL
        // -----------------------------------------------------

        Vector3Int cellPosition =
            grid.WorldToCell(worldPosition);

        Vector3 snappedPosition =
            grid.GetCellCenterWorld(cellPosition);

        // -----------------------------------------------------
        // MOVE PREVIEW
        // -----------------------------------------------------

        previewObject.transform.position =
            snappedPosition + previewVisualOffset;

        // Make sure Physics2D immediately knows about
        // the preview's new position.
        Physics2D.SyncTransforms();

        // -----------------------------------------------------
        // GRID POSITION
        // -----------------------------------------------------

        Vector2Int gridPosition =
            new Vector2Int(
                cellPosition.x,
                cellPosition.y
            );

        // -----------------------------------------------------
        // CHECK 1:
        // GRID FOOTPRINT
        //
        // 3 × 1
        // X X X
        //
        // 2 × 2
        // X X
        // X X
        //
        // 3 × 2
        // X X X
        // X X X
        // -----------------------------------------------------

        bool gridIsFree =
            gridManager.IsAreaEmpty(
                gridPosition,
                selectedItemData.width,
                selectedItemData.height
            );

        // -----------------------------------------------------
        // CHECK 2:
        // ACTUAL COLLIDER SHAPE
        // -----------------------------------------------------

        bool colliderIsFree =
            IsColliderOverlapFree();

        // -----------------------------------------------------
        // FINAL RESULT
        // -----------------------------------------------------

        bool canPlace =
            gridIsFree &&
            colliderIsFree;

        // -----------------------------------------------------
        // CHANGE PREVIEW COLOR
        // -----------------------------------------------------

        if (previewRenderer != null)
        {
            if (canPlace)
            {
                // Valid
                previewRenderer.color =
                    new Color(
                        1f,
                        1f,
                        1f,
                        0.5f
                    );
            }
            else
            {
                // Invalid
                previewRenderer.color =
                    new Color(
                        1f,
                        0.2f,
                        0.2f,
                        0.5f
                    );
            }
        }

        // -----------------------------------------------------
        // CLICK / TAP
        // -----------------------------------------------------

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
                PlaceItem(gridPosition);
            }
            else
            {
                Debug.Log(
                    "Cannot build here! " +
                    "Grid area or collider is occupied."
                );
            }
        }
    }

    // =========================================================
    // CREATE PREVIEW
    // =========================================================

    public void SetPreviewObject(ShopItemData data)
    {
        // Remove old preview
        if (previewObject != null)
        {
            Destroy(previewObject);
        }

        // Validate data
        if (data == null ||
            data.itemPrefab == null)
        {
            Debug.LogError(
                "ShopItemData or its prefab is NULL!"
            );

            return;
        }

        // -----------------------------------------------------
        // SAVE SELECTED ITEM
        // -----------------------------------------------------

        selectedItemData = data;
        selectedPrefab = data.itemPrefab;

        // -----------------------------------------------------
        // CREATE PREVIEW
        // -----------------------------------------------------

        previewObject =
            Instantiate(selectedPrefab);

        // -----------------------------------------------------
        // GET SPRITE RENDERER
        // -----------------------------------------------------

        previewRenderer =
            previewObject.GetComponentInChildren<SpriteRenderer>();

        if (previewRenderer != null)
        {
            previewRenderer.color =
                new Color(
                    1f,
                    1f,
                    1f,
                    0.5f
                );
        }

        // -----------------------------------------------------
        // GET ALL COLLIDERS FROM PREVIEW
        // -----------------------------------------------------

        previewColliders =
            previewObject.GetComponentsInChildren<Collider2D>();

        if (previewColliders.Length == 0)
        {
            Debug.LogWarning(
                "The selected prefab has no Collider2D. " +
                "Physical overlap detection will be skipped."
            );
        }

        // -----------------------------------------------------
        // PREVIEW COLLIDERS MUST STAY ENABLED
        // -----------------------------------------------------
        //
        // We DO NOT disable them because we need their
        // actual PolygonCollider2D shape for the overlap test.
        //
        // But we disable Rigidbody2D simulation so the preview
        // cannot physically push other objects.
        // -----------------------------------------------------

        Rigidbody2D[] previewRigidbodies =
            previewObject.GetComponentsInChildren<Rigidbody2D>();

        foreach (Rigidbody2D rb in previewRigidbodies)
        {
            rb.simulated = false;
        }

        Debug.Log(
            "Selected: " +
            selectedItemData.itemName +
            " | Width: " +
            selectedItemData.width +
            " | Height: " +
            selectedItemData.height
        );
    }

    // =========================================================
    // ACTUAL COLLIDER OVERLAP CHECK
    // =========================================================
    //
    // This checks the REAL collider shape of the preview.
    //
    // PolygonCollider2D → actual polygon shape
    // BoxCollider2D     → actual box shape
    // CircleCollider2D  → actual circle shape
    //
    // It does NOT use a rectangular OverlapBox.
    // =========================================================

    private bool IsColliderOverlapFree()
    {
        // No preview
        if (previewObject == null)
            return false;

        // No colliders
        if (previewColliders == null ||
            previewColliders.Length == 0)
        {
            return true;
        }

        // Make sure physics sees the current preview position
        Physics2D.SyncTransforms();

        // -----------------------------------------------------
        // CREATE CONTACT FILTER
        // -----------------------------------------------------

        ContactFilter2D filter =
            new ContactFilter2D();

        filter.useLayerMask = true;

        filter.layerMask =
            placementBlockingLayers;

        filter.useTriggers =
            blockTriggerColliders;

        // -----------------------------------------------------
        // TEMP RESULT ARRAY
        // -----------------------------------------------------

        Collider2D[] results =
            new Collider2D[100];

        // -----------------------------------------------------
        // CHECK EVERY COLLIDER ON THE PREVIEW
        // -----------------------------------------------------

        foreach (Collider2D previewCollider in previewColliders)
        {
            if (previewCollider == null)
                continue;

            if (!previewCollider.enabled)
                continue;

            // Ask Unity:
            //
            // "What other colliders overlap this exact
            //  collider shape?"

            int count =
                previewCollider.Overlap(
                    filter,
                    results
                );

            // -------------------------------------------------
            // CHECK RESULTS
            // -------------------------------------------------

            for (int i = 0; i < count; i++)
            {
                Collider2D other =
                    results[i];

                if (other == null)
                    continue;

                // -------------------------------------------------
                // IGNORE OUR OWN PREVIEW COLLIDERS
                // -------------------------------------------------

                if (other.transform.IsChildOf(
                        previewObject.transform))
                {
                    continue;
                }

                // Extra safety check
                if (other.transform.root ==
                    previewObject.transform.root)
                {
                    continue;
                }

                // -------------------------------------------------
                // IGNORE TRIGGERS IF DISABLED
                // -------------------------------------------------

                if (!blockTriggerColliders &&
                    other.isTrigger)
                {
                    continue;
                }

                // -------------------------------------------------
                // FOUND REAL OBJECT
                // -------------------------------------------------

                Debug.Log(
                    "PLACEMENT BLOCKED BY: " +
                    other.gameObject.name
                );

                return false;
            }
        }

        // Nothing overlaps
        return true;
    }

    // =========================================================
    // PLACE ITEM
    // =========================================================
    //
    // IMPORTANT:
    // The preview is NOT destroyed here.
    //
    // The selectedPrefab and selectedItemData are NOT cleared.
    //
    // This allows the player to place multiple copies.
    // =========================================================

    private void PlaceItem(
        Vector2Int targetCell)
    {
        // -----------------------------------------------------
        // CHECK GRID AGAIN
        // -----------------------------------------------------

        if (!gridManager.IsAreaEmpty(
                targetCell,
                selectedItemData.width,
                selectedItemData.height))
        {
            Debug.Log(
                "Area is already occupied!"
            );

            return;
        }

        // -----------------------------------------------------
        // CHECK COLLIDER AGAIN
        // -----------------------------------------------------

        if (!IsColliderOverlapFree())
        {
            Debug.Log(
                "Cannot place! " +
                "Another object is overlapping."
            );

            return;
        }

        // -----------------------------------------------------
        // CHECK MONEY
        // -----------------------------------------------------

        if (!EconomyManager.Instance.SpendMoney(
                selectedItemData.cost))
        {
            Debug.Log(
                "Cannot afford this item!"
            );

            // IMPORTANT:
            // We do NOT cancel placement.
            //
            // The item stays in hand so the player can
            // press OK or choose another action.
            return;
        }

        // -----------------------------------------------------
        // WORLD POSITION
        // -----------------------------------------------------

        Vector3Int cellPosition =
            new Vector3Int(
                targetCell.x,
                targetCell.y,
                0
            );

        Vector3 worldPosition =
            grid.GetCellCenterWorld(
                cellPosition
            ) + previewVisualOffset;

        // -----------------------------------------------------
        // CREATE ACTUAL BUILDING
        // -----------------------------------------------------

        GameObject placedBuilding =
            Instantiate(
                selectedPrefab,
                worldPosition,
                previewObject.transform.rotation
            );

        // -----------------------------------------------------
        // INITIALIZE PLACEABLE ITEM
        // -----------------------------------------------------

        PlaceableItem placeable =
            placedBuilding.GetComponent<PlaceableItem>();

        if (placeable != null)
        {
            placeable.Initialize(
                selectedItemData,
                targetCell
            );
        }

        // -----------------------------------------------------
        // REGISTER WALKABLE AREA
        // -----------------------------------------------------

        if (selectedItemData.isWalkable)
        {
            for (int x = 0;
                 x < selectedItemData.width;
                 x++)
            {
                for (int y = 0;
                     y < selectedItemData.height;
                     y++)
                {
                    Vector2Int pathCell =
                        new Vector2Int(
                            targetCell.x + x,
                            targetCell.y + y
                        );

                    if (gridManager != null)
                    {
                        gridManager.RegisterWalkableTile(
                            pathCell
                        );
                    }
                }
            }
        }

        // -----------------------------------------------------
        // MARK GRID FOOTPRINT AS OCCUPIED
        // -----------------------------------------------------

        gridManager.OccupyArea(
            targetCell,
            selectedItemData.width,
            selectedItemData.height
        );

        Debug.Log(
            "Placed: " +
            placedBuilding.name +
            " at " +
            worldPosition
        );

        // -----------------------------------------------------
        // IMPORTANT:
        // DO NOT DESTROY PREVIEW
        // DO NOT CLEAR SELECTED PREFAB
        // DO NOT CLEAR SELECTED ITEM DATA
        //
        // The player is still holding the item.
        // The preview simply moves to the next location.
        // -----------------------------------------------------

        // Make sure preview is still active
        if (previewObject != null)
        {
            previewObject.SetActive(true);
        }
    }

    // =========================================================
    // OK / DONE BUTTON
    // =========================================================
    //
    // Connect this function to your UI OK/Done button.
    //
    // When pressed:
    // - Placement mode ends
    // - Preview disappears
    // - Selected item is cleared
    // =========================================================

    public void ConfirmPlacement()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
        }

        previewObject = null;
        selectedPrefab = null;
        selectedItemData = null;
        previewRenderer = null;
        previewColliders = null;

        Debug.Log(
            "Placement mode finished."
        );
    }

    // =========================================================
    // CANCEL PLACEMENT
    // =========================================================

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
        previewColliders = null;

        Debug.Log(
            "Placement cancelled."
        );
    }
}