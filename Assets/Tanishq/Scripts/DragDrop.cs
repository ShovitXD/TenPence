using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragDrop : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask draggableMask = ~0;
    [SerializeField] private float maxPickDistance = 500f;

    [Header("Movement")]
    [SerializeField] private float followSpeed = 30f;
    [SerializeField] private float castRadiusPadding = 0.02f;

    private Rigidbody grabbedRb;
    private Collider grabbedCol;

    private float grabDistance;
    private Vector3 grabOffset;

    private bool prevKinematic;
    private bool prevUseGravity;
    private bool prevFreezeRotation;

    private float castRadius;

    private void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    private void Update()
    {
        if (!cam) return;

        if (PointerPressedThisFrame())
            TryGrab();

        if (PointerReleasedThisFrame())
            DropAndRelease();
    }

    private void FixedUpdate()
    {
        if (!grabbedRb) return;

        Ray ray = cam.ScreenPointToRay(GetPointerScreenPosition());
        Vector3 desired = ray.GetPoint(grabDistance) + grabOffset;

        Vector3 current = grabbedRb.position;
        Vector3 step = Vector3.Lerp(current, desired, 1f - Mathf.Exp(-followSpeed * Time.fixedDeltaTime));
        Vector3 moveDelta = step - current;

        float dist = moveDelta.magnitude;
        if (dist > 0.0001f)
        {
            Vector3 dir = moveDelta / dist;

            // Collision-aware drag movement.
            if (Physics.SphereCast(current, castRadius, dir, out RaycastHit hit, dist, ~0, QueryTriggerInteraction.Ignore))
                step = hit.point - dir * castRadius;
        }

        grabbedRb.MovePosition(step);
    }

    private void TryGrab()
    {
        if (grabbedRb) return;

        Ray ray = cam.ScreenPointToRay(GetPointerScreenPosition());

        if (!Physics.Raycast(ray, out RaycastHit hit, maxPickDistance, draggableMask, QueryTriggerInteraction.Ignore))
            return;

        Rigidbody rb = hit.rigidbody;
        if (!rb) return;

        // Tags gate what can be dragged.
        bool isIngredient = rb.CompareTag("Ingredients");
        bool isSandwichStack = rb.CompareTag("SandwichStack");
        if (!isIngredient && !isSandwichStack) return;

        DraggableState state = rb.GetComponentInParent<DraggableState>();
        if (state != null && !state.CanDrag) return;

        Collider col = rb.GetComponent<Collider>();
        if (!col) return;

        grabbedRb = rb;
        grabbedCol = col;

        grabDistance = hit.distance;
        grabOffset = rb.position - hit.point;

        castRadius = Mathf.Max(0.01f, grabbedCol.bounds.extents.magnitude * 0.25f) + castRadiusPadding;

        prevKinematic = grabbedRb.isKinematic;
        prevUseGravity = grabbedRb.useGravity;
        prevFreezeRotation = grabbedRb.freezeRotation;

        grabbedRb.isKinematic = false;
        grabbedRb.useGravity = false;
        grabbedRb.freezeRotation = true;
        grabbedRb.linearVelocity = Vector3.zero;
        grabbedRb.angularVelocity = Vector3.zero;
    }

    private void DropAndRelease()
    {
        if (!grabbedRb) return;

        bool dropHandled = false;

        // Raycast on release to find a drop target. Triggers are allowed for drop targets.
        Ray ray = cam.ScreenPointToRay(GetPointerScreenPosition());
        if (Physics.Raycast(ray, out RaycastHit hit, maxPickDistance, ~0, QueryTriggerInteraction.Collide))
        {
            IDropTarget target = hit.collider.GetComponentInParent<IDropTarget>();
            if (target != null && target.CanAccept(grabbedRb.gameObject))
            {
                target.Accept(grabbedRb.gameObject);
                dropHandled = true;
            }
        }

        // If a target handled the drop, it owns the rigidbody state now.
        if (!dropHandled)
        {
            grabbedRb.freezeRotation = prevFreezeRotation;
            grabbedRb.useGravity = prevUseGravity;
            grabbedRb.isKinematic = prevKinematic;
        }

        grabbedRb = null;
        grabbedCol = null;
    }

    private static Vector2 GetPointerScreenPosition()
    {
        if (Mouse.current != null) return Mouse.current.position.ReadValue();
        if (Touchscreen.current != null) return Touchscreen.current.primaryTouch.position.ReadValue();
        return Vector2.zero;
    }

    private static bool PointerPressedThisFrame()
    {
        if (Mouse.current != null) return Mouse.current.leftButton.wasPressedThisFrame;
        if (Touchscreen.current != null) return Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
        return false;
    }

    private static bool PointerReleasedThisFrame()
    {
        if (Mouse.current != null) return Mouse.current.leftButton.wasReleasedThisFrame;
        if (Touchscreen.current != null) return Touchscreen.current.primaryTouch.press.wasReleasedThisFrame;
        return false;
    }
}
