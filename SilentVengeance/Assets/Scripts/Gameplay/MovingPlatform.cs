using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Точки движения")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Параметры")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTime = 1f;

    private Rigidbody2D _rb;
    private float _t = 0f;
    private bool _goingForward = true;
    private float _waitTimer = 0f;
    private bool _waiting = false;

    private PlayerController _playerController;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (_waiting)
        {
            _waitTimer -= Time.fixedDeltaTime;
            if (_waitTimer <= 0f)
                _waiting = false;

            // Обнуляем скорость платформы во время паузы
            UpdatePlayerVelocity(Vector2.zero);
            return;
        }

        _t += (_goingForward ? 1 : -1) * speed * Time.fixedDeltaTime;
        _t = Mathf.Clamp01(_t);

        float smoothT = Mathf.SmoothStep(0f, 1f, _t);
        Vector2 targetPos = Vector2.Lerp(pointA.position, pointB.position, smoothT);

        // Считаем скорость платформы и передаём игроку
        Vector2 platformVel = (targetPos - _rb.position) / Time.fixedDeltaTime;
        UpdatePlayerVelocity(platformVel);

        _rb.MovePosition(targetPos);

        if (_t >= 1f || _t <= 0f)
        {
            _goingForward = !_goingForward;
            _waitTimer = waitTime;
            _waiting = true;
        }
    }

    private void UpdatePlayerVelocity(Vector2 vel)
    {
        if (_playerController != null)
            _playerController.platformVelocity = vel;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
            _playerController = col.gameObject.GetComponent<PlayerController>();
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player") && _playerController == null)
            _playerController = col.gameObject.GetComponent<PlayerController>();
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            if (_playerController != null)
                _playerController.platformVelocity = Vector2.zero;
            _playerController = null;
        }
    }

    private void OnDrawGizmos()
    {
        if (pointA == null || pointB == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(pointA.position, 0.2f);
        Gizmos.DrawSphere(pointB.position, 0.2f);
        Gizmos.DrawLine(pointA.position, pointB.position);
    }
}