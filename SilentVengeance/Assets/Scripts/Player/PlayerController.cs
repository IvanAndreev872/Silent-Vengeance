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

    [Header("Ladder")]
    [SerializeField] private float climbSpeed = 5f;

    [Header("Stealth / Noise")]
    [SerializeField] private float landingNoiseDuration = 0.3f;
    [SerializeField] private float landingNoiseThreshold = -3f;

    private Rigidbody2D _rb;
    private Animator _animator;
    private PlayerInputActions _input;

    private Vector2 _moveInput;
    private bool _isGrounded;
    private bool _isCrouching;
    private bool _isRolling;
    private bool _canRoll = true;
    private float _facingDirection = 1f;

    private bool _isOnLadder = false;

    private bool _wasGroundedLastFrame = true;
    private float _landingTimer = 0f;
    private float _verticalVelocityLastFrame = 0f;

    [HideInInspector] public Vector2 platformVelocity = Vector2.zero;

    public bool IsRunning =>
        _isGrounded &&
        !_isCrouching &&
        !_isRolling &&
        Mathf.Abs(_moveInput.x) > 0.1f;

    public bool IsLanding => _landingTimer > 0f;
    public bool IsCrouching => _isCrouching;
    public bool IsRolling => _isRolling;
    public bool IsOnLadder => _isOnLadder;
    public bool IsGrounded => _isGrounded;

    public float NoiseLevel
    {
        get
        {
            if (_isRolling) return 0.1f;
            if (IsLanding) return 0.9f;
            if (_isCrouching)
            {
                return Mathf.Abs(_moveInput.x) > 0.1f ? 0.2f : 0f;
            }
            if (Mathf.Abs(_moveInput.x) > 0.1f)
            {
                return _isGrounded ? 0.6f : 0.1f;
            }
            return 0f;
        }
    }
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _input = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _input.Player.Enable();
        _input.Player.Jump.performed     += OnJump;
        _input.Player.Crouch.performed   += OnCrouchStart;
        _input.Player.Crouch.canceled    += OnCrouchEnd;
        _input.Player.Roll.performed     += OnRoll;
        _input.Player.Interact.performed += OnInteract;
    }

    private void OnDisable()
    {
        _input.Player.Jump.performed     -= OnJump;
        _input.Player.Crouch.performed   -= OnCrouchStart;
        _input.Player.Crouch.canceled    -= OnCrouchEnd;
        _input.Player.Roll.performed     -= OnRoll;
        _input.Player.Interact.performed -= OnInteract;
        _input.Player.Disable();
    }

    private void Update()
    {
        _moveInput  = _input.Player.Move.ReadValue<Vector2>();
        _isGrounded = Physics2D.OverlapCircle(
            groundCheck.position, groundCheckRadius, groundLayer);

        if (_moveInput.x != 0)
            _facingDirection = Mathf.Sign(_moveInput.x);

            DetectLanding();

        if (_isOnLadder)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _moveInput.y * climbSpeed);
            _rb.gravityScale = 0f;
        }
        else
        {
            _rb.gravityScale = 1f;
        }

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
        if (_isRolling) return;

        float speed = _isCrouching ? crouchSpeed : moveSpeed;
        _rb.linearVelocity = new Vector2(_moveInput.x * speed + platformVelocity.x, _rb.linearVelocity.y);

        if (_moveInput.x != 0)
            transform.localScale = new Vector3(_facingDirection, 1f, 1f);
    }

    private void DetectLanding()
    {
        if (_landingTimer > 0f)
        {
            _landingTimer -= Time.deltaTime;
        }

        if (_isGrounded && !_wasGroundedLastFrame)
        {
            if (_verticalVelocityLastFrame < landingNoiseThreshold)
            {
                _landingTimer = landingNoiseDuration;

                float fallIntensity = Mathf.Abs(_verticalVelocityLastFrame) /
                                      Mathf.Abs(landingNoiseThreshold);
                _landingTimer *= Mathf.Clamp(fallIntensity, 1f, 3f);
            }
        }

        _wasGroundedLastFrame = _isGrounded;
        _verticalVelocityLastFrame = _rb.linearVelocity.y;
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
        _canRoll   = false;

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

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Ladder"))
            _isOnLadder = true;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Ladder"))
            _isOnLadder = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}