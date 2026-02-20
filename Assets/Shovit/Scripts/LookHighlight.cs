using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class LookHighlight : MonoBehaviour
{
    [Header("Materials")]
    [Tooltip("Material to use when NOT highlighted. If left empty, it will be captured from the Renderer on Awake.")]
    [SerializeField] private Material normalMaterial;

    [Tooltip("Material to use when highlighted.")]
    [SerializeField] private Material highlightMaterial;

    [Header("Apply Mode")]
    [Tooltip("If true, changes only one material slot (materialIndex). If false, swaps the whole materials array.")]
    [SerializeField] private bool singleSlotOnly = true;

    [Tooltip("Which material slot to swap when singleSlotOnly is true.")]
    [SerializeField] private int materialIndex = 0;

    private Renderer _renderer;
    private Material[] _cachedNormalMaterials;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();

        // Cache the current materials (keeps multiple material slots intact)
        _cachedNormalMaterials = _renderer.sharedMaterials;

        // If user didn't assign normalMaterial, take it from the renderer slot
        if (normalMaterial == null)
        {
            if (_cachedNormalMaterials != null && _cachedNormalMaterials.Length > 0)
            {
                materialIndex = Mathf.Clamp(materialIndex, 0, _cachedNormalMaterials.Length - 1);
                normalMaterial = _cachedNormalMaterials[materialIndex];
            }
        }
    }

    public void SetHighlighted(bool on)
    {
        if (_renderer == null) return;

        if (highlightMaterial == null)
        {
            Debug.LogWarning($"{nameof(LookHighlight)}: highlightMaterial not assigned.", this);
            return;
        }

        if (singleSlotOnly)
        {
            // Swap only one slot, keep others unchanged
            var mats = _renderer.sharedMaterials;
            if (mats == null || mats.Length == 0) return;

            int idx = Mathf.Clamp(materialIndex, 0, mats.Length - 1);
            mats[idx] = on ? highlightMaterial : (normalMaterial != null ? normalMaterial : _cachedNormalMaterials[idx]);
            _renderer.sharedMaterials = mats;
        }
        else
        {
            // Swap ALL materials: restore original array or replace with highlight everywhere
            if (on)
            {
                var mats = (Material[])_cachedNormalMaterials.Clone();
                for (int i = 0; i < mats.Length; i++)
                    mats[i] = highlightMaterial;

                _renderer.sharedMaterials = mats;
            }
            else
            {
                _renderer.sharedMaterials = _cachedNormalMaterials;
            }
        }
    }
}
