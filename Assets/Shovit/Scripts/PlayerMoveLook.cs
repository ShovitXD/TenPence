using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMoveLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform body;
    [SerializeField] private Transform cameraPivot;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 0.08f;
    [SerializeField] private float gamepadSensitivity = 140f;
    [SerializeField] private float minPitch = -85f;
    [SerializeField] private float maxPitch = 85f;

    private CharacterController controller;
    private PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction lookAction;

    private Vector3 velocity;
    private float pitch;
    private float yaw;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        if (body == null) body = transform;
        if (cameraPivot == null) cameraPivot = body;

        // SAFE lookup
        moveAction = playerInput.actions.FindAction("Move", false);
        lookAction = playerInput.actions.FindAction("Look", false);

        if (moveAction == null) Debug.LogError("Missing InputAction 'Move' in the active action map.");
        if (lookAction == null) Debug.LogError("Missing InputAction 'Look' in the active action map.");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = body.eulerAngles.y;

        pitch = cameraPivot.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
    }

    private void OnEnable()
    {
        if (moveAction != null) moveAction.Enable();
        if (lookAction != null) lookAction.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.Disable();
        if (lookAction != null) lookAction.Disable();
    }

    private void Update()
    {
        HandleLook();
        HandleMove();
        HandleGravity();
    }

    private void HandleMove()
    {
        if (moveAction == null) return;

        Vector2 move = moveAction.ReadValue<Vector2>();
        Vector3 moveWorld = (body.right * move.x) + (body.forward * move.y);
        controller.Move(moveWorld * (moveSpeed * Time.deltaTime));
    }

    private void HandleLook()
    {
        if (lookAction == null || body == null || cameraPivot == null) return;

        Vector2 look = lookAction.ReadValue<Vector2>();

        bool isMouse = playerInput.currentControlScheme != null &&
                       playerInput.currentControlScheme.ToLower().Contains("mouse");

        float yawDelta = isMouse ? look.x * mouseSensitivity : look.x * gamepadSensitivity * Time.deltaTime;
        float pitchDelta = isMouse ? look.y * mouseSensitivity : look.y * gamepadSensitivity * Time.deltaTime;

        yaw += yawDelta;
        pitch -= pitchDelta;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        body.rotation = Quaternion.Euler(0f, yaw, 0f);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleGravity()
    {
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
