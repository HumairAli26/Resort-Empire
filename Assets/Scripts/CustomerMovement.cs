using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves a customer/NPC through the tycoon path system.
///
/// Features:
/// - Direct movement for entrance/exit
/// - BFS pathfinding across registered walkable tiles
/// - Movement to a specific world position
/// - Movement to a random walkable tile
/// - Automatic forward/backward animation
/// - Safe Animator handling
/// - Automatically switches walking animation on/off
/// - Notifies CustomerSpawner when the NPC finishes
/// </summary>
public class CustomerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Animation")]
    [Tooltip("Animator used for the walking animations.")]
    public Animator animator;

    [Tooltip("Animator parameter for normal/forward walking.")]
    public string forwardWalkParameter = "IsWalkingForward";

    [Tooltip("Animator parameter for backward walking.")]
    public string backwardWalkParameter = "IsWalkingBackward";

    // References
    private CustomerSpawner spawner;
    private Grid grid;
    private GridManager gridManager;

    // Movement
    private readonly Queue<Vector3> waypoints =
        new Queue<Vector3>();

    private Action onArrived;

    // Last position is used to determine movement direction.
    private Vector3 previousPosition;

    // Four-direction movement
    private static readonly Vector2Int[] Dirs =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    public bool IsMoving => waypoints.Count > 0;


    // =========================================================
    // SETUP
    // =========================================================

    public void Setup(
        CustomerSpawner s,
        Grid g,
        GridManager gm)
    {
        spawner = s;
        grid = g;
        gridManager = gm;

        // Try to find Animator on this object.
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // If Animator is not on this object,
        // search the children.
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        // Store starting position.
        previousPosition = transform.position;

        // Start idle.
        SetIdleAnimation();

        // Validation
        if (grid == null)
        {
            Debug.LogWarning(
                $"{name}: CustomerMovement has no Grid reference."
            );
        }

        if (gridManager == null)
        {
            Debug.LogWarning(
                $"{name}: CustomerMovement has no GridManager reference."
            );
        }

        if (animator == null)
        {
            Debug.LogWarning(
                $"{name}: No Animator was found. " +
                "Movement will work, but animations will not."
            );
        }
    }


    // =========================================================
    // DIRECT MOVEMENT
    // =========================================================

    /// <summary>
    /// Walk directly to a world position.
    ///
    /// Used for:
    /// Spawn point -> gate
    /// Gate -> exit
    /// </summary>
    public void WalkDirect(
        Vector3 world,
        Action arrived)
    {
        waypoints.Clear();

        waypoints.Enqueue(world);

        onArrived = arrived;

        previousPosition = transform.position;

        SetWalkingAnimation();
    }


    // =========================================================
    // WALK TO WORLD POSITION
    // =========================================================

    /// <summary>
    /// Finds a BFS route over registered walkable tiles.
    /// </summary>
    public bool WalkTo(
        Vector3 world,
        Action arrived)
    {
        // -----------------------------------------------------
        // VALIDATE REFERENCES
        // -----------------------------------------------------

        if (grid == null)
        {
            Debug.LogWarning(
                $"{name}: Cannot WalkTo because Grid is missing."
            );

            return false;
        }

        if (gridManager == null)
        {
            Debug.LogWarning(
                $"{name}: Cannot WalkTo because GridManager is missing."
            );

            return false;
        }

        // -----------------------------------------------------
        // GET WALKABLE CELLS
        // -----------------------------------------------------

        Vector2Int[] cells =
            gridManager.GetWalkablePathArray();

        if (cells == null || cells.Length == 0)
        {
            Debug.LogWarning(
                $"{name}: No walkable path tiles are registered."
            );

            return false;
        }

        HashSet<Vector2Int> walkable =
            new HashSet<Vector2Int>(cells);

        // -----------------------------------------------------
        // FIND START / GOAL
        // -----------------------------------------------------

        Vector2Int currentCell =
            ToCell(transform.position);

        Vector2Int targetCell =
            ToCell(world);

        Vector2Int start =
            Nearest(walkable, currentCell);

        Vector2Int goal =
            Nearest(walkable, targetCell);

        // -----------------------------------------------------
        // BFS
        // -----------------------------------------------------

        Dictionary<Vector2Int, Vector2Int> previous =
            new Dictionary<Vector2Int, Vector2Int>();

        Queue<Vector2Int> searchQueue =
            new Queue<Vector2Int>();

        searchQueue.Enqueue(start);

        previous[start] = start;

        while (
            searchQueue.Count > 0 &&
            !previous.ContainsKey(goal))
        {
            Vector2Int current =
                searchQueue.Dequeue();

            foreach (Vector2Int direction in Dirs)
            {
                Vector2Int next =
                    current + direction;

                // Tile isn't walkable.
                if (!walkable.Contains(next))
                    continue;

                // Already visited.
                if (previous.ContainsKey(next))
                    continue;

                previous[next] = current;

                searchQueue.Enqueue(next);
            }
        }

        // -----------------------------------------------------
        // NO PATH
        // -----------------------------------------------------

        if (!previous.ContainsKey(goal))
        {
            Debug.Log(
                $"{name}: No path found from " +
                $"{start} to {goal}."
            );

            SetIdleAnimation();

            return false;
        }

        // -----------------------------------------------------
        // BUILD PATH
        // -----------------------------------------------------

        List<Vector2Int> path =
            new List<Vector2Int>();

        Vector2Int currentPathCell =
            goal;

        while (currentPathCell != start)
        {
            path.Add(currentPathCell);

            currentPathCell =
                previous[currentPathCell];
        }

        path.Reverse();

        // -----------------------------------------------------
        // CREATE WAYPOINTS
        // -----------------------------------------------------

        waypoints.Clear();

        foreach (Vector2Int cell in path)
        {
            Vector3 waypoint =
                grid.GetCellCenterWorld(
                    new Vector3Int(
                        cell.x,
                        cell.y,
                        0
                    )
                );

            waypoints.Enqueue(waypoint);
        }

        // Final exact destination.
        waypoints.Enqueue(world);

        onArrived = arrived;

        previousPosition = transform.position;

        SetWalkingAnimation();

        return true;
    }


    // =========================================================
    // RANDOM WALK
    // =========================================================

    public bool WalkToRandomTile(
        Action arrived)
    {
        if (grid == null)
        {
            Debug.LogWarning(
                $"{name}: Cannot wander because Grid is missing."
            );

            return false;
        }

        if (gridManager == null)
        {
            Debug.LogWarning(
                $"{name}: Cannot wander because GridManager is missing."
            );

            return false;
        }

        Vector2Int[] cells =
            gridManager.GetWalkablePathArray();

        if (cells == null || cells.Length == 0)
        {
            Debug.LogWarning(
                $"{name}: No walkable tiles available."
            );

            return false;
        }

        Vector2Int randomCell =
            cells[
                UnityEngine.Random.Range(
                    0,
                    cells.Length
                )
            ];

        Vector3 worldPosition =
            grid.GetCellCenterWorld(
                new Vector3Int(
                    randomCell.x,
                    randomCell.y,
                    0
                )
            );

        return WalkTo(
            worldPosition,
            arrived
        );
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (waypoints.Count == 0)
        {
            return;
        }

        // -----------------------------------------------------
        // GET TARGET
        // -----------------------------------------------------

        Vector3 target =
            waypoints.Peek();

        // -----------------------------------------------------
        // DETERMINE DIRECTION
        // -----------------------------------------------------

        Vector3 movementDirection =
            target - transform.position;

        // -----------------------------------------------------
        // MOVE
        // -----------------------------------------------------

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed * Time.deltaTime
            );

        // -----------------------------------------------------
        // UPDATE ANIMATION DIRECTION
        // -----------------------------------------------------

        UpdateWalkingDirection(
            movementDirection
        );

        // -----------------------------------------------------
        // CHECK TARGET
        // -----------------------------------------------------

        if (
            (transform.position - target).sqrMagnitude
            > 0.0025f
        )
        {
            previousPosition = transform.position;

            return;
        }

        // Snap exactly to target.
        transform.position = target;

        // Remove waypoint.
        waypoints.Dequeue();

        previousPosition = transform.position;

        // -----------------------------------------------------
        // COMPLETELY FINISHED
        // -----------------------------------------------------

        if (waypoints.Count == 0)
        {
            SetIdleAnimation();

            Action callback =
                onArrived;

            onArrived = null;

            callback?.Invoke();
        }
    }


    // =========================================================
    // ANIMATION DIRECTION
    // =========================================================

    /// <summary>
    /// Determines whether the NPC is walking forward
    /// or backward based on its movement direction.
    ///
    /// For this setup:
    ///
    /// Positive Y = forward/up
    /// Negative Y = backward/down
    ///
    /// X movement uses the forward animation.
    /// </summary>
    private void UpdateWalkingDirection(
        Vector3 direction)
    {
        if (animator == null)
        {
            return;
        }

        // Ignore tiny movement.
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        bool walkingBackward =
            direction.y < -0.01f;

        bool walkingForward =
            direction.y >= -0.01f;

        // Forward animation
        animator.SetBool(
            forwardWalkParameter,
            walkingForward
        );

        // Backward animation
        animator.SetBool(
            backwardWalkParameter,
            walkingBackward
        );
    }


    // =========================================================
    // START WALKING
    // =========================================================

    private void SetWalkingAnimation()
    {
        if (animator == null)
        {
            return;
        }

        // Initially use forward animation.
        animator.SetBool(
            forwardWalkParameter,
            true
        );

        animator.SetBool(
            backwardWalkParameter,
            false
        );
    }


    // =========================================================
    // IDLE
    // =========================================================

    private void SetIdleAnimation()
    {
        if (animator == null)
        {
            return;
        }

        animator.SetBool(
            forwardWalkParameter,
            false
        );

        animator.SetBool(
            backwardWalkParameter,
            false
        );
    }


    // =========================================================
    // FINISH
    // =========================================================

    public void Finish()
    {
        waypoints.Clear();

        onArrived = null;

        SetIdleAnimation();

        if (spawner != null)
        {
            spawner.NPCFinished(this);
        }

        Destroy(gameObject);
    }


    // =========================================================
    // WORLD → CELL
    // =========================================================

    private Vector2Int ToCell(
        Vector3 world)
    {
        Vector3Int cell =
            grid.WorldToCell(world);

        return new Vector2Int(
            cell.x,
            cell.y
        );
    }


    // =========================================================
    // FIND NEAREST WALKABLE TILE
    // =========================================================

    private static Vector2Int Nearest(
        HashSet<Vector2Int> set,
        Vector2Int from)
    {
        if (set.Contains(from))
        {
            return from;
        }

        Vector2Int best =
            from;

        int bestDistance =
            int.MaxValue;

        foreach (Vector2Int cell in set)
        {
            int distance =
                Mathf.Abs(cell.x - from.x) +
                Mathf.Abs(cell.y - from.y);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = cell;
            }
        }

        return best;
    }
}