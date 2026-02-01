using Unity.VisualScripting;
using UnityEngine;

public class PlayerControll : MonoBehaviour
{
    public Animator animator;
    Rigidbody2D rb;
    public float jumpForce = 5f;
    private bool isGrounded;

    private float movement;
    public float moveSpeed = 5f;
    private bool directionRight = true;

    [Header("Combat")]
    public int attackDamage = 1;
    public float attackRange = 1.5f;
    public Transform attackPoint;
    public LayerMask enemyLayers;

    [Header("Health")]
    public int maxHealth = 5;
    private int currentHealth;
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

        movement = Input.GetAxis("Horizontal");
        //// new input system
        //movement = UnityEngine.InputSystem.Keyboard.current.dKey.isPressed ? 1 : UnityEngine.InputSystem.Keyboard.current.aKey.isPressed ? -1 : 0;

        if (movement < 0 && directionRight) Flip();

        else if (movement > 0 && !directionRight) Flip();

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && isGrounded)
        {
            Jump(jumpForce);
            isGrounded = false;
            animator.SetBool("isJumping", true);
        }

        // animate idle to running or running to idle
        isRunning(movement);

        // Kiểm tra nhấn chuột trái
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
        {
            transform.Translate(new Vector3(movement, 0, 0) * Time.fixedDeltaTime * moveSpeed);
        }
    }

    // Flip the player sprite based on movement direction
    void Flip()
    {
        directionRight = !directionRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // Jump
    public void Jump(float jumpForce)
    {
        rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
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

    private void isRunning(float movement)
    {
        if (movement > 0 || movement < 0)
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
    }

    void Attack()
    {
        if (attackPoint == null) return;

        // Detect enemies in range of attack
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // Damage them
        foreach (Collider2D enemy in hitEnemies)
        {
            // Đánh PatrolEnemy
            PatrolEnemy patrolEnemy = enemy.GetComponent<PatrolEnemy>();
            if (patrolEnemy != null)
            {
                patrolEnemy.TakeDamage(attackDamage);
                continue;
            }

            // Đánh BatEnemy
            BatEnemy batEnemy = enemy.GetComponent<BatEnemy>();
            if (batEnemy != null)
            {
                batEnemy.TakeDamage(attackDamage);
            }
        }
    }

    // Enemy gọi hàm này để gây damage cho player
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Player bị đánh! Máu còn: {currentHealth}/{maxHealth}");

        // animator.SetTrigger("Hurt"); // Nếu có animation bị đau

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Player chết!");

        // animator.SetTrigger("Death"); // Nếu có animation chết
        rb.linearVelocity = Vector2.zero;

        Invoke(nameof(RestartGame), 2f);
    }

    void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
