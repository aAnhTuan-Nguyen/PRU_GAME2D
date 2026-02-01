using UnityEngine;

public class BatEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float flySpeed = 2f;         
    public float flyRadius = 2f;       
    public float chaseSpeed = 3f;      

    [Header("Combat")]
    public float detectRange = 5f;    
    public float attackRange = 2f;      
    public float attackCooldown = 1.5f; 
    public int attackDamage = 1;       
    public int health = 2;              

    [Header("Flying Settings")]
    public float minHeightAbovePlayer = 1.5f;
    public float stopDistance = 1.5f;           // Khoảng cách dừng lại (không lao vào player)

    [Header("References")]
    public Animator animator;

    [Header("Sprite Settings")]
    public bool spriteDefaultFacingLeft = false; // Tick nếu sprite mặc định quay trái

    // Private
    private Transform player;
    private Vector3 startPosition;
    private float flyAngle = 0f;
    private float lastAttackTime;
    private int attackComboCount = 0;
    private bool isDead = false;
    private bool isAttacking = false;

    void Start()
    {
        startPosition = transform.position;

        if (animator == null)
            animator = GetComponent<Animator>();

        // Tìm player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            // Trong tầm đánh -> hover + tấn công
            FacePlayer();
            HoverAbovePlayer();

            if (Time.time >= lastAttackTime + attackCooldown && !isAttacking)
                AttackPlayer();
        }
        else if (distance <= detectRange)
        {
            ChasePlayer();
        }
        else
        {
            FlyAround();
        }
    }

    // Bay vòng tròn quanh vị trí ban đầu
    void FlyAround()
    {
        flyAngle += flySpeed * Time.deltaTime;
        float x = startPosition.x + Mathf.Cos(flyAngle) * flyRadius;
        float y = startPosition.y + Mathf.Sin(flyAngle) * flyRadius * 0.5f;

        Vector3 target = new Vector3(x, y, 0);
        FaceDirection(target.x < transform.position.x ? -1 : 1);
        transform.position = Vector3.MoveTowards(transform.position, target, flySpeed * Time.deltaTime);
    }

    // Đuổi theo player nhưng giữ khoảng cách và độ cao
    void ChasePlayer()
    {
        FacePlayer();

        // Mục tiêu: phía trên đầu player
        Vector3 target = new Vector3(player.position.x, player.position.y + minHeightAbovePlayer, 0);

        // Nếu đã gần đủ theo chiều ngang -> chỉ điều chỉnh độ cao
        float horizontalDist = Mathf.Abs(transform.position.x - player.position.x);
        if (horizontalDist <= stopDistance)
            target.x = transform.position.x;

        transform.position = Vector3.MoveTowards(transform.position, target, chaseSpeed * Time.deltaTime);
    }

    // Hover tại chỗ phía trên player khi tấn công
    void HoverAbovePlayer()
    {
        float targetY = player.position.y + minHeightAbovePlayer;
        Vector3 pos = transform.position;
        pos.y = Mathf.MoveTowards(pos.y, targetY, chaseSpeed * Time.deltaTime);
        transform.position = pos;
    }

    void FacePlayer()
    {
        FaceDirection(player.position.x < transform.position.x ? -1 : 1);
    }

    void FaceDirection(int dir)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (spriteDefaultFacingLeft ? -dir : dir);
        transform.localScale = scale;
    }

    void AttackPlayer()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        attackComboCount++;

        // Luân phiên Attack1 / Attack2
        if (animator != null)
            animator.SetTrigger(attackComboCount % 2 == 1 ? "Attack1" : "Attack2");

        Invoke(nameof(DealDamage), 0.3f);
        Invoke(nameof(EndAttack), 0.5f);
    }

    void DealDamage()
    {
        if (isDead || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackRange * 1.5f)
        {
            PlayerControll p = player.GetComponent<PlayerControll>();
            if (p != null) p.TakeDamage(attackDamage);
        }
    }

    void EndAttack() => isAttacking = false;

    // Gọi từ PlayerControll khi bị đánh
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        if (animator != null) animator.SetTrigger("Hurt");

        if (health <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        if (animator != null) animator.SetTrigger("Die");

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 1.5f);
    }

    // Gizmos hiển thị phạm vi trong Scene
    void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? startPosition : transform.position;

        Gizmos.color = Color.cyan;   // Vòng bay
        Gizmos.DrawWireSphere(center, flyRadius);

        Gizmos.color = Color.yellow; // Phạm vi phát hiện
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;    // Phạm vi tấn công
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
