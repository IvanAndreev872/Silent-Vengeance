using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class RopeGrab : MonoBehaviour
{
    [Header("References")]
    public RopeLineRenderer rope;

    [Header("Grab")]
    public float grabRadius = 0.35f;

    [Header("Pendulum")]
    public float inputTorque = 12f;          // насколько игрок может раскачивать маятник
    public float angularDamping = 0.995f;    // затухание, близко к 1 = плавнее
    public float gravityScale = 1.0f;        // сила тяжести маятника
    public float maxAngularSpeed = 4.5f;     // ограничение скорости раскачки

    [Header("Release")]
    public float launchMultiplier = 1.0f;

    private Rigidbody2D _rb;
    private InputAction _grabAction;
    private InputAction _moveAction;

    private bool _isGrabbing;

    private float _ropeLength;
    private float _angle;            // угол относительно вертикали вниз
    private float _angularVelocity;  // угловая скорость

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _grabAction = InputSystem.actions.FindAction("Player/Interact");
        _moveAction = InputSystem.actions.FindAction("Player/Move");
    }

    void Update()
    {
        if (!_grabAction.WasPressedThisFrame())
            return;

        if (!_isGrabbing)
        {
            float distToTip = Vector2.Distance(_rb.position, rope.TipPosition);
            if (distToTip <= grabRadius)
                StartGrab();
        }
        else
        {
            StopGrab();
        }
    }

    void StartGrab()
    {
        _isGrabbing = true;

        Vector2 anchor = rope.AnchorPosition;
        Vector2 player = _rb.position;
        Vector2 dir = player - anchor;

        _ropeLength = dir.magnitude;
        if (_ropeLength < 0.05f)
            _ropeLength = 0.05f;

        // Угол относительно вертикали вниз:
        // 0 = строго под якорем
        _angle = Mathf.Atan2(dir.x, -dir.y);

        // Перевод текущей линейной скорости в угловую
        Vector2 tangent = new Vector2(Mathf.Cos(_angle), Mathf.Sin(_angle));
        float tangentialSpeed = Vector2.Dot(_rb.linearVelocity, tangent);
        _angularVelocity = tangentialSpeed / _ropeLength;

        // На верёвке Rigidbody больше не управляет положением
        _rb.linearVelocity = Vector2.zero;
        _rb.gravityScale = 0f;
        _rb.bodyType = RigidbodyType2D.Kinematic;

        // Кончик верёвки сразу в игрока
        rope.SnapTip(player);
    }

    void StopGrab()
    {
        _isGrabbing = false;

        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.gravityScale = 1f;

        float dir = Mathf.Sign(_angularVelocity);
        if (Mathf.Abs(_angularVelocity) < 0.05f)
            dir = Mathf.Sign(_angle);

        float horizontalReleaseSpeed = 3.2f;
        float verticalReleaseSpeed = 0.5f; // очень маленький

        _rb.linearVelocity = new Vector2(dir * horizontalReleaseSpeed, verticalReleaseSpeed);
    }

    void FixedUpdate()
    {
        if (!_isGrabbing)
            return;

        float inputX = _moveAction.ReadValue<Vector2>().x;

        // Уравнение маятника:
        // theta'' = -(g / L) * sin(theta) + input
        float angularAcceleration =
            -(Physics2D.gravity.magnitude * gravityScale / _ropeLength) * Mathf.Sin(_angle);

        // Игрок помогает раскачке, но не взлетает мгновенно
        angularAcceleration += inputX * inputTorque / _ropeLength;

        _angularVelocity += angularAcceleration * Time.fixedDeltaTime;
        _angularVelocity *= angularDamping;
        _angularVelocity = Mathf.Clamp(_angularVelocity, -maxAngularSpeed, maxAngularSpeed);

        _angle += _angularVelocity * Time.fixedDeltaTime;

        Vector2 anchor = rope.AnchorPosition;

        Vector2 offset = new Vector2(
        Mathf.Sin(_angle) * _ropeLength,
        -Mathf.Cos(_angle) * _ropeLength
        );

        Vector2 endPoint = anchor + offset;

        // ЖЁСТКАЯ синхронизация
        _rb.position = endPoint;
        transform.position = endPoint;
        rope.SnapTip(endPoint);
        }
}