using UnityEngine;

public class Node : MonoBehaviour
{
    [HideInInspector] public float gScore;
    [HideInInspector] public float hScore;
    [HideInInspector] public Node camefrom;

    public Vector2Int GridPosition =>
        new Vector2Int(Mathf.RoundToInt(transform.position.x * 2f),
                       Mathf.RoundToInt(transform.position.y * 2f));

    public float fScore() => gScore + hScore;

    public void ResetPathData()
    {
        gScore = float.MaxValue;
        hScore = 0f;
        camefrom = null;
    }
}