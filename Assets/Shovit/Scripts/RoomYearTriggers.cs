using System;
using UnityEngine;
using UnityEngine.Events;

public class RoomYearTriggers : MonoBehaviour
{
    [Serializable]
    public class TriggerElement
    {
        [Min(0)] public int yearsToPass = 10;
        public bool triggerOnce = true;
        public UnityEvent onTriggered;

        [NonSerialized] public bool fired;
    }

    [Header("References (auto from RoomHandler)")]
    [SerializeField] private string roomHandlerName = "RoomHandler";
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private RoomYearDirector yearDirector;

    [Header("Spoil Thresholds (years)")]
    public int rawToAgedYears = 10;
    public int agedToSpoiledYears = 10;

    [Header("Triggers (add as many as you want)")]
    public TriggerElement[] triggers;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private bool debugRoomHandlerComponents = false;

    [Header("Read-only Debug")]
    [SerializeField] private int currentRoomIndex = -1;
    [SerializeField] private int currentRoomYear = 0;

    [Tooltip("Sum of years experienced across all rooms (does NOT reset on room change).")]
    [SerializeField] private int totalYearsPassed = 0;

    private int _lastObservedYearInThatRoom = 0;

    public int CurrentRoomIndex => currentRoomIndex;
    public int CurrentRoomYear => currentRoomYear;
    public int TotalYearsPassed => totalYearsPassed;

    private void Awake()
    {
        EnsureRefs();
    }

    private void OnEnable()
    {
        EnsureRefs();
        RefreshRoom(force: true);
    }

    private void Start()
    {
        EnsureRefs();
        RefreshRoom(force: true);
        RefreshAndAccumulateYears();
        EvaluateTriggers();
    }

    private void Update()
    {
        if (!EnsureRefs())
            return;

        RefreshRoom(force: false);
        RefreshAndAccumulateYears();
        EvaluateTriggers();
    }

    private bool EnsureRefs()
    {
        if (roomManager != null && yearDirector != null)
            return true;

        GameObject roomHandler = GameObject.Find(roomHandlerName);
        if (roomHandler == null)
        {
            if (debugLogs)
                Debug.LogWarning($"[RoomYearTriggers] '{name}' -> GameObject '{roomHandlerName}' not found.");
            return false;
        }

        if (debugRoomHandlerComponents)
        {
            var comps = roomHandler.GetComponents<Component>();
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i] == null) continue;
                Debug.Log($"[RoomYearTriggers] RoomHandler component: {comps[i].GetType().FullName}");
            }
        }

        if (roomManager == null)
            roomManager = roomHandler.GetComponent<RoomManager>();

        if (yearDirector == null)
            yearDirector = roomHandler.GetComponent<RoomYearDirector>();

        if (debugLogs)
        {
            Debug.Log($"[RoomYearTriggers] EnsureRefs '{name}' -> " +
                      $"roomManager={(roomManager ? "FOUND" : "NULL")} " +
                      $"yearDirector={(yearDirector ? "FOUND" : "NULL")} " +
                      $"(RoomHandler='{roomHandler.name}')");
        }

        return roomManager != null && yearDirector != null;
    }

    private void RefreshRoom(bool force)
    {
        if (roomManager == null || yearDirector == null)
            return;

        // Try object position first
        int newRoom = roomManager.GetRoomIndex(transform.position);

        // If object isn't inside any room (common when held), fall back to player's room
        if (newRoom < 0)
            newRoom = yearDirector.CurrentPlayerRoomIndex;

        if (force || newRoom != currentRoomIndex)
        {
            currentRoomIndex = newRoom;

            if (currentRoomIndex >= 0)
                _lastObservedYearInThatRoom = yearDirector.GetRoomYearInt(currentRoomIndex);
            else
                _lastObservedYearInThatRoom = 0;

            if (debugLogs)
                Debug.Log($"[RoomYearTriggers] '{name}' roomIndex={currentRoomIndex} baselineYear={_lastObservedYearInThatRoom}");
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

        int delta = currentRoomYear - _lastObservedYearInThatRoom;
        if (delta > 0)
        {
            totalYearsPassed += delta;

            if (debugLogs)
                Debug.Log($"[RoomYearTriggers] '{name}' +{delta} years (Total={totalYearsPassed}) roomIndex={currentRoomIndex} yearNow={currentRoomYear}");
        }

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
