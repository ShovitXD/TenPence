using TMPro;
using UnityEngine;

public class RoomYearText3D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoomYearDirector yearDirector;

    [Header("3D TextMeshPro (index 0=RoomS, 1=RoomN, 2=RoomF)")]
    [SerializeField] private TMP_Text[] roomYearTexts = new TMP_Text[3];

    [Header("Update")]
    [Tooltip("How many times per second to update. 0 = every frame.")]
    [SerializeField] private float updatesPerSecond = 5f;

    private float _timer;

    private void Awake()
    {
        if (yearDirector == null)
            yearDirector = FindFirstObjectByType<RoomYearDirector>();
    }

    private void Update()
    {
        if (yearDirector == null || roomYearTexts == null || roomYearTexts.Length < 3)
            return;

        if (updatesPerSecond <= 0f)
        {
            UpdateTexts();
            return;
        }

        _timer += Time.deltaTime;
        float interval = 1f / updatesPerSecond;

        if (_timer >= interval)
        {
            _timer = 0f;
            UpdateTexts();
        }
    }

    private void UpdateTexts()
    {
        // Get all years as ints (expects indices 0..2)
        int[] years = yearDirector.GetAllRoomYearsInt();
        if (years == null || years.Length < 3)
            return;

        // 0 = RoomS
        if (roomYearTexts[0] != null)
            roomYearTexts[0].text = years[0].ToString();

        // 1 = RoomN
        if (roomYearTexts[1] != null)
            roomYearTexts[1].text = years[1].ToString();

        // 2 = RoomF
        if (roomYearTexts[2] != null)
            roomYearTexts[2].text = years[2].ToString();
    }
}
