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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // input old
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
            animator.SetTrigger("isAttack" );
        }


    }

    private void FixedUpdate()
    {
        transform.Translate(new Vector3(movement, 0, 0) * Time.fixedDeltaTime * moveSpeed);
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
}
