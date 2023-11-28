using System.Collections;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Functional Options")] [SerializeField]
    private bool canSprint = true;

    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canUseHeadbob = true;

    [Header("Controls")] [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Movement Parameters")] [SerializeField]
    private float walkSpeed = 3.0f;

    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;

    [Header("Look Parameters")] [SerializeField] [Range(1, 10)]
    private float lookSpeedX = 2.0f;

    [SerializeField] [Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField] [Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField] [Range(1, 180)] private float lowerLookLimit = -80.0f;

    [Header("Jumping Parameters")] [SerializeField]
    private float jumpForce = 8.0f;

    [SerializeField] private float gravity = 30.0f;

    [Header("Crouch Parameters")] [SerializeField]
    private float crouchHeight = 0.5f;

    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new(0, 0, 0);

    [Header("Headbob Parameters")] [SerializeField]
    private float walkBobSpeed = 14f;

    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.1f;
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    private CharacterController _characterController;
    private Vector2 _currentInput;
    private float _defaultYPos;
    private bool _duringCrouchAnimation;
    private bool _isCrouching;

    private Vector3 _moveDirection;

    private Camera _playerCamera;

    private float _rotationX;
    private float _timer;
    private bool CanMove { get; set; } = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    private bool ShouldJump => Input.GetKeyDown(jumpKey) && _characterController.isGrounded;

    private bool ShouldCrouch =>
        Input.GetKeyDown(crouchKey) && !_duringCrouchAnimation && _characterController.isGrounded;

    private void Start()
    {
        _playerCamera = GetComponentInChildren<Camera>();
        _characterController = GetComponent<CharacterController>();
        _defaultYPos = _playerCamera.transform.localPosition.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!CanMove) return;
        HandleMovementInput();
        HandleMouseLook();

        if (canJump)
            HandleJump();

        if (canCrouch)
            HandleCrouch();

        if (canUseHeadbob)
            HandleHeadbob();

        ApplyFinalMovements();
    }

    private void HandleMovementInput()
    {
        _currentInput =
            new Vector2(
                (_isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"),
                (_isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal"));
        var moveDirectionY = _moveDirection.y;
        var transformForward = transform.TransformDirection(Vector3.forward);
        var transformRight = transform.TransformDirection(Vector3.right);
        _moveDirection = transformForward * _currentInput.x + transformRight * _currentInput.y;
        _moveDirection.y = moveDirectionY;
    }

    private void HandleMouseLook()
    {
        _rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        _rotationX = Mathf.Clamp(_rotationX, -upperLookLimit, lowerLookLimit);
        _playerCamera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }

    private void HandleJump()
    {
        if (ShouldJump)
            _moveDirection.y = jumpForce;
    }

    private void HandleCrouch()
    {
        if (ShouldCrouch)
            StartCoroutine(CrouchStand());
    }

    private void ApplyFinalMovements()
    {
        if (!_characterController.isGrounded)
            _moveDirection.y -= gravity * Time.deltaTime;

        _characterController.Move(_moveDirection * Time.deltaTime);
    }

    private void HandleHeadbob()
    {
        if (!_characterController.isGrounded) return;

        if (!(Mathf.Abs(_moveDirection.x) > 0.1f) && !(Mathf.Abs(_moveDirection.z) > 0.1f)) return;
        _timer += Time.deltaTime * (_isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : walkBobSpeed);

        var cameraTransform = _playerCamera.transform;
        cameraTransform.localPosition = new Vector3(
            cameraTransform.localPosition.x,
            _defaultYPos + Mathf.Sin(_timer) *
            (_isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount : walkBobAmount),
            _playerCamera.transform.localPosition.z);
    }

    private IEnumerator CrouchStand()
    {
        _duringCrouchAnimation = true;

        float timeElapsed = 0;
        var targetHeight = _isCrouching ? standingHeight : crouchHeight;
        var currentHeight = _characterController.height;
        var targetCenter = _isCrouching ? standingCenter : crouchingCenter;
        var currentCenter = _characterController.center;

        while (timeElapsed < timeToCrouch)
        {
            _characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            _characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        _characterController.height = targetHeight;
        _characterController.center = targetCenter;

        _isCrouching = !_isCrouching;

        _duringCrouchAnimation = false;
    }
}