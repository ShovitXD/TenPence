using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class RoomTeleporterIA : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoomRelativeTeleporter teleporter;

    [Header("Settings UI")]
    [Tooltip("Panel GameObject to toggle on Settings input.")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Action Map / Action Names")]
    [Tooltip("This script switches PlayerInput to this map on enable. Make sure Settings is in THIS map.")]
    [SerializeField] private string actionMapName = "Player";
    [SerializeField] private string teleport0ActionName = "TeleportRoom0";
    [SerializeField] private string teleport1ActionName = "TeleportRoom1";
    [SerializeField] private string teleport2ActionName = "TeleportRoom2";
    [SerializeField] private string settingsActionName = "Settings";

    [Header("Debug")]
    [SerializeField] private bool logRoomIndexOnPress = true;
    [SerializeField] private bool logActionLookup = false;

    private PlayerInput playerInput;
    private InputAction tp0, tp1, tp2, settings;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (teleporter == null)
            teleporter = GetComponent<RoomRelativeTeleporter>();
    }

    private void OnEnable()
    {
        if (playerInput == null)
        {
            Debug.LogError("RoomTeleporterIA: PlayerInput missing.");
            return;
        }

        if (playerInput.actions == null)
        {
            Debug.LogError("RoomTeleporterIA: PlayerInput.actions is null. Assign an InputActionAsset.");
            return;
        }

        // Force map (IMPORTANT: Settings must be in this map)
        if (!string.IsNullOrEmpty(actionMapName))
            playerInput.SwitchCurrentActionMap(actionMapName);

        var map = playerInput.actions.FindActionMap(actionMapName, false);
        if (map == null)
        {
            Debug.LogError($"RoomTeleporterIA: ActionMap '{actionMapName}' not found.");
            return;
        }

        tp0 = map.FindAction(teleport0ActionName, false);
        tp1 = map.FindAction(teleport1ActionName, false);
        tp2 = map.FindAction(teleport2ActionName, false);
        settings = map.FindAction(settingsActionName, false);

        if (logActionLookup)
        {
            Debug.Log($"[RoomTeleporterIA] Using ActionMap='{actionMapName}'");
            Debug.Log($"[RoomTeleporterIA] Found '{teleport0ActionName}'? {tp0 != null}");
            Debug.Log($"[RoomTeleporterIA] Found '{teleport1ActionName}'? {tp1 != null}");
            Debug.Log($"[RoomTeleporterIA] Found '{teleport2ActionName}'? {tp2 != null}");
            Debug.Log($"[RoomTeleporterIA] Found '{settingsActionName}'? {settings != null}");
        }

        if (tp0 == null) Debug.LogError($"RoomTeleporterIA: Missing action '{teleport0ActionName}'.");
        if (tp1 == null) Debug.LogError($"RoomTeleporterIA: Missing action '{teleport1ActionName}'.");
        if (tp2 == null) Debug.LogError($"RoomTeleporterIA: Missing action '{teleport2ActionName}'.");
        if (settings == null) Debug.LogError($"RoomTeleporterIA: Missing action '{settingsActionName}'.");

        if (tp0 != null) { tp0.performed += OnTp0; tp0.Enable(); }
        if (tp1 != null) { tp1.performed += OnTp1; tp1.Enable(); }
        if (tp2 != null) { tp2.performed += OnTp2; tp2.Enable(); }

        // IMPORTANT: only subscribe via code (prevents double-toggle if PlayerInput is on Send Messages)
        if (settings != null) { settings.performed += OnSettingsPerformed; settings.Enable(); }
    }

    private void OnDisable()
    {
        if (tp0 != null) { tp0.performed -= OnTp0; tp0.Disable(); }
        if (tp1 != null) { tp1.performed -= OnTp1; tp1.Disable(); }
        if (tp2 != null) { tp2.performed -= OnTp2; tp2.Disable(); }
        if (settings != null) { settings.performed -= OnSettingsPerformed; settings.Disable(); }
    }

    private void OnTp0(InputAction.CallbackContext ctx) => Teleport(0);
    private void OnTp1(InputAction.CallbackContext ctx) => Teleport(1);
    private void OnTp2(InputAction.CallbackContext ctx) => Teleport(2);

    private void OnSettingsPerformed(InputAction.CallbackContext ctx) => ToggleSettingsPanel();

    private void ToggleSettingsPanel()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("RoomTeleporterIA: settingsPanel not assigned.");
            return;
        }

        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    private void Teleport(int roomIndex)
    {
        if (teleporter == null)
        {
            Debug.LogError("RoomTeleporterIA: RoomRelativeTeleporter not found/assigned.");
            return;
        }

        if (logRoomIndexOnPress)
            Debug.Log($"[RoomTeleporterIA] Pressed teleport -> currentRoom={teleporter.CurrentRoomIndex}, targetRoom={roomIndex}");

        bool ok = teleporter.TeleportToRoom(roomIndex);
        if (!ok)
            Debug.LogWarning($"RoomTeleporterIA: TeleportToRoom({roomIndex}) failed.");
    }
}
