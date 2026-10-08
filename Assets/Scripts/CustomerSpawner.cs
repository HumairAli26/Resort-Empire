using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("NPC Settings")]
    public GameObject npcPrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 5f;
    public int maxNPCs = 10;

    [Header("Resort Entry Settings")]
    public Grid grid;
    public GridManager gridManager;
    public Vector2Int resortEntryCell; // The grid coordinate of the resort entrance/gate

    private int currentNPCs = 0;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnNPC), 1f, spawnInterval);
    }

    private void SpawnNPC()
    {
        if (npcPrefab == null)
        {
            Debug.LogWarning("NPC Prefab is not assigned!");
            return;
        }

        if (currentNPCs >= maxNPCs)
        {
            return;
        }

        // Spawns outside the grid at the spawner's transform position
        GameObject newNPC = Instantiate(
            npcPrefab,
            transform.position,
            Quaternion.identity
        );

        currentNPCs++;

        CustomerMovement npcMovement = newNPC.GetComponent<CustomerMovement>();

        if (npcMovement != null)
        {
            npcMovement.SetSpawner(this);

            // Fetch current walkable path array from the GridManager
            Vector2Int[] walkablePaths = gridManager != null ? gridManager.GetWalkablePathArray() : new Vector2Int[0];

            // Convert the grid entry cell to world position
            Vector3 entryWorldPos = grid != null ? grid.GetCellCenterWorld(new Vector3Int(resortEntryCell.x, resortEntryCell.y, 0)) : transform.position;

            // Initialize the customer's route (Outside spawn -> Entry Gate -> Walkable Paths)
            npcMovement.InitializeRoute(transform.position, entryWorldPos, walkablePaths);
        }
    }

    public void NPCFinished(CustomerMovement npc)
    {
        currentNPCs--;

        if (currentNPCs < 0)
        {
            currentNPCs = 0;
        }
    }
}