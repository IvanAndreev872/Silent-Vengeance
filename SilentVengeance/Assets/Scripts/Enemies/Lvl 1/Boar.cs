using UnityEngine;
using System.Collections;

public class BoarAI : EnemyBase
{
    [Header("=== КАБАН — РАЗГОН ===")]
    [SerializeField] private float chargeSpeed = 10f;
    [SerializeField] private float chargeDistance = 6f;
    [SerializeField] private float chargeWindupTime = 0.7f;
    [SerializeField] private float chargeRecoveryTime = 1.2f;
    [SerializeField] private float chargeDamage = 25f;
    [SerializeField] private float chargeKnockbackForce = 8f;

    [Header("Визуал разгона")]
    [SerializeField] private float windupShakeIntensity = 0.04f;
    [SerializeField] private Color windupColor = new Color(1f, 0.4f, 0.2f, 1f);
    [SerializeField] private GameObject chargeTrailPrefab;
    [SerializeField] private GameObject wallHitEffectPrefab;

    [Header("Столкновение со стеной")]
    [SerializeField] private float wallCheckDistance = 0.6f;
    [SerializeField] private float wallStunDuration = 1.5f;
    [SerializeField] private float wallSelfDamage = 10f;
    [SerializeField] private LayerMask wallLayer;

    private bool isCharging = false;
    private bool isWindingUp = false;
    private Vector2 chargeDirection;
    private float chargedDistance;
    private Vector3 originalPosition;
    private Color originalColor;
    private bool hasHitPlayer = false;
    private GameObject activeTrail;

    protected override void Awake()
    {
        base.Awake();

        maxHealth = 100f;
        moveSpeed = 2f;
        chaseSpeed = 3.5f;
        attackDamage = 25f;
        attackRange = 3f;
        attackCooldown = 3f;
        detectionRange = 7f;
        hearingRange = 4f;
        suspicionTime = 2f;
        losePlayerTime = 6f;
        fieldOfViewAngle = 90f;
        currentHealth = maxHealth;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    protected override void Update()
    {
        base.Update();

        if (isCharging)
            CheckWallCollision();
    }


    protected override void HandleAttack()
    {
        if (player != null && !isCharging && !isWindingUp)
            FlipTowards(player.position);

        if (attackTimer <= 0 && !isCharging && !isWindingUp)
        {
            StartCoroutine(ChargeCoroutine());
            attackTimer = attackCooldown;
        }

        if (!isCharging && !isWindingUp)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            float distToPlayer = Vector2.Distance(
                transform.position, player.position
            );

            if (distToPlayer > attackRange)
                ChangeState(EnemyState.Chase);
        }
    }

    private IEnumerator ChargeCoroutine()
    {
        isWindingUp = true;
        originalPosition = transform.position;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (player != null)
        {
            chargeDirection = new Vector2(
                Mathf.Sign(player.position.x - transform.position.x), 0f
            );
            FlipTowards(player.position);
        }
        else
        {
            chargeDirection = new Vector2(facingRight ? 1f : -1f, 0f);
        }

        animator?.SetTrigger("ChargeWindup");

        if (spriteRenderer != null)
            spriteRenderer.color = windupColor;

        float elapsed = 0f;
        Vector3 shakeOrigin = transform.position;
        while (elapsed < chargeWindupTime)
        {
            float ox = Random.Range(-windupShakeIntensity, windupShakeIntensity);
            float oy = Random.Range(-windupShakeIntensity, windupShakeIntensity);
            transform.position = shakeOrigin + new Vector3(ox, oy, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = shakeOrigin;

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        isWindingUp = false;
        isCharging = true;
        hasHitPlayer = false;
        chargedDistance = 0f;

        animator?.SetTrigger("ChargeRun");

        if (chargeTrailPrefab != null)
        {
            activeTrail = Instantiate(
                chargeTrailPrefab, transform.position,
                Quaternion.identity, transform
            );
        }

        while (chargedDistance < chargeDistance && isCharging)
        {
            float step = chargeSpeed * Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(
                chargeDirection.x * chargeSpeed,
                rb.linearVelocity.y
            );
            chargedDistance += step;

            yield return new WaitForFixedUpdate();
        }

        EndCharge(false);
    }

    private void EndCharge(bool hitWall)
    {
        if (!isCharging && !isWindingUp) return;

        isCharging = false;
        isWindingUp = false;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (activeTrail != null)
            Destroy(activeTrail);

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        if (hitWall)
        {
            animator?.SetTrigger("WallHit");

            if (wallHitEffectPrefab != null)
            {
                Vector2 effectPos = (Vector2)transform.position +
                    chargeDirection * wallCheckDistance;
                GameObject effect = Instantiate(
                    wallHitEffectPrefab, effectPos, Quaternion.identity
                );
                Destroy(effect, 1f);
            }

            currentHealth -= wallSelfDamage;
            if (currentHealth <= 0)
            {
                Die();
                return;
            }

            Stun(wallStunDuration);
        }
        else
        {
            StartCoroutine(RecoveryCoroutine());
        }
    }

    private IEnumerator RecoveryCoroutine()
    {
        animator?.SetTrigger("ChargeEnd");

        float elapsed = 0f;
        while (elapsed < chargeRecoveryTime)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= attackRange && CanSeePlayer())
            {
            }
            else if (dist <= detectionRange && CanSeePlayer())
            {
                lastKnownPlayerPosition = player.position;
                ChangeState(EnemyState.Chase);
            }
            else
            {
                ChangeState(EnemyState.Return);
            }
        }
    }

    private void CheckWallCollision()
    {
        if (!isCharging) return;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            chargeDirection,
            wallCheckDistance,
            wallLayer
        );

        if (hit.collider != null)
        {
            StopAllCoroutines();
            EndCharge(true);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!isCharging || hasHitPlayer) return;

        if (((1 << collision.gameObject.layer) & playerLayer) == 0) return;

        hasHitPlayer = true;

        PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
        ph?.TakeDamage(chargeDamage);

        Rigidbody2D targetRb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            Vector2 knockDir = (chargeDirection + Vector2.up * 0.4f).normalized;
            targetRb.AddForce(knockDir * chargeKnockbackForce, ForceMode2D.Impulse);
        }
    }

    protected override void PerformAttack() { }

    protected override void OnExitState(EnemyState state)
    {
        base.OnExitState(state);

        if (state == EnemyState.Attack && (isCharging || isWindingUp))
        {
            StopAllCoroutines();
            isCharging = false;
            isWindingUp = false;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            if (activeTrail != null)
                Destroy(activeTrail);

            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;
        }
    }

    public override void TakeDamage(float damage)
    {
        float actualDamage = isCharging ? damage * 0.3f : damage;
        base.TakeDamage(actualDamage);
    }

    public override void Stun(float duration)
    {
        if (isCharging || isWindingUp)
        {
            StopAllCoroutines();
            isCharging = false;
            isWindingUp = false;

            if (activeTrail != null)
                Destroy(activeTrail);

            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;
        }

        base.Stun(duration);
    }


    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Vector3 dir = facingRight ? Vector3.right : Vector3.left;

        Gizmos.color = new Color(1f, 0.3f, 0f, 0.4f);
        Gizmos.DrawLine(
            transform.position,
            transform.position + dir * chargeDistance
        );
        Gizmos.DrawWireSphere(
            transform.position + dir * chargeDistance, 0.3f
        );

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(
            transform.position,
            transform.position + dir * wallCheckDistance
        );
    }
}
