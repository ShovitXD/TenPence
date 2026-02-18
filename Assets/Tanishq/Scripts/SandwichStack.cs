using System.Collections.Generic;
using UnityEngine;

public class SandwichStack : MonoBehaviour
{
    [Header("Stack Settings")]
    [SerializeField] private Transform stackRoot;
    [SerializeField] private Transform stackOrigin;
    [SerializeField] private float padding = 0.001f;

    [Header("Physics / Interaction")]
    [SerializeField] private bool disableCollidersAfterPlace = false;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private readonly List<Ingredient> ingredients = new();
    private float currentTopY;

    private void Awake()
    {
        if (debugLogs)
        {
            Debug.Log("[SandwichStack] Awake() on: " + name);
        }

        if (!stackRoot)
        {
            stackRoot = transform;

            if (debugLogs)
            {
                Debug.Log("[SandwichStack] stackRoot was NULL, set to transform: " + stackRoot.name);
            }
        }
        else
        {
            if (debugLogs)
            {
                Debug.Log("[SandwichStack] stackRoot assigned: " + stackRoot.name);
            }
        }

        if (!stackOrigin)
        {
            stackOrigin = transform;

            if (debugLogs)
            {
                Debug.Log("[SandwichStack] stackOrigin was NULL, set to transform: " + stackOrigin.name);
            }
        }
        else
        {
            if (debugLogs)
            {
                Debug.Log("[SandwichStack] stackOrigin assigned: " + stackOrigin.name);
            }
        }

        currentTopY = stackOrigin.position.y;

        if (debugLogs)
        {
            Debug.Log("[SandwichStack] currentTopY initialized to: " + currentTopY);
            Debug.Log("[SandwichStack] padding: " + padding + " disableCollidersAfterPlace: " + disableCollidersAfterPlace);
        }
    }

    public void AddIngredient(Ingredient ingredient)
    {
        if (debugLogs)
        {
            Debug.Log("[SandwichStack] AddIngredient() called with: " + (ingredient != null ? ingredient.name : "NULL"));
        }

        if (!ingredient)
        {
            if (debugLogs)
            {
                Debug.LogWarning("[SandwichStack] AddIngredient() aborted. ingredient is NULL.");
            }

            return;
        }

        ingredients.Add(ingredient);

        if (debugLogs)
        {
            Debug.Log("[SandwichStack] Added to list. Total ingredients: " + ingredients.Count);
        }

        ingredient.transform.SetParent(stackRoot, true);

        if (debugLogs)
        {
            Debug.Log("[SandwichStack] SetParent -> stackRoot: " + stackRoot.name);
        }

        float t = ingredient.GetThickness();

        if (debugLogs)
        {
            Debug.Log("[SandwichStack] Thickness returned: " + t);
            Debug.Log("[SandwichStack] currentTopY before place: " + currentTopY);
        }

        float y = currentTopY + (t * 0.5f) + padding;

        Vector3 pos = stackOrigin.position;
        pos.y = y;

        if (debugLogs)
        {
            Debug.Log("[SandwichStack] Computed placement y: " + y);
            Debug.Log("[SandwichStack] stackOrigin position: " + stackOrigin.position);
            Debug.Log("[SandwichStack] target pos: " + pos);
        }

        Vector3 pivotOffset = Vector3.zero;

        if (ingredient.centerPivot)
        {
            pivotOffset = ingredient.centerPivot.position - ingredient.transform.position;

            if (debugLogs)
            {
                Debug.Log("[SandwichStack] centerPivot found: " + ingredient.centerPivot.name);
                Debug.Log("[SandwichStack] pivotOffset: " + pivotOffset);
            }
        }
        else
        {
            if (debugLogs)
            {
                Debug.Log("[SandwichStack] centerPivot is NULL. pivotOffset = Vector3.zero");
            }
        }

        ingredient.transform.position = pos - pivotOffset;

        if (debugLogs)
        {
            Debug.Log("[SandwichStack] Ingredient positioned to: " + ingredient.transform.position);
        }

        currentTopY = y + (t * 0.5f);

        if (debugLogs)
        {
            Debug.Log("[SandwichStack] currentTopY updated to: " + currentTopY);
        }

        if (ingredient.TryGetComponent<Rigidbody>(out var rb))
        {
            if (debugLogs)
            {
                Debug.Log("[SandwichStack] Rigidbody found on ingredient. Freezing it (kinematic).");
            }

            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (debugLogs)
            {
                Debug.Log("[SandwichStack] Rigidbody set. isKinematic: " + rb.isKinematic);
            }
        }
        else
        {
            if (debugLogs)
            {
                Debug.Log("[SandwichStack] No Rigidbody found on ingredient.");
            }
        }

        if (disableCollidersAfterPlace)
        {
            if (debugLogs)
            {
                Debug.Log("[SandwichStack] disableCollidersAfterPlace is ON. Disabling ingredient colliders.");
            }

            var colliders = ingredient.GetComponentsInChildren<Collider>();

            if (debugLogs)
            {
                Debug.Log("[SandwichStack] Found colliders count: " + colliders.Length);
            }

            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;

                if (debugLogs)
                {
                    Debug.Log("[SandwichStack] Disabled collider: " + colliders[i].name);
                }
            }
        }

        if (debugLogs)
        {
            Debug.Log("[SandwichStack] AddIngredient() DONE for: " + ingredient.name);
        }
    }

    public void ResetSandwich()
    {
        if (debugLogs)
        {
            Debug.Log("[SandwichStack] ResetSandwich() called. Ingredients count: " + ingredients.Count);
        }

        for (int i = 0; i < ingredients.Count; i++)
        {
            var ing = ingredients[i];

            if (ing != null)
            {
                if (debugLogs)
                {
                    Debug.Log("[SandwichStack] Destroying ingredient: " + ing.name);
                }

                Destroy(ing.gameObject);
            }
            else
            {
                if (debugLogs)
                {
                    Debug.Log("[SandwichStack] Ingredient at index " + i + " is NULL.");
                }
            }
        }

        ingredients.Clear();
        currentTopY = stackOrigin.position.y;

        if (debugLogs)
        {
            Debug.Log("[SandwichStack] Cleared list. currentTopY reset to: " + currentTopY);
            Debug.Log("[SandwichStack] ResetSandwich() DONE.");
        }
    }
}