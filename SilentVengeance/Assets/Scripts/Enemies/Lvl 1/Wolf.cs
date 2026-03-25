using UnityEngine;

public class WolfAI : EnemyBase
{
    [Header("=== ВОЛК ===")]
    [SerializeField] private float howlAlertRadius = 8f;
    [SerializeField] private float leapForce = 6f;
    [SerializeField] private float leapCooldown = 4f;
    [SerializeField] private float packBoostMultiplier = 1.3f;

    private float leapTimer = 0f;
    private bool isLeaping = false;
    private bool hasPackNearby = false;

    protected override void Awake()
    {
        base.Awake();
        maxHealth = 60f;
        moveSpeed = 2.5f;
        chaseSpeed = 5.5f;
        attackDamage = 15f;
        attackRange = 1.2f;
        attackCooldown = 1.0f;
        detectionRange = 7f;
        hearingRange = 5f;
        suspicionTime = 1.5f;
        losePlayerTime = 8f;
        fieldOfViewAngle = 120f;
        currentHealth = maxHealth;
    }

    protected override void Update()
    {
        base.Update();
        leapTimer -= Time.deltaTime;
        CheckForPack();
    }

    private void CheckForPack()
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(
            transform.position, howlAlertRadius
        );

        hasPackNearby = false;
        foreach (Collider2D col in nearby)
        {
            WolfAI other = col.GetComponent<WolfAI>();
            if (other != null && other != this && !other.IsDead)
            {
                hasPackNearby = true;
                break;
            }
        }
    }

    protected override void OnEnterState(EnemyState state)
    {
        base.OnEnterState(state);

        if (state == EnemyState.Chase)
        {
            AlertPack();
            animator?.SetTrigger("Howl");
        }
    }

    private void AlertPack()
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(
            transform.position, howlAlertRadius
        );

        foreach (Collider2D col in nearby)
        {
            WolfAI other = col.GetComponent<WolfAI>();
            if (other != null && other != this && !other.IsDead)
            {
                other.ReceiveAlert(lastKnownPlayerPosition);
            }
        }
    }

    public void ReceiveAlert(Vector2 playerPos)
    {
        if (currentState == EnemyState.Chase) return;
        lastKnownPlayerPosition = playerPos;
        ChangeState(EnemyState.Chase);
    }

    protected override void HandleChase()
    {
        if (player == null)
        {
            ChangeState(EnemyState.Return);
            return;
        }

        float distToPlayer = Vector2.Distance(
            transform.position, player.position
        );

        if (distToPlayer <= attackRange * 2.5f &&
            distToPlayer > attackRange &&
            leapTimer <= 0 && !isLeaping)
        {
            Vector2 leapDir = (
                player.position - transform.position
            ).normalized;
            PerformLeap(leapDir);
            return;
        }

        if (isLeaping) return;

        float currentSpeed = hasPackNearby
            ? chaseSpeed * packBoostMultiplier
            : chaseSpeed;

        UpdateNavigationToTarget(lastKnownPlayerPosition);
        MoveAlongPath(currentSpeed);

        if (distToPlayer <= attackRange && CanSeePlayer())
        {
            pathfinding?.ClearPath();
            ChangeState(EnemyState.Attack);
        }
    }

    private void PerformLeap(Vector2 direction)
    {
        isLeaping = true;
        leapTimer = leapCooldown;
        pathfinding?.ClearPath();

        Vector2 leapDir = new Vector2(direction.x, 0.5f).normalized;
        rb.linearVelocity = leapDir * leapForce;

        animator?.SetTrigger("Leap");
        Invoke(nameof(EndLeap), 0.5f);
    }

    private void EndLeap()
    {
        isLeaping = false;
    }
}