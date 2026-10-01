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

    [Header("Collider Overlap Settings")]
    [Tooltip("Layers containing objects that should block placement.")]
    public LayerMask placementBlockingLayers = ~0;

    [Tooltip("Ignore trigger colliders when checking placement.")]
    public bool ignoreTriggerColliders = true;

    private GameObject previewObject;
    private GameObject selectedPrefab;
    private ShopItemData selectedItemData;
    private SpriteRenderer previewRenderer;

    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        if (previewObject == null)
            return;

        if (Pointer.current == null)
            return;


        // -------------------------------------------------
        // RIGHT CLICK = CANCEL
        // -------------------------------------------------

        if (Mouse.current != null &&
            Mouse.current.rightButton.wasPressedThisFrame)
        {
            CancelPlacement();
            return;
        }


        // -------------------------------------------------
        // GET POINTER POSITION
        // -------------------------------------------------

        Vector2 screenPosition =
            Pointer.current.position.ReadValue();


        // -------------------------------------------------
        // SCREEN → WORLD
        // -------------------------------------------------

        Ray ray =
            mainCamera.ScreenPointToRay(screenPosition);


        Plane groundPlane =
            new Plane(Vector3.forward, Vector3.zero);


        if (!groundPlane.Raycast(ray, out float distance))
            return;


        Vector3 worldPosition =
            ray.GetPoint(distance);

        worldPosition.z = 0f;


        // -------------------------------------------------
        // WORLD → GRID
        // -------------------------------------------------

        Vector3Int cellPosition =
            grid.WorldToCell(worldPosition);


        Vector3 snappedPosition =
            grid.GetCellCenterWorld(cellPosition);


        // Move preview
        previewObject.transform.position =
            snappedPosition + previewVisualOffset;


        // -------------------------------------------------
        // GRID POSITION
        // -------------------------------------------------

        Vector2Int gridPosition =
            new Vector2Int(
                cellPosition.x,
                cellPosition.y
            );


        // -------------------------------------------------
        // GRID CHECK
        // -------------------------------------------------

        bool gridIsFree =
            gridManager.IsAreaEmpty(
                gridPosition,
                selectedItemData.width,
                selectedItemData.height
            );


        // -------------------------------------------------
        // COLLIDER CHECK
        // -------------------------------------------------

        bool colliderIsFree =
            IsColliderAreaFree();


        // -------------------------------------------------
        // FINAL CHECK
        // -------------------------------------------------

        bool canPlace =
            gridIsFree &&
            colliderIsFree;


        // -------------------------------------------------
        // PREVIEW COLOR
        // -------------------------------------------------

        if (previewRenderer != null)
        {
            if (canPlace)
            {
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
                previewRenderer.color =
                    new Color(
                        1f,
                        0.2f,
                        0.2f,
                        0.5f
                    );
            }
        }


        // -------------------------------------------------
        // CLICK / TAP
        // -------------------------------------------------

        if (Pointer.current.press.wasPressedThisFrame)
        {
            // Don't place when clicking UI
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
                Debug.Log(
                    "Cannot build here! Object or grid area is occupied."
                );
            }
        }
    }


    // =====================================================
    // CREATE PREVIEW
    // =====================================================

    public void SetPreviewObject(ShopItemData data)
    {
        // Remove old preview
        if (previewObject != null)
        {
            Destroy(previewObject);
        }


        if (data == null ||
            data.itemPrefab == null)
        {
            Debug.LogError(
                "ShopItemData or its prefab is NULL!"
            );

            return;
        }


        // Store selected item
        selectedItemData = data;
        selectedPrefab = data.itemPrefab;


        // Create preview
        previewObject =
            Instantiate(selectedPrefab);


        // -------------------------------------------------
        // FIND SPRITE RENDERER
        // -------------------------------------------------

        previewRenderer =
            previewObject.GetComponentInChildren<SpriteRenderer>();


        // Make preview transparent
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


        // -------------------------------------------------
        // PREPARE COLLIDERS
        // -------------------------------------------------

        PreparePreviewColliders();


        Debug.Log(
            "Selected: " +
            selectedItemData.itemName
        );
    }


    // =====================================================
    // PREPARE PREVIEW COLLIDERS
    // =====================================================

    private void PreparePreviewColliders()
    {
        if (previewObject == null)
            return;


        Collider2D[] colliders =
            previewObject.GetComponentsInChildren<Collider2D>();


        if (colliders.Length == 0)
        {
            Debug.LogWarning(
                "Selected prefab has no Collider2D. " +
                "Collider overlap detection will not work."
            );

            return;
        }


        foreach (Collider2D collider in colliders)
        {
            // Keep the collider enabled because
            // we use its actual shape for overlap detection.
            collider.enabled = true;
        }


        // Disable rigidbody simulation so the preview
        // doesn't physically move other objects.
        Rigidbody2D[] rigidbodies =
            previewObject.GetComponentsInChildren<Rigidbody2D>();


        foreach (Rigidbody2D rb in rigidbodies)
        {
            rb.simulated = false;
        }
    }


    // =====================================================
    // ACTUAL COLLIDER OVERLAP CHECK
    // =====================================================

    private bool IsColliderAreaFree()
    {
        if (previewObject == null)
            return false;


        Collider2D[] previewColliders =
            previewObject.GetComponentsInChildren<Collider2D>();


        if (previewColliders.Length == 0)
        {
            // No collider means there is nothing
            // to physically check.
            return true;
        }


        // -------------------------------------------------
        // CHECK EVERY COLLIDER ON THE PREVIEW
        // -------------------------------------------------

        foreach (Collider2D previewCollider in previewColliders)
        {
            if (previewCollider == null)
                continue;


            if (!previewCollider.enabled)
                continue;


            // Get every collider touching/overlapping
            // this exact collider shape.
            ContactFilter2D contactFilter =
                new ContactFilter2D();


            contactFilter.useLayerMask = true;
            contactFilter.layerMask =
                placementBlockingLayers;


            contactFilter.useTriggers =
                !ignoreTriggerColliders;


            Collider2D[] results =
                new Collider2D[100];


            int count =
                previewCollider.Overlap(
                    contactFilter,
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


                // Ignore the preview object's own colliders
                if (other.transform.IsChildOf(
                        previewObject.transform))
                {
                    continue;
                }


                // Ignore anything belonging to
                // this preview object.
                if (other.transform.root ==
                    previewObject.transform.root)
                {
                    continue;
                }


                // Ignore triggers if selected
                if (ignoreTriggerColliders &&
                    other.isTrigger)
                {
                    continue;
                }


                // We found another object.
                Debug.Log(
                    "Placement blocked by: " +
                    other.gameObject.name
                );


                return false;
            }
        }


        // Nothing overlapping
        return true;
    }


    // =====================================================
    // CONFIRM PLACEMENT
    // =====================================================

    private void ConfirmPlacement(
        Vector2Int targetCell)
    {
        // -------------------------------------------------
        // CHECK GRID AGAIN
        // -------------------------------------------------

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


        // -------------------------------------------------
        // CHECK COLLIDER AGAIN
        // -------------------------------------------------

        if (!IsColliderAreaFree())
        {
            Debug.Log(
                "Cannot place! Another object is overlapping."
            );

            return;
        }


        // -------------------------------------------------
        // CHECK MONEY
        // -------------------------------------------------

        if (!EconomyManager.Instance.SpendMoney(
                selectedItemData.cost))
        {
            Debug.Log(
                "Cannot afford this item!"
            );

            return;
        }


        // -------------------------------------------------
        // WORLD POSITION
        // -------------------------------------------------

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


        // -------------------------------------------------
        // CREATE ACTUAL OBJECT
        // -------------------------------------------------

        GameObject placedBuilding =
            Instantiate(
                selectedPrefab,
                worldPosition,
                previewObject.transform.rotation
            );


        // -------------------------------------------------
        // INITIALIZE PLACEABLE ITEM
        // -------------------------------------------------

        PlaceableItem placeable =
            placedBuilding.GetComponent<PlaceableItem>();


        if (placeable != null)
        {
            placeable.Initialize(
                selectedItemData,
                targetCell
            );
        }


        // -------------------------------------------------
        // OCCUPY GRID AREA
        // -------------------------------------------------

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


        // -------------------------------------------------
        // REMOVE PREVIEW
        // -------------------------------------------------

        Destroy(previewObject);

        previewObject = null;
        selectedPrefab = null;
        selectedItemData = null;
        previewRenderer = null;
    }


    // =====================================================
    // CANCEL PLACEMENT
    // =====================================================

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


        Debug.Log(
            "Placement cancelled."
        );
    }


    // =====================================================
    // DEBUG GIZMOS
    // =====================================================

    private void OnDrawGizmosSelected()
    {
        if (previewObject == null)
            return;


        Collider2D[] colliders =
            previewObject.GetComponentsInChildren<Collider2D>();


        foreach (Collider2D collider in colliders)
        {
            if (collider == null ||
                !collider.enabled)
                continue;


            Gizmos.matrix =
                collider.transform.localToWorldMatrix;


            if (collider is PolygonCollider2D polygon)
            {
                for (int path = 0;
                     path < polygon.pathCount;
                     path++)
                {
                    Vector2[] points =
                        polygon.GetPath(path);


                    for (int i = 0;
                         i < points.Length;
                         i++)
                    {
                        Vector2 current =
                            points[i];


                        Vector2 next =
                            points[
                                (i + 1) %
                                points.Length
                            ];


                        Gizmos.DrawLine(
                            current,
                            next
                        );
                    }
                }
            }


            Gizmos.matrix =
                Matrix4x4.identity;
        }
    }
}