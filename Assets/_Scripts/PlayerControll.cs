using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerControll : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("Combat")]
    public int attackDamage = 1;
    public float attackRange = 1.5f;
    public Transform attackPoint;
    public LayerMask enemyLayers;

    [Header("Health")]
    public int maxHealth = 5;

    [Header("References")]
    public Animator animator;

    // Private
    private Rigidbody2D rb;
    private int currentHealth;
    private float movement;
    private bool isGrounded;
    private bool directionRight = true;
    private bool isDead = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;

        // Movement input
        movement = Input.GetAxis("Horizontal");

        // Flip
        if (movement < 0 && directionRight) Flip();
        else if (movement > 0 && !directionRight) Flip();

        // Jump
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            animator.SetBool("isJumping", true);
        }

        // Animation
        animator.SetBool("isRunning", movement != 0);

        // Attack
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("isAttack");
            // Call attack after a short delay to match animation
            Invoke(nameof(Attack), 0.2f);
        }
    }

    private void FixedUpdate()
    {
        if (!isDead)
            transform.Translate(Vector3.right * movement * moveSpeed * Time.fixedDeltaTime);
    }

    // Flip the player sprite based on movement direction
    void Flip()
    {
        directionRight = !directionRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void Attack()
    {
        if (attackPoint == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (var hit in hits)
        {
            // Thử tất cả loại enemy
            hit.GetComponent<PatrolEnemy>()?.TakeDamage(attackDamage);
            hit.GetComponent<BatEnemy>()?.TakeDamage(attackDamage);
            hit.GetComponent<GolemEnemy>()?.TakeDamage(attackDamage);
        }
    }

    // Enemy gọi hàm này để gây damage cho player
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Player HP: {currentHealth}/{maxHealth}");

        // animator.SetTrigger("Hurt"); // Nếu có animation bị đau

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        // animator.SetTrigger("Death"); // Nếu có animation chết
        Invoke(nameof(Restart), 2f);
    }

    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("Collided with: " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.SetBool("isJumping", false);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
