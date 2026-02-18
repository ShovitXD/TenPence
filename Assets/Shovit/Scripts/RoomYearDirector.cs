using System;
using UnityEngine;

public class RoomYearDirector : MonoBehaviour
{
    // Your fixed indices:
    // 0 = RoomS, 1 = RoomN, 2 = RoomF
    public const int RoomS = 0;
    public const int RoomN = 1;
    public const int RoomF = 2;

    // Defines how many real seconds are in 1 "year" for the Normal case (1 sec = 1 sec).
    // Using 365 days. Change if you want a different calendar.
    private const double SecondsPerYear = 365.0 * 24.0 * 60.0 * 60.0; // 31,536,000

    [Serializable]
    public struct TimePreset
    {
        [Min(0.000001f)] public float seconds; // X
        [Min(0f)] public float years;          // Y
        public double YearsPerSecond => years / Math.Max(0.000001, seconds);
    }

    [Header("References")]
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private Transform player;

    [Header("Starting Years (index 0=S, 1=N, 2=F)")]
    [SerializeField] private double[] roomYears = new double[3] { 100, 2026, 3000 };

    [Header("Speed Presets (designer editable: X seconds = Y years)")]
    [Tooltip("Slow: used for RoomS when player is in RoomN. (Default: 1 year = 1 sec)")]
    [SerializeField] private TimePreset slow = new TimePreset { seconds = 31_536_000f, years = 1f };

    [Tooltip("Super Slow: used for RoomS when player is in RoomF. (Default: 10 years = 1 sec)")]
    [SerializeField] private TimePreset superSlow = new TimePreset { seconds = 315_360_000f, years = 1f };

    [Tooltip("Fast: used for RoomF when player is in RoomN, and RoomN when player is in RoomS. (Default: 1 sec = 1 year)")]
    [SerializeField] private TimePreset fast = new TimePreset { seconds = 1f, years = 1f };

    [Tooltip("Super Fast: used for RoomF when player is in RoomS. (Default: 1 sec = 10 years)")]
    [SerializeField] private TimePreset superFast = new TimePreset { seconds = 1f, years = 10f };

    [Header("Debug")]
    [SerializeField] private bool logPlayerRoomChanges = false;

    // Normal (not exposed): 1 sec = 1 sec
    // => 1 real second advances the year-counter by 1 second worth of a year.
    private static readonly double normalYearsPerSecond = 1.0 / SecondsPerYear;

    public int CurrentPlayerRoomIndex { get; private set; } = -1;

    private int _lastPlayerRoomIndex = int.MinValue;

    private void Reset()
    {
        roomManager = FindFirstObjectByType<RoomManager>();
    }

    private void Update()
    {
        if (roomManager == null || player == null || roomYears == null || roomYears.Length < 3)
            return;

        // Detect player room
        CurrentPlayerRoomIndex = roomManager.GetRoomIndex(player.position);

        if (logPlayerRoomChanges && CurrentPlayerRoomIndex != _lastPlayerRoomIndex)
        {
            _lastPlayerRoomIndex = CurrentPlayerRoomIndex;
            Debug.Log($"RoomYearDirector: Player room index = {CurrentPlayerRoomIndex}");
        }

        double dt = Time.deltaTime;

        // Advance each room year by its computed "years per real second"
        for (int i = 0; i < 3; i++)
        {
            double yps = GetYearsPerSecondForRoom(i, CurrentPlayerRoomIndex);
            roomYears[i] += yps * dt;
        }
    }

    /// <summary>
    /// Returns YEARS per REAL second for a target room, based on where the player is.
    ///
    /// Baseline when player in RoomN:
    /// - RoomN: Normal (1 sec = 1 sec)
    /// - RoomS: Slow   (1 year = 1 sec)
    /// - RoomF: Fast   (1 sec = 1 year)
    ///
    /// When player in RoomS:
    /// - RoomN becomes Fast
    /// - RoomF becomes Super Fast
    /// - RoomS stays Slow
    ///
    /// When player in RoomF:
    /// - RoomN becomes Slow
    /// - RoomS becomes Super Slow
    /// - RoomF stays Fast
    /// </summary>
    private double GetYearsPerSecondForRoom(int targetRoom, int playerRoom)
    {
        // Baseline (player in RoomN)
        double s = slow.YearsPerSecond;
        double n = normalYearsPerSecond;
        double f = fast.YearsPerSecond;

        if (playerRoom == RoomS)
        {
            n = fast.YearsPerSecond;
            f = superFast.YearsPerSecond;
        }
        else if (playerRoom == RoomF)
        {
            n = slow.YearsPerSecond;
            s = superSlow.YearsPerSecond;
        }

        return targetRoom switch
        {
            RoomS => s,
            RoomN => n,
            RoomF => f,
            _ => normalYearsPerSecond
        };
    }

    // ---------- Public API (years as double) ----------
    public double GetRoomYear(int roomIndex)
    {
        if (roomYears == null || roomIndex < 0 || roomIndex >= roomYears.Length) return 0;
        return roomYears[roomIndex];
    }

    // ---------- Public API (years as int) ----------
    public int GetRoomYearInt(int roomIndex)
    {
        double y = GetRoomYear(roomIndex);
        return (int)Math.Floor(y);
    }

    public int[] GetAllRoomYearsInt()
    {
        if (roomYears == null) return Array.Empty<int>();

        int[] result = new int[roomYears.Length];
        for (int i = 0; i < roomYears.Length; i++)
            result[i] = (int)Math.Floor(roomYears[i]);

        return result;
    }

    public void GetAllRoomYearsInt(out int yearS, out int yearN, out int yearF)
    {
        yearS = GetRoomYearInt(RoomS);
        yearN = GetRoomYearInt(RoomN);
        yearF = GetRoomYearInt(RoomF);
    }
}
