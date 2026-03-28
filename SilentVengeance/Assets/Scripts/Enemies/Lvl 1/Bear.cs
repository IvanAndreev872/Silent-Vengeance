using UnityEngine;
using System.Collections;

public class BearAI : EnemyBase
{
    [Header("=== МЕДВЕДЬ — УДАР ПО ЗЕМЛЕ ===")]
    [SerializeField] private float slamDelay = 0.8f;
    [SerializeField] private float slamRadius = 1.8f;
    [SerializeField] private float slamKnockbackForce = 5f;
    [SerializeField] private GameObject slamEffectPrefab;

    [Header("Визуал подготовки")]
    [SerializeField] private float shakeIntensity = 0.05f;
    [SerializeField] private Color warningColor = new Color(1f, 0.3f, 0.3f, 1f);

    private bool isSlamming = false;
    private Vector3 originalPosition;
    private Color originalColor;

    protected override void Awake()
    {
        base.Awake();

        maxHealth = 150f;
        moveSpeed = 1.5f;
        chaseSpeed = 2.8f;
        attackDamage = 30f;
        attackRange = 2f;
        attackCooldown = 2.5f;
        detectionRange = 7f;
        hearingRange = 5f;
        suspicionTime = 2.5f;
        losePlayerTime = 8f;
        fieldOfViewAngle = 100f;
        currentHealth = maxHealth;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    protected override void HandleAttack()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (player != null)
            FlipTowards(player.position);

        if (attackTimer <= 0 && !isSlamming)
        {
            StartCoroutine(GroundSlamCoroutine());
            attackTimer = attackCooldown;
        }

        if (!isSlamming)
        {
            float distToPlayer = Vector2.Distance(
                transform.position, player.position
            );

            if (distToPlayer > attackRange)
            {
                ChangeState(EnemyState.Chase);
            }
        }
    }

    private IEnumerator GroundSlamCoroutine()
    {
        isSlamming = true;
        originalPosition = transform.position;

        animator?.SetTrigger("SlamWindup");

        if (spriteRenderer != null)
            spriteRenderer.color = warningColor;

        float elapsed = 0f;
        while (elapsed < slamDelay)
        {
            float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
            float offsetY = Random.Range(-shakeIntensity, shakeIntensity);
            transform.position = originalPosition + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        PerformSlam();

        yield return new WaitForSeconds(0.3f);

        isSlamming = false;
    }

    private void PerformSlam()
    {
        animator?.SetTrigger("SlamHit");

        if (slamEffectPrefab != null && attackPoint != null)
        {
            GameObject effect = Instantiate(
                slamEffectPrefab,
                attackPoint.position,
                Quaternion.identity
            );
            Destroy(effect, 1f);
        }

        if (attackPoint == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position, slamRadius, playerLayer
        );

        foreach (Collider2D hit in hits)
        {
            PlayerHealth ph = hit.GetComponent<PlayerHealth>();
            ph?.TakeDamage(attackDamage);

            Rigidbody2D targetRb = hit.GetComponent<Rigidbody2D>();
            if (targetRb != null)
            {
                Vector2 knockDir = (
                    hit.transform.position - transform.position
                ).normalized;

                knockDir = (knockDir + Vector2.up * 0.5f).normalized;

                targetRb.AddForce(
                    knockDir * slamKnockbackForce,
                    ForceMode2D.Impulse
                );
            }
        }
    }

    protected override void PerformAttack() { }

    protected override void OnExitState(EnemyState state)
    {
        base.OnExitState(state);

        if (state == EnemyState.Attack && isSlamming)
        {
            StopAllCoroutines();
            isSlamming = false;
            transform.position = originalPosition;

            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;
        }
    }

    public override void TakeDamage(float damage)
    {
        float actualDamage = isSlamming ? damage * 0.5f : damage;
        base.TakeDamage(actualDamage);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (attackPoint != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(attackPoint.position, slamRadius);
        }
    }
}
