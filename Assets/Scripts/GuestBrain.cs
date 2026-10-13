using System;
using System.Collections.Generic;
using UnityEngine;

/// Likes-driven guest. Day visitors only use facilities; overnight guests also book a room.
/// Sits on the customer prefab next to CustomerMovement.
[RequireComponent(typeof(CustomerMovement))]
public class GuestBrain : MonoBehaviour
{
    enum State { Entering, CheckingIn, Idle, Navigating, Wandering, Using, Leaving }

    [Serializable]
    public class Like
    {
        public FacilityType type;
        public float strength;   // 0.6 = mild fan, 1.4 = obsessed
        public float craving;    // 0..100, rises over time, drops when they visit
        public int visits;
    }

    public static event Action<int> OnGuestPaid;   // EconomyManager subscribes

    [Header("Behaviour")]
    public float cravingPerSecond = 3f;
    public float seekThreshold = 40f;
    public float leaveMood = 20f;
    public float graceSeconds = 20f;

    [Header("Visit length (seconds)")]
    public Vector2 dayVisitSeconds = new(90f, 180f);
    public Vector2 overnightStaySeconds = new(150f, 300f);
    public bool debugLog = true;

    [Header("Runtime (read only)")]
    public List<Like> likes = new();
    public bool isOvernight;

    CustomerMovement move;
    State state = State.Entering;
    Facility target, bookedRoom;
    Like targetLike;
    Vector3 entryPoint, exitPoint;
    int cash;
    float age, idleTimer, useTimer, visitLimit;

    public float Mood
    {
        get
        {
            if (likes.Count == 0) return 100f;
            float s = 0; foreach (var l in likes) s += l.craving;
            return 100f - s / likes.Count;
        }
    }

    /// Random liked facility types. Lodging (Room, Lodge) is never a "like".
    public static List<Like> RollLikes(List<FacilityType> available, int minLikes, int maxLikes, Vector2 strengthRange)
    {
        var pool = new List<FacilityType>(available);
        pool.RemoveAll(Facility.IsLodgingType);
        int n = Mathf.Min(UnityEngine.Random.Range(minLikes, maxLikes + 1), pool.Count);
        var result = new List<Like>();
        for (int i = 0; i < n; i++)
        {
            int idx = UnityEngine.Random.Range(0, pool.Count);
            result.Add(new Like
            {
                type = pool[idx],
                strength = UnityEngine.Random.Range(strengthRange.x, strengthRange.y),
                craving = UnityEngine.Random.Range(0f, 30f)
            });
            pool.RemoveAt(idx);
        }
        return result;
    }

    /// `room` must already be reserved with room.TryEnter() by the spawner; null = day visitor.
    public void Init(Vector3 entry, Vector3 exit, int startCash, List<Like> guestLikes, Facility room)
    {
        move = GetComponent<CustomerMovement>();
        entryPoint = entry; exitPoint = exit; cash = startCash;
        likes = guestLikes; bookedRoom = room; isOvernight = room != null;
        var r = isOvernight ? overnightStaySeconds : dayVisitSeconds;
        visitLimit = UnityEngine.Random.Range(r.x, r.y);
        move.WalkDirect(entryPoint, OnEnteredResort);
    }

    void OnEnteredResort()
    {
        if (bookedRoom == null) { state = State.Idle; return; }
        state = State.CheckingIn;
        if (!move.WalkTo(bookedRoom.UsePosition, OnCheckedIn))
        {   // can't reach the room: give it up and just be a day visitor
            bookedRoom.Leave(); bookedRoom = null; isOvernight = false; state = State.Idle;
        }
    }

    void OnCheckedIn()
    {
        cash -= bookedRoom.price;
        bookedRoom.RecordVisit(bookedRoom.price);
        OnGuestPaid?.Invoke(bookedRoom.price);
        state = State.Idle; idleTimer = 0.5f;
    }

    void Update()
    {
        if (move == null) return;
        float dt = Time.deltaTime;
        age += dt;

        if (state != State.Using && state != State.Entering)
            foreach (var l in likes)
                l.craving = Mathf.Min(100f, l.craving + l.strength * cravingPerSecond * dt);

        if (state != State.Leaving && state != State.Entering && state != State.CheckingIn)
        {
            bool broke = !isOvernight && cash < 5;                 // room already paid for
            bool unhappy = age > graceSeconds && Mood < leaveMood;
            bool timeUp = age >= visitLimit && (state == State.Idle || state == State.Wandering);
            if (broke || unhappy || timeUp) StartLeaving();
        }

        if (state == State.Idle) TickIdle(dt);
        else if (state == State.Using) TickUsing(dt);
    }

    void TickIdle(float dt)
    {
        idleTimer -= dt;
        if (idleTimer > 0) return;
        idleTimer = 1f;

        var order = new List<Like>(likes);
        order.Sort((a, b) => (b.craving * b.strength).CompareTo(a.craving * a.strength));

        foreach (var l in order)
        {
            if (l.craving < seekThreshold) break;
            var f = FacilityRegistry.FindClosest(l.type, transform.position, cash);
            if (debugLog) Debug.Log($"[Guest] likes {l.type} craving={l.craving:F0} cash={cash} facility={(f ? f.name : "NONE")}", this);
            if (f == null || !f.TryEnter()) continue;
            if (move.WalkTo(f.UsePosition, OnArrivedAtFacility)) { target = f; targetLike = l; state = State.Navigating; return; }
            if (debugLog) Debug.LogWarning("[Guest] no walkable route to " + f.name, this);
            f.Leave();
        }
        if (move.WalkToRandomTile(() => state = State.Idle)) state = State.Wandering;
    }

    void OnArrivedAtFacility()
    {
        cash -= target.price;
        target.RecordVisit(target.price);
        OnGuestPaid?.Invoke(target.price);
        useTimer = target.useSeconds;
        state = State.Using;
    }

    void TickUsing(float dt)
    {
        useTimer -= dt;
        targetLike.craving = Mathf.Max(0f, targetLike.craving - (100f / Mathf.Max(0.1f, target.useSeconds)) * dt);
        if (useTimer > 0) return;

        targetLike.craving = 0f;
        targetLike.visits++;
        target.Leave(); target = null; targetLike = null;
        state = State.Idle; idleTimer = 0.5f;
    }

    void StartLeaving()
    {
        if (target != null) { target.Leave(); target = null; }
        if (bookedRoom != null) { bookedRoom.Leave(); bookedRoom = null; }   // check out
        state = State.Leaving;
        bool ok = move.WalkTo(entryPoint, () => move.WalkDirect(exitPoint, move.Finish));
        if (!ok) move.WalkDirect(exitPoint, move.Finish);
    }

    void OnDestroy()
    {
        if (target != null) target.Leave();
        if (bookedRoom != null) bookedRoom.Leave();
    }
}