using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerPickupHands : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform HandL;
    [SerializeField] private Transform HandR;

    [Header("Pickup")]
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private LayerMask pickupMask = ~0;
    [SerializeField] private bool disablePhysicsWhileHeld = true;

    private PlayerInput playerInput;

    private InputAction interactAction;
    private InputAction dropLAction;
    private InputAction dropRAction;

    private LookHighlight currentHighlight;
    private Transform currentLookTarget;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        interactAction = playerInput.actions.FindAction("Interact", false);
        dropLAction = playerInput.actions.FindAction("DropL", false);
        dropRAction = playerInput.actions.FindAction("DropR", false);
    }

    private void OnEnable()
    {
        if (playerInput != null && playerInput.actions != null)
            playerInput.actions.Enable();

        interactAction?.Enable();
        dropLAction?.Enable();
        dropRAction?.Enable();
    }

    private void Update()
    {
        UpdateLookHighlight();

        if (interactAction != null && interactAction.WasPressedThisFrame())
            TryPickup();

        if (dropLAction != null && dropLAction.WasPressedThisFrame())
            DropFromHand(HandL);

        if (dropRAction != null && dropRAction.WasPressedThisFrame())
            DropFromHand(HandR);
    }

    private void UpdateLookHighlight()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupMask, QueryTriggerInteraction.Ignore))
        {
            Transform t = hit.collider.transform;

            if (HandL != null && t.IsChildOf(HandL)) { ClearHighlight(); return; }
            if (HandR != null && t.IsChildOf(HandR)) { ClearHighlight(); return; }

            if (t != currentLookTarget)
            {
                ClearHighlight();

                currentLookTarget = t;
                currentHighlight = t.GetComponent<LookHighlight>() ?? t.GetComponentInParent<LookHighlight>();
                currentHighlight?.SetHighlighted(true);
            }
        }
        else
        {
            ClearHighlight();
        }
    }

    private void ClearHighlight()
    {
        currentHighlight?.SetHighlighted(false);
        currentHighlight = null;
        currentLookTarget = null;
    }

    private void TryPickup()
    {
        if (playerCamera == null || HandL == null || HandR == null) return;

        Transform targetHand = null;
        if (HandL.childCount == 0) targetHand = HandL;
        else if (HandR.childCount == 0) targetHand = HandR;
        else return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupMask, QueryTriggerInteraction.Ignore))
            return;

        Transform picked = hit.collider.transform;

        if (picked.IsChildOf(HandL) || picked.IsChildOf(HandR))
            return;

        AttachToHand(picked, targetHand);
        ClearHighlight();
    }

    private void AttachToHand(Transform item, Transform hand)
    {
        if (disablePhysicsWhileHeld)
            SetHeldPhysics(item, held: true);

        item.SetParent(hand, worldPositionStays: false);
        item.localPosition = Vector3.zero;
        item.localRotation = Quaternion.identity;
    }

    private void DropFromHand(Transform hand)
    {
        if (hand == null || hand.childCount == 0) return;

        Transform item = hand.GetChild(0);
        item.SetParent(null, worldPositionStays: true);

        if (disablePhysicsWhileHeld)
            SetHeldPhysics(item, held: false);
    }

    private void SetHeldPhysics(Transform item, bool held)
    {
        if (item.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = held;
            rb.useGravity = !held;

            if (held)
            {
#if UNITY_2023_1_OR_NEWER
                rb.linearVelocity = Vector3.zero;
#else
                rb.velocity = Vector3.zero;
#endif
                rb.angularVelocity = Vector3.zero;
            }
        }

        if (item.TryGetComponent<Collider>(out var col))
            col.enabled = !held;
    }
}
