using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Node camefrom;
    public List<Node> connections;

    public float gScore;
    public float hScore;

    public float fScore()
    {
        return gScore + hScore;
    }
}