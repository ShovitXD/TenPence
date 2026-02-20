using UnityEngine;

/// <summary>
/// Attach this to the bread prefab spawner (or call it from your existing spawner).
/// It assigns the selected order ticket ID to a spawned bread so the bag can submit correctly.
/// 
/// Usage:
/// - Call AssignTicketToBread(spawnedBread, ticketId) right after spawning the bread.
/// </summary>
public class GBreadTicketAssigner : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    public bool AssignTicketToBread(GameObject breadObject, int ticketId)
    {
        if (breadObject == null)
        {
            if (debugLogs) Debug.LogWarning("[GBreadTicketAssigner] breadObject is null.");
            return false;
        }

        GSandwichBreadStack bread = breadObject.GetComponent<GSandwichBreadStack>();
        if (bread == null)
        {
            if (debugLogs) Debug.LogWarning("[GBreadTicketAssigner] No GSandwichBreadStack on: " + breadObject.name);
            return false;
        }

        bread.SetTicketId(ticketId);

        if (debugLogs)
            Debug.Log($"[GBreadTicketAssigner] Assigned Ticket #{ticketId} to bread '{breadObject.name}'");

        return true;
    }
}