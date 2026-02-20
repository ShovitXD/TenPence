using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject itemPrefab;

    [Header("Hand Names (your names)")]
    [SerializeField] private string handLName = "HandL";
    [SerializeField] private string handRName = "HandR";

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private Transform HandL;
    private Transform HandR;

    private Transform _currentItem;

    private void Start()
    {
        if (debugLogs) Debug.Log($"[ItemSpawner] Start on '{name}'");
        SpawnNew();
    }

    private void Update()
    {
        if (_currentItem == null)
        {
            if (debugLogs) Debug.Log("[ItemSpawner] _currentItem is NULL -> respawning");
            SpawnNew();
            return;
        }

        if (HandL == null || HandR == null)
            TryAutoFindHandsFromActivePlayer();

        if (HandL == null || HandR == null)
        {
            if (debugLogs) Debug.Log("[ItemSpawner] Hands not found yet. Waiting...");
            return;
        }

        if (_currentItem.IsChildOf(HandL) || _currentItem.IsChildOf(HandR))
        {
            if (debugLogs)
            {
                string which = _currentItem.IsChildOf(HandL) ? HandL.name : HandR.name;
                Debug.Log($"[ItemSpawner] Detected pickup: '{_currentItem.name}' is now child of '{which}'. Spawning new.");
            }

            SpawnNew();
        }
    }

    private void SpawnNew()
    {
        if (itemPrefab == null)
        {
            if (debugLogs) Debug.LogError("[ItemSpawner] itemPrefab is NULL. Assign it in Inspector.");
            return;
        }

        GameObject go = Instantiate(itemPrefab, transform.position, transform.rotation);
        _currentItem = go.transform;

        if (debugLogs)
            Debug.Log($"[ItemSpawner] Spawned '{go.name}' at '{name}' pos={transform.position} rot={transform.rotation.eulerAngles}");
    }

    private void TryAutoFindHandsFromActivePlayer()
    {
        // Find ALL PlayerPickupHands, pick the one on an ACTIVE GameObject
        var all = FindObjectsByType<PlayerPickupHands>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        PlayerPickupHands activePickup = null;
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == null) continue;

            // only use active in hierarchy
            if (all[i].gameObject.activeInHierarchy)
            {
                activePickup = all[i];
                break;
            }
        }

        if (activePickup == null)
        {
            if (debugLogs) Debug.LogWarning("[ItemSpawner] No ACTIVE PlayerPickupHands found (both players inactive?).");
            return;
        }

        if (HandL == null)
        {
            HandL = FindDeepChildByName(activePickup.transform, handLName);
            if (debugLogs)
            {
                if (HandL != null) Debug.Log($"[ItemSpawner] Found HandL on ACTIVE player: {GetPath(HandL, activePickup.transform)}");
                else Debug.LogWarning($"[ItemSpawner] Could not find '{handLName}' under ACTIVE player '{activePickup.name}'.");
            }
        }

        if (HandR == null)
        {
            HandR = FindDeepChildByName(activePickup.transform, handRName);
            if (debugLogs)
            {
                if (HandR != null) Debug.Log($"[ItemSpawner] Found HandR on ACTIVE player: {GetPath(HandR, activePickup.transform)}");
                else Debug.LogWarning($"[ItemSpawner] Could not find '{handRName}' under ACTIVE player '{activePickup.name}'.");
            }
        }
    }

    private static Transform FindDeepChildByName(Transform root, string nameToFind)
    {
        if (root == null || string.IsNullOrEmpty(nameToFind)) return null;

        // includes inactive children
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
