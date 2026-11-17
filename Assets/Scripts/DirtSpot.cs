
using UnityEngine;

public class DirtSpot : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }

    public void Initialize(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
    }

    public void Clean()
    {
        Destroy(gameObject);
    }
}
