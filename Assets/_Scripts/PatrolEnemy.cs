using UnityEngine;

public class PatrolEnemy : MonoBehaviour
{
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
    public float groundCheckDistance = 0.5f;

    private Vector3 startPosition;
    private float leftLimit;
    private float rightLimit;
    private int direction = 1; // 1 = right, -1 = left
    private bool facingRight = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
        leftLimit = startPosition.x - Mathf.Abs(patrolDistance);
        rightLimit = startPosition.x + Mathf.Abs(patrolDistance);
    }

    // Update is called once per frame
    void Update()
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

    bool IsGroundAhead()
    {
        // Cast a ray downward from a point slightly ahead of the enemy
        Vector2 rayOrigin = new Vector2(transform.position.x + (direction * 0.5f), transform.position.y);
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

    // Draw patrol range in the Scene view for easier tweaking
    void OnDrawGizmosSelected()
    {
        Vector3 pos = Application.isPlaying ? startPosition : transform.position;
        float distance = Mathf.Abs(patrolDistance);
        Vector3 left = new Vector3(pos.x - distance, pos.y, pos.z);
        Vector3 right = new Vector3(pos.x + distance, pos.y, pos.z);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(left, right);
        Gizmos.DrawSphere(left, 0.1f);
        Gizmos.DrawSphere(right, 0.1f);

        // Draw ground check rays
        if (avoidFalling)
        {
            int dir = Application.isPlaying ? direction : 1;
            Vector3 rayOrigin = new Vector3(transform.position.x + (dir * 0.5f), transform.position.y, transform.position.z);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * groundCheckDistance);
        }
    }
}
