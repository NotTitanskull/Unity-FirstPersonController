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
    private CharacterController characterController;
    private Vector2 currentInput;
    private float defaultYPos = 0;
    private bool duringCrouchAnimation;
    private bool isCrouching;

    private Vector3 moveDirection;

    private Camera playerCamera;

    private float rotationX;
    private float timer;
    public bool CanMove { get; } = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    private bool ShouldJump => Input.GetKeyDown(jumpKey) && characterController.isGrounded;

    private bool ShouldCrouch =>
        Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && characterController.isGrounded;

    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        defaultYPos = playerCamera.transform.localPosition.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (CanMove)
        {
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
    }

    private void HandleMovementInput()
    {
        currentInput =
            new Vector2((isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"),
                (isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal"));
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

    private void HandleCrouch()
    {
        if (ShouldCrouch)
            StartCoroutine(CrouchStand());
    }

    private void ApplyFinalMovements()
    {
        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleHeadbob()
    {
        if (!characterController.isGrounded) return;

        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) *
                (isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount : walkBobAmount),
                playerCamera.transform.localPosition.z);
        }
    }

    private IEnumerator CrouchStand()
    {
        duringCrouchAnimation = true;

        float timeElapsed = 0;
        var targetHeight = isCrouching ? standingHeight : crouchHeight;
        var currentHeight = characterController.height;
        var targetCenter = isCrouching ? standingCenter : crouchingCenter;
        var currentCenter = characterController.center;

        while (timeElapsed < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchAnimation = false;
    }
}