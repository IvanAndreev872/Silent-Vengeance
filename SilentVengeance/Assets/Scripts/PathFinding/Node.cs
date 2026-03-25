using UnityEngine;

public class Node : MonoBehaviour
{
    [HideInInspector] public float gScore;
    [HideInInspector] public float hScore;
    [HideInInspector] public Node camefrom;

    public Vector2Int GridPosition =>
        new Vector2Int(Mathf.RoundToInt(transform.position.x),
                       Mathf.RoundToInt(transform.position.y));

    public float fScore() => gScore + hScore;

    public void ResetPathData()
    {
        gScore = float.MaxValue;
        hScore = 0f;
        camefrom = null;
    }
}