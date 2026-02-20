using System.Collections.Generic;
using UnityEngine;

public class GDeliveryBagZone : MonoBehaviour
{
    [Header("Auto Find")]
    [SerializeField] private GGameManager gameManager;

    [Header("Trigger Settings")]
    [SerializeField] private bool requireSandwichReady = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GGameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        // Try this object first, then parent
        GSandwichBreadStack stack = other.GetComponent<GSandwichBreadStack>();
        if (stack == null)
            stack = other.GetComponentInParent<GSandwichBreadStack>();

        if (stack == null) return;

        TrySubmit(stack.gameObject, stack);
    }

    private void TrySubmit(GameObject sandwichObject, GSandwichBreadStack stack)
    {
        if (gameManager == null)
        {
            if (debugLogs) Debug.LogWarning("[GDeliveryBagZone] No GGameManager found.");
            return;
        }

        if (stack == null)
        {
            if (debugLogs) Debug.LogWarning("[GDeliveryBagZone] Missing GSandwichBreadStack.");
            return;
        }

        if (requireSandwichReady && !stack.IsSandwichReady)
        {
            if (debugLogs) Debug.Log("[GDeliveryBagZone] Sandwich not ready yet (top bread missing).");
            return;
        }

        int ticketId = stack.AssignedTicketId;
        List<string> deliveredTags = stack.GetDeliveredIngredientTags();

        GGameManager.SandwichScore score = gameManager.SubmitSandwichForTicket(ticketId, deliveredTags);

        if (debugLogs)
        {
            Debug.Log($"[GDeliveryBagZone] Delivered Ticket #{ticketId} -> {score.starsEarned}/{score.starsPossible} stars");
            Debug.Log("[GDeliveryBagZone] Tags: " + string.Join(", ", deliveredTags));
            Debug.Log("[GDeliveryBagZone] RequiredSig: " + score.requiredNumericSignature);
            Debug.Log("[GDeliveryBagZone] DeliveredSig: " + score.deliveredNumericSignature);
        }

        // destroy submitted sandwich
        if (sandwichObject != null)
            Destroy(sandwichObject);
    }
}