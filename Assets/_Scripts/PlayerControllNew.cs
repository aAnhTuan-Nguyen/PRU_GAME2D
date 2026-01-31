//using UnityEngine;
//using UnityEngine.InputSystem;

//public class PlayerControllNew : MonoBehaviour
//{
//    private PlayerControls controls;
//    private Vector2 moveInput;

//    void Awake()
//    {
//        controls = new PlayerControls(); // class này Unity tự sinh ra từ file .inputactions
//    }

//    void OnEnable()
//    {
//        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
//        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
//        controls.Enable();
//    }

//    void OnDisable()
//    {
//        controls.Disable();
//    }

//    void Update()
//    {
//        Debug.Log("Move input: " + moveInput); // kiểm tra input có nhận không
//        transform.Translate(new Vector3(moveInput.x, 0, moveInput.y) * Time.deltaTime * 5f);
//    }
//}
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllNew : MonoBehaviour
{
    private Vector2 moveInput;
    private PlayerInput playerInput;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.actions.Disable(); // tắt tất cả
        playerInput.actions.FindActionMap("Player").Enable(); // bật đúng map
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        Debug.Log("Move input: " + moveInput);
    }

    void Update()
    {
        transform.Translate(new Vector3(moveInput.x, 0, moveInput.y) * Time.deltaTime * 5f);
    }
}