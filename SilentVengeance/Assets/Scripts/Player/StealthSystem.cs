using UnityEngine;

public class StealthSystem : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private float hideTransparency = 0.3f;
    [SerializeField] private float fadeSpeed = 8f;
    [SerializeField] private float bushDetectionRadius = 1f;
    [SerializeField] private LayerMask bushLayer;

    [Header("Ссылки")]
    [SerializeField] private SpriteRenderer playerSprite;

    private bool isHidden = false;
    private bool isNearBush = false;
    private Bush currentBush;
    private float targetAlpha = 1f;
    private Color originalColor;

    private PlayerInputActions _input;

    public bool IsHidden => isHidden;
    public Bush CurrentBush => currentBush;

    private void Awake()
    {
        _input = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _input.Player.Enable();
        
       _input.Player.Hide.performed += OnHide;
    }

    private void OnDisable()
    {
        _input.Player.Hide.performed -= OnHide;
        _input.Player.Disable();
    }

    private void Start()
    {
        if (playerSprite == null)
            playerSprite = GetComponent<SpriteRenderer>();

        if (playerSprite != null)
            originalColor = playerSprite.color;
    }

    private void Update()
    {
        DetectNearbyBush();
        UpdateVisuals();
    }

    private void OnHide(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (isHidden)
        {
            ExitHiding();
        }
        else if (isNearBush && currentBush != null)
        {
            EnterHiding();
        }
    }

    private void DetectNearbyBush()
    {
        Collider2D bushCollider = Physics2D.OverlapCircle(
            transform.position, bushDetectionRadius, bushLayer
        );

        if (bushCollider != null)
        {
            Bush bush = bushCollider.GetComponent<Bush>();
            if (bush != null)
            {
                isNearBush = true;
                currentBush = bush;
                return;
            }
        }

        if (isNearBush)
        {
            isNearBush = false;

            if (isHidden)
                ExitHiding();

            currentBush = null;
        }
    }

    private void EnterHiding()
    {
        isHidden = true;
        targetAlpha = hideTransparency;

        if (currentBush != null)
            currentBush.OnPlayerEnter();

        Debug.Log("Игрок спрятался в кустах");
    }

    private void ExitHiding()
    {
        isHidden = false;
        targetAlpha = 1f;

        if (currentBush != null)
            currentBush.OnPlayerExit();

        Debug.Log("Игрок вышел из укрытия");
    }

    private void UpdateVisuals()
    {
        if (playerSprite == null) return;

        Color current = playerSprite.color;
        float newAlpha = Mathf.Lerp(current.a, targetAlpha, Time.deltaTime * fadeSpeed);

        playerSprite.color = new Color(
            originalColor.r,
            originalColor.g,
            originalColor.b,
            newAlpha
        );
    }

    public void ForceReveal()
    {
        if (isHidden)
            ExitHiding();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isHidden ? Color.green : Color.white;
        Gizmos.DrawWireSphere(transform.position, bushDetectionRadius);
    }
}