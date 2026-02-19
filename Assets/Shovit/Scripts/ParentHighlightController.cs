using UnityEngine;

public class ParentHighlightController : MonoBehaviour
{
    private LookHighlight[] childHighlights;

    private void Awake()
    {
        childHighlights = GetComponentsInChildren<LookHighlight>(includeInactive: true);
    }

    public void SetHighlighted(bool on)
    {
        Debug.Log($"{name}: Highlight {(on ? "ON" : "OFF")}");

        foreach (var highlight in childHighlights)
        {
            if (highlight != null)
            {
                Debug.Log($"   Applying to child: {highlight.name}");
                highlight.SetHighlighted(on);
            }
            else
            {
                Debug.LogWarning("   Found a null child highlight reference.");
            }
        }
    }
}
