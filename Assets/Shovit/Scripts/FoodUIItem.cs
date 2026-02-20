using UnityEngine;

public class FoodUIItem : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private string foodName = "Food";
    [SerializeField] private Sprite icon;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private RoomYearTriggers ryt;
    private FoodUIRow row;
    private bool registered = false;

    // active player hands
    private Transform HandL;
    private Transform HandR;
    private const string HandLName = "HandL";
    private const string HandRName = "HandR";

    // stable id per instance
    private int uniqueId;

    private void Awake()
    {
        ryt = GetComponent<RoomYearTriggers>();
        uniqueId = GetInstanceID();
    }

    private void Update()
    {
        if (!registered)
        {
            if (TryEnsureHands() && IsHeldNow())
            {
                RegisterRowOnce();
            }
            return;
        }

        if (row == null || ryt == null) return;

        row.SetProgress(
            ryt.TotalYearsPassed,
            ryt.rawToAgedYears,
            ryt.agedToSpoiledYears
        );
    }

    private void RegisterRowOnce()
    {
        if (registered) return;

        if (FoodUIRegistry.Instance == null)
        {
            if (debugLogs) Debug.LogWarning("[FoodUIItem] FoodUIRegistry.Instance not found in scene.");
            return;
        }

        row = FoodUIRegistry.Instance.RegisterOrGetRow(uniqueId, icon, foodName);
        registered = row != null;

        if (debugLogs && registered)
            Debug.Log($"[FoodUIItem] Registered row for '{foodName}' id={uniqueId}");
    }

    private bool IsHeldNow()
    {
        if (HandL == null || HandR == null) return false;
        return transform.IsChildOf(HandL) || transform.IsChildOf(HandR);
    }

    private bool TryEnsureHands()
    {
        if (HandL != null && HandR != null) return true;

        var all = FindObjectsByType<PlayerPickupHands>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        PlayerPickupHands activePickup = null;
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && all[i].gameObject.activeInHierarchy)
            {
                activePickup = all[i];
                break;
            }
        }

        if (activePickup == null) return false;

        HandL = FindDeepChildByName(activePickup.transform, HandLName);
        HandR = FindDeepChildByName(activePickup.transform, HandRName);

        return HandL != null && HandR != null;
    }

    private static Transform FindDeepChildByName(Transform root, string nameToFind)
    {
        if (root == null || string.IsNullOrEmpty(nameToFind)) return null;

        Transform[] all = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && all[i].name == nameToFind)
                return all[i];
        }

        return null;
    }
}
