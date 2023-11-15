using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Functional Options")] [SerializeField]
    private bool canSprint = true;

    [SerializeField] private bool canJump = true;

    [Header("Controls")] [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    [Header("Movement Parameters")] [SerializeField]
    private float walkSpeed = 3.0f;

    [SerializeField] private float sprintSpeed = 6.0f;

    [Header("Look Parameter")] [SerializeField, Range(1, 10)]
    private float horizontalLookSpeed = 2.0f;

    [SerializeField, Range(1, 10)] private float verticalLookSpeed = 2.0f;
    [SerializeField, Range(1, 180)] private float maxVerticalLookAngle = 80.0f;
    [SerializeField, Range(1, 180)] private float minVerticalLookAngle = 80.0f;

    [Header("Jumping Parameters")] [SerializeField]
    private float jumpForce = 8.0f;

    [SerializeField] private float gravity = 30.0f;
    private CharacterController _characterController;
    private Vector2 _currentInput;

    private Vector3 _moveDirection;

    private Camera _playerCamera;

    private float _rotationX;
    private bool CanMove { get; set; } = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    private bool ShouldJump => Input.GetKeyDown(jumpKey) && _characterController.isGrounded;

    // Start is called before the first frame update
    private void Start()
    {
        _playerCamera = GetComponentInChildren<Camera>();
        _characterController = GetComponent<CharacterController>();
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
        _currentInput = new Vector2((IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"),
            (IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal"));

        var moveDirectionY = _moveDirection.y;
        _moveDirection = (transform.TransformDirection(Vector3.forward) * _currentInput.x) +
                         (transform.TransformDirection(Vector3.right) * _currentInput.y);
        _moveDirection.y = moveDirectionY;
    }

    private void HandleMouseLook()
    {
        _rotationX -= Input.GetAxis("Mouse Y") * verticalLookSpeed;
        _rotationX = Mathf.Clamp(_rotationX, -maxVerticalLookAngle, minVerticalLookAngle);
        _playerCamera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * horizontalLookSpeed, 0);
    }

    private void HandleJump()
    {
        if (ShouldJump)
            _moveDirection.y = jumpForce;
    }

    private void ApplyFinalMovements()
    {
        if (!_characterController.isGrounded)
            _moveDirection.y -= gravity * Time.deltaTime;

        _characterController.Move(_moveDirection * Time.deltaTime);
    }
}