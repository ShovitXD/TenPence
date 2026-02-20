using System.Collections.Generic;
using UnityEngine;

public class SandwichOrder : MonoBehaviour
{
    [SerializeField] private List<string> requiredIngredients = new();

    public int RequiredCount => requiredIngredients.Count;

    public IReadOnlyList<string> RequiredIngredients => requiredIngredients;

    public void SetRequiredIngredients(IEnumerable<string> ids)
    {
        requiredIngredients.Clear();
        if (ids == null) return;

        foreach (var id in ids)
        {
            if (!string.IsNullOrWhiteSpace(id))
                requiredIngredients.Add(id);
        }
    }

    public int EvaluateStars(List<string> placedIngredients)
    {
        var requiredCounts = new Dictionary<string, int>();
        foreach (var id in requiredIngredients)
        {
            if (!requiredCounts.ContainsKey(id)) requiredCounts[id] = 0;
            requiredCounts[id]++;
        }

        var placedCounts = new Dictionary<string, int>();
        foreach (var id in placedIngredients)
        {
            if (!placedCounts.ContainsKey(id)) placedCounts[id] = 0;
            placedCounts[id]++;
        }

        int matches = 0;
        foreach (var kv in requiredCounts)
        {
            placedCounts.TryGetValue(kv.Key, out int have);
            matches += Mathf.Min(kv.Value, have);
        }

        int requiredTotal = requiredIngredients.Count;
        int placedTotal = placedIngredients.Count;

        if (requiredTotal == 0) return 0;

        // Penalize missing AND extra ingredients.
        float score = matches / (float)Mathf.Max(requiredTotal, placedTotal);
        int stars = Mathf.Clamp(Mathf.RoundToInt(score * 5f), 0, 5);
        return stars;
    }
}