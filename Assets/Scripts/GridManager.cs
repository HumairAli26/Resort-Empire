using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    private HashSet<Vector2Int> occupiedTiles = new HashSet<Vector2Int>();
    
    // NEW: Stores all tiles that are marked as walkable paths
    private HashSet<Vector2Int> walkableTiles = new HashSet<Vector2Int>();

    public bool IsAreaEmpty(Vector2Int startPosition, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int currentTile = new Vector2Int(startPosition.x + x, startPosition.y + y);
                if (occupiedTiles.Contains(currentTile))
                {
                    return false; 
                }
            }
        }
        return true; 
    }

    public void OccupyArea(Vector2Int startPosition, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int currentTile = new Vector2Int(startPosition.x + x, startPosition.y + y);
                occupiedTiles.Add(currentTile);
            }
        }
    }

    // NEW: Register a walkable tile when placed by the player
    public void RegisterWalkableTile(Vector2Int gridPosition)
    {
        if (!walkableTiles.Contains(gridPosition))
        {
            walkableTiles.Add(gridPosition);
        }
    }

    // NEW: Provide the array of walkable paths to customers
    public Vector2Int[] GetWalkablePathArray()
    {
        Vector2Int[] pathArray = new Vector2Int[walkableTiles.Count];
        walkableTiles.CopyTo(pathArray);
        return pathArray;
    }

    public void FreeTile(Vector2Int gridPosition)
    {
        
    }
}