using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    private HashSet<Vector2Int> occupiedTiles = new HashSet<Vector2Int>();

    public bool IsTileEmpty(Vector2Int gridPosition)
    {
        return !occupiedTiles.Contains(gridPosition);
    }

    public void OccupyTile(Vector2Int gridPosition)
    {
        occupiedTiles.Add(gridPosition);
    }

    public void FreeTile(Vector2Int gridPosition)
    {
        occupiedTiles.Remove(gridPosition);
    }
}