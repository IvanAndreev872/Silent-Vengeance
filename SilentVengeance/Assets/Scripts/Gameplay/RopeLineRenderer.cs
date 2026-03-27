using UnityEngine;

public class RopeLineRenderer : MonoBehaviour
{
    [Header("Rope Settings")]
    public int segmentCount = 20;
    public float ropeLength = 5f;
    public int solverIterations = 40;

    [Header("Collision")]
    public LayerMask groundLayer;
    public float collisionRadius = 0.1f;

    private Vector2[] points;
    private Vector2[] prevPoints;
    private LineRenderer lr;

    private float SegmentLength => ropeLength / segmentCount;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = segmentCount + 1;

        points     = new Vector2[segmentCount + 1];
        prevPoints = new Vector2[segmentCount + 1];

        // Инициализация: верёвка висит вертикально вниз
        for (int i = 0; i <= segmentCount; i++)
        {
            Vector2 p = (Vector2)transform.position + Vector2.down * (SegmentLength * i);
            points[i]     = p;
            prevPoints[i] = p;
        }
    }

    void FixedUpdate()
    {
        Simulate();
        for (int i = 0; i < solverIterations; i++)
        {
            ApplyConstraints();
            SolveCollisions();
        }
        UpdateLineRenderer();
    }

    void Simulate()
    {
        Vector2 gravity = Physics2D.gravity * Time.fixedDeltaTime * Time.fixedDeltaTime;

        for (int i = 1; i <= segmentCount; i++) // i=0 закреплён (якорь)
        {
            Vector2 velocity = points[i] - prevPoints[i];
            prevPoints[i] = points[i];
            points[i] += velocity + gravity;
        }
    }

    void ApplyConstraints()
    {
        // Якорь неподвижен
        points[0] = transform.position;

        float segLen = SegmentLength;

        for (int i = 0; i < segmentCount; i++)
        {
            Vector2 a = points[i];
            Vector2 b = points[i + 1];

            float dist  = Vector2.Distance(a, b);
            if (dist < 0.0001f) continue;

            float diff  = (dist - segLen) / dist;
            Vector2 correction = (b - a) * 0.5f * diff;

            if (i == 0)
                points[i + 1] -= correction * 2f;
            else
            {
                points[i]     += correction;
                points[i + 1] -= correction;
            }
        }
    }

    void SolveCollisions()
    {
        for (int i = 1; i <= segmentCount; i++)
        {
            Collider2D hit = Physics2D.OverlapCircle(points[i], collisionRadius, groundLayer);
            if (hit != null)
            {
                Vector2 closest = hit.ClosestPoint(points[i]);
                Vector2 normal  = (points[i] - closest).normalized;
                if (normal == Vector2.zero) normal = Vector2.up;
                points[i] = closest + normal * collisionRadius;
            }
        }
    }

    void UpdateLineRenderer()
    {
        for (int i = 0; i <= segmentCount; i++)
            lr.SetPosition(i, points[i]);
    }

    public Vector2 GetTipPosition()    => points[segmentCount];
    public void   SetTipPosition(Vector2 pos) { points[segmentCount] = pos; prevPoints[segmentCount] = pos; }
    public Vector2 GetTipVelocity()    => points[segmentCount] - prevPoints[segmentCount];
    public Vector2 GetAnchorPosition() => points[0];
}