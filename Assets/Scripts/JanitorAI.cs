
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JanitorAI : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float cleaningTime = 1.5f;

    private JanitorCleaningArea cleaningArea;
    private GridManager gridManager;
    private bool isWorking;

    public void Initialize(JanitorCleaningArea area)
    {
        cleaningArea = area;
        gridManager = GridManager.Instance;
    }

    private IEnumerator Start()
    {
        while (cleaningArea == null ||
               GridManager.Instance == null ||
               GridManager.Instance.Grid == null)
        {
            yield return null;
        }

        gridManager = GridManager.Instance;

        while (true)
        {
            if (cleaningArea == null)
                yield break;

            DirtSpot target = FindNearestReachableDirt();

            if (target == null)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            isWorking = true;

            List<Vector2Int> path = FindPath(
                CurrentCell(),
                target.GridPosition
            );

            if (path == null || path.Count == 0)
            {
                isWorking = false;
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            bool reached = true;

            foreach (Vector2Int cell in path)
            {
                Vector3 destination = CellCenter(cell);

                while (Vector3.Distance(transform.position, destination) > 0.03f)
                {
                    if (target == null ||
                        !cleaningArea.IsTileInRange(
                            new Vector3Int(
                                target.GridPosition.x,
                                target.GridPosition.y,
                                0
                            )
                        ))
                    {
                        reached = false;
                        break;
                    }

                    transform.position = Vector3.MoveTowards(
                        transform.position,
                        destination,
                        moveSpeed * Time.deltaTime
                    );

                    yield return null;
                }

                if (!reached)
                    break;

                transform.position = destination;
            }

            if (reached && target != null)
            {
                yield return new WaitForSeconds(cleaningTime);

                if (target != null)
                    target.Clean();
            }

            isWorking = false;
            yield return new WaitForSeconds(0.2f);
        }
    }

    private Vector2Int CurrentCell()
    {
        Vector3Int cell = gridManager.Grid.WorldToCell(transform.position);
        return new Vector2Int(cell.x, cell.y);
    }

    private Vector3 CellCenter(Vector2Int cell)
    {
        return gridManager.Grid.GetCellCenterWorld(
            new Vector3Int(cell.x, cell.y, 0)
        );
    }

    private DirtSpot FindNearestReachableDirt()
    {
        DirtSpot[] dirtSpots = FindObjectsOfType<DirtSpot>();

        Vector2Int start = CurrentCell();
        DirtSpot best = null;
        int bestDistance = int.MaxValue;

        foreach (DirtSpot dirt in dirtSpots)
        {
            if (dirt == null)
                continue;

            Vector3Int targetCell = new Vector3Int(
                dirt.GridPosition.x,
                dirt.GridPosition.y,
                0
            );

            if (!cleaningArea.IsTileInRange(targetCell))
                continue;

            List<Vector2Int> path = FindPath(start, dirt.GridPosition);

            if (path == null)
                continue;

            if (path.Count < bestDistance)
            {
                bestDistance = path.Count;
                best = dirt;
            }
        }

        return best;
    }

    private List<Vector2Int> FindPath(
        Vector2Int start,
        Vector2Int goal)
    {
        HashSet<Vector2Int> walkable =
            new HashSet<Vector2Int>(
                gridManager.GetWalkablePathArray()
            );

        // Permit the janitor's current cell as the starting point.
        walkable.Add(start);

        if (!walkable.Contains(goal))
            return null;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> previous =
            new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(start);
        previous[start] = start;

        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == goal)
                break;

            foreach (Vector2Int direction in directions)
            {
                Vector2Int next = current + direction;

                if (!walkable.Contains(next) ||
                    previous.ContainsKey(next))
                    continue;

                previous[next] = current;
                queue.Enqueue(next);
            }
        }

        if (!previous.ContainsKey(goal))
            return null;

        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int step = goal;

        while (step != start)
        {
            path.Add(step);
            step = previous[step];
        }

        path.Reverse();
        return path;
    }
}
