using System.Collections.Generic;
using UnityEngine;

public class FoodUIRegistry : MonoBehaviour
{
    public static FoodUIRegistry Instance { get; private set; }

    [Header("Scroll View Content")]
    [Tooltip("Drag ScrollView/Viewport/Content here.")]
    [SerializeField] private Transform contentRoot;

    [Header("Row Prefab")]
    [Tooltip("Prefab for one row: Image + TMP name + TMP age + Slider")]
    [SerializeField] private FoodUIRow rowPrefab;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private readonly Dictionary<int, FoodUIRow> rowsById = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public FoodUIRow RegisterOrGetRow(int uniqueId, Sprite icon, string foodName)
    {
        if (rowsById.TryGetValue(uniqueId, out var existing) && existing != null)
            return existing;

        if (contentRoot == null || rowPrefab == null)
        {
            if (debugLogs) Debug.LogWarning("[FoodUIRegistry] Missing contentRoot or rowPrefab.");
            return null;
        }

        FoodUIRow row = Instantiate(rowPrefab, contentRoot);
        row.Setup(icon, foodName);

        rowsById[uniqueId] = row;

        if (debugLogs) Debug.Log($"[FoodUIRegistry] Row created for id={uniqueId} name='{foodName}'");
        return row;
    }
}
