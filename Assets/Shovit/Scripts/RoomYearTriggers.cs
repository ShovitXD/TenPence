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

        [Tooltip("If true, fires once total (lifetime). If false, can fire again every frame after threshold.")]
        public bool triggerOnce = true;

        [Tooltip("Called when TotalYearsPassed >= YearsToPass")]
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
    [SerializeField] private int currentRoomYear = 0;

    [Tooltip("Sum of years experienced across all rooms (does NOT reset on room change).")]
    [SerializeField] private int totalYearsPassed = 0;

    private int _lastRoomIndex = int.MinValue;
    private int _lastObservedYearInThatRoom = 0;

    public int CurrentRoomIndex => currentRoomIndex;
    public int CurrentRoomYear => currentRoomYear;
    public int TotalYearsPassed => totalYearsPassed;

    private void Awake()
    {
        if (roomManager == null) roomManager = FindFirstObjectByType<RoomManager>();
        if (yearDirector == null) yearDirector = FindFirstObjectByType<RoomYearDirector>();
    }

    private void Start()
    {
        // Initialize
        RefreshRoom(force: true);
        RefreshAndAccumulateYears();
        EvaluateTriggers();
    }

    private void Update()
    {
        RefreshRoom(force: false);
        RefreshAndAccumulateYears();
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
            _lastRoomIndex = newRoom;

            if (currentRoomIndex >= 0)
            {
                // When entering a room, start tracking from the room's current year
                _lastObservedYearInThatRoom = yearDirector.GetRoomYearInt(currentRoomIndex);
            }
            else
            {
                _lastObservedYearInThatRoom = 0;
            }
        }
    }

    private void RefreshAndAccumulateYears()
    {
        if (yearDirector == null || currentRoomIndex < 0)
        {
            currentRoomYear = 0;
            return;
        }

        currentRoomYear = yearDirector.GetRoomYearInt(currentRoomIndex);

        // Add only positive forward progress since last frame in THIS room
        int delta = currentRoomYear - _lastObservedYearInThatRoom;
        if (delta > 0)
            totalYearsPassed += delta;

        _lastObservedYearInThatRoom = currentRoomYear;
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

            if (totalYearsPassed >= t.yearsToPass)
            {
                t.fired = true;
                t.onTriggered?.Invoke();
            }
        }
    }
}
