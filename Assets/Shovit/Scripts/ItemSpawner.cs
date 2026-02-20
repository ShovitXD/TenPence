using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject itemPrefab;

    [Header("Hand Names (your names)")]
    [SerializeField] private string handLName = "HandL";
    [SerializeField] private string handRName = "HandR";

    [Header("Drag Lock Until Equipped")]
    [SerializeField] private bool disableDragUntilPickedUp = true;

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

        bool inLeftHand = _currentItem.IsChildOf(HandL);
        bool inRightHand = _currentItem.IsChildOf(HandR);

        // When the player equips/picks the item, enable DragItem on THIS item, then spawn the next one.
        if (inLeftHand || inRightHand)
        {
            EnableDragIfPresent(_currentItem.gameObject);

            if (debugLogs)
            {
                string which = inLeftHand ? HandL.name : HandR.name;
                Debug.Log($"[ItemSpawner] Detected pickup: '{_currentItem.name}' is now child of '{which}'. Enabling DragItem + spawning new.");
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

        if (disableDragUntilPickedUp)
            DisableDragIfPresent(go);

        if (debugLogs)
            Debug.Log($"[ItemSpawner] Spawned '{go.name}' at '{name}' pos={transform.position} rot={transform.rotation.eulerAngles}");
    }

    private void DisableDragIfPresent(GameObject go)
    {
        if (go == null) return;

        DragItem drag = go.GetComponent<DragItem>();
        if (drag != null)
        {
            drag.enabled = false;
            if (debugLogs) Debug.Log($"[ItemSpawner] DragItem DISABLED on '{go.name}' until equipped.");
        }
    }

    private void EnableDragIfPresent(GameObject go)
    {
        if (go == null) return;

        DragItem drag = go.GetComponent<DragItem>();
        if (drag != null && !drag.enabled)
        {
            drag.enabled = true;
            if (debugLogs) Debug.Log($"[ItemSpawner] DragItem ENABLED on '{go.name}' (equipped by player).");
        }
    }

    private void TryAutoFindHandsFromActivePlayer()
    {
        // Find ALL PlayerPickupHands, pick the one on an ACTIVE GameObject
        var all = FindObjectsByType<PlayerPickupHands>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        PlayerPickupHands activePickup = null;
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == null) continue;

            if (all[i].gameObject.activeInHierarchy)
            {
                activePickup = all[i];
                break;
            }
        }

        if (activePickup == null)
        {
            if (debugLogs) Debug.LogWarning("[ItemSpawner] No ACTIVE PlayerPickupHands found.");
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