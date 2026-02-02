using UnityEngine;

public class GolemEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 1.5f;
    public float patrolDistance = 4f;
    public float chaseSpeed = 2.5f;

    [Header("Combat")]
    public float detectRange = 4f;
    public float attackRange = 1.8f;
    public float attackCooldown = 2f;
    public int attackDamage = 2;
    public int maxHealth = 5;

    [Header("Revive System")]
    public int reviveCount = 1;
    public float reviveDelay = 2f;
    public int healthAfterRevive = 3;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 2f;
    public float groundCheckOffset = 0.8f;

    [Header("References")]
    public Animator animator;

    [Header("Sprite")]
    public bool spriteDefaultFacingLeft = false;

    // Private
    private Transform player;
    private Vector3 startPosition;
    private float leftLimit, rightLimit;
    private int direction = 1;
    private float lastAttackTime;
    private bool isDead = false;
    private bool isAttacking = false;
    private bool isReviving = false;
    private int currentReviveCount;
    private int currentHealth;
    private Rigidbody2D rb;
    private Collider2D col;

    void Start()
    {
        startPosition = transform.position;
        leftLimit = startPosition.x - patrolDistance;
        rightLimit = startPosition.x + patrolDistance;

        animator = animator != null ? animator : GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        currentHealth = maxHealth;
        currentReviveCount = reviveCount;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (isDead || isReviving) return;

        if (player == null)
        {
            Patrol();
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange)
        {
            SetWalking(false);
            FacePlayer();
            if (Time.time >= lastAttackTime + attackCooldown && !isAttacking)
                Attack();
        }
        else if (dist <= detectRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        if (!IsGroundAhead())
        {
            direction *= -1;
            FaceDirection(direction);
        }

        transform.Translate(Vector2.right * direction * walkSpeed * Time.deltaTime);
        SetWalking(true);

        if (transform.position.x >= rightLimit)
        {
            direction = -1;
            FaceDirection(-1);
        }
        else if (transform.position.x <= leftLimit)
        {
            direction = 1;
            FaceDirection(1);
        }
    }

    void ChasePlayer()
    {
        FacePlayer();

        if (!IsGroundAhead())
        {
            SetWalking(false);
            return;
        }

        float dir = player.position.x > transform.position.x ? 1 : -1;
        transform.Translate(Vector2.right * dir * chaseSpeed * Time.deltaTime);
        SetWalking(true);
    }

    void Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        if (animator != null)
            animator.SetTrigger("Attack");

        Invoke(nameof(DealDamage), 0.5f);
        Invoke(nameof(EndAttack), 1f);
    }

    void DealDamage()
    {
        if (isDead || player == null) return;

        if (Vector2.Distance(transform.position, player.position) <= attackRange * 1.3f)
        {
            PlayerControll p = player.GetComponent<PlayerControll>();
            if (p != null) p.TakeDamage(attackDamage);
        }
    }

    void EndAttack() => isAttacking = false;

    bool IsGroundAhead()
    {
        Vector2 origin = new Vector2(
            transform.position.x + (direction * groundCheckOffset),
            transform.position.y
        );
        return Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer).collider != null;
    }

    void FacePlayer()
    {
        if (player != null)
            FaceDirection(player.position.x < transform.position.x ? -1 : 1);
    }

    void FaceDirection(int dir)
    {
        direction = dir;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (spriteDefaultFacingLeft ? -dir : dir);
        transform.localScale = scale;
    }

    void SetWalking(bool walking)
    {
        if (animator != null)
            animator.SetBool("isWalking", walking);
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isReviving) return;

        currentHealth -= damage;

        if (animator != null)
            animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            if (currentReviveCount > 0)
                FakeDie();
            else
                RealDie();
        }
    }

    // Chết giả - sẽ hồi sinh
    void FakeDie()
    {
        isDead = true;
        isReviving = true;
        SetWalking(false);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
        }

        if (col != null)
            col.enabled = false;

        // Chạy animation Die
        if (animator != null)
            animator.SetTrigger("Die");

        // Sau reviveDelay giây -> chạy animation Revive
        Invoke(nameof(StartRevive), reviveDelay);
    }

    // Bắt đầu animation Revive
    void StartRevive()
    {
        if (animator != null)
            animator.SetTrigger("Revive");

        // Sau khi animation Revive chạy xong -> hoàn tất hồi sinh
        Invoke(nameof(FinishRevive), 1f);
    }

    // Hoàn tất hồi sinh - bật lại collider, gravity
    void FinishRevive()
    {
        currentReviveCount--;
        currentHealth = healthAfterRevive;
        isDead = false;
        isReviving = false;

        if (rb != null)
            rb.gravityScale = 1;

        if (col != null)
            col.enabled = true;
    }

    void RealDie()
    {
        isDead = true;
        SetWalking(false);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
        }

        if (col != null)
            col.enabled = false;

        if (animator != null)
            animator.SetTrigger("Die");

        Destroy(gameObject, 2f);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? startPosition : transform.position;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(
            new Vector3(center.x - patrolDistance, center.y, 0),
            new Vector3(center.x + patrolDistance, center.y, 0)
        );

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.green;
        Vector3 rayOrigin = new Vector3(
            transform.position.x + (direction * groundCheckOffset),
            transform.position.y, 0
        );
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * groundCheckDistance);
    }
}
