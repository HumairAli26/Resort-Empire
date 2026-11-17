
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    public Grid Grid;
    public int Size = 20;

    private HashSet<Vector2Int> occupiedTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> walkableTiles = new HashSet<Vector2Int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple GridManager instances found!");
            return;
        }

        Instance = this;

        if (Grid == null)
        {
            Grid = GetComponent<Grid>();
        }

        if (Grid == null)
        {
            Debug.LogError("GridManager: No Grid component assigned or found!");
        }
    }

    public bool IsAreaEmpty(Vector2Int startPosition, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int currentTile = new Vector2Int(
                    startPosition.x + x,
                    startPosition.y + y
                );

                if (occupiedTiles.Contains(currentTile))
                    return false;
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
                Vector2Int currentTile = new Vector2Int(
                    startPosition.x + x,
                    startPosition.y + y
                );

                occupiedTiles.Add(currentTile);
            }
        }
    }

    public void RegisterWalkableTile(Vector2Int gridPosition)
    {
        if (!walkableTiles.Contains(gridPosition))
        {
            walkableTiles.Add(gridPosition);
        }
    }

    public Vector2Int[] GetWalkablePathArray()
    {
        Vector2Int[] pathArray = new Vector2Int[walkableTiles.Count];
        walkableTiles.CopyTo(pathArray);
        return pathArray;
    }

    public void FreeTile(Vector2Int gridPosition)
    {
        occupiedTiles.Remove(gridPosition);
    }
}
