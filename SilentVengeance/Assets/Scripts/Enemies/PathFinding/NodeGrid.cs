using System.Collections.Generic;
using UnityEngine;

public class NodeGrid : MonoBehaviour
{
    public static NodeGrid Instance { get; private set; }

    private Dictionary<Vector2Int, Node> nodeMap = new Dictionary<Vector2Int, Node>();
    private List<Node> dirtyNodes = new List<Node>();

    private void Awake()
    {
        Debug.Log("NodeGrid Awake() вызван");
        Instance = this;
        BuildGrid();
    }

    public void BuildGrid()
    {
        nodeMap.Clear();
        Node[] allNodes = FindObjectsByType<Node>(FindObjectsSortMode.None);
        foreach (Node node in allNodes)
        {
            Vector2Int key = node.GridPosition;
            if (!nodeMap.ContainsKey(key))
            {
                nodeMap[key] = node;
            }
            else
            {
                Debug.LogWarning(
                    $"Дублирующийся нод на позиции {key}. " +
                    $"Объект '{node.gameObject.name}' проигнорирован."
                );
            }
        }
        Debug.Log($"NodeGrid построена: {nodeMap.Count} нодов");
    }

    public Vector2Int ToGrid(Vector2 pos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(pos.x * 2f),
            Mathf.RoundToInt(pos.y * 2f)
        );
    }

    public bool HasNode(Vector2Int gridKey)
    {
        return nodeMap.ContainsKey(gridKey);
    }

    public bool HasNode(Vector2 worldPos)
    {
        return nodeMap.ContainsKey(ToGrid(worldPos));
    }

    public Node GetNode(Vector2Int gridKey)
    {
        nodeMap.TryGetValue(gridKey, out Node node);
        return node;
    }

    public Node GetNode(Vector2 worldPos)
    {
        return GetNode(ToGrid(worldPos));
    }

    public Node GetNearestNode(Vector2 worldPos)
    {
        Vector2Int rounded = ToGrid(worldPos);

        if (nodeMap.TryGetValue(rounded, out Node exact))
            return exact;

        Node nearest = null;
        float minDist = float.MaxValue;

        for (int gridRadius = 1; gridRadius <= 10; gridRadius++)
        {
            for (int x = -gridRadius; x <= gridRadius; x++)
            {
                for (int y = -gridRadius; y <= gridRadius; y++)
                {
                    if (Mathf.Abs(x) != gridRadius && Mathf.Abs(y) != gridRadius)
                        continue;

                    Vector2Int check = rounded + new Vector2Int(x, y);
                    if (nodeMap.TryGetValue(check, out Node candidate))
                    {
                        float dist = Vector2.Distance(
                            worldPos, candidate.transform.position
                        );
                        if (dist < minDist)
                        {
                            minDist = dist;
                            nearest = candidate;
                        }
                    }
                }
            }
            if (nearest != null) return nearest;
        }

        return null;
    }

    public void MarkDirty(Node node)
    {
        dirtyNodes.Add(node);
    }

    public void ResetDirtyNodes()
    {
        foreach (Node node in dirtyNodes)
        {
            if (node != null)
                node.ResetPathData();
        }
        dirtyNodes.Clear();
    }

    public void RegisterNode(Node node)
    {
        Vector2Int key = node.GridPosition;
        nodeMap[key] = node;
    }

    public void UnregisterNode(Node node)
    {
        Vector2Int key = node.GridPosition;
        nodeMap.Remove(key);
    }
}
