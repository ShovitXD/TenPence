using UnityEngine;
using UnityEngine.InputSystem;

public class DragDrop : MonoBehaviour
{
    [Header("Required Refferences")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask draggableMask = ~0;
    [SerializeField] private LayerMask sandwichZoneMask = 0;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference pointAction;
    [SerializeField] private InputActionReference clickAction;

    [Header("Rigidbody handling")]
    [SerializeField] private bool moveRigidbodyIfPresent = true;

    [Header("On Place In Sandwich Zone")]
    [SerializeField] private bool makeKinematicWhenPlaced = true;
    [SerializeField] private bool disableCollidersWhenPlaced = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private Transform dragged;
    private Rigidbody draggedRigidbody;
    private Ingredient draggedIngredient;

    private Plane dragPlane;
    private Vector3 grabOffset;

    private void Awake()
    {
        if (debugLogs)
        {
            Debug.Log("[DragDrop] Awake()");
        }

        if (!mainCamera)
        {
            mainCamera = Camera.main;

            if (debugLogs)
            {
                Debug.Log("[DragDrop] mainCamera not assigned, using Camera.main");
            }
        }

        if (!mainCamera)
        {
            Debug.LogError("[DragDrop] No Camera assigned and no MainCamera found. Dragging won't work.");
        }
    }

    private void Update()
    {
        if (!dragged || !mainCamera || pointAction == null)
        {
            return;
        }

        Vector2 screenPosition = pointAction.action.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        if (dragPlane.Raycast(ray, out float enter))
        {
            Vector3 planeHit = ray.GetPoint(enter);
            Vector3 target = planeHit + grabOffset;

            if (moveRigidbodyIfPresent && draggedRigidbody)
            {
                draggedRigidbody.MovePosition(target);
            }
            else
            {
                dragged.position = target;
            }

            if (debugLogs)
            {
                Debug.Log("[DragDrop] Update() dragging: " + dragged.name + " target: " + target);
            }
        }
    }

    private void OnEnable()
    {
        if (debugLogs)
        {
            Debug.Log("[DragDrop] OnEnable()");
        }

        if (pointAction)
        {
            pointAction.action.Enable();

            if (debugLogs)
            {
                Debug.Log("[DragDrop] pointAction enabled: " + pointAction.action.name);
            }
        }

        if (clickAction)
        {
            clickAction.action.Enable();
            clickAction.action.started += OnPress;
            clickAction.action.canceled += OnRelease;

            if (debugLogs)
            {
                Debug.Log("[DragDrop] clickAction enabled + callbacks bound: " + clickAction.action.name);
            }
        }
    }

    private void OnDisable()
    {
        if (debugLogs)
        {
            Debug.Log("[DragDrop] OnDisable()");
        }

        if (clickAction)
        {
            clickAction.action.started -= OnPress;
            clickAction.action.canceled -= OnRelease;
            clickAction.action.Disable();
        }

        if (pointAction)
        {
            pointAction.action.Disable();
        }
    }

    private void OnPress(InputAction.CallbackContext ctx)
    {
        if (debugLogs)
        {
            Debug.Log("[DragDrop] OnPress()");
        }

        if (!mainCamera || pointAction == null)
        {
            if (debugLogs)
            {
                Debug.LogWarning("[DragDrop] OnPress() aborted. Missing mainCamera or pointAction.");
            }

            return;
        }

        Vector2 screenPosition = pointAction.action.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, draggableMask, QueryTriggerInteraction.Ignore))
        {
            if (debugLogs)
            {
                Debug.Log("[DragDrop] OnPress() Raycast FAILED. Nothing draggable hit.");
            }

            return;
        }

        if (debugLogs)
        {
            Debug.Log("[DragDrop] OnPress() HIT: " + hit.transform.name);
        }

        draggedIngredient = hit.transform.GetComponentInParent<Ingredient>();

        if (draggedIngredient != null)
        {
            dragged = draggedIngredient.transform;

            if (debugLogs)
            {
                Debug.Log("[DragDrop] OnPress() Ingredient found: " + draggedIngredient.name);
            }
        }
        else
        {
            dragged = hit.transform;

            if (debugLogs)
            {
                Debug.Log("[DragDrop] OnPress() Ingredient NOT found. Dragging hit transform.");
            }
        }

        draggedRigidbody = dragged.GetComponent<Rigidbody>();
        dragPlane = new Plane(mainCamera.transform.forward, hit.point);

        if (dragPlane.Raycast(ray, out float enter))
        {
            Vector3 planeHit = ray.GetPoint(enter);
            grabOffset = dragged.position - planeHit;

            if (debugLogs)
            {
                Debug.Log("[DragDrop] OnPress() grabOffset: " + grabOffset);
            }
        }

        if (debugLogs)
        {
            Debug.Log("[DragDrop] OnPress() DONE. Now dragging: " + dragged.name);
        }
    }

    private void OnRelease(InputAction.CallbackContext ctx)
    {
        if (debugLogs)
        {
            Debug.Log("[DragDrop] OnRelease()");
        }

        bool placed = false;

        if (dragged == null)
        {
            if (debugLogs)
            {
                Debug.Log("[DragDrop] OnRelease() dragged is NULL.");
            }

            return;
        }

        if (draggedIngredient != null && sandwichZoneMask.value != 0 && mainCamera && pointAction != null)
        {
            Vector2 screenPosition = pointAction.action.ReadValue<Vector2>();
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, sandwichZoneMask, QueryTriggerInteraction.Collide))
            {
                if (debugLogs)
                {
                    Debug.Log("[DragDrop] OnRelease() SandwichZone HIT: " + hit.transform.name);
                }

                var stack = hit.transform.GetComponentInParent<SandwichStack>();

                if (stack != null)
                {
                    if (debugLogs)
                    {
                        Debug.Log("[DragDrop] OnRelease() SandwichStack found: " + stack.name + " -> AddIngredient(" + draggedIngredient.name + ")");
                    }

                    stack.AddIngredient(draggedIngredient);
                    placed = true;

                    if (makeKinematicWhenPlaced && draggedRigidbody)
                    {
                        draggedRigidbody.isKinematic = true;
                        draggedRigidbody.linearVelocity = Vector3.zero;
                        draggedRigidbody.angularVelocity = Vector3.zero;

                        if (debugLogs)
                        {
                            Debug.Log("[DragDrop] OnRelease() Rigidbody set to kinematic (PLACED).");
                        }
                    }

                    if (disableCollidersWhenPlaced)
                    {
                        var cols = dragged.GetComponentsInChildren<Collider>();

                        for (int i = 0; i < cols.Length; i++)
                        {
                            cols[i].enabled = false;
                        }

                        if (debugLogs)
                        {
                            Debug.Log("[DragDrop] OnRelease() Colliders disabled (PLACED). Count: " + cols.Length);
                        }
                    }
                }
                else
                {
                    if (debugLogs)
                    {
                        Debug.Log("[DragDrop] OnRelease() HIT SandwichZone but no SandwichStack found.");
                    }
                }
            }
            else
            {
                if (debugLogs)
                {
                    Debug.Log("[DragDrop] OnRelease() Not over sandwich zone.");
                }
            }
        }
        else
        {
            if (debugLogs)
            {
                Debug.Log("[DragDrop] OnRelease() Not an Ingredient or sandwichZoneMask is 0 or missing refs.");
            }
        }

        if (debugLogs)
        {
            Debug.Log("[DragDrop] OnRelease() placed: " + placed);
            Debug.Log("[DragDrop] OnRelease() Clearing drag refs.");
        }

        dragged = null;
        draggedRigidbody = null;
        draggedIngredient = null;
    }
}
