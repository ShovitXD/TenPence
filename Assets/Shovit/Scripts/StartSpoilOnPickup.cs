using UnityEngine;

public class StartSpoilOnPickup : MonoBehaviour
{
    [Header("Hand Names (your names)")]
    [SerializeField] private string handLName = "HandL";
    [SerializeField] private string handRName = "HandR";

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private RoomYearTriggers _ryt;
    private bool _started = false;

    private Transform HandL;
    private Transform HandR;

    private Transform _pickupRoot; // the real object that gets parented (rigidbody root)

    private void Awake()
    {
        _pickupRoot = GetPickupRoot(transform);

        // IMPORTANT: always control the triggers on the pickup root
        _ryt = _pickupRoot != null ? _pickupRoot.GetComponent<RoomYearTriggers>() : null;

        if (_ryt != null)
        {
            _ryt.enabled = false;

            if (debugLogs)
                Debug.Log($"[StartSpoilOnPickup] DISABLE RYT on '{_pickupRoot.name}' id={_pickupRoot.GetInstanceID()} (this={name} id={GetInstanceID()})");
        }
        else
        {
            if (debugLogs)
                Debug.LogWarning($"[StartSpoilOnPickup] RoomYearTriggers NOT FOUND on pickup root. this='{name}' root='{(_pickupRoot ? _pickupRoot.name : "NULL")}'");
        }
    }

    private void Update()
    {
        if (_started) return;
        if (_ryt == null) return;

        if (HandL == null || HandR == null)
            TryAutoFindHandsFromActivePlayer();

        if (HandL == null || HandR == null)
            return;

        // Check using the pickup root (not this transform)
        if (_pickupRoot.IsChildOf(HandL) || _pickupRoot.IsChildOf(HandR))
        {
            _ryt.enabled = true;
            _started = true;

            if (debugLogs)
            {
                string which = _pickupRoot.IsChildOf(HandL) ? HandL.name : HandR.name;
                Debug.Log($"[StartSpoilOnPickup] ENABLE RYT on '{_pickupRoot.name}' id={_pickupRoot.GetInstanceID()} in {which} (enabled={_ryt.enabled})");
            }
        }
    }

    private void TryAutoFindHandsFromActivePlayer()
    {
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

        if (activePickup == null)
        {
            if (debugLogs) Debug.LogWarning("[StartSpoilOnPickup] No ACTIVE PlayerPickupHands found.");
            return;
        }

        if (HandL == null)
        {
            HandL = FindDeepChildByName(activePickup.transform, handLName);
            if (debugLogs && HandL != null) Debug.Log($"[StartSpoilOnPickup] Found HandL: {GetPath(HandL, activePickup.transform)}");
        }

        if (HandR == null)
        {
            HandR = FindDeepChildByName(activePickup.transform, handRName);
            if (debugLogs && HandR != null) Debug.Log($"[StartSpoilOnPickup] Found HandR: {GetPath(HandR, activePickup.transform)}");
        }
    }

    private static Transform GetPickupRoot(Transform t)
    {
        if (t == null) return null;

        // If there is a rigidbody in parents, that's usually the pickup root
        var rb = t.GetComponentInParent<Rigidbody>();
        if (rb != null) return rb.transform;

        // else fallback to topmost
        return t.root;
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

    private static string GetPath(Transform t, Transform stopAt)
    {
        if (t == null) return "NULL";
        string path = t.name;

        Transform cur = t.parent;
        while (cur != null && cur != stopAt)
        {
            path = cur.name + "/" + path;
            cur = cur.parent;
        }

        if (stopAt != null)
            path = stopAt.name + "/" + path;

        return path;
    }
}
