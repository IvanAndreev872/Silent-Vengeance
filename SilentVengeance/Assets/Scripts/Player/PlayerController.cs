using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Roll")]
    [SerializeField] private float rollSpeed = 12f;
    [SerializeField] private float rollDuration = 0.3f;
    [SerializeField] private float rollCooldown = 1f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D _rb;
    private Animator _animator;
    private PlayerInputActions _input;

    private Vector2 _moveInput;
    private bool _isGrounded;
    private bool _isCrouching;
    private bool _isRolling;
    private bool _canRoll = true;
    private float _facingDirection = 1f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _input = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _input.Player.Enable();
        _input.Player.Jump.performed   += OnJump;
        _input.Player.Crouch.performed += OnCrouchStart;
        _input.Player.Crouch.canceled  += OnCrouchEnd;
        _input.Player.Roll.performed   += OnRoll;
        _input.Player.Interact.performed += OnInteract;
    }

    private void OnDisable()
    {
        _input.Player.Jump.performed   -= OnJump;
        _input.Player.Crouch.performed -= OnCrouchStart;
        _input.Player.Crouch.canceled  -= OnCrouchEnd;
        _input.Player.Roll.performed   -= OnRoll;
        _input.Player.Interact.performed -= OnInteract;
        _input.Player.Disable();
    }

    private void Update()
    {
        _moveInput = _input.Player.Move.ReadValue<Vector2>();
        _isGrounded = Physics2D.OverlapCircle(
            groundCheck.position, groundCheckRadius, groundLayer);

        // Запоминаем направление взгляда
        if (_moveInput.x != 0)
            _facingDirection = Mathf.Sign(_moveInput.x);

        // Animator
        if (_animator != null)
        {
            _animator.SetFloat("Speed", Mathf.Abs(_moveInput.x));
            _animator.SetBool("IsGrounded", _isGrounded);
            _animator.SetBool("IsCrouching", _isCrouching);
            _animator.SetBool("IsRolling", _isRolling);
        }
    }

    private void FixedUpdate()
    {
        if (_isRolling) return; // во время переката не управляем движением

        float speed = _isCrouching ? crouchSpeed : moveSpeed;
        _rb.linearVelocity = new Vector2(_moveInput.x * speed, _rb.linearVelocity.y);

        if (_moveInput.x != 0)
            transform.localScale = new Vector3(_facingDirection, 1f, 1f);
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (_isGrounded && !_isRolling)
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void OnCrouchStart(InputAction.CallbackContext ctx) => _isCrouching = true;
    private void OnCrouchEnd(InputAction.CallbackContext ctx)   => _isCrouching = false;

    private void OnRoll(InputAction.CallbackContext ctx)
    {
        if (_isGrounded && _canRoll && !_isRolling)
            StartCoroutine(RollCoroutine());
    }

    private IEnumerator RollCoroutine()
    {
        _isRolling = true;
        _canRoll = false;

        // Рывок в направлении взгляда
        _rb.linearVelocity = new Vector2(_facingDirection * rollSpeed, _rb.linearVelocity.y);

        yield return new WaitForSeconds(rollDuration);

        _isRolling = false;

        yield return new WaitForSeconds(rollCooldown);

        _canRoll = true;
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        Debug.Log("Interact — реализуем позже");
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
