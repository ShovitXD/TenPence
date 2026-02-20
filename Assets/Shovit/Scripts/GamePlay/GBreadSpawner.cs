using System.Collections;
using UnityEngine;

public class GBreadSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject breadPrefab;

    [Header("Refs")]
    [SerializeField] private GGameManager gameManager;

    [Header("Hand Names")]
    [SerializeField] private string handLName = "HandL";
    [SerializeField] private string handRName = "HandR";

    [Header("Behavior")]
    [SerializeField] private bool disableDragUntilPicked = true;
    [SerializeField] private bool waitForActiveTicketBeforeSpawn = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private Transform handL;
    private Transform handR;
    private Transform currentBread;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GGameManager>();
    }

    private void Start()
    {
        if (debugLogs) Debug.Log($"[GBreadSpawner] Start on '{name}'");

        if (waitForActiveTicketBeforeSpawn)
            StartCoroutine(SpawnWhenTicketAvailable());
        else
            SpawnNewBread();
    }

    private void Update()
    {
        if (currentBread == null)
            return;

        if (handL == null || handR == null)
            TryAutoFindHandsFromActivePlayer();

        if (handL == null || handR == null)
            return;

        if (currentBread.IsChildOf(handL) || currentBread.IsChildOf(handR))
        {
            if (debugLogs)
            {
                string which = currentBread.IsChildOf(handL) ? handL.name : handR.name;
                Debug.Log($"[GBreadSpawner] Detected pickup: '{currentBread.name}' is now child of '{which}'. Enabling DragItem + spawning next bread.");
            }

            // enable drag now
            if (disableDragUntilPicked && currentBread != null)
            {
                DragItem drag = currentBread.GetComponent<DragItem>();
                if (drag != null) drag.enabled = true;
            }

            // stop tracking this one
            currentBread = null;

            // spawn next one (wait for ticket if needed)
            if (waitForActiveTicketBeforeSpawn)
                StartCoroutine(SpawnWhenTicketAvailable());
            else
                SpawnNewBread();
        }
    }

    private IEnumerator SpawnWhenTicketAvailable()
    {
        while (true)
        {
            if (gameManager == null)
                gameManager = FindFirstObjectByType<GGameManager>();

            if (gameManager != null && gameManager.HasAnyActiveTicket())
            {
                SpawnNewBread();
                yield break;
            }

            yield return null;
        }
    }

    private void SpawnNewBread()
    {
        if (breadPrefab == null)
        {
            if (debugLogs) Debug.LogError("[GBreadSpawner] breadPrefab is NULL.");
            return;
        }

        GameObject go = Instantiate(breadPrefab, transform.position, transform.rotation);
        currentBread = go.transform;

        // Disable DragItem until player picks it up
        if (disableDragUntilPicked)
        {
            DragItem drag = go.GetComponent<DragItem>();
            if (drag != null)
                drag.enabled = false;
        }

        AssignTicketToBread(go);

        if (debugLogs)
            Debug.Log($"[GBreadSpawner] Spawned '{go.name}' at '{name}' pos={transform.position} rot={transform.rotation.eulerAngles}");
    }

    private void AssignTicketToBread(GameObject bread)
    {
        if (bread == null) return;

        GSandwichBreadStack stack = bread.GetComponent<GSandwichBreadStack>();
        if (stack == null)
        {
            if (debugLogs) Debug.LogWarning("[GBreadSpawner] Spawned bread has no GSandwichBreadStack.");
            return;
        }

        if (gameManager == null)
            gameManager = FindFirstObjectByType<GGameManager>();

        if (gameManager == null)
        {
            stack.SetTicketId(-1);
            if (debugLogs) Debug.LogWarning("[GBreadSpawner] No GGameManager found. TicketId = -1");
            return;
        }

        if (gameManager.TryGetAnyActiveTicketId(out int ticketId))
        {
            stack.SetTicketId(ticketId);
            if (debugLogs) Debug.Log($"[GBreadSpawner] Assigned Ticket #{ticketId} to '{bread.name}'");
        }
        else
        {
            stack.SetTicketId(-1);
            if (debugLogs) Debug.LogWarning("[GBreadSpawner] No active order tickets available. Bread will get ticketId = -1");
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

        if (activePickup == null) return;

        if (handL == null)
        {
            handL = FindDeepChildByName(activePickup.transform, handLName);
            if (debugLogs && handL != null)
                Debug.Log($"[GBreadSpawner] Found HandL: {GetPath(handL, activePickup.transform)}");
        }

        if (handR == null)
        {
            handR = FindDeepChildByName(activePickup.transform, handRName);
            if (debugLogs && handR != null)
                Debug.Log($"[GBreadSpawner] Found HandR: {GetPath(handR, activePickup.transform)}");
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