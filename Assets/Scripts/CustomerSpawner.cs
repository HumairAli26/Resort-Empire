using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("NPC Settings")]
    public GameObject npcPrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 5f;
    public int maxNPCs = 10;
    public int minCash = 60;
    public int maxCash = 180;

    [Header("Resort Entry Settings")]
    public Grid grid;
    public GridManager gridManager;
    public Vector2Int resortEntryCell;

    private int currentNPCs = 0;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnNPC), 1f, spawnInterval);
    }

    private void SpawnNPC()
    {
        if (npcPrefab == null) { Debug.LogWarning("NPC Prefab is not assigned!"); return; }
        if (currentNPCs >= maxNPCs) return;

        GameObject newNPC = Instantiate(npcPrefab, transform.position, Quaternion.identity);
        currentNPCs++;

        var move = newNPC.GetComponent<CustomerMovement>();
        var brain = newNPC.GetComponent<GuestBrain>();
        if (move == null || brain == null)
        {
            Debug.LogWarning("NPC prefab needs CustomerMovement and GuestBrain.");
            return;
        }

        Vector3 entryWorld = grid.GetCellCenterWorld(new Vector3Int(resortEntryCell.x, resortEntryCell.y, 0));
        move.Setup(this, grid, gridManager);
        brain.Init(entryWorld, transform.position, Random.Range(minCash, maxCash));
    }

    public void NPCFinished(CustomerMovement npc)
    {
        currentNPCs = Mathf.Max(0, currentNPCs - 1);
    }
}