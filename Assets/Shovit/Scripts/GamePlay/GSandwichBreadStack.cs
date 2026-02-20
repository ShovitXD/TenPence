using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class GSandwichBreadStack : MonoBehaviour
{
    [Header("Stacking")]
    [SerializeField] private string ingredientPickTag = "Pick";
    [SerializeField] private float stackYOffset = 0.08f;
    [SerializeField] private float snapXZTolerance = 0.35f;

    [Header("Top Bread")]
    [SerializeField] private bool isTopBreadCandidate = true;

    [Header("Table Snap")]
    [SerializeField] private string tableTag = "Table";
    [SerializeField] private float tablePaddingY = 0.02f;
    [SerializeField] private bool snapRotationToTable = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    // Runtime state
    private readonly List<GameObject> stackedIngredientParents = new();
    private readonly List<string> deliveredIngredientTags = new();

    private Rigidbody rb;
    private DragItem dragItem;
    private GGameManager gameManager;

    private bool hasTopBread = false;
    private bool sandwichReady = false;

    // ticket assigned by spawner
    [SerializeField] private int ticketId = -1;

    // public API expected by other scripts
    public bool IsSandwichReady => sandwichReady;
    public int AssignedTicketId => ticketId;
    public int TicketId => ticketId; // compatibility if any script still uses TicketId

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        dragItem = GetComponent<DragItem>();
        gameManager = FindFirstObjectByType<GGameManager>();

        // Keep bread upright always
        if (rb != null)
        {
            rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    public void SetTicketId(int newTicketId)
    {
        ticketId = newTicketId;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        // Snap this bread to table trigger
        if (other.CompareTag(tableTag))
        {
            SnapBreadToTable(other.transform);
            return;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null || collision.gameObject == null) return;
        TryAddObject(collision.gameObject);
    }

    private void TryAddObject(GameObject candidate)
    {
        if (candidate == null) return;

        if (sandwichReady)
        {
            if (debugLogs) Debug.Log("[GSandwichBreadStack] Sandwich already complete. Ignoring new object.");
            return;
        }

        if (candidate == gameObject) return;

        // top bread?
        GSandwichBreadStack otherBread = candidate.GetComponent<GSandwichBreadStack>();
        if (otherBread != null)
        {
            // don't allow self / already completed / bread with contents to become top bread
            if (otherBread == this) return;

            if (otherBread.hasTopBread || otherBread.sandwichReady || otherBread.stackedIngredientParents.Count > 0)
            {
                if (debugLogs) Debug.Log("[GSandwichBreadStack] Top bread candidate already has contents. Ignored.");
                return;
            }

            if (!isTopBreadCandidate) return;

            AddTopBread(candidate, otherBread);
            return;
        }

        // ingredient parent must have pick tag
        if (!candidate.CompareTag(ingredientPickTag))
            return;

        AddIngredientParent(candidate);
    }

    private void AddIngredientParent(GameObject ingredientParent)
    {
        if (ingredientParent == null) return;

        // avoid duplicates
        if (stackedIngredientParents.Contains(ingredientParent)) return;

        // detect active child tag from ingredient variants
        string activeVariantTag = GetActiveChildTag(ingredientParent);
        if (string.IsNullOrEmpty(activeVariantTag))
        {
            if (debugLogs) Debug.LogWarning($"[GSandwichBreadStack] No active child tag found on '{ingredientParent.name}'.");
            return;
        }

        // Parent it to bread
        ingredientParent.transform.SetParent(transform, true);

        // place on stack (without moving bread)
        Vector3 targetPos = transform.position + Vector3.up * (stackYOffset * (stackedIngredientParents.Count + 1));
        ingredientParent.transform.position = targetPos;

        // disable ingredient dragging/physics so only bread moves
        DisableDraggingAndPhysics(ingredientParent);

        stackedIngredientParents.Add(ingredientParent);
        deliveredIngredientTags.Add(activeVariantTag);

        if (debugLogs)
        {
            string sig = (gameManager != null) ? gameManager.BuildNumericSignature(deliveredIngredientTags) : "(no gm)";
            Debug.Log($"[GSandwichBreadStack] Added '{ingredientParent.name}' => {activeVariantTag} | Count:{deliveredIngredientTags.Count} | Sig:{sig}");
        }
    }

    private void AddTopBread(GameObject topBreadObject, GSandwichBreadStack topBreadScript)
    {
        if (topBreadObject == null) return;
        if (hasTopBread || sandwichReady) return;

        hasTopBread = true;
        sandwichReady = true;

        // parent top bread to this bread
        topBreadObject.transform.SetParent(transform, true);

        // place above current stack
        int totalLayersUnderTop = stackedIngredientParents.Count + 1; // +1 for bottom bread base
        Vector3 topPos = transform.position + Vector3.up * (stackYOffset * (totalLayersUnderTop + 0.2f));
        topBreadObject.transform.position = topPos;

        // disable top bread drag/physics
        DisableDraggingAndPhysics(topBreadObject);

        // disable its stacking logic so it won't interfere
        if (topBreadScript != null)
            topBreadScript.enabled = false;

        // enable this bread drag if present (this becomes the one moving whole sandwich)
        if (dragItem != null)
            dragItem.enabled = true;

        if (debugLogs)
        {
            string sig = (gameManager != null) ? gameManager.BuildNumericSignature(deliveredIngredientTags) : "(no gm)";
            Debug.Log($"[GSandwichBreadStack] TOP BREAD added. Ready. IngredientCount={deliveredIngredientTags.Count}, Sig={sig}");
        }
    }

    private string GetActiveChildTag(GameObject ingredientParent)
    {
        if (ingredientParent == null) return string.Empty;

        // Read only direct children (raw/aged/spoiled variants)
        for (int i = 0; i < ingredientParent.transform.childCount; i++)
        {
            Transform child = ingredientParent.transform.GetChild(i);
            if (child == null || !child.gameObject.activeInHierarchy) continue;

            // prefer tag if set properly
            string childTag = child.tag;

            // Unity "Untagged" means no useful tag
            if (!string.IsNullOrEmpty(childTag) && childTag != "Untagged")
                return childTag;

            // fallback to object name if tag isn't assigned
            return child.gameObject.name;
        }

        return string.Empty;
    }

    private void SnapBreadToTable(Transform tableTransform)
    {
        if (tableTransform == null) return;

        // Use table transform origin directly (you asked for table origin)
        Vector3 target = tableTransform.position;
        target.y += tablePaddingY;

        // Move only the bread root
        transform.position = target;

        if (snapRotationToTable)
        {
            Vector3 e = tableTransform.eulerAngles;
            transform.rotation = Quaternion.Euler(0f, e.y, 0f);
        }

        // lock it in place until top bread is added (sandwich complete)
        if (!sandwichReady && rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (debugLogs)
            Debug.Log($"[GSandwichBreadStack] Snapped to table: {tableTransform.name}");
    }

    private void DisableDraggingAndPhysics(GameObject go)
    {
        if (go == null) return;

        DragItem di = go.GetComponent<DragItem>();
        if (di != null)
            di.enabled = false;

        Rigidbody childRb = go.GetComponent<Rigidbody>();
        if (childRb != null)
        {
            childRb.isKinematic = true;

            // Avoid velocity warning on kinematic RB:
            // set velocities BEFORE/while non-kinematic OR just skip setting them.
            // safest = skip setting velocity entirely.
            childRb.constraints = RigidbodyConstraints.FreezeAll;
        }

        Collider col = go.GetComponent<Collider>();
        if (col != null)
        {
            // keep enabled so trigger submit can still hit parent colliders if needed,
            // but ingredients themselves don't need collisions after stacking
            col.enabled = false;
        }
    }

    // Called by delivery zone
    public List<string> GetDeliveredIngredientTags()
    {
        // return a copy so nobody edits internal list
        return new List<string>(deliveredIngredientTags);
    }

    // Optional helpers for debugging/other scripts
    public string GetDeliveredNumericSignature()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GGameManager>();

        if (gameManager == null) return string.Empty;
        return gameManager.BuildNumericSignature(deliveredIngredientTags);
    }

    public int GetIngredientCount()
    {
        return deliveredIngredientTags.Count;
    }
}