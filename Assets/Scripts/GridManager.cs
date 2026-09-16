using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    // The master logbook: Maps a 2D Grid Position (Vector2Int) to a True/False occupancy status
    private Dictionary<Vector2Int, bool> occupiedTiles = new Dictionary<Vector2Int, bool>();

    // Call this to check if a player can build on a specific tile
    public bool IsTileEmpty(Vector2Int gridPosition)
    {
        if (occupiedTiles.ContainsKey(gridPosition))
        {
            // If it exists in our dictionary, return whether it is occupied
            return !occupiedTiles[gridPosition]; 
        }
        
        // If the tile isn't in our dictionary yet, it means it's brand new and empty!
        return true; 
    }

    // Call this when an object is successfully placed
    public void OccupyTile(Vector2Int gridPosition)
    {
        if (occupiedTiles.ContainsKey(gridPosition))
        {
            occupiedTiles[gridPosition] = true; // Update existing tile
        }
        else
        {
            occupiedTiles.Add(gridPosition, true); // Add new tile to logbook
        }
    }

    // Call this if a player bulldozes/deletes an object
    public void FreeTile(Vector2Int gridPosition)
    {
        if (occupiedTiles.ContainsKey(gridPosition))
        {
            occupiedTiles[gridPosition] = false;
        }
    }
}
