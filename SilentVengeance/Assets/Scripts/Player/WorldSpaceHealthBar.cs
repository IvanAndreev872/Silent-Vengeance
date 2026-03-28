using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceHealthBar : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image backgroundImage;

    [Header("Настройки")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color midHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private float midHealthThreshold = 0.6f;

    [Header("Плавность")]
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Скрытие при полном HP")]
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField] private float fadeSpeed = 3f;

    private Transform targetTransform;
    private CanvasGroup canvasGroup;
    private float displayedFill = 1f;
    private float targetAlpha = 0f;

    private void Awake()
    {
        canvasGroup = GetComponentInParent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = GetComponentInChildren<CanvasGroup>();
    }

    private void Start()
    {
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<PlayerHealth>();
                targetTransform = player.transform;
            }
        }
        else
        {
            targetTransform = playerHealth.transform;
        }

        displayedFill = 1f;
        if (fillImage != null)
            fillImage.fillAmount = 1f;
    }

    private void LateUpdate()
    {
        if (playerHealth == null || targetTransform == null) return;

        transform.position = targetTransform.position + offset;

        float targetFill = playerHealth.CurrentHealth / playerHealth.MaxHealth;
        displayedFill = Mathf.Lerp(displayedFill, targetFill, Time.deltaTime * smoothSpeed);

        if (fillImage != null)
        {
            fillImage.fillAmount = displayedFill;
            fillImage.color = GetHealthColor(displayedFill);
        }

        if (canvasGroup != null && hideWhenFull)
        {
            targetAlpha = (targetFill < 0.99f) ? 1f : 0f;
            canvasGroup.alpha = Mathf.Lerp(
                canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed
            );
        }
    }

    private Color GetHealthColor(float ratio)
    {
        if (ratio <= lowHealthThreshold)
            return Color.Lerp(lowHealthColor, midHealthColor, ratio / lowHealthThreshold);
        if (ratio <= midHealthThreshold)
            return Color.Lerp(midHealthColor, fullHealthColor,
                (ratio - lowHealthThreshold) / (midHealthThreshold - lowHealthThreshold));
        return fullHealthColor;
    }
}
