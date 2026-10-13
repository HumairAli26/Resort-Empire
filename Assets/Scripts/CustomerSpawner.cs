using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("NPC Settings")]
    [Tooltip("Add as many guest prefabs as you like. One is picked at random for each spawn.")]
    public GameObject[] npcPrefabs;
    [Tooltip("Avoid spawning the same prefab twice in a row (when you have more than one).")]
    public bool avoidRepeats = true;

    [Header("Spawn Settings")]
    [Tooltip("Seconds between spawn ATTEMPTS. Whether one succeeds depends on resort appeal.")]
    public float spawnInterval = 3f;
    public int maxNPCs = 10;

    [Header("Guest Likes")]
    public int minLikes = 2;
    public int maxLikes = 3;
    public Vector2 strengthRange = new(0.6f, 1.4f);

    [Header("Appeal")]
    [Tooltip("Total appeal of one facility type beyond this adds nothing. A normal facility has appeal 1.")]
    public float appealCap = 3f;
    [Tooltip("Max guests = total facility capacity x this (never above Max NPCs).")]
    public float capacityMultiplier = 1.5f;

    [Header("Day visitors vs overnight guests")]
    [Range(0f, 1f), Tooltip("Chance a guest wants a room, if one is free.")]
    public float overnightChance = 0.4f;
    public Vector2Int dayCash = new(60, 140);
    public Vector2Int overnightCash = new(150, 350);

    [Header("Resort Entry Settings")]
    public Grid grid;
    public GridManager gridManager;
    public Vector2Int resortEntryCell;

    private int currentNPCs = 0;
    private int lastPrefabIndex = -1;
    private readonly List<FacilityType> availableTypes = new();

    private void Start()
    {
        InvokeRepeating(nameof(SpawnNPC), 1f, spawnInterval);
    }

    /// 0..1: how attractive the resort is to a guest with these likes.
    float Appeal(List<GuestBrain.Like> likes)
    {
        float sum = 0f;
        foreach (var l in likes)
        {
            float a = Mathf.Min(FacilityRegistry.AppealOfType(l.type), appealCap);
            sum += l.strength * a / Mathf.Max(0.01f, appealCap);
        }
        return Mathf.Clamp01(sum / Mathf.Max(1, likes.Count));
    }

    /// Random prefab from the list, skipping empty slots and (optionally) the last one used.
    GameObject PickPrefab()
    {
        var valid = new List<int>();
        for (int i = 0; i < npcPrefabs.Length; i++)
            if (npcPrefabs[i] != null) valid.Add(i);
        if (valid.Count == 0) return null;

        if (avoidRepeats && valid.Count > 1) valid.Remove(lastPrefabIndex);

        lastPrefabIndex = valid[Random.Range(0, valid.Count)];
        return npcPrefabs[lastPrefabIndex];
    }

    private void SpawnNPC()
    {
        if (npcPrefabs == null || npcPrefabs.Length == 0) { Debug.LogWarning("No NPC prefabs assigned!"); return; }
        int cap = Mathf.Min(maxNPCs, Mathf.CeilToInt(FacilityRegistry.TotalCapacity() * capacityMultiplier));
        if (currentNPCs >= cap) return;

        // 1. Roll who is thinking about visiting, and decide if your resort tempts them.
        FacilityRegistry.GetLikeableTypes(availableTypes);
        if (availableTypes.Count == 0) return;   // nothing built for guests to enjoy: nobody comes
        var likes = GuestBrain.RollLikes(availableTypes, minLikes, maxLikes, strengthRange);
        if (Random.value > Appeal(likes)) return;

        // 2. Overnight guest (needs a free room they can afford) or day visitor.
        Facility room = null;
        int cash = Random.Range(dayCash.x, dayCash.y + 1);
        if (Random.value < overnightChance)
        {
            var free = FacilityRegistry.FindFreeRoom();
            int richCash = Random.Range(overnightCash.x, overnightCash.y + 1);
            if (free != null && free.price <= richCash && free.TryEnter())
            {
                room = free; cash = richCash;
            }
        }

        // 3. Spawn.
        GameObject prefab = PickPrefab();
        if (prefab == null)
        {
            Debug.LogWarning("All NPC prefab slots are empty!");
            if (room != null) room.Leave();
            return;
        }
        GameObject newNPC = Instantiate(prefab, transform.position, Quaternion.identity);
        var move = newNPC.GetComponent<CustomerMovement>();
        var brain = newNPC.GetComponent<GuestBrain>();
        if (move == null || brain == null)
        {
            Debug.LogWarning("NPC prefab needs CustomerMovement and GuestBrain.");
            if (room != null) room.Leave();
            Destroy(newNPC);
            return;
        }

        currentNPCs++;
        Vector3 entryWorld = grid.GetCellCenterWorld(new Vector3Int(resortEntryCell.x, resortEntryCell.y, 0));
        move.Setup(this, grid, gridManager);
        brain.Init(entryWorld, transform.position, cash, likes, room);
    }

    public void NPCFinished(CustomerMovement npc)
    {
        currentNPCs = Mathf.Max(0, currentNPCs - 1);
    }
}