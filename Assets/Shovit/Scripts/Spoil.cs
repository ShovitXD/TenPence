using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Spoil : MonoBehaviour
{
    private Renderer _renderer;
    private MaterialPropertyBlock _mpb;

    // URP Lit uses _BaseColor (not _Color)
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();
    }

    // Call this to turn the cube BLUE
    public void TurnBlue()
    {
        SetColor(Color.blue);
    }

    // Call this to turn the cube GREEN
    public void TurnGreen()
    {
        SetColor(Color.green);
    }

    // Call this to turn the cube RED
    public void TurnRed()
    {
        SetColor(Color.red);
    }

    private void SetColor(Color c)
    {
        if (_renderer == null) return;

        // Use MaterialPropertyBlock so we don't instantiate materials per object
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(BaseColorId, c);
        _renderer.SetPropertyBlock(_mpb);
    }
}
