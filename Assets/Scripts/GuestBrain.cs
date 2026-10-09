using System;
using UnityEngine;

/// Needs + decision making. Sits on the customer prefab next to CustomerMovement.
[RequireComponent(typeof(CustomerMovement))]
public class GuestBrain : MonoBehaviour
{
    enum State { Entering, Idle, Navigating, Wandering, Using, Leaving }

    public static event Action<int> OnGuestPaid;   // EconomyManager subscribes

    [Header("Tuning")]
    public float seekThreshold = 45f;
    public float leaveHappiness = 18f;
    public float graceSeconds = 15f;
    public Vector2 decayPerSecond = new(1.4f, 2.6f);
    public bool debugLog = true;   // turn off once it works

    CustomerMovement move;
    readonly float[] needs = new float[4];
    readonly float[] decay = new float[4];
    State state = State.Entering;
    Facility target;
    Vector3 entryPoint, exitPoint;
    int cash;
    float age, idleTimer;

    public float Happiness { get { float s = 0; foreach (var n in needs) s += n; return s / needs.Length; } }

    public void Init(Vector3 entry, Vector3 exit, int startCash)
    {
        move = GetComponent<CustomerMovement>();
        entryPoint = entry; exitPoint = exit; cash = startCash;
        for (int i = 0; i < needs.Length; i++)
        {
            needs[i] = UnityEngine.Random.Range(60f, 100f);
            decay[i] = UnityEngine.Random.Range(decayPerSecond.x, decayPerSecond.y);
        }
        // Outside -> gate in a straight line, then the resort loop begins.
        move.WalkDirect(entryPoint, () => state = State.Idle);
    }

    void Update()
    {
        if (move == null) return;
        float dt = Time.deltaTime;
        age += dt;

        if (state != State.Using && state != State.Entering)
            for (int i = 0; i < needs.Length; i++) needs[i] = Mathf.Max(0, needs[i] - decay[i] * dt);

        if (state != State.Leaving && state != State.Entering && age > graceSeconds
            && (cash < 5 || Happiness < leaveHappiness))
            StartLeaving();

        if (state == State.Idle) TickIdle(dt);
        else if (state == State.Using) TickUsing(dt);
    }

    void TickIdle(float dt)
    {
        idleTimer -= dt;
        if (idleTimer > 0) return;
        idleTimer = 1f;

        // Try every need below the threshold, lowest first, until one has a usable facility.
        var order = new int[] { 0, 1, 2, 3 };
        Array.Sort(order, (a, b) => needs[a].CompareTo(needs[b]));
        foreach (int n in order)
        {
            if (needs[n] >= seekThreshold) break;
            var f = FacilityRegistry.FindClosest((NeedType)n, transform.position, cash);
            if (debugLog) Debug.Log($"[Guest] need {(NeedType)n}={needs[n]:F0} cash={cash} facility={(f ? f.name : "NONE")} registered={FacilityRegistry.Count}", this);
            if (f == null || !f.TryEnter()) continue;
            if (move.WalkTo(f.UsePosition, OnArrivedAtFacility)) { target = f; state = State.Navigating; return; }
            if (debugLog) Debug.LogWarning("[Guest] no walkable route to " + f.name, this);
            f.Leave();   // unreachable: release the slot
        }
        // Nothing urgent (or nothing available): stroll somewhere.
        if (move.WalkToRandomTile(() => state = State.Idle)) state = State.Wandering;
    }

    void OnArrivedAtFacility()
    {
        cash -= target.price;
        OnGuestPaid?.Invoke(target.price);
        state = State.Using;
    }

    void TickUsing(float dt)
    {
        int i = (int)target.serves;
        needs[i] = Mathf.Min(100, needs[i] + target.restorePerSecond * dt);
        if (needs[i] >= 95) { target.Leave(); target = null; state = State.Idle; idleTimer = 0.5f; }
    }

    void StartLeaving()
    {
        if (target != null) { target.Leave(); target = null; }
        state = State.Leaving;
        // Walk back along the paths to the gate, then straight out and despawn.
        bool ok = move.WalkTo(entryPoint, () => move.WalkDirect(exitPoint, move.Finish));
        if (!ok) move.WalkDirect(exitPoint, move.Finish);
    }

    void OnDestroy() { if (target != null) target.Leave(); }
}