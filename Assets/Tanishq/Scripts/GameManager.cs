using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum IngredientMatchMode
    {
        IgnoreOrder,
        ExactOrder
    }

    [Serializable]
    public class OrderDefinition
    {
        public string orderName;
        public List<string> ingredientIds = new();
    }

    [Serializable]
    public class SandwichScore
    {
        public string orderName;
        public int requiredCount;
        public int deliveredCount;
        public int correctCount;
        public int wrongCount;
        public int missingCount;
        public int stars;
        public List<string> required = new();
        public List<string> delivered = new();
    }

    [Header("Orders")]
    [SerializeField] private List<OrderDefinition> availableOrders = new();
    [SerializeField] private IngredientMatchMode matchMode = IngredientMatchMode.IgnoreOrder;
    [SerializeField] private bool perfectRequiresNoExtras = true;

    [Header("Order Timing")]
    [SerializeField] private bool autoGenerateOrders = true;
    [SerializeField] private float minOrderIntervalSeconds = 30f;
    [SerializeField] private float maxOrderIntervalSeconds = 60f;
    [SerializeField] private bool generateFirstOrderImmediately = true;

    [Header("Wiring")]
    [SerializeField] private SandwichOrder sandwichOrder;          // assign your SandwichOrder in inspector
    [SerializeField] private SandwichStackZone sandwichStackZone;  // optional: assign to clear stack on new order
    [SerializeField] private bool resetStackOnNewOrder = true;

    [Header("Star Rating")]
    [SerializeField] private int maxStars = 5;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private OrderDefinition currentOrder;
    private int currentOrderIndex = -1;
    private Coroutine orderLoopRoutine;

    public event Action<OrderDefinition> OnNewOrder;
    public event Action<SandwichScore> OnSandwichSubmitted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (debugLogs) Debug.LogWarning("[GameManager] Duplicate instance detected. Destroying: " + name);
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (debugLogs)
        {
            Debug.Log("[GameManager] Awake() Instance set: " + name);
            Debug.Log("[GameManager] availableOrders count: " + (availableOrders != null ? availableOrders.Count : 0));
        }
    }

    private void Start()
    {
        if (autoGenerateOrders)
        {
            orderLoopRoutine = StartCoroutine(OrderLoop());
        }
        else if (generateFirstOrderImmediately)
        {
            StartNewOrder();
        }
    }

    private void OnDisable()
    {
        if (orderLoopRoutine != null)
        {
            StopCoroutine(orderLoopRoutine);
            orderLoopRoutine = null;
        }
    }

    private IEnumerator OrderLoop()
    {
        if (generateFirstOrderImmediately)
            StartNewOrder();

        while (true)
        {
            float wait = UnityEngine.Random.Range(
                Mathf.Min(minOrderIntervalSeconds, maxOrderIntervalSeconds),
                Mathf.Max(minOrderIntervalSeconds, maxOrderIntervalSeconds)
            );

            if (debugLogs) Debug.Log($"[GameManager] Next order in {wait:0.0}s");

            yield return new WaitForSeconds(wait);

            StartNewOrder();
        }
    }

    public OrderDefinition GetCurrentOrder() => currentOrder;

    public void StartNewOrder(int index = -1)
    {
        if (availableOrders == null || availableOrders.Count == 0)
        {
            if (debugLogs) Debug.LogWarning("[GameManager] StartNewOrder() aborted. No availableOrders configured.");
            currentOrder = null;
            currentOrderIndex = -1;
            return;
        }

        if (index < 0)
            index = UnityEngine.Random.Range(0, availableOrders.Count);

        index = Mathf.Clamp(index, 0, availableOrders.Count - 1);
        currentOrderIndex = index;
        currentOrder = availableOrders[index];

        if (debugLogs)
        {
            Debug.Log("[GameManager] New Order Selected. Index: " + currentOrderIndex);
            Debug.Log("[GameManager] Order Name: " + (currentOrder != null ? currentOrder.orderName : "NULL"));
            Debug.Log("[GameManager] Required Ingredients: " + (currentOrder != null ? string.Join(", ", currentOrder.ingredientIds) : "NULL"));
        }

        // Push into SandwichOrder so SandwichStackZone uses the new required count
        if (sandwichOrder != null && currentOrder != null)
        {
            sandwichOrder.SetRequiredIngredients(currentOrder.ingredientIds);
        }

        // Optional: clear current sandwich when new order arrives (prevents “Franken-sandwich” scoring)
        if (resetStackOnNewOrder && sandwichStackZone != null)
        {
            sandwichStackZone.ResetStack();
        }

        OnNewOrder?.Invoke(currentOrder);
    }

    // Optional scoring pipeline if you want DeliveryBag to go through GameManager later:
    public SandwichScore SubmitSandwich(SandwichStackZone stackZone)
    {
        SandwichScore score = EvaluateSandwich(stackZone);
        OnSandwichSubmitted?.Invoke(score);
        return score;
    }

    private SandwichScore EvaluateSandwich(SandwichStackZone stackZone)
    {
        SandwichScore score = new();

        if (currentOrder == null)
        {
            score.orderName = "NO_ORDER";
            return score;
        }

        score.orderName = currentOrder.orderName;
        score.required = new List<string>(currentOrder.ingredientIds);
        score.requiredCount = score.required.Count;

        score.delivered = stackZone != null ? stackZone.GetPlacedIngredientIds() : new List<string>();
        score.deliveredCount = score.delivered.Count;

        // IgnoreOrder multiset matching (simple + fair)
        Dictionary<string, int> requiredCounts = new(StringComparer.OrdinalIgnoreCase);
        foreach (var id in score.required)
        {
            if (string.IsNullOrWhiteSpace(id)) continue;
            if (!requiredCounts.ContainsKey(id)) requiredCounts[id] = 0;
            requiredCounts[id]++;
        }

        int correct = 0, wrong = 0;
        foreach (var got in score.delivered)
        {
            if (string.IsNullOrWhiteSpace(got)) { wrong++; continue; }

            if (requiredCounts.TryGetValue(got, out int count) && count > 0)
            {
                correct++;
                requiredCounts[got] = count - 1;
            }
            else
            {
                wrong++;
            }
        }

        int missing = 0;
        foreach (var kv in requiredCounts) missing += kv.Value;

        score.correctCount = correct;
        score.wrongCount = wrong;
        score.missingCount = missing;

        score.stars = CalculateStars(score);
        return score;
    }

    private int CalculateStars(SandwichScore score)
    {
        if (score == null || score.requiredCount <= 0) return 0;

        float ratio = score.correctCount / (float)score.requiredCount;
        int stars = Mathf.Clamp(Mathf.RoundToInt(ratio * maxStars), 0, maxStars);

        if (perfectRequiresNoExtras && score.wrongCount > 0 && stars == maxStars)
            stars = maxStars - 1;

        return stars;
    }
}