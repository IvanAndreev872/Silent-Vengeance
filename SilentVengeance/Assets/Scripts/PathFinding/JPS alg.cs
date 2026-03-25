using System.Collections.Generic;
using UnityEngine;

public class JumpPointSearch : MonoBehaviour
{
    public static JumpPointSearch Instance { get; private set; }

    private List<Node> openSet;
    private HashSet<Node> closedSet;
    private NodeGrid grid;
    private const int MAX_JUMP_DEPTH = 200;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        grid = NodeGrid.Instance;
    }

    public List<Node> GeneratePath(Node start, Node end)
    {
        if (start == null || end == null) return new List<Node>();
        if (start == end) return new List<Node> { start };

        grid.ResetDirtyNodes();

        openSet = new List<Node>();
        closedSet = new HashSet<Node>();

        start.gScore = 0;
        start.hScore = GetHeuristic(start.transform.position,
                                     end.transform.position);
        start.camefrom = null;
        grid.MarkDirty(start);

        openSet.Add(start);

        int safetyCounter = 0;
        int maxIterations = 5000;

        while (openSet.Count > 0)
        {
            safetyCounter++;
            if (safetyCounter > maxIterations)
            {
                Debug.LogWarning("JPS: превышен лимит итераций");
                break;
            }
            Node currentNode = GetLowestFScore();
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == end)
            {
                return ReconstructPath(start, end);
            }
            List<Node> successors = IdentifySuccessors(
                currentNode, start, end
            );

            foreach (Node successor in successors)
            {
                if (closedSet.Contains(successor))
                    continue;

                float tentativeG = currentNode.gScore +
                    Vector2.Distance(currentNode.transform.position,
                                     successor.transform.position);

                if (!openSet.Contains(successor))
                {
                    successor.gScore = tentativeG;
                    successor.hScore = GetHeuristic(
                        successor.transform.position,
                        end.transform.position
                    );
                    successor.camefrom = currentNode;
                    grid.MarkDirty(successor);
                    openSet.Add(successor);
                }
                else if (tentativeG < successor.gScore)
                {
                    successor.gScore = tentativeG;
                    successor.camefrom = currentNode;
                }
            }
        }

        return new List<Node>();
    }


    private List<Node> IdentifySuccessors(Node current, Node start, Node end)
    {
        List<Node> successors = new List<Node>();
        List<Vector2Int> directions = GetPrunedDirections(current);

        foreach (Vector2Int dir in directions)
        {
            Node jumpPoint = Jump(
                current.transform.position, dir,
                end.transform.position, 0
            );

            if (jumpPoint != null && !closedSet.Contains(jumpPoint))
            {
                successors.Add(jumpPoint);
            }
        }

        return successors;
    }


    private Node Jump(Vector2 current, Vector2Int dir, Vector2 end, int depth)
{
    if (depth > MAX_JUMP_DEPTH) return null;

    Vector2 next = current + (Vector2)dir;

    if (!grid.HasNode(next)) return null;

    Vector2Int nextGrid = new Vector2Int(
        Mathf.RoundToInt(next.x),
        Mathf.RoundToInt(next.y)
    );
    Vector2Int endGrid = new Vector2Int(
        Mathf.RoundToInt(end.x),
        Mathf.RoundToInt(end.y)
    );

    if (nextGrid == endGrid)
        return grid.GetNode(nextGrid);

    if (dir.x != 0 && dir.y == 0)
    {
        bool wallAbove = !grid.HasNode(new Vector2(next.x, next.y + 1));
        bool diagAbove =  grid.HasNode(new Vector2(next.x + dir.x, next.y + 1));
        bool wallBelow = !grid.HasNode(new Vector2(next.x, next.y - 1));
        bool diagBelow =  grid.HasNode(new Vector2(next.x + dir.x, next.y - 1));

        if ((wallAbove && diagAbove) || (wallBelow && diagBelow))
        {
            return grid.GetNode(next);
        }
    }
    else if (dir.x == 0 && dir.y != 0)
    {
        bool wallRight = !grid.HasNode(new Vector2(next.x + 1, next.y));
        bool diagRight =  grid.HasNode(new Vector2(next.x + 1, next.y + dir.y));
        bool wallLeft  = !grid.HasNode(new Vector2(next.x - 1, next.y));
        bool diagLeft  =  grid.HasNode(new Vector2(next.x - 1, next.y + dir.y));

        if ((wallRight && diagRight) || (wallLeft && diagLeft))
        {
            return grid.GetNode(next);
        }
    }
    else if (dir.x != 0 && dir.y != 0)
    {
        bool canPassHoriz = grid.HasNode(new Vector2(current.x + dir.x, current.y));
        bool canPassVert  = grid.HasNode(new Vector2(current.x, current.y + dir.y));

        if (!canPassHoriz && !canPassVert)
            return null;

        bool wallBehindH = !grid.HasNode(new Vector2(next.x - dir.x, next.y));
        bool diagBehindH =  grid.HasNode(new Vector2(next.x - dir.x, next.y + dir.y));

        if (wallBehindH && diagBehindH)
            return grid.GetNode(next);

        bool wallBehindV = !grid.HasNode(new Vector2(next.x, next.y - dir.y));
        bool diagBehindV =  grid.HasNode(new Vector2(next.x + dir.x, next.y - dir.y));

        if (wallBehindV && diagBehindV)
            return grid.GetNode(next);

        if (Jump(next, new Vector2Int(dir.x, 0), end, depth + 1) != null)
            return grid.GetNode(next);

        if (Jump(next, new Vector2Int(0, dir.y), end, depth + 1) != null)
            return grid.GetNode(next);
    }

    return Jump(next, dir, end, depth + 1);
}


    private List<Vector2Int> GetPrunedDirections(Node node)
    {
        List<Vector2Int> directions = new List<Vector2Int>(8);
        Vector3 pos = node.transform.position;

        if (node.camefrom == null)
        {
            directions.Add(new Vector2Int(1, 0));
            directions.Add(new Vector2Int(-1, 0));
            directions.Add(new Vector2Int(0, 1));
            directions.Add(new Vector2Int(0, -1));
            directions.Add(new Vector2Int(1, 1));
            directions.Add(new Vector2Int(-1, 1));
            directions.Add(new Vector2Int(1, -1));
            directions.Add(new Vector2Int(-1, -1));
            return directions;
        }

        Vector3 parentPos = node.camefrom.transform.position;
        int dx = (int)Mathf.Sign(pos.x - parentPos.x);
        int dy = (int)Mathf.Sign(pos.y - parentPos.y);

        if (dx != 0 && dy != 0)
        {
            directions.Add(new Vector2Int(dx, dy));
            directions.Add(new Vector2Int(dx, 0));
            directions.Add(new Vector2Int(0, dy));
            if (!grid.HasNode(new Vector2(pos.x - dx, pos.y)))
            {
                directions.Add(new Vector2Int(-dx, dy));
            }

            if (!grid.HasNode(new Vector2(pos.x, pos.y - dy)))
            {
                directions.Add(new Vector2Int(dx, -dy));
            }
        }
        else if (dx != 0 && dy == 0)
        {
            directions.Add(new Vector2Int(dx, 0));

            if (!grid.HasNode(new Vector2(pos.x, pos.y + 1)))
            {
                directions.Add(new Vector2Int(dx, 1));
            }

            if (!grid.HasNode(new Vector2(pos.x, pos.y - 1)))
            {
                directions.Add(new Vector2Int(dx, -1));
            }
        }
        else if (dx == 0 && dy != 0)
        {
            directions.Add(new Vector2Int(0, dy));

            if (!grid.HasNode(new Vector2(pos.x + 1, pos.y)))
            {
                directions.Add(new Vector2Int(1, dy));
            }

            if (!grid.HasNode(new Vector2(pos.x - 1, pos.y)))
            {
                directions.Add(new Vector2Int(-1, dy));
            }
        }

        return directions;
    }

    private Node GetLowestFScore()
    {
        Node best = openSet[0];
        for (int i = 1; i < openSet.Count; i++)
        {
            float f = openSet[i].fScore();
            float bestF = best.fScore();

            if (f < bestF || (f == bestF && openSet[i].hScore < best.hScore))
            {
                best = openSet[i];
            }
        }
        return best;
    }

    private List<Node> ReconstructPath(Node start, Node end)
    {
        List<Node> path = new List<Node>();
        Node current = end;

        int safety = 0;
        while (current != null && current != start && safety < 10000)
        {
            path.Add(current);
            current = current.camefrom;
            safety++;
        }

        if (current == start)
            path.Add(start);

        path.Reverse();
        return path;
    }

    private float GetHeuristic(Vector2 a, Vector2 b)
    {
        float dx = Mathf.Abs(a.x - b.x);
        float dy = Mathf.Abs(a.y - b.y);
        return 1.414f * Mathf.Min(dx, dy) + Mathf.Abs(dx - dy);
    }
}