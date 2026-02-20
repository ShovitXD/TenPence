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
        bool isIngredient = rb.CompareTag("Pick");
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

        // The naive approach (raycast from mouse on release) usually hits the *thing you're holding* first,
        // so the sandwich zone never receives the drop.
        // Fix: search for nearby drop targets around the held object, then fall back to a filtered raycast.

        if (TryFindNearbyDropTarget(out IDropTarget nearby) && nearby.CanAccept(grabbedRb.gameObject))
        {
            nearby.Accept(grabbedRb.gameObject);
            dropHandled = true;
        }
        else if (TryRaycastDropTarget(out IDropTarget rayTarget) && rayTarget.CanAccept(grabbedRb.gameObject))
        {
            rayTarget.Accept(grabbedRb.gameObject);
            dropHandled = true;
        }

        // Restore rigidbody state unless the drop target "locked" the object (typically sets isKinematic = true).
        // Example: SandwichStackZone locks ingredients into the stack.
        if (!dropHandled || (grabbedRb != null && !grabbedRb.isKinematic))
        {
            grabbedRb.freezeRotation = prevFreezeRotation;
            grabbedRb.useGravity = prevUseGravity;
            grabbedRb.isKinematic = prevKinematic;
        }

        grabbedRb = null;
        grabbedCol = null;
    }

    private bool TryFindNearbyDropTarget(out IDropTarget bestTarget)
    {
        bestTarget = null;
        if (!grabbedRb || !grabbedCol) return false;

        Vector3 pos = grabbedRb.worldCenterOfMass;

        // Make this generously sized so "hovering" slightly above the zone still counts.
        float radius = Mathf.Max(0.15f, grabbedCol.bounds.extents.magnitude + 0.06f);

        Collider[] hits = Physics.OverlapSphere(pos, radius, ~0, QueryTriggerInteraction.Collide);
        if (hits == null || hits.Length == 0) return false;

        float bestDist = float.PositiveInfinity;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider c = hits[i];
            if (!c) continue;

            // Ignore any collider belonging to the grabbed object.
            if (c == grabbedCol) continue;
            if (c.attachedRigidbody == grabbedRb) continue;
            if (c.transform.IsChildOf(grabbedRb.transform)) continue;

            IDropTarget t = c.GetComponentInParent<IDropTarget>();
            if (t == null) continue;
            if (!t.CanAccept(grabbedRb.gameObject)) continue;

            float d = Vector3.Distance(c.ClosestPoint(pos), pos);
            if (d < bestDist)
            {
                bestDist = d;
                bestTarget = t;
            }
        }

        return bestTarget != null;
    }

    private bool TryRaycastDropTarget(out IDropTarget bestTarget)
    {
        bestTarget = null;
        if (!cam || !grabbedRb) return false;

        Ray ray = cam.ScreenPointToRay(GetPointerScreenPosition());
        RaycastHit[] hits = Physics.RaycastAll(ray, maxPickDistance, ~0, QueryTriggerInteraction.Collide);
        if (hits == null || hits.Length == 0) return false;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        for (int i = 0; i < hits.Length; i++)
        {
            Collider c = hits[i].collider;
            if (!c) continue;

            // Ignore the grabbed object itself.
            if (grabbedCol != null && (c == grabbedCol || c.attachedRigidbody == grabbedRb || c.transform.IsChildOf(grabbedRb.transform)))
                continue;

            IDropTarget t = c.GetComponentInParent<IDropTarget>();
            if (t == null) continue;

            bestTarget = t;
            return true;
        }

        return false;
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