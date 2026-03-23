using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class JumpPointSearch : MonoBehaviour
{
    public static JumpPointSearch instance;

    List<Node> openSet;
    HashSet<Node> closedSet;
    int NodeLayerMask;

    private void Awake()
    {
        instance = this;
        NodeLayerMask = LayerMask.GetMask("Node Layer");
    }


    public List<Node> GeneratePath(Node start, Node end)
    {
        openSet = new List<Node>();
        closedSet = new HashSet<Node>();

        openSet.Add(start);

        while (openSet.Count > 0)
        {

            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fScore() < currentNode.fScore() ||
                    openSet[i].fScore() == currentNode.fScore() && openSet[i].hScore < currentNode.hScore)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode.transform.position == end.transform.position)
            {
                if (closedSet.Count > 10)
                {
                    // Debug.Log(closedSet.Count);
                }
                return ReconstructPath(start, end);
            }

            List<Node> neighbors = IdentifySuccessors(currentNode, start, end);

            foreach (Node neighbor in neighbors)
            {
                if (closedSet.Contains(neighbor))
                {
                    continue;
                }
                float futuregScore = currentNode.gScore + Vector2.Distance(currentNode.transform.position, neighbor.transform.position);
                if (!openSet.Contains(neighbor))
                {
                    neighbor.gScore = futuregScore;
                    neighbor.hScore = GetDistance(neighbor.transform.position, end.transform.position);
                    neighbor.camefrom = currentNode;
                    openSet.Add(neighbor);
                } else if (futuregScore < neighbor.gScore)
                {
                    neighbor.gScore = futuregScore;
                    neighbor.camefrom = currentNode;
                }
            }
        }
        return new List<Node>();
    }

    private List<Node> IdentifySuccessors(Node current, Node start, Node end)
    {
        List<Node> successors = new List<Node>();
        List<Vector2Int> directions = PruningDirections(current);
        foreach (Vector2Int dir in directions)
        {
            Node jumpPoint = Jump(current.transform.position, dir, end.transform.position);
            if (jumpPoint == end)
            {
                successors.Clear();
                successors.Add(jumpPoint);
                break;
            }
            if (jumpPoint != null && !closedSet.Contains(jumpPoint))
            {
                successors.Add(jumpPoint);
            }
        }
        return successors;
    }


    private Node Jump(Vector2 current, Vector2Int direction, Vector2 end)
    {
        Vector2 next = current + direction;
        if(next == end)
        {
            return FindNode(end);
        }
        if (!DoesNodeExists(next))
        {
            return null;
        }
        if (direction.x != 0 && direction.y == 0)
        {
            if (!DoesNodeExists(new Vector2(next.x, next.y + 1)) && DoesNodeExists(new Vector2(next.x + direction.x, next.y + 1)) ||
                !DoesNodeExists(new Vector2(next.x, next.y - 1)) && DoesNodeExists(new Vector2(next.x + direction.x, next.y - 1)))
            {
                return FindNode(next);
            }
        } else if (direction.x == 0 && direction.y != 0)
        {
            if (!DoesNodeExists(new Vector2(next.x + 1, next.y)) && DoesNodeExists(new Vector2(next.x + 1, next.y + direction.y)) ||
                !DoesNodeExists(new Vector2(next.x - 1, next.y)) && DoesNodeExists(new Vector2(next.x - 1, next.y + direction.y)))
            {
                return FindNode(next);
            }
        } else
        {
            if (Jump(next, new Vector2Int(direction.x, 0), end) != null ||
                Jump(next, new Vector2Int(0, direction.y), end) != null)
            {
                return FindNode(next);
            }
        }

        return Jump(next, direction, end);
    }

    private List<Vector2Int> PruningDirections(Node node)
    {
        List<Vector2Int> directions = new List<Vector2Int>();
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

        Vector2Int FromParentToNode = new Vector2Int((int)Mathf.Sign((node.transform.position.x - node.camefrom.transform.position.x)),
                                               (int)Mathf.Sign(node.transform.position.y - node.camefrom.transform.position.y));

        if (FromParentToNode.x != 0 && FromParentToNode.y != 0)
        {
            directions.Add(FromParentToNode);
            directions.Add(new Vector2Int(FromParentToNode.x, 0));
            directions.Add(new Vector2Int(0, FromParentToNode.y));

            if (DoesNodeExists(new Vector2(node.transform.position.x - FromParentToNode.x, node.transform.position.y)))
            {
                directions.Add(new Vector2Int(FromParentToNode.x, -FromParentToNode.y));
            }
            if (DoesNodeExists(new Vector2(node.transform.position.x, node.transform.position.y - FromParentToNode.y)))
            {
                directions.Add(new Vector2Int(-FromParentToNode.x, FromParentToNode.y));
            }
        }
        else
        {
            directions.Add(FromParentToNode);
            bool IsNeighborUpForced = false;
            bool IsNeighborDownForced = false;


            if (FromParentToNode.x != 0)
            {
                if (!DoesNodeExists(new Vector2(node.transform.position.x, node.transform.position.y + 1)))
                {
                    directions.Add(new Vector2Int(FromParentToNode.x, 1));
                    IsNeighborUpForced = true;
                }
                if (!DoesNodeExists(new Vector2(node.transform.position.x, node.transform.position.y - 1)))
                {
                    directions.Add(new Vector2Int(FromParentToNode.x, -1));
                    IsNeighborDownForced = true;
                }
                if (IsNeighborUpForced && IsNeighborDownForced)
                {
                    directions.Add(new Vector2Int(0, 1));
                    directions.Add(new Vector2Int(0, -1));
                }
            }
            else
            {
                if (!DoesNodeExists(new Vector2(node.transform.position.x + 1, node.transform.position.y)))
                {
                    directions.Add(new Vector2Int(1, FromParentToNode.y));
                    IsNeighborUpForced = true;
                }
                if (!DoesNodeExists(new Vector2(node.transform.position.x - 1, node.transform.position.y)))
                {
                    directions.Add(new Vector2Int(-1, FromParentToNode.y));
                    IsNeighborDownForced = true;
                }
                if (IsNeighborUpForced && IsNeighborDownForced)
                {
                    directions.Add(new Vector2Int(1, 0));
                    directions.Add(new Vector2Int(-1, 0));
                }
            }
        }
        return directions;
    }

    private List<Node> ReconstructPath(Node start, Node end)
    {
        List<Node> path = new List<Node>();
        Node curNode = end;

        while (curNode != start)
        {
            path.Add(curNode);
            curNode = curNode.camefrom;
        }

        path.Add(start);
        path.Reverse();
        return path;
    }

    private bool DoesNodeExists(Vector2 position)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.1f);
        foreach (Collider2D col in colliders)
        {
            if (col.gameObject.GetComponent<Node>() != null)
            {
                return true;
            }
        }
        return false;
    }

    private Node FindNode(Vector2 position)
    {
        Node node = null;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.1f);
        foreach (Collider2D col in colliders)
        {
            if (col.gameObject.GetComponent<Node>() != null)
            {
                node = col.GetComponent<Node>();
            }
        }
        return node;
    }

    private float GetDistance(Vector2 position, Vector2 Destination)
    {
        float dx = Mathf.Abs(position.x - Destination.x);
        float dy = Mathf.Abs(position.y - Destination.y);

        return 1.414f * Mathf.Min(dx, dy) + Mathf.Abs(dx - dy);
    }

}
