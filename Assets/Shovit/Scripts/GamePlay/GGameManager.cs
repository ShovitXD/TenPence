using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GGameManager : MonoBehaviour
{
    public static GGameManager Instance { get; private set; }

    [Serializable]
    public class OrderDefinition
    {
        [Header("Display")]
        public string orderName;
        public Sprite foodImage; // NEW: image shown on order slip

        [Header("Recipe")]
        public List<string> ingredientIds = new();

        [Header("Optional")]
        public bool includeWine = false;
        public string wineIngredientId = "agedW";
    }

    [Serializable]
    public class SandwichScore
    {
        public int ticketId;
        public string orderName;

        public int correctCount;
        public int wrongCount;
        public int missingCount;
        public int orderPenalty;

        public int starsEarned;
        public int starsPossible;

        public List<string> requiredTags = new();
        public List<string> deliveredTags = new();

        public string requiredNumericSignature;
        public string deliveredNumericSignature;
    }

    [Serializable]
    public class GOrderSlipRuntimeData
    {
        public int ticketId;
        public int slotIndex;
        public OrderDefinition orderDefinition;
        public GameObject slipInstance;
    }

    [Header("Orders")]
    [SerializeField] private List<OrderDefinition> availableOrders = new();

    [Header("Order Timing")]
    [SerializeField] private bool autoGenerateOrders = true;
    [SerializeField] private float minOrderIntervalSeconds = 30f;
    [SerializeField] private float maxOrderIntervalSeconds = 60f;
    [SerializeField] private bool generateFirstOrderImmediately = true;

    [Header("Order Slip Spawning")]
    [SerializeField] private GameObject orderSlipPrefab;
    [SerializeField] private List<Transform> orderSlipSlots = new();

    [Header("Hotel Rating (Average)")]
    [SerializeField] private UnityEngine.UI.Slider hotelRatingSlider; // optional
    [SerializeField] private int hotelMaxStarsDisplay = 5;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    // Runtime
    private Coroutine orderLoopRoutine;
    private int nextTicketId = 1;

    private readonly Dictionary<int, GOrderSlipRuntimeData> activeTickets = new();
    private readonly Dictionary<string, int> ingredientTagToNumericId = new(StringComparer.OrdinalIgnoreCase);

    private int totalDeliveredSandwiches = 0;
    private float totalStarsAccumulated = 0f;

    public event Action<GOrderSlipRuntimeData> OnOrderCreated;
    public event Action<int> OnOrderRemoved;
    public event Action<SandwichScore> OnSandwichSubmitted;

    public float HotelAverageStars => totalDeliveredSandwiches <= 0 ? 0f : (totalStarsAccumulated / totalDeliveredSandwiches);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BuildHardcodedIngredientDictionary();

        if (debugLogs)
        {
            Debug.Log("[GGameManager] Awake() Instance set: " + name);
            Debug.Log("[GGameManager] Orders configured: " + (availableOrders?.Count ?? 0));
            Debug.Log("[GGameManager] Order slip slots: " + (orderSlipSlots?.Count ?? 0));
            Debug.Log("[GGameManager] Hardcoded ingredient tag IDs loaded: " + ingredientTagToNumericId.Count);
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

        RefreshHotelSlider();
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

            if (debugLogs) Debug.Log($"[GGameManager] Next order attempt in {wait:0.0}s");
            yield return new WaitForSeconds(wait);

            // Only spawn if a slot is free
            if (TryGetFreeSlipSlot(out _))
            {
                StartNewOrder();
            }
            else if (debugLogs)
            {
                Debug.Log("[GGameManager] No free order slip slots. Skipping this spawn attempt.");
            }
        }
    }

    public void StartNewOrder(int forcedOrderIndex = -1)
    {
        if (availableOrders == null || availableOrders.Count == 0)
        {
            if (debugLogs) Debug.LogWarning("[GGameManager] No available orders configured.");
            return;
        }

        if (!TryGetFreeSlipSlot(out int freeSlotIndex))
        {
            if (debugLogs) Debug.LogWarning("[GGameManager] No free order slip slots.");
            return;
        }

        int orderIndex = forcedOrderIndex >= 0
            ? Mathf.Clamp(forcedOrderIndex, 0, availableOrders.Count - 1)
            : UnityEngine.Random.Range(0, availableOrders.Count);

        OrderDefinition selected = availableOrders[orderIndex];
        if (selected == null)
        {
            if (debugLogs) Debug.LogWarning("[GGameManager] Selected order is null.");
            return;
        }

        int ticketId = nextTicketId++;

        var runtime = new GOrderSlipRuntimeData
        {
            ticketId = ticketId,
            slotIndex = freeSlotIndex,
            orderDefinition = selected,
            slipInstance = null
        };

        // Spawn order slip prefab in slot
        if (orderSlipPrefab != null && orderSlipSlots != null && freeSlotIndex >= 0 && freeSlotIndex < orderSlipSlots.Count)
        {
            Transform slot = orderSlipSlots[freeSlotIndex];
            if (slot != null)
            {
                GameObject slip = Instantiate(orderSlipPrefab, slot.position, slot.rotation, slot);
                runtime.slipInstance = slip;
            }
        }

        activeTickets[ticketId] = runtime;

        // Let slip fill itself
        if (runtime.slipInstance != null)
        {
            GOrderSlipText slipText = runtime.slipInstance.GetComponent<GOrderSlipText>();
            if (slipText != null)
            {
                slipText.Bind(runtime);
            }
        }

        if (debugLogs)
        {
            List<string> required = GetRequiredTagsForOrder(selected);
            Debug.Log($"[GGameManager] New Order Ticket #{ticketId} in slot {freeSlotIndex}: {selected.orderName}");
            Debug.Log("[GGameManager] Required Tags: " + string.Join(", ", required));
            Debug.Log("[GGameManager] Required Numeric Signature: " + BuildNumericSignature(required));
        }

        OnOrderCreated?.Invoke(runtime);
    }

    public bool HasAnyActiveTicket()
    {
        return activeTickets.Count > 0;
    }

    public bool TryGetAnyActiveTicketId(out int ticketId)
    {
        foreach (var kv in activeTickets)
        {
            ticketId = kv.Key;
            return true;
        }

        ticketId = -1;
        return false;
    }

    public bool TryGetTicketData(int ticketId, out GOrderSlipRuntimeData data)
    {
        return activeTickets.TryGetValue(ticketId, out data);
    }

    public SandwichScore SubmitSandwichForTicket(int ticketId, List<string> deliveredTags)
    {
        SandwichScore score = new SandwichScore
        {
            ticketId = ticketId,
            deliveredTags = deliveredTags != null ? new List<string>(deliveredTags) : new List<string>()
        };

        if (!activeTickets.TryGetValue(ticketId, out var runtime) || runtime == null || runtime.orderDefinition == null)
        {
            if (debugLogs) Debug.LogWarning($"[GGameManager] Submit failed. Invalid ticketId: {ticketId}");
            score.orderName = "INVALID_TICKET";
            score.starsEarned = 0;
            score.starsPossible = 0;
            return score;
        }

        OrderDefinition order = runtime.orderDefinition;
        score.orderName = order.orderName;

        List<string> requiredTags = GetRequiredTagsForOrder(order);
        score.requiredTags = requiredTags;

        // Multiset match
        Dictionary<string, int> requiredCounts = new(StringComparer.OrdinalIgnoreCase);
        foreach (string tag in requiredTags)
        {
            if (string.IsNullOrWhiteSpace(tag)) continue;
            if (!requiredCounts.ContainsKey(tag)) requiredCounts[tag] = 0;
            requiredCounts[tag]++;
        }

        int correct = 0;
        int wrong = 0;

        if (deliveredTags != null)
        {
            foreach (string tag in deliveredTags)
            {
                if (string.IsNullOrWhiteSpace(tag))
                {
                    wrong++;
                    continue;
                }

                if (requiredCounts.TryGetValue(tag, out int count) && count > 0)
                {
                    requiredCounts[tag] = count - 1;
                    correct++;
                }
                else
                {
                    wrong++;
                }
            }
        }

        int missing = 0;
        foreach (var kv in requiredCounts)
            missing += kv.Value;

        score.correctCount = correct;
        score.wrongCount = wrong;
        score.missingCount = missing;
        score.orderPenalty = 0; // kept for compatibility if you add exact-order later

        score.starsPossible = requiredTags.Count;
        score.starsEarned = Mathf.Clamp(score.starsPossible - score.wrongCount - score.missingCount, 0, score.starsPossible);

        score.requiredNumericSignature = BuildNumericSignature(score.requiredTags);
        score.deliveredNumericSignature = BuildNumericSignature(score.deliveredTags);

        totalDeliveredSandwiches++;
        totalStarsAccumulated += score.starsEarned;
        RefreshHotelSlider();

        if (debugLogs)
        {
            Debug.Log($"[GGameManager] Ticket #{ticketId} delivered '{score.orderName}' => {score.starsEarned}/{score.starsPossible}");
            Debug.Log($"[GGameManager] Correct:{score.correctCount} Wrong:{score.wrongCount} Missing:{score.missingCount} OrderPenalty:{score.orderPenalty}");
            Debug.Log($"[GGameManager] RequiredSig:  {score.requiredNumericSignature}");
            Debug.Log($"[GGameManager] DeliveredSig: {score.deliveredNumericSignature}");
            Debug.Log($"[GGameManager] Hotel Average: {HotelAverageStars:0.00}/{hotelMaxStarsDisplay} ({totalDeliveredSandwiches} deliveries)");
        }

        OnSandwichSubmitted?.Invoke(score);

        RemoveOrderTicket(ticketId);

        return score;
    }

    private void RemoveOrderTicket(int ticketId)
    {
        if (!activeTickets.TryGetValue(ticketId, out var runtime))
            return;

        if (runtime != null && runtime.slipInstance != null)
            Destroy(runtime.slipInstance);

        int freedSlot = runtime != null ? runtime.slotIndex : -1;
        activeTickets.Remove(ticketId);

        if (debugLogs)
            Debug.Log($"[GGameManager] Removed order ticket #{ticketId}. Freed slot {freedSlot}");

        OnOrderRemoved?.Invoke(ticketId);
    }

    private void RefreshHotelSlider()
    {
        if (hotelRatingSlider == null) return;

        hotelRatingSlider.minValue = 0f;
        hotelRatingSlider.maxValue = hotelMaxStarsDisplay;
        hotelRatingSlider.value = Mathf.Clamp(HotelAverageStars, 0f, hotelMaxStarsDisplay);
    }

    private bool TryGetFreeSlipSlot(out int slotIndex)
    {
        slotIndex = -1;

        if (orderSlipSlots == null || orderSlipSlots.Count == 0)
            return false;

        bool[] used = new bool[orderSlipSlots.Count];
        foreach (var kv in activeTickets)
        {
            if (kv.Value == null) continue;
            int s = kv.Value.slotIndex;
            if (s >= 0 && s < used.Length) used[s] = true;
        }

        for (int i = 0; i < used.Length; i++)
        {
            if (!used[i] && orderSlipSlots[i] != null)
            {
                slotIndex = i;
                return true;
            }
        }

        return false;
    }

    private List<string> GetRequiredTagsForOrder(OrderDefinition order)
    {
        List<string> list = new();

        if (order == null) return list;

        if (order.ingredientIds != null)
        {
            foreach (var tag in order.ingredientIds)
            {
                if (!string.IsNullOrWhiteSpace(tag))
                    list.Add(tag.Trim());
            }
        }

        if (order.includeWine && !string.IsNullOrWhiteSpace(order.wineIngredientId))
        {
            list.Add(order.wineIngredientId.Trim());
        }

        return list;
    }

    public string BuildNumericSignature(List<string> tags)
    {
        if (tags == null || tags.Count == 0) return string.Empty;

        List<string> parts = new(tags.Count);

        foreach (string tag in tags)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                parts.Add("XXX");
                continue;
            }

            if (ingredientTagToNumericId.TryGetValue(tag.Trim(), out int id))
                parts.Add(id.ToString("000"));
            else
                parts.Add("XXX");
        }

        return string.Join("-", parts);
    }

    public bool TryGetNumericIdForTag(string tag, out int numericId)
    {
        numericId = -1;
        if (string.IsNullOrWhiteSpace(tag)) return false;
        return ingredientTagToNumericId.TryGetValue(tag.Trim(), out numericId);
    }

    private void BuildHardcodedIngredientDictionary()
    {
        ingredientTagToNumericId.Clear();

        // Hardcoded tags you gave earlier
        // raw/aged/spoiled are lowercase; suffix codes are uppercase where applicable
        string[] tags =
        {
            "rawM","agedM","spoiledM",
            "rawP","agedP","spoiledP",
            "rawC","agedC","spoiledC",
            "rawMM","agedMM","spoiledMM",
            "rawST","agedST","spoiledST",
            "rawVM","agedVM","spoiledVM",
            "rawMG","agedMG","spoiledMG",
            "rawBV","agedBV","spoiledBV",
            "lettuce",
            "mayo",
            "rawW","agedW","spoiledW"
        };

        for (int i = 0; i < tags.Length; i++)
        {
            ingredientTagToNumericId[tags[i]] = i;
        }
    }
}