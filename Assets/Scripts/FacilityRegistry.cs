using System.Collections.Generic;
using UnityEngine;

/// Global list of facilities so guests never scan the scene.
public static class FacilityRegistry
{
    static readonly List<Facility> all = new();

    // Needed if you have Domain Reload disabled in Enter Play Mode settings.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() => all.Clear();

    public static int Count => all.Count;

    public static void Register(Facility f)   { if (!all.Contains(f)) all.Add(f); }
    public static void Unregister(Facility f) => all.Remove(f);

    public static Facility FindClosest(NeedType need, Vector3 from, int cash)
    {
        Facility best = null; float bestD = float.MaxValue;
        foreach (var f in all)
        {
            if (f.serves != need || !f.HasSpace || f.price > cash) continue;
            float d = (f.UsePosition - from).sqrMagnitude;
            if (d < bestD) { bestD = d; best = f; }
        }
        return best;
    }
}