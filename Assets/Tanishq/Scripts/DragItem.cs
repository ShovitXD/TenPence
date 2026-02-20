using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class DragItem : MonoBehaviour
{
    private Camera cam;
    private Rigidbody rb;

    private bool isDragging = false;
    private bool isSnapped = false;
    private float distanceToCamera;

    private RigidbodyConstraints originalConstraints;

    [Header("Drop Behavior")]
    [SerializeField] private bool clearConstraintsOnDrop = true;
    [SerializeField] private bool keepUprightOnDrop = true;

    private void Awake()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        if (rb != null) originalConstraints = rb.constraints;
    }

    private void Start()
    {
        if (cam == null) cam = Camera.main;
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb != null) originalConstraints = rb.constraints;
    }

    private void Update()
    {
        if (isSnapped) return;
        if (!enabled) return;
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame) TryStartDrag();
        if (Mouse.current.leftButton.wasReleasedThisFrame) StopDrag();
        if (isDragging) Drag();
    }

    private void TryStartDrag()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null || rb == null) return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
        {
            isDragging = true;
            distanceToCamera = Vector3.Distance(transform.position, cam.transform.position);

            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezePositionY; // flat drag
        }
    }

    private void Drag()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        Vector3 point = ray.GetPoint(distanceToCamera);
        point.y = transform.position.y;
        transform.position = point;
    }

    private void StopDrag()
    {
        isDragging = false;
        if (rb == null) return;

        rb.isKinematic = false;

        if (clearConstraintsOnDrop)
        {
            if (keepUprightOnDrop)
            {
                rb.constraints =
                    RigidbodyConstraints.FreezeRotationX |
                    RigidbodyConstraints.FreezeRotationY |
                    RigidbodyConstraints.FreezeRotationZ;
            }
            else
            {
                rb.constraints = RigidbodyConstraints.None;
            }
        }
        else
        {
            rb.constraints = originalConstraints;
        }
    }

    public void SnapTo(Vector3 position)
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        isSnapped = true;
        isDragging = false;

        rb.isKinematic = true;
        transform.position = position;
    }

    public bool IsSnapped() => isSnapped;

    public void SetSnapped(bool snapped)
    {
        isSnapped = snapped;
        if (!snapped) isDragging = false;
    }

    public void SetBaseConstraints(RigidbodyConstraints constraints)
    {
        originalConstraints = constraints;
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb != null && !isDragging && !clearConstraintsOnDrop)
            rb.constraints = originalConstraints;
    }
}