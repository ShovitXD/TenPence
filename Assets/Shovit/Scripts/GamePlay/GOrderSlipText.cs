using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GOrderSlipText : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private Image foodImage;
    [SerializeField] private TMP_Text foodNameText;
    [SerializeField] private TMP_Text orderNumberText;

    [Header("Fallback (Auto Find GGameManager if needed)")]
    [SerializeField] private GGameManager gameManager;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private int boundTicketId = -1;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GGameManager>();
    }

    public void Bind(GGameManager.GOrderSlipRuntimeData runtimeData)
    {
        if (runtimeData == null || runtimeData.orderDefinition == null)
        {
            if (debugLogs) Debug.LogWarning("[GOrderSlipText] Bind failed: runtimeData or orderDefinition is null.");
            return;
        }

        boundTicketId = runtimeData.ticketId;

        // Food image
        if (foodImage != null)
        {
            foodImage.sprite = runtimeData.orderDefinition.foodImage;
            foodImage.enabled = (runtimeData.orderDefinition.foodImage != null);
        }

        // Food name
        if (foodNameText != null)
            foodNameText.text = runtimeData.orderDefinition.orderName;

        // Order number
        if (orderNumberText != null)
            orderNumberText.text = $"Order #{runtimeData.ticketId}";

        if (debugLogs)
            Debug.Log($"[GOrderSlipText] Bound slip -> Ticket #{runtimeData.ticketId}, Name='{runtimeData.orderDefinition.orderName}'");
    }

    // Optional fallback: if slip spawns first and Bind() happens a frame later, no issue.
    // This script doesn't need to poll.
}