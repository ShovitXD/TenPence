using System.Collections.Generic;
using UnityEngine;

public class SandwichStackZone : MonoBehaviour, IDropTarget
{
    [Header("Setup")]
    [SerializeField] private Transform stackAnchor;          // Where stacking starts (bottom)
    [SerializeField] private SandwichOrder order;            // Reference to current order
    [SerializeField] private float verticalPadding = 0.002f; // Small gap to avoid z-fighting

    private readonly List<IngredientItem> stacked = new();
    private float currentHeight = 0f;

    private DraggableState draggableState;
    private BoxCollider stackCollider;

    public bool IsComplete => order != null && stacked.Count >= order.RequiredCount;

    private void Awake()
    {
        if (!stackAnchor) stackAnchor = transform;

        draggableState = GetComponent<DraggableState>();
        stackCollider = GetComponent<BoxCollider>();

        // Stack should NOT be draggable until complete.
        if (draggableState != null)
            draggableState.SetCanDrag(false);
    }

    public bool CanAccept(GameObject dragged)
    {
        if (!dragged.CompareTag("Pick")) return false;

        IngredientItem ingredient = dragged.GetComponent<IngredientItem>();
        if (!ingredient) return false;

        // Prevent stacking the same object twice.
        return !stacked.Contains(ingredient);
    }

    public void Accept(GameObject dragged)
    {
        IngredientItem ingredient = dragged.GetComponent<IngredientItem>();
        if (!ingredient) return;

        float height = GetHeight(ingredient);
        float yPos = currentHeight + (height * 0.5f);

        // Parent and snap into stack position.
        ingredient.transform.SetParent(stackAnchor, true);
        ingredient.transform.localRotation = Quaternion.identity;
        ingredient.transform.localPosition = new Vector3(0f, yPos, 0f);

        currentHeight += height + verticalPadding;
        stacked.Add(ingredient);

        LockIngredient(ingredient);
        RebuildStackCollider();

        if (IsComplete && draggableState != null)
            draggableState.SetCanDrag(true);
    }

    public List<string> GetPlacedIngredientIds()
    {
        var ids = new List<string>(stacked.Count);
        foreach (var ing in stacked)
            ids.Add(ing.IngredientId);
        return ids;
    }

    public void ResetStack()
    {
        // Remove stacked ingredients from scene (or pool them if you want to be fancy).
        foreach (var ing in stacked)
        {
            if (ing != null)
                Destroy(ing.gameObject);
        }

        stacked.Clear();
        currentHeight = 0f;

        if (draggableState != null)
            draggableState.SetCanDrag(false);

        RebuildStackCollider();
    }

    private float GetHeight(IngredientItem ingredient)
    {
        if (ingredient.Col != null)
            return ingredient.Col.bounds.size.y;

        // Fallback for missing collider (should not happen).
        return 0.05f;
    }

    private void LockIngredient(IngredientItem ingredient)
    {
        // Ingredient should become part of the stack and stop being draggable/physical.
        DraggableState state = ingredient.GetComponent<DraggableState>();
        if (state != null) state.SetCanDrag(false);

        if (ingredient.Rb != null)
        {
            ingredient.Rb.linearVelocity = Vector3.zero;
            ingredient.Rb.angularVelocity = Vector3.zero;
            ingredient.Rb.useGravity = false;
            ingredient.Rb.isKinematic = true;
        }

        // Disable collider to prevent physics freakouts inside the stack.
        if (ingredient.Col != null)
            ingredient.Col.enabled = false;
    }

    private void RebuildStackCollider()
    {
        if (!stackCollider) return;

        // Collider should cover the whole stacked sandwich for easy dragging + delivery detection.
        float height = Mathf.Max(0.05f, currentHeight);
        stackCollider.center = new Vector3(0f, height * 0.5f, 0f);
        stackCollider.size = new Vector3(stackCollider.size.x, height, stackCollider.size.z);
    }
}