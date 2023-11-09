using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Parameters")] [SerializeField]
    private float walkSpeed = 3.0f;

    [SerializeField] private float gravity = 30.0f;

    [Header("Look Parameters")] [SerializeField] [Range(1, 10)]
    private float horizontalLookSpeed = 2.0f;

    [SerializeField] [Range(1, 10)] private float verticalLookSpeed = 2.0f;
    [SerializeField] [Range(1, 180)] private float maxVerticalLookAngle = 80.0f;
    [SerializeField] [Range(1, 180)] private float minVerticalLookAngle = 80.0f;
    private CharacterController characterController;
    private Vector2 currentInput;

    private Vector3 moveDirection;

    private Camera playerCamera;

    private float rotationX;
    private static bool IsMovementEnabled => true;

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
        if (!IsMovementEnabled) return;
        HandleMovementInput();
        HandleMouseLook();

        ApplyFinalMovements();
    }

    private void HandleMovementInput()
    {
        currentInput = new Vector2(walkSpeed * Input.GetAxis("Vertical"), walkSpeed * Input.GetAxis("Horizontal"));

        var moveDirectionY = moveDirection.y;
        moveDirection = transform.TransformDirection(Vector3.forward) * currentInput.x +
                        transform.TransformDirection(Vector3.right) * currentInput.y;
        moveDirection.y = moveDirectionY;
    }

    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * verticalLookSpeed;
        rotationX = Mathf.Clamp(rotationX, -maxVerticalLookAngle, minVerticalLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * horizontalLookSpeed, 0);
    }

    private void ApplyFinalMovements()
    {
        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);
    }
}