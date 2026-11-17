using UnityEngine;
public enum RoomType
{
    Reception,
    Bedroom,
    Bathroom,
    Kitchen,
    Office,
    Restaurant,
    Storage
}

public class RoomCategory : MonoBehaviour
{
    public RoomType type;
}
