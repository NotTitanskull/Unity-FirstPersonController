using System.Numerics;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public bool CanMove { get; private set; } = true;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float gravity = 30.0f;

    [Header("Look Parameter")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;

    private Camera playerCamera;
    private CharacterController characterController;

    private UnityEngine.Vector3 moveDirection;
    private UnityEngine.Vector2 currentInput;

    private float rotationX = 0;

    // Start is called before the first frame update
    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (CanMove)
        {
            HandleMovementInput();
            HandleMouseLook();

            ApplyFinalMovements();
        }
    }

    private void HandleMovementInput()
    {
        currentInput = new UnityEngine.Vector2(walkSpeed * Input.GetAxis("Vertical"), walkSpeed * Input.GetAxis("Horizontal"));

        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(UnityEngine.Vector3.forward) * currentInput.x) + (transform.TransformDirection(UnityEngine.Vector3.right) * currentInput.y);
        moveDirection.y = moveDirectionY;
    }

    private void HandleMouseLook()
    {

    }

    private void ApplyFinalMovements()
    {
        if(!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);
    }
}
