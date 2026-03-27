using UnityEngine;
using UnityEngine.InputSystem;

public class RopeGrab : MonoBehaviour
{
    [Header("References")]
    public RopeLineRenderer rope;
    public Rigidbody2D rb;

    [Header("Settings")]
    public float grabRadius        = 0.8f;
    public float torqueStrength    = 2f;   // «подталкивание» маятника
    public float launchMultiplier  = 1.2f; // сколько скоростит сохранить при отпускании
    public float tensionStiffness  = 40f;  // жёсткость верёвки
    public float horizontalDamping = 0.995f; // очень лёгкое затухание

    private bool  isGrabbing     = false;
    private float grabLength;             // длина маятника
    private float originalDamping;
    private InputAction grabAction;
    private InputAction moveAction;

    void Awake()
    {
        grabAction = InputSystem.actions.FindAction("Player/Interact");
        moveAction = InputSystem.actions.FindAction("Player/Move");
    }

    void Update()
    {
        float dist = Vector2.Distance(transform.position, rope.GetTipPosition());

        if (!isGrabbing && dist <= grabRadius && grabAction.WasPressedThisFrame())
            StartGrab();
        else if (isGrabbing && grabAction.WasPressedThisFrame())
            StopGrab();
    }

    void FixedUpdate()
    {
        if (!isGrabbing) return;

        Vector2 anchorPos = rope.GetAnchorPosition();
        Vector2 pos       = rb.position;
        Vector2 toAnchor  = anchorPos - pos;
        float   currentL  = toAnchor.magnitude;

        // 1) Натяжение верёвки (ограничение длины)
        if (currentL > grabLength)
        {
            Vector2 dir     = toAnchor.normalized;
            float   stretch = currentL - grabLength;
            rb.AddForce(dir * stretch * tensionStiffness, ForceMode2D.Force);
        }

        // 2) «Подталкивание» по касательной, а не разгон по X
        // Касательная — вектор, перпендикулярный радиусу маятника
        Vector2 radiusDir     = (pos - anchorPos).normalized; // от якоря к игроку
        Vector2 tangentRight  = new Vector2(-radiusDir.y, radiusDir.x); // вправо относительно дуги
        float   horizontal    = moveAction.ReadValue<Vector2>().x;

        // Добавляем небольшое ускорение вдоль касательной
        rb.AddForce(tangentRight * (horizontal * torqueStrength), ForceMode2D.Force);

        // 3) Очень лёгкое затухание только по касательной
        Vector2 v = rb.linearVelocity;
        float tangentSpeed = Vector2.Dot(v, tangentRight);
        float radialSpeed  = Vector2.Dot(v, radiusDir);

        tangentSpeed *= horizontalDamping; // гасим лишь движение вдоль дуги

        rb.linearVelocity = tangentRight * tangentSpeed + radiusDir * radialSpeed;

        // 4) Конец верёвки визуально = позиция игрока
        rope.SetTipPosition(rb.position);
    }

    void StartGrab()
    {
        isGrabbing       = true;
        originalDamping  = rb.linearDamping;
        rb.gravityScale  = 1f;
        rb.linearDamping = 0f;
        rb.linearVelocity = Vector2.zero;

        // Длина маятника фиксируется по фактической дистанции
        grabLength = Vector2.Distance(rb.position, rope.GetAnchorPosition());
    }

    void StopGrab()
    {
        isGrabbing       = false;
        rb.linearDamping = originalDamping;

        // Сохраняем почти ту же скорость, что была на конце верёвки
        rb.linearVelocity *= launchMultiplier;
    }
}