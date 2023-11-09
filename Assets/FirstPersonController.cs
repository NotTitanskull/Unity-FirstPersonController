using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Parameters")] [SerializeField]
    private float walkSpeed = 3.0f;

    [SerializeField] private float gravity = 30.0f;

    [Header("Look Parameters")] [SerializeField, Range(1, 10)]
    private float horizontalLookSpeed = 2.0f;

    [SerializeField, Range(1, 10)] private float verticalLookSpeed = 2.0f;
    [SerializeField, Range(1, 180)] private float maxVerticalLookAngle = 80.0f;
    [SerializeField, Range(1, 180)] private float minVerticalLookAngle = 80.0f;
    private CharacterController characterController;

    private Camera playerCamera;
    public bool CanMove { get; private set; } = true;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}