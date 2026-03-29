using UnityEngine;

public class Bush : MonoBehaviour
{
    [Header("Визуал")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color occupiedColor = new Color(0.7f, 1f, 0.7f, 0.8f);
    [SerializeField] private float colorFadeSpeed = 5f;
    [SerializeField] private bool animateSway = true;
    [SerializeField] private float swayAmount = 2f;
    [SerializeField] private float swaySpeed = 3f;

    private SpriteRenderer spriteRenderer;
    private Color targetColor;
    private bool isOccupied = false;
    private float swayTimer = 0f;
    private Quaternion originalRotation;

    public bool IsOccupied => isOccupied;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        targetColor = normalColor;
        originalRotation = transform.rotation;

        if (spriteRenderer != null)
            spriteRenderer.color = normalColor;
    }

    private void Update()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(
                spriteRenderer.color, targetColor,
                Time.deltaTime * colorFadeSpeed
            );
        }

        if (animateSway && isOccupied)
        {
            swayTimer += Time.deltaTime * swaySpeed;
            float angle = Mathf.Sin(swayTimer) * swayAmount;
            transform.rotation = originalRotation * Quaternion.Euler(0, 0, angle);
        }
        else
        {
            swayTimer = 0f;
            transform.rotation = Quaternion.Lerp(
                transform.rotation, originalRotation,
                Time.deltaTime * colorFadeSpeed
            );
        }
    }

    public void OnPlayerEnter()
    {
        isOccupied = true;
        targetColor = occupiedColor;

        swayTimer = 0f;
    }

    public void OnPlayerExit()
    {
        isOccupied = false;
        targetColor = normalColor;
    }
}
