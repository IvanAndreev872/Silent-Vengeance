using System.Collections.Generic;
using UnityEngine;

public class EnemyPathfinding : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private float pathRecalcInterval = 0.5f;
    [SerializeField] private float nodeReachDistance = 0.3f;
    [SerializeField] private bool drawDebugPath = true;

    private List<Node> currentPath = new List<Node>();
    private int currentPathIndex = 0;
    private float recalcTimer = 0f;
    private bool hasPath = false;

    private NodeGrid grid;
    private JumpPointSearch jps;

    public bool HasPath => hasPath && currentPath.Count > 0;
    public bool ReachedEnd => hasPath && currentPathIndex >= currentPath.Count;
    public List<Node> CurrentPath => currentPath;

    private void Start()
    {
        grid = NodeGrid.Instance;
        jps = JumpPointSearch.Instance;
    }

    public bool RequestPath(Vector2 targetPosition)
    {
        if (grid == null || jps == null) return false;
        Node startNode = grid.GetNearestNode(transform.position);
        Node endNode = grid.GetNearestNode(targetPosition);

        if (startNode == null || endNode == null)
        {
            hasPath = false;
            return false;
        }

        if (startNode == endNode)
        {
            currentPath.Clear();
            currentPath.Add(startNode);
            currentPathIndex = 0;
            hasPath = true;
            return true;
        }
        currentPath = jps.GeneratePath(startNode, endNode);

        if (currentPath.Count > 0)
        {
            currentPathIndex = 0;
            hasPath = true;
            if (currentPath.Count > 1 &&
                Vector2.Distance(transform.position,
                currentPath[0].transform.position) < nodeReachDistance)
            {
                currentPathIndex = 1;
            }

            return true;
        }

        hasPath = false;
        return false;
    }

    public bool UpdatePathToTarget(Vector2 targetPosition)
    {
        recalcTimer -= Time.deltaTime;

        if (recalcTimer <= 0f)
        {
            recalcTimer = pathRecalcInterval;
            return RequestPath(targetPosition);
        }

        return hasPath;
    }

    public Vector2 GetMoveDirection()
    {
        if (!HasPath || currentPathIndex >= currentPath.Count)
            return Vector2.zero;

        Node targetNode = currentPath[currentPathIndex];

        if (targetNode == null)
        {
            hasPath = false;
            return Vector2.zero;
        }

        Vector2 direction = (
            (Vector2)targetNode.transform.position -
            (Vector2)transform.position
        );

        float distance = direction.magnitude;

        if (distance < nodeReachDistance)
        {
            currentPathIndex++;

            if (currentPathIndex >= currentPath.Count)
                return Vector2.zero;

            targetNode = currentPath[currentPathIndex];
            direction = (
                (Vector2)targetNode.transform.position -
                (Vector2)transform.position
            );
        }

        return direction.normalized;
    }

    public Vector2 GetCurrentTargetPosition()
    {
        if (!HasPath || currentPathIndex >= currentPath.Count)
            return transform.position;

        return currentPath[currentPathIndex].transform.position;
    }

    public float GetRemainingDistance()
    {
        if (!HasPath || currentPath.Count == 0) return float.MaxValue;

        float total = Vector2.Distance(
            transform.position,
            currentPath[currentPathIndex].transform.position
        );

        for (int i = currentPathIndex; i < currentPath.Count - 1; i++)
        {
            total += Vector2.Distance(
                currentPath[i].transform.position,
                currentPath[i + 1].transform.position
            );
        }

        return total;
    }

    public void ClearPath()
    {
        currentPath.Clear();
        currentPathIndex = 0;
        hasPath = false;
    }

    public void ForceRecalculate()
    {
        recalcTimer = 0f;
    }

    private void OnDrawGizmos()
    {
        if (!drawDebugPath || !hasPath || currentPath.Count == 0)
            return;

        if (currentPathIndex < currentPath.Count &&
            currentPath[currentPathIndex] != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(
                transform.position,
                currentPath[currentPathIndex].transform.position
            );
        }

        Gizmos.color = Color.green;
        for (int i = currentPathIndex; i < currentPath.Count - 1; i++)
        {
            if (currentPath[i] == null || currentPath[i + 1] == null)
                continue;

            Gizmos.DrawLine(
                currentPath[i].transform.position,
                currentPath[i + 1].transform.position
            );

            Gizmos.DrawSphere(
                currentPath[i].transform.position, 0.15f
            );
        }

        if (currentPath.Count > 0)
        {
            Node last = currentPath[currentPath.Count - 1];
            if (last != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(last.transform.position, 0.2f);
            }
        }
    }
}