using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Functional Options")] [SerializeField]
    private bool canSprint = true;

    [Header("Controls")] [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Movement Parameters")] [SerializeField]
    private float walkSpeed = 3.0f;

    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float gravity = 30.0f;

    [Header("Look Parameter")] [SerializeField] [Range(1, 10)]
    private float lookSpeedX = 2.0f;

    [SerializeField] [Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField] [Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField] [Range(1, 180)] private float lowerLookLimit = 80.0f;

    [Header("Jumping Parameters")] [SerializeField]
    private float jumpForce = 8.0f;

    private readonly bool canJump = true;
    private readonly KeyCode jumpKey = KeyCode.Space;

    private CharacterController characterController;
    private Vector2 currentInput;

    private Vector3 moveDirection;

    private Camera playerCamera;

    private float rotationX;
    private bool CanMove => true;

    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);

    private bool ShouldJump => Input.GetKeyDown(jumpKey) && characterController.isGrounded;

    // Start is called before the first frame update
    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!CanMove) return;
        HandleMovementInput();
        HandleMouseLook();

        if (canJump)
            HandleJump();

        ApplyFinalMovements();
    }

    private void HandleMovementInput()
    {
        currentInput = new Vector2((IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"),
            (IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal"));

        var moveDirectionY = moveDirection.y;
        moveDirection = transform.TransformDirection(Vector3.forward) * currentInput.x +
                        transform.TransformDirection(Vector3.right) * currentInput.y;
        moveDirection.y = moveDirectionY;
    }

    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }

    private void HandleJump()
    {
        if (ShouldJump)
            moveDirection.y = jumpForce;
    }

    private void ApplyFinalMovements()
    {
        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);
    }
}