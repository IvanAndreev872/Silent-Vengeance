using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RopeLineRenderer : MonoBehaviour
{
    [Header("Верёвка")]
    public int   segments       = 20;
    public float ropeLength     = 5f;
    public int   solverIterations = 30;

    [Header("Коллизии")]
    public LayerMask groundLayer;
    public float     collisionRadius = 0.08f;

    // Якорь — точка крепления (transform этого объекта)
    public Vector2 AnchorPosition => (Vector2)transform.position;

    // Кончик верёвки (последняя точка)
    public Vector2 TipPosition    => _pts[segments];

    private Vector2[]    _pts;
    private Vector2[]    _prev;
    private LineRenderer _lr;
    private float        SegLen => ropeLength / segments;

    void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _lr.positionCount = segments + 1;

        _pts  = new Vector2[segments + 1];
        _prev = new Vector2[segments + 1];

        for (int i = 0; i <= segments; i++)
        {
            var p = AnchorPosition + Vector2.down * (SegLen * i);
            _pts[i] = _prev[i] = p;
        }
    }

    void FixedUpdate()
    {
        Integrate();
        for (int i = 0; i < solverIterations; i++)
        {
            Constrain();
            Collide();
        }
        Draw();
    }

    void Integrate()
    {
        var g = Physics2D.gravity * (Time.fixedDeltaTime * Time.fixedDeltaTime);
        for (int i = 1; i <= segments; i++)
        {
            var vel   = _pts[i] - _prev[i];
            _prev[i]  = _pts[i];
            _pts[i]  += vel + g;
        }
    }

    void Constrain()
    {
        // Якорь всегда неподвижен
        _pts[0] = AnchorPosition;

        float len = SegLen;
        for (int i = 0; i < segments; i++)
        {
            var  a    = _pts[i];
            var  b    = _pts[i + 1];
            float d   = Vector2.Distance(a, b);
            if (d < 0.0001f) continue;

            float diff       = (d - len) / d;
            var   correction = (b - a) * 0.5f * diff;

            if (i == 0)
                _pts[i + 1] -= correction * 2f;
            else
            {
                _pts[i]     += correction;
                _pts[i + 1] -= correction;
            }
        }
    }

    void Collide()
    {
        for (int i = 1; i <= segments; i++)
        {
            var hit = Physics2D.OverlapCircle(_pts[i], collisionRadius, groundLayer);
            if (hit == null) continue;

            var closest = hit.ClosestPoint(_pts[i]);
            var normal  = (_pts[i] - closest).normalized;
            if (normal == Vector2.zero) normal = Vector2.up;
            _pts[i] = closest + normal * collisionRadius;
        }
    }

    void Draw()
    {
        for (int i = 0; i <= segments; i++)
            _lr.SetPosition(i, _pts[i]);
    }

    // Позволяет внешнему скрипту двигать кончик (без сброса скорости Verlet)
    public void MoveTip(Vector2 newPos)
    {
        var velocity = _pts[segments] - _prev[segments]; // сохраняем скорость
        _prev[segments] = newPos - velocity;             // чтобы Verlet не менял её
        _pts[segments]  = newPos;
    }

    // Полный сброс кончика (при захвате)
    public void SnapTip(Vector2 pos)
    {
        _pts[segments]  = pos;
        _prev[segments] = pos;
    }
}