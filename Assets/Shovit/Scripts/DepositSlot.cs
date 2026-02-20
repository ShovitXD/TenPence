using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DepositSlot : MonoBehaviour
{
    [SerializeField] private DepositAltarManager manager;

    private void Reset()
    {
        // Make sure the collider is set to Trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (manager == null) return;

        // Deposit the root object (handles crystals with child colliders too)
        GameObject target = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.gameObject;

        manager.TryDeposit(target);
    }
}