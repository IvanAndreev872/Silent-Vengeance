using System.Collections.Generic;
using UnityEngine;

public class NodeGrid : MonoBehaviour
{
    public static NodeGrid Instance { get; private set; }

    private Dictionary<Vector2Int, Node> nodeMap = new Dictionary<Vector2Int, Node>();

    private List<Node> dirtyNodes = new List<Node>();

    private void Awake()
    {
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

    public bool HasNode(Vector2Int pos)
    {
        return nodeMap.ContainsKey(pos);
    }

    public bool HasNode(Vector2 pos)
    {
        return HasNode(ToGrid(pos));
    }

    public Node GetNode(Vector2Int pos)
    {
        nodeMap.TryGetValue(pos, out Node node);
        return node;
    }

    public Node GetNode(Vector2 pos)
    {
        return GetNode(ToGrid(pos));
    }

    public Node GetNearestNode(Vector2 worldPos)
    {
        Vector2Int rounded = ToGrid(worldPos);

        if (nodeMap.TryGetValue(rounded, out Node exact))
            return exact;

        Node nearest = null;
        float minDist = float.MaxValue;

        for (int radius = 1; radius <= 5; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius)
                        continue;

                    Vector2Int check = rounded + new Vector2Int(x, y);
                    if (nodeMap.TryGetValue(check, out Node candidate))
                    {
                        float dist = Vector2.Distance(worldPos,
                                     candidate.transform.position);
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

    private Vector2Int ToGrid(Vector2 pos)
    {
        return new Vector2Int(Mathf.RoundToInt(pos.x),
                              Mathf.RoundToInt(pos.y));
    }
}