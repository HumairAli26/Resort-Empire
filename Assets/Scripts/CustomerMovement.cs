using UnityEngine;
using System.Collections.Generic;

public class CustomerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;

    private List<Vector3> dynamicWaypoints = new List<Vector3>();
    private int currentWaypoint = 0;
    private CustomerSpawner spawner;

    public void SetSpawner(CustomerSpawner customerSpawner)
    {
        spawner = customerSpawner;
    }

    // Called by the spawner to set up the journey from outside to inside the resort
    public void InitializeRoute(Vector3 outsideSpawnPos, Vector3 resortEntryPos, Vector2Int[] walkableGridPaths)
    {
        dynamicWaypoints.Clear();

        // 1. Start from outside spawn position
        dynamicWaypoints.Add(outsideSpawnPos);

        // 2. Move to the resort's entry gate/path
        dynamicWaypoints.Add(resortEntryPos);

        // 3. Add active walkable paths inside the resort (converted from Grid to World positions)
        if (walkableGridPaths != null && walkableGridPaths.Length > 0)
        {
            // Optional: Shuffle or pick a few walkable tiles for them to wander around on
            foreach (Vector2Int cell in walkableGridPaths)
            {
                // Assuming you have access to the grid or can calculate world position
                // For simplicity, we add the world positions of placed tiles
                Vector3 worldTilePos = new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f); // Adjust based on your grid cell layout
                dynamicWaypoints.Add(worldTilePos);
            }
        }

        currentWaypoint = 0;
    }

    private void Update()
    {
        if (dynamicWaypoints == null || dynamicWaypoints.Count == 0)
            return;

        if (currentWaypoint >= dynamicWaypoints.Count)
            return;

        Vector3 target = dynamicWaypoints[currentWaypoint];

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            currentWaypoint++;

            if (currentWaypoint >= dynamicWaypoints.Count)
            {
                // NPC reached the end of their route
                if (spawner != null)
                {
                    spawner.NPCFinished(this);
                }

                Destroy(gameObject);
            }
        }
    }
}