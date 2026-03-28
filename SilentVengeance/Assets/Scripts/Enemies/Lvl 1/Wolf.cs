using UnityEngine;

public class WolfAI : EnemyBase
{
    [Header("=== ВОЛК — СТАЯ ===")]
    [SerializeField] private float howlAlertRadius = 8f;

    private bool hasAlertedPack = false;

    protected override void Awake()
    {
        base.Awake();

        maxHealth = 60f;
        moveSpeed = 2.5f;
        chaseSpeed = 4.5f;
        attackDamage = 15f;
        attackRange = 1.2f;
        attackCooldown = 1.2f;
        detectionRange = 6f;
        hearingRange = 4f;
        suspicionTime = 2f;
        losePlayerTime = 6f;
        fieldOfViewAngle = 110f;
        currentHealth = maxHealth;
    }

    protected override void OnEnterState(EnemyState state)
    {
        base.OnEnterState(state);

        if (state == EnemyState.Chase && !hasAlertedPack)
        {
            AlertPack();
            hasAlertedPack = true;
        }

        if (state == EnemyState.Patrol || state == EnemyState.Idle)
        {
            hasAlertedPack = false;
        }
    }

    private void AlertPack()
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(
            transform.position, howlAlertRadius
        );

        foreach (Collider2D col in nearby)
        {
            WolfAI otherWolf = col.GetComponent<WolfAI>();

            if (otherWolf != null && otherWolf != this && !otherWolf.IsDead)
            {
                otherWolf.ReceiveAlert(lastKnownPlayerPosition);
            }
        }
    }

    public void ReceiveAlert(Vector2 playerPos)
    {
        if (currentState == EnemyState.Chase ||
            currentState == EnemyState.Attack)
            return;

        lastKnownPlayerPosition = playerPos;
        ChangeState(EnemyState.Chase);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, howlAlertRadius);
    }
}