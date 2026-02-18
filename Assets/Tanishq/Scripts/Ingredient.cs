using UnityEngine;

public class Ingredient : MonoBehaviour
{
    [Tooltip("If > 0, overrides automatic thickness from bounds.")]
    public float thicknessOverride = 0f;

    [Tooltip("Optional: what point should be centered on the sandwich (defaults to transform).")]
    public Transform centerPivot;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private Renderer cachedRenderer;
    private Collider cachedCollider;

    private void Awake()
    {
        if (debugLogs)
        {
            Debug.Log("[Ingredient] Awake() on: " + name);
        }

        cachedRenderer = GetComponentInChildren<Renderer>();
        cachedCollider = GetComponentInChildren<Collider>();

        if (debugLogs)
        {
            Debug.Log("[Ingredient] cachedRenderer: " + (cachedRenderer != null ? cachedRenderer.name : "NULL"));
            Debug.Log("[Ingredient] cachedCollider: " + (cachedCollider != null ? cachedCollider.name : "NULL"));
        }

        if (centerPivot == null)
        {
            centerPivot = transform;

            if (debugLogs)
            {
                Debug.Log("[Ingredient] centerPivot was NULL, set to transform: " + centerPivot.name);
            }
        }
        else
        {
            if (debugLogs)
            {
                Debug.Log("[Ingredient] centerPivot already assigned: " + centerPivot.name);
            }
        }
    }

    public float GetThickness()
    {
        if (debugLogs)
        {
            Debug.Log("[Ingredient] GetThickness() called on: " + name);
            Debug.Log("[Ingredient] thicknessOverride: " + thicknessOverride);
        }

        if (thicknessOverride > 0f)
        {
            if (debugLogs)
            {
                Debug.Log("[Ingredient] Using thicknessOverride: " + thicknessOverride);
            }

            return thicknessOverride;
        }

        if (cachedCollider != null)
        {
            float t = cachedCollider.bounds.size.y;

            if (debugLogs)
            {
                Debug.Log("[Ingredient] Using Collider bounds thickness: " + t + " collider: " + cachedCollider.name);
            }

            return t;
        }

        if (cachedRenderer != null)
        {
            float t = cachedRenderer.bounds.size.y;

            if (debugLogs)
            {
                Debug.Log("[Ingredient] Using Renderer bounds thickness: " + t + " renderer: " + cachedRenderer.name);
            }

            return t;
        }

        if (debugLogs)
        {
            Debug.LogWarning("[Ingredient] No Collider/Renderer found. Returning fallback thickness 0.02");
        }

        return 0.02f;
    }
}