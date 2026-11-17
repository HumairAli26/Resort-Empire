
using System.Collections.Generic;
using UnityEngine;

public class JanitorCleaningArea : MonoBehaviour
{
    [Header("Cleaning Range")]
    [Min(1)]
    public int cleaningRange = 3;

    [Header("Prefabs")]
    public GameObject janitorPrefab;
    public GameObject dirtPrefab;

    [Header("Dirt Spawning")]
    public float dirtSpawnInterval = 5f;
    public int maximumDirtSpots = 5;

    [Header("Range Preview")]
    public bool showRange = true;
    public Color rangeColor = new Color(0f, 1f, 0f, 0.3f);

    private GridManager gridManager;
    private float spawnTimer;
    private bool initialized;
    private JanitorAI janitor;

    private void Start()
    {
        gridManager = GridManager.Instance;

        if (gridManager == null || gridManager.Grid == null)
        {
            Debug.LogError(
                "JanitorCleaningArea requires a working GridManager and Grid.",
                this
            );
            return;
        }

        SpawnJanitor();
        initialized = true;
        SpawnDirt();
    }

    private void Update()
    {
        if (!initialized || dirtPrefab == null)
            return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= dirtSpawnInterval)
        {
            spawnTimer = 0f;
            SpawnDirt();
        }
    }

    public bool IsTileInRange(Vector3Int tilePosition)
    {
        if (gridManager == null || gridManager.Grid == null)
            return false;

        Vector3Int center =
            gridManager.Grid.WorldToCell(transform.position);

        return Mathf.Abs(tilePosition.x - center.x)
             + Mathf.Abs(tilePosition.y - center.y)
             <= cleaningRange;
    }

    private void SpawnJanitor()
    {
        if (janitorPrefab == null)
        {
            Debug.LogWarning("Assign the Janitor Prefab.", this);
            return;
        }

        Vector2Int[] walkable = gridManager.GetWalkablePathArray();

        if (walkable.Length == 0)
        {
            Debug.LogWarning(
                "Register walkable tiles before spawning the janitor.",
                this
            );
            return;
        }

        Vector2Int facilityCell = CellPosition(transform.position);
        Vector2Int spawnCell = walkable[0];
        int closestDistance = int.MaxValue;

        foreach (Vector2Int cell in walkable)
        {
            if (!IsTileInRange(new Vector3Int(cell.x, cell.y, 0)))
                continue;

            int distance =
                Mathf.Abs(cell.x - facilityCell.x) +
                Mathf.Abs(cell.y - facilityCell.y);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                spawnCell = cell;
            }
        }

        if (closestDistance == int.MaxValue)
        {
            Debug.LogWarning(
                "No walkable tile is inside the janitor facility's range.",
                this
            );
            return;
        }

        GameObject worker = Instantiate(
            janitorPrefab,
            gridManager.Grid.GetCellCenterWorld(
                new Vector3Int(spawnCell.x, spawnCell.y, 0)
            ),
            Quaternion.identity
        );

        janitor = worker.GetComponent<JanitorAI>();

        if (janitor == null)
        {
            Debug.LogError(
                "The Janitor Prefab needs the JanitorAI component.",
                worker
            );
            Destroy(worker);
            return;
        }

        janitor.Initialize(this);
    }

    private void SpawnDirt()
    {
        if (dirtPrefab == null)
            return;

        DirtSpot[] existingDirt = FindObjectsOfType<DirtSpot>();

        if (existingDirt.Length >= maximumDirtSpots)
            return;

        Vector2Int[] walkable = gridManager.GetWalkablePathArray();
        List<Vector2Int> candidates = new List<Vector2Int>();

        foreach (Vector2Int cell in walkable)
        {
            if (!IsTileInRange(new Vector3Int(cell.x, cell.y, 0)))
                continue;

            bool alreadyDirty = false;

            foreach (DirtSpot dirt in existingDirt)
            {
                if (dirt != null && dirt.GridPosition == cell)
                {
                    alreadyDirty = true;
                    break;
                }
            }

            if (!alreadyDirty)
                candidates.Add(cell);
        }

        if (candidates.Count == 0)
            return;

        Vector2Int chosen = candidates[Random.Range(0, candidates.Count)];

        GameObject dirtObject = Instantiate(
            dirtPrefab,
            gridManager.Grid.GetCellCenterWorld(
                new Vector3Int(chosen.x, chosen.y, 0)
            ),
            Quaternion.identity
        );

        DirtSpot dirtSpot = dirtObject.GetComponent<DirtSpot>();

        if (dirtSpot == null)
        {
            Debug.LogError(
                "The Dirt Prefab needs the DirtSpot component.",
                dirtObject
            );
            Destroy(dirtObject);
            return;
        }

        dirtSpot.Initialize(chosen);
    }

    private Vector2Int CellPosition(Vector3 worldPosition)
    {
        Vector3Int cell = gridManager.Grid.WorldToCell(worldPosition);
        return new Vector2Int(cell.x, cell.y);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showRange)
            return;

        Grid grid = gridManager != null
            ? gridManager.Grid
            : FindObjectOfType<Grid>();

        if (grid == null)
            return;

        Vector3Int center = grid.WorldToCell(transform.position);
        Gizmos.color = rangeColor;

        for (int x = -cleaningRange; x <= cleaningRange; x++)
        {
            for (int y = -cleaningRange; y <= cleaningRange; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) > cleaningRange)
                    continue;

                Vector3 cellCenter = grid.GetCellCenterWorld(
                    new Vector3Int(center.x + x, center.y + y, center.z)
                );

                Vector3 size = grid.cellSize;

                Gizmos.DrawCube(
                    cellCenter,
                    new Vector3(size.x * 0.9f, size.y * 0.9f, 0.05f)
                );
            }
        }
    }
}
