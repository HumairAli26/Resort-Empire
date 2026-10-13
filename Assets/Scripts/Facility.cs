using System;
using UnityEngine;

public enum FacilityType { Room, Lodge, IceCream, Gym, Suana, Taco, HotDog, Arcade, Bar, Pool, Massage, Market, Restraunt, Hospital }

/// Put on every placeable facility prefab (room, restaurant, gym, sauna...).
/// Registers itself with FacilityRegistry, so PlacementManager needs no changes.
public class Facility : MonoBehaviour
{
    [Header("Identity")]
    public FacilityType type = FacilityType.Restraunt;
    [Tooltip("Shown in UI. Falls back to the type name.")]
    public string displayName;

    [Header("Economy")]
    [Tooltip("Paid per visit. For Room/Lodge: price per stay.")]
    public int price = 10;

    [Header("Capacity")]
    [Tooltip("Guests at once. For Room/Lodge this is the number of beds.")]
    public int capacity = 3;
    [Tooltip("Seconds a guest spends inside per visit. Ignored for Room/Lodge.")]
    public float useSeconds = 8f;

    [Header("Appeal")]
    [Tooltip("How strongly this attracts guests who like its type. 1 = normal. Upgrades raise it.")]
    public float appeal = 1f;
    public int level = 1;

    [Header("Positions")]
    [Tooltip("Where guests stand/walk to. Falls back to this transform.")]
    public Transform useSpot;

    [Header("Stats (read only)")]
    public int totalVisits;
    public int totalRevenue;

    int users;

    /// Fires whenever someone enters or leaves (for busy/full indicators).
    public event Action<Facility> OnOccupancyChanged;

    public int Users => users;
    public bool HasSpace => users < capacity;
    public Vector3 UsePosition => useSpot ? useSpot.position : transform.position;
    public string Label => string.IsNullOrEmpty(displayName) ? type.ToString() : displayName;

    /// Lodging types are booked for a whole stay and are never a guest's "like".
    public static bool IsLodgingType(FacilityType t) => t == FacilityType.Room || t == FacilityType.Lodge;
    public bool IsLodging => IsLodgingType(type);

    public bool TryEnter()
    {
        if (!HasSpace) return false;
        users++;
        OnOccupancyChanged?.Invoke(this);
        return true;
    }

    public void Leave()
    {
        users = Mathf.Max(0, users - 1);
        OnOccupancyChanged?.Invoke(this);
    }

    /// Called by guests when they pay, so stats stay in one place.
    public void RecordVisit(int paid)
    {
        totalVisits++;
        totalRevenue += paid;
    }

    /// One upgrade step. Call from your upgrade UI after the player pays.
    public void Upgrade(float appealBonus = 0.5f, int capacityBonus = 1, int priceBonus = 2)
    {
        level++;
        appeal += appealBonus;
        capacity += capacityBonus;
        price += priceBonus;
    }

    void OnEnable()  => FacilityRegistry.Register(this);
    void OnDisable() => FacilityRegistry.Unregister(this);

    void OnValidate()
    {
        capacity = Mathf.Max(1, capacity);
        useSeconds = Mathf.Max(0.1f, useSeconds);
        appeal = Mathf.Max(0f, appeal);
        price = Mathf.Max(0, price);
        level = Mathf.Max(1, level);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = IsLodging ? Color.cyan : Color.yellow;
        Gizmos.DrawWireSphere(UsePosition, 0.25f);
    }
}