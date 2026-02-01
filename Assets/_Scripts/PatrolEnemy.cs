using UnityEngine;

public class PatrolEnemy : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement speed in units per second")]
    public float speed = 2f;
    [Tooltip("Distance to patrol to the left and right from the start position")]
    public float patrolDistance = 3f;
    [Tooltip("If true, the sprite will be flipped when changing direction")]
    public bool flipSprite = true;
    [Tooltip("If true, enemy will turn around at ledges to avoid falling")]
    public bool avoidFalling = true;
    [Tooltip("Layer mask for ground detection")]
    public LayerMask groundLayer;
    [Tooltip("Distance to check for ground ahead")]
    public float groundCheckDistance = 3f;
    [Tooltip("How far ahead to check for ground (default is half the sprite width)")]
    public float groundCheckOffset = 0.5f;

    [Header("Combat")]
    [Tooltip("Detection range for player")]
    public float detectRange = 3f;
    [Tooltip("Attack range")]
    public float attackRange = 1.5f;
    [Tooltip("Time between attacks")]
    public float attackCooldown = 1.5f;
    [Tooltip("Layer mask for player detection")]
    public LayerMask playerLayer;
    [Tooltip("Health of enemy")]
    public int health = 3;

    [Header("References")]
    public Animator animator;

    private Vector3 startPosition;
    private float leftLimit;
    private float rightLimit;
    private int direction = 1; // 1 = right, -1 = left
    private bool facingRight = true;
    
    // Combat variables
    private Transform player;
    private float lastAttackTime;
    private int attackComboCount = 0;
    private bool isAttacking = false;
    private bool isDead = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
        leftLimit = startPosition.x - Mathf.Abs(patrolDistance);
        rightLimit = startPosition.x + Mathf.Abs(patrolDistance);
        
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Find player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;

        // Check for player in range
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            
            // If player in attack range, attack
            if (distanceToPlayer <= attackRange)
            {
                if (Time.time >= lastAttackTime + attackCooldown && !isAttacking)
                {
                    AttackPlayer();
                }
                return; // Don't patrol while attacking
            }
            // If player in detection range but not attack range, stop and look at player
            else if (distanceToPlayer <= detectRange)
            {
                // Face the player
                if (player.position.x < transform.position.x && facingRight)
                {
                    Flip();
                }
                else if (player.position.x > transform.position.x && !facingRight)
                {
                    Flip();
                }
                return; // Don't patrol while player is nearby
            }
        }

        // Normal patrol behavior
        if (!isAttacking)
        {
            Patrol();
        }
    }

    void Patrol()
    {
        // Check if there's ground ahead (to avoid falling)
        if (avoidFalling && !IsGroundAhead())
        {
            // Turn around if no ground ahead
            direction *= -1;
            if (flipSprite) Flip();
        }

        // Move
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);

        // Check limits and change direction
        if (direction == 1 && transform.position.x >= rightLimit)
        {
            direction = -1;
            if (flipSprite) Flip();
        }
        else if (direction == -1 && transform.position.x <= leftLimit)
        {
            direction = 1;
            if (flipSprite) Flip();
        }
    }

    void AttackPlayer()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // Combo logic: 2 attack1 -> 1 attack2
        attackComboCount++;
        
        if (attackComboCount <= 2)
        {
            // Attack 1
            if (animator != null)
            {
                animator.SetBool("isAttack1", true);
            }
            Invoke(nameof(ResetAttack1), 0.5f); // Reset after animation
        }
        else
        {
            // Attack 2 (third hit)
            if (animator != null)
            {
                animator.SetBool("isAttack2", true);
            }
            Invoke(nameof(ResetAttack2), 0.7f); // Reset after animation
            attackComboCount = 0; // Reset combo
        }
        
        Invoke(nameof(EndAttack), attackCooldown);
    }

    void ResetAttack1()
    {
        if (animator != null)
        {
            animator.SetBool("isAttack1", false);
        }
    }

    void ResetAttack2()
    {
        if (animator != null)
        {
            animator.SetBool("isAttack2", false);
        }
    }

    void EndAttack()
    {
        isAttacking = false;
    }

    // Called when player attacks this enemy
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        health -= damage;
        
        if (health <= 0)
        {
            Die();
        }
        else
        {
            // Trigger hurt animation
            if (animator != null)
            {
                animator.SetTrigger("Hurt");
            }
        }
    }

    void Die()
    {
        isDead = true;
        
        // You can add death animation here
        // animator.SetTrigger("Death");
        
        // Disable components
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
        
        // Destroy after delay
        Destroy(gameObject, 2f);
    }

    bool IsGroundAhead()
    {
        // Cast a ray downward from a point slightly ahead of the enemy
        Vector2 rayOrigin = new Vector2(transform.position.x + (direction * groundCheckOffset), transform.position.y);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, groundLayer);
        
        return hit.collider != null;
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1 : -1);
        transform.localScale = scale;
    }

    // Draw patrol range and detection ranges in the Scene view
    void OnDrawGizmosSelected()
    {
        Vector3 pos = Application.isPlaying ? startPosition : transform.position;
        float distance = Mathf.Abs(patrolDistance);
        Vector3 left = new Vector3(pos.x - distance, pos.y, pos.z);
        Vector3 right = new Vector3(pos.x + distance, pos.y, pos.z);

        // Patrol range
        Gizmos.color = Color.red;
        Gizmos.DrawLine(left, right);
        Gizmos.DrawSphere(left, 0.1f);
        Gizmos.DrawSphere(right, 0.1f);

        // Ground check rays
        if (avoidFalling)
        {
            int dir = Application.isPlaying ? direction : 1;
            Vector3 rayOrigin = new Vector3(transform.position.x + (dir * groundCheckOffset), transform.position.y, transform.position.z);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * groundCheckDistance);
            Gizmos.DrawSphere(rayOrigin, 0.1f);
        }

        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
