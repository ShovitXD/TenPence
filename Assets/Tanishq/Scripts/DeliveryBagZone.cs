using UnityEngine;

public class DeliveryBagZone : MonoBehaviour, IDropTarget
{
    [SerializeField] private SandwichOrder order;

    public bool CanAccept(GameObject dragged)
    {
        if (!dragged.CompareTag("SandwichStack")) return false;

        SandwichStackZone stack = dragged.GetComponent<SandwichStackZone>();
        if (!stack) return false;

        return stack.IsComplete;
    }

    public void Accept(GameObject dragged)
    {
        SandwichStackZone stack = dragged.GetComponent<SandwichStackZone>();
        if (!stack || order == null) return;

        int stars = order.EvaluateStars(stack.GetPlacedIngredientIds());
        Debug.Log($"DELIVERED: {stars}/5 stars");

        // Reset after delivery (replace with new order logic later).
        stack.ResetStack();
    }
}