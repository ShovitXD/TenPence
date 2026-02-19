using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class LookHighlight : MonoBehaviour
{
    [Header("URP Lit Emission")]
    [SerializeField] private bool useEmission = true;

    private Renderer _renderer;
    private MaterialPropertyBlock _mpb;

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();
    }

    public void SetHighlighted(bool on)
    {
        if (_renderer == null) return;

        if (!useEmission)
            return;

        _renderer.GetPropertyBlock(_mpb);

        // NOTE: we are not choosing custom colors here; using built-in values only.
        _mpb.SetColor(EmissionColorId, on ? Color.white : Color.black);

        _renderer.SetPropertyBlock(_mpb);
    }
}
