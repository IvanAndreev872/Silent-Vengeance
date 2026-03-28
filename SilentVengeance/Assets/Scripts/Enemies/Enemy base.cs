using UnityEngine;
using System.Collections.Generic;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("=== ОСНОВНЫЕ ХАРАКТЕРИСТИКИ ===")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float chaseSpeed = 4f;
    [SerializeField] protected float attackDamage = 10f;
    [SerializeField] protected float attackRange = 1f;
    [SerializeField] protected float attackCooldown = 1.5f;

    [Header("=== ОБНАРУЖЕНИЕ ===")]
    [SerializeField] protected float detectionRange = 5f;
    [SerializeField] protected float hearingRange = 3f;
    [SerializeField] protected float suspicionTime = 3f;
    [SerializeField] protected float losePlayerTime = 5f;
    [SerializeField] protected float fieldOfViewAngle = 90f;
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected LayerMask obstacleLayer;

    [Header("=== ПАТРУЛЬ ===")]
    [SerializeField] protected Transform[] patrolPoints;
    [SerializeField] protected float patrolWaitTime = 2f;

    [Header("=== ССЫЛКИ ===")]
    [SerializeField] protected Transform attackPoint;

    protected float currentHealth;
    protected EnemyState currentState = EnemyState.Patrol;
    protected EnemyState previousState;
    protected Transform player;
    protected Rigidbody2D rb;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected EnemyPathfinding pathfinding;

    protected int currentPatrolIndex = 0;
    protected float stateTimer = 0f;
    protected float attackTimer = 0f;
    protected bool facingRight = false;
    protected Vector2 lastKnownPlayerPosition;
    protected float suspicionTimer = 0f;

    public EnemyState CurrentState => currentState;
    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        pathfinding = GetComponent<EnemyPathfinding>();
        currentHealth = maxHealth;
    }

    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    protected virtual void Update()
    {
        if (IsDead) return;

        attackTimer -= Time.deltaTime;

        switch (currentState)
        {
            case EnemyState.Idle:     HandleIdle(); break;
            case EnemyState.Patrol:   HandlePatrol(); break;
            case EnemyState.Suspicious: HandleSuspicious(); break;
            case EnemyState.Chase:    HandleChase(); break;
            case EnemyState.Attack:   HandleAttack(); break;
            case EnemyState.Return:   HandleReturn(); break;
            case EnemyState.Stunned:  HandleStunned(); break;
        }

        if (currentState != EnemyState.Stunned &&
            currentState != EnemyState.Attack)
        {
            CheckForPlayer();
        }

        Debug.Log(currentState);

        UpdateAnimations();
    }

    protected void MoveAlongPath(float speed)
    {
        if (pathfinding == null || !pathfinding.HasPath)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        Vector2 direction = pathfinding.GetMoveDirection();

        if (direction == Vector2.zero)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(
            direction.x * speed,
            rb.linearVelocity.y
        );

        if (direction.x != 0)
        {
            FlipTowards(
                (Vector2)transform.position + direction
            );
        }
    }

    protected void MoveAlongPathFull2D(float speed)
    {
        if (pathfinding == null || !pathfinding.HasPath)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = pathfinding.GetMoveDirection();

        if (direction == Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = direction * speed;

        if (direction.x != 0)
        {
            FlipTowards(
                (Vector2)transform.position + direction
            );
        }
    }

    protected bool NavigateTo(Vector2 target)
    {
        if (pathfinding == null) return false;
        return pathfinding.RequestPath(target);
    }

    protected bool UpdateNavigationToTarget(Vector2 target)
    {
        if (pathfinding == null) return false;
        return pathfinding.UpdatePathToTarget(target);
    }

    protected virtual void HandleIdle()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            ChangeState(EnemyState.Patrol);
        }
    }

    protected virtual void HandlePatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        Transform target = patrolPoints[currentPatrolIndex];

        UpdateNavigationToTarget(target.position);

        MoveAlongPathFull2D(moveSpeed);

        if (Vector2.Distance(transform.position, target.position) < 0.5f)
        {
            currentPatrolIndex =
                (currentPatrolIndex + 1) % patrolPoints.Length;
            stateTimer = patrolWaitTime;
            pathfinding?.ClearPath();
            ChangeState(EnemyState.Idle);
        }
    }

    protected virtual void HandleSuspicious()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        FlipTowards(lastKnownPlayerPosition);
    }

    protected virtual void HandleChase()
    {
        if (player == null)
        {
            ChangeState(EnemyState.Return);
            return;
        }

        UpdateNavigationToTarget(lastKnownPlayerPosition);

        MoveAlongPathFull2D(chaseSpeed);

        float distToPlayer = Vector2.Distance(
            transform.position, player.position
        );

        if (distToPlayer <= attackRange && CanSeePlayer())
        {
            pathfinding?.ClearPath();
            ChangeState(EnemyState.Attack);
        }

        if (pathfinding != null && pathfinding.ReachedEnd &&
            distToPlayer > attackRange)
        {
            pathfinding.ForceRecalculate();
        }
    }

    protected virtual void HandleAttack()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (player != null)
            FlipTowards(player.position);

        if (attackTimer <= 0)
        {
            PerformAttack();
            attackTimer = attackCooldown;
        }

        float distToPlayer = Vector2.Distance(
            transform.position, player.position
        );

        if (distToPlayer > attackRange)
        {
            ChangeState(EnemyState.Chase);
        }
    }

    protected virtual void HandleReturn()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        Transform closest = GetClosestPatrolPoint();

        UpdateNavigationToTarget(closest.position);
        MoveAlongPathFull2D(moveSpeed);

        if (Vector2.Distance(
                transform.position, closest.position) < 0.5f)
        {
            pathfinding?.ClearPath();
            ChangeState(EnemyState.Patrol);
        }
    }

    protected virtual void HandleStunned()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        pathfinding?.ClearPath();

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            ChangeState(EnemyState.Suspicious);
        }
    }

    protected virtual void CheckForPlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(
            transform.position, player.position
        );
        bool canSeePlayer = CanSeePlayer();
        bool canHearPlayer = distanceToPlayer <= hearingRange;

        switch (currentState)
        {
            case EnemyState.Idle:
            case EnemyState.Patrol:
            case EnemyState.Return:
                if (canSeePlayer)
                {
                    if (distanceToPlayer <= detectionRange * 0.4f)
                    {
                        lastKnownPlayerPosition = player.position;
                        ChangeState(EnemyState.Chase);
                    }
                    else
                    {
                        lastKnownPlayerPosition = player.position;
                        suspicionTimer = suspicionTime;
                        ChangeState(EnemyState.Suspicious);
                    }
                }
                else if (canHearPlayer && IsPlayerMakingNoise())
                {
                    lastKnownPlayerPosition = player.position;
                    suspicionTimer = suspicionTime;
                    ChangeState(EnemyState.Suspicious);
                }
                break;

            case EnemyState.Suspicious:
                if (canSeePlayer)
                {
                    suspicionTimer -= Time.deltaTime * 2f;
                    lastKnownPlayerPosition = player.position;
                    if (suspicionTimer <= 0)
                    {
                        ChangeState(EnemyState.Chase);
                    }
                }
                else
                {
                    suspicionTimer -= Time.deltaTime * 0.5f;
                    if (suspicionTimer <= 0)
                    {
                        ChangeState(EnemyState.Return);
                    }
                }
                break;

            case EnemyState.Chase:
                if (canSeePlayer)
                {
                    lastKnownPlayerPosition = player.position;
                    stateTimer = losePlayerTime;
                }
                else
                {
                    stateTimer -= Time.deltaTime;
                    if (stateTimer <= 0)
                    {
                        pathfinding?.ClearPath();
                        ChangeState(EnemyState.Return);
                    }
                }
                break;
        }
    }

    protected bool CanSeePlayer()
    {
        if (player == null) return false;

        float dist = Vector2.Distance(
            transform.position, player.position
        );
        if (dist > detectionRange) return false;

        Vector2 dirToPlayer = (
            player.position - transform.position
        ).normalized;

        Vector2 facing = facingRight ? Vector2.right : Vector2.left;
        float angle = Vector2.Angle(facing, dirToPlayer);
        if (angle > fieldOfViewAngle / 2f) return false;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, dirToPlayer,
            dist, obstacleLayer
        );

        return hit.collider == null;
    }

    protected virtual bool IsPlayerMakingNoise()
    {
        PlayerController pc = player?.GetComponent<PlayerController>();
        return pc != null && (pc.IsRunning || pc.IsLanding);
    }

    protected virtual void PerformAttack()
    {
        animator?.SetTrigger("Attack");

        Collider2D hit = Physics2D.OverlapCircle(
            attackPoint.position, attackRange * 0.5f, playerLayer
        );

        if (hit != null)
        {
            PlayerHealth ph = hit.GetComponent<PlayerHealth>();
            ph?.TakeDamage(attackDamage);
        }

        OnAttack();
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        animator?.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (player != null)
        {
            lastKnownPlayerPosition = player.position;
            pathfinding?.ForceRecalculate();
            ChangeState(EnemyState.Chase);
        }
    }

    public virtual void Stun(float duration)
    {
        stateTimer = duration;
        ChangeState(EnemyState.Stunned);
    }

    protected virtual void Die()
    {
        animator?.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero;
        pathfinding?.ClearPath();
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
        Destroy(gameObject, 3f);
    }

    protected void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        OnExitState(currentState);
        previousState = currentState;
        currentState = newState;
        stateTimer = GetDefaultStateTimer(newState);
        OnEnterState(newState);
    }

    protected virtual float GetDefaultStateTimer(EnemyState state)
    {
        return state switch
        {
            EnemyState.Chase => losePlayerTime,
            _ => stateTimer
        };
    }

    protected virtual void OnEnterState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Chase:
                NavigateTo(lastKnownPlayerPosition);
                break;
            case EnemyState.Return:
                Transform closest = GetClosestPatrolPoint();
                if (closest != null)
                    NavigateTo(closest.position);
                break;
            case EnemyState.Patrol:
                if (patrolPoints != null && patrolPoints.Length > 0)
                    NavigateTo(
                        patrolPoints[currentPatrolIndex].position
                    );
                break;
        }
    }

    protected virtual void OnExitState(EnemyState state)
    {
        if (state == EnemyState.Chase ||
            state == EnemyState.Return ||
            state == EnemyState.Patrol)
        {
            pathfinding?.ClearPath();
        }
    }

    protected virtual void OnAttack() { }

    protected void FlipTowards(Vector2 target)
    {
        bool shouldFaceRight = target.x > transform.position.x;
        if (shouldFaceRight != facingRight)
        {
            facingRight = shouldFaceRight;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (facingRight ? -1 : 1);
            transform.localScale = scale;
        }
    }

    protected Transform GetClosestPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return null;

        Transform closest = patrolPoints[0];
        float minDist = float.MaxValue;

        foreach (Transform point in patrolPoints)
        {
            float dist = Vector2.Distance(
                transform.position, point.position
            );
            if (dist < minDist)
            {
                minDist = dist;
                closest = point;
            }
        }
        Debug.Log(closest.transform);
        return closest;
    }

    protected virtual void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetInteger("State", (int)currentState);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hearingRange);

        Gizmos.color = Color.red;
        if (attackPoint != null)
            Gizmos.DrawWireSphere(
                attackPoint.position, attackRange * 0.5f
            );

        Gizmos.color = Color.green;
        Vector3 forward = facingRight ? Vector3.right : Vector3.left;
        Vector3 left = Quaternion.Euler(
            0, 0, fieldOfViewAngle / 2f
        ) * forward;
        Vector3 right = Quaternion.Euler(
            0, 0, -fieldOfViewAngle / 2f
        ) * forward;

        Gizmos.DrawLine(
            transform.position,
            transform.position + left * detectionRange
        );
        Gizmos.DrawLine(
            transform.position,
            transform.position + right * detectionRange
        );
    }
}