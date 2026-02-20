using UnityEngine;
using System.Collections.Generic;

public class SandwichZone : MonoBehaviour
{
    private List<DragItem> stackedItems = new List<DragItem>();
    public float stackHeightOffset = 0.2f;

    private void OnTriggerStay(Collider other)
    {
        DragItem item = other.GetComponent<DragItem>();
        if (item == null) return;
        if (item.IsSnapped()) return;

        if (!UnityEngine.InputSystem.Mouse.current.leftButton.isPressed)
        {
            StackItem(item);
        }
    }

    void StackItem(DragItem item)
    {
        float height = stackedItems.Count * stackHeightOffset;
        Vector3 snapPosition = transform.position + Vector3.up * height;

        item.SnapTo(snapPosition);
        stackedItems.Add(item);
    }
}