using System;
using UnityEngine;
using UnityEngine.Events;

public class RoomYearTriggers : MonoBehaviour
{
    [Serializable]
    public class TriggerElement
    {
        [Min(0)]
        public int yearsToPass = 10;

        [Tooltip("If true, fires once per room-entry. If false, can fire again every frame after threshold (usually keep true).")]
        public bool triggerOnce = true;

        [Tooltip("Called when YearsPassedInRoom >= YearsToPass")]
        public UnityEvent onTriggered;

        [NonSerialized] public bool fired;
    }

    [Header("References")]
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private RoomYearDirector yearDirector;

    [Header("Triggers (add as many elements as you want)")]
    public TriggerElement[] triggers;

    [Header("Read-only Debug")]
    [SerializeField] private int currentRoomIndex = -1;
    [SerializeField] private int entryYear = 0;
    [SerializeField] private int currentRoomYear = 0;
    [SerializeField] private int yearsPassedInRoom = 0;

    public int CurrentRoomIndex => currentRoomIndex;
    public int EntryYear => entryYear;
    public int CurrentRoomYear => currentRoomYear;
    public int YearsPassedInRoom => yearsPassedInRoom;

    private void Awake()
    {
        if (roomManager == null) roomManager = FindFirstObjectByType<RoomManager>();
        if (yearDirector == null) yearDirector = FindFirstObjectByType<RoomYearDirector>();
    }

    private void Start()
    {
        // Initialize on spawn
        RefreshRoom(force: true);
        RefreshYearsPassed();
        EvaluateTriggers();
    }

    private void Update()
    {
        RefreshRoom(force: false);
        RefreshYearsPassed();
        EvaluateTriggers();
    }

    private void RefreshRoom(bool force)
    {
        if (roomManager == null || yearDirector == null)
            return;

        int newRoom = roomManager.GetRoomIndex(transform.position);

        if (force || newRoom != currentRoomIndex)
        {
            currentRoomIndex = newRoom;

            if (currentRoomIndex >= 0)
            {
                // Record the room's CURRENT year as the entry year
                entryYear = yearDirector.GetRoomYearInt(currentRoomIndex);
            }
            else
            {
                entryYear = 0;
            }

            ResetTriggerFlags(); // per-room-entry behavior
        }
    }

    private void RefreshYearsPassed()
    {
        if (yearDirector == null || currentRoomIndex < 0)
        {
            currentRoomYear = 0;
            yearsPassedInRoom = 0;
            return;
        }

        currentRoomYear = yearDirector.GetRoomYearInt(currentRoomIndex);
        yearsPassedInRoom = Mathf.Max(0, currentRoomYear - entryYear);
    }

    private void EvaluateTriggers()
    {
        if (triggers == null || triggers.Length == 0)
            return;

        for (int i = 0; i < triggers.Length; i++)
        {
            TriggerElement t = triggers[i];
            if (t == null) continue;

            if (t.triggerOnce && t.fired)
                continue;

            if (yearsPassedInRoom >= t.yearsToPass)
            {
                t.fired = true;
                t.onTriggered?.Invoke();
            }
        }
    }

    private void ResetTriggerFlags()
    {
        if (triggers == null) return;

        for (int i = 0; i < triggers.Length; i++)
        {
            if (triggers[i] != null)
                triggers[i].fired = false;
        }
    }
}
