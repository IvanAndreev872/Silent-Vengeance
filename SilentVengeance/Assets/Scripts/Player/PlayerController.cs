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
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Ladder")]
    [SerializeField] private float climbSpeed = 5f;

    [Header("Slope")]
    [SerializeField] private float maxSlopeAngle = 45f;

    [Header("Step Up")]
    [SerializeField] private float stepHeight = 0.4f;

    [Header("Jump Buffer")]
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float jumpCooldownTime = 0.25f;

    [Header("Coyote Time")]
    [SerializeField] private float coyoteTime = 0.12f;

    [Header("Stealth / Noise")]
    [SerializeField] private float landingNoiseDuration = 0.3f;
    [SerializeField] private float landingNoiseThreshold = -3f;

    private Rigidbody2D _rb;
    private Animator _animator;
    private PlayerInputActions _input;
    private StealthSystem _stealth;

    private Vector2 _moveInput;
    private bool _isGrounded;
    private bool _isCrouching;
    private bool _isRolling;
    private bool _canRoll = true;
    private float _facingDirection = 1f;

    private bool _isOnLadder = false;
    private Vector2 _groundNormal = Vector2.up;

    private bool _jumpRequested = false;
    private float _jumpBufferTimer = 0f;
    private float _coyoteTimer = 0f;
    private float _jumpCooldown = 0f;
    private bool _justJumped = false;

    private bool _wasGroundedLastFrame = true;
    private float _landingTimer = 0f;
    private float _verticalVelocityLastFrame = 0f;

    [HideInInspector] public Vector2 platformVelocity = Vector2.zero;

    public bool IsRunning =>
        _isGrounded && !_isCrouching && !_isRolling && Mathf.Abs(_moveInput.x) > 0.1f;
    public bool IsLanding  => _landingTimer > 0f;
    public bool IsCrouching => _isCrouching;
    public bool IsRolling  => _isRolling;
    public bool IsOnLadder => _isOnLadder;
    public bool IsGrounded => _isGrounded;

    public float NoiseLevel
    {
        get
        {
            if (_stealth != null && _stealth.IsHidden)
                return 0f;
            
            if (_isRolling) return 0.1f;
            if (IsLanding)  return 0.9f;
            if (_isCrouching)
                return Mathf.Abs(_moveInput.x) > 0.1f ? 0.2f : 0f;
            if (Mathf.Abs(_moveInput.x) > 0.1f)
                return _isGrounded ? 0.6f : 0.1f;
            return 0f;
        }
    }

    public bool IsHiddenInBush
    {
        get
        {
            return _stealth != null && _stealth.IsHidden;
        }
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _input = new PlayerInputActions();
        _stealth = GetComponent<StealthSystem>();
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

        if (_jumpCooldown > 0f)
            _jumpCooldown -= Time.deltaTime;

        if (_isGrounded && _jumpCooldown <= 0f)
            _coyoteTimer = coyoteTime;
        else if (!_isGrounded)
            _coyoteTimer -= Time.deltaTime;

        if (_jumpBufferTimer > 0f)
            _jumpBufferTimer -= Time.deltaTime;
        else
            _jumpRequested = false;

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
            _animator.SetFloat("VerticalVelocity", _rb.linearVelocity.y);
        }
    }

    private void FixedUpdate()
    {
        _justJumped = false;

        if (_stealth != null && _stealth.IsHidden)
        {
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
            return;
        }

        if (_jumpRequested && _coyoteTimer > 0f && !_isRolling)
        {
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            _jumpRequested = false;
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
            _justJumped = true;
            _jumpCooldown = jumpCooldownTime;
        }

        if (_isRolling) return;

        float speed = _isCrouching ? crouchSpeed : moveSpeed;

        if (_moveInput.x != 0 && !_justJumped && _rb.linearVelocity.y <= 0f && _jumpCooldown <= 0f)
            GroundSnap();

        bool isOnSlope = _isGrounded && _groundNormal != Vector2.up;
        float slopeAngle = Vector2.Angle(_groundNormal, Vector2.up);
        bool isSlopeWalkable = slopeAngle <= maxSlopeAngle;

        bool applySlope = isOnSlope && isSlopeWalkable && _moveInput.x != 0
                          && !_justJumped && _jumpCooldown <= 0f;

        Vector2 velocity;

        if (applySlope)
        {
            Vector2 slopeDir = Vector2.Perpendicular(_groundNormal) * -Mathf.Sign(_moveInput.x);
            velocity = slopeDir * speed;
        }
        else
        {
            velocity = new Vector2(_moveInput.x * speed, _rb.linearVelocity.y);
        }

        _rb.linearVelocity = new Vector2(
            velocity.x + platformVelocity.x,
            applySlope ? velocity.y : _rb.linearVelocity.y
        );

        if (_moveInput.x != 0)
            transform.localScale = new Vector3(_facingDirection, 1f, 1f);
    }

    private void DetectLanding()
    {
        if (_landingTimer > 0f)
            _landingTimer -= Time.deltaTime;

        bool justLanded = !_wasGroundedLastFrame && _isGrounded
                          && _verticalVelocityLastFrame < landingNoiseThreshold;

        if (justLanded)
            _landingTimer = landingNoiseDuration;

        _wasGroundedLastFrame = _isGrounded;
        _verticalVelocityLastFrame = _rb.linearVelocity.y;
    }

    private void GroundSnap()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, Vector2.down, stepHeight + 0.1f, groundLayer);

        if (hit.collider == null) return;

        float dist = transform.position.y - hit.point.y;
        if (dist > 0.05f && dist <= stepHeight)
        {
            Vector2 targetPos = new Vector2(_rb.position.x, hit.point.y);
            _rb.MovePosition(Vector2.Lerp(_rb.position, targetPos, 0.5f));
        }
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        foreach (ContactPoint2D contact in col.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                _groundNormal = contact.normal;
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        _groundNormal = Vector2.up;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {   
        _jumpRequested = true;
        _jumpBufferTimer = jumpBufferTime;

        if (_animator != null)
            _animator.SetTrigger("Jump");
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

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, Vector2.down * (stepHeight + 0.1f));
    }
}