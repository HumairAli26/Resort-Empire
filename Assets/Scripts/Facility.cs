using UnityEngine;

public enum NeedType { Hunger, Energy, Fun, Bladder }

/// Put on every placeable facility prefab (restaurant, room, arcade, restroom).
public class Facility : MonoBehaviour
{
    public NeedType serves = NeedType.Hunger;
    public int price = 10;
    public float restorePerSecond = 30f;
    public int capacity = 3;
    [Tooltip("Where guests stand/walk to. Falls back to this transform.")]
    public Transform useSpot;

    int users;
    public bool HasSpace => users < capacity;
    public Vector3 UsePosition => useSpot ? useSpot.position : transform.position;

    public bool TryEnter() { if (!HasSpace) return false; users++; return true; }
    public void Leave() { users = Mathf.Max(0, users - 1); }

    // Registering here means PlacementManager needs no changes:
    // placing the prefab enables it, demolishing destroys it.
    void OnEnable()  => FacilityRegistry.Register(this);
    void OnDisable() => FacilityRegistry.Unregister(this);
}
