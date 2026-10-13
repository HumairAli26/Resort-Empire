using System.Collections.Generic;
using UnityEngine;

/// Global list of facilities so guests never scan the scene.
public static class FacilityRegistry
{
    static readonly List<Facility> all = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() => all.Clear();

    public static int Count => all.Count;

    public static void Register(Facility f)   { if (!all.Contains(f)) all.Add(f); }
    public static void Unregister(Facility f) => all.Remove(f);

    public static int CountOfType(FacilityType t)
    {
        int n = 0;
        foreach (var f in all) if (f.type == t) n++;
        return n;
    }

    /// Sum of `appeal` across built facilities of this type (a level-3 sauna counts for more).
    public static float AppealOfType(FacilityType t)
    {
        float a = 0f;
        foreach (var f in all) if (f.type == t) a += f.appeal;
        return a;
    }

    public static bool HasType(FacilityType t) => CountOfType(t) > 0;

    /// Distinct non-lodging facility types currently built (what guests can "like").
    public static void GetLikeableTypes(List<FacilityType> result)
    {
        result.Clear();
        foreach (var f in all)
            if (!f.IsLodging && !result.Contains(f.type)) result.Add(f.type);
    }

    /// Total guests all facilities and beds can hold at once.
    public static int TotalCapacity()
    {
        int n = 0;
        foreach (var f in all) n += f.capacity;
        return n;
    }

    /// A room or lodge with a free bed, or null. Caller must still call TryEnter() to reserve it.
    public static Facility FindFreeRoom()
    {
        foreach (var f in all) if (f.IsLodging && f.HasSpace) return f;
        return null;
    }

    public static Facility FindClosest(FacilityType type, Vector3 from, int cash)
    {
        Facility best = null; float bestD = float.MaxValue;
        foreach (var f in all)
        {
            if (f.type != type || !f.HasSpace || f.price > cash) continue;
            float d = (f.UsePosition - from).sqrMagnitude;
            if (d < bestD) { bestD = d; best = f; }
        }
        return best;
    }
}