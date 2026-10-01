using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    private HashSet<Vector2Int> occupiedTiles = new HashSet<Vector2Int>();

    public bool IsAreaEmpty(Vector2Int startPosition, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int currentTile = new Vector2Int(startPosition.x + x, startPosition.y + y);
                if (occupiedTiles.Contains(currentTile))
                {
                    return false; // Found a tile that is already blocked!
                }
            }
        }
        return true; // The whole area is clear
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

    public void FreeTile(Vector2Int gridPosition)
    {
        
    }
}