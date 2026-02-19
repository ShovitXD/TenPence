using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class RoomTeleporterIA : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoomRelativeTeleporter teleporter;

    [Header("Action Map / Action Names")]
    [SerializeField] private string actionMapName = "Player";
    [SerializeField] private string teleport0ActionName = "TeleportRoom0";
    [SerializeField] private string teleport1ActionName = "TeleportRoom1";
    [SerializeField] private string teleport2ActionName = "TeleportRoom2";

    [Header("Debug")]
    [SerializeField] private bool logRoomIndexOnPress = true;

    private PlayerInput playerInput;
    private InputAction tp0, tp1, tp2;

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

        if (tp0 == null) Debug.LogError($"RoomTeleporterIA: Missing action '{teleport0ActionName}'.");
        if (tp1 == null) Debug.LogError($"RoomTeleporterIA: Missing action '{teleport1ActionName}'.");
        if (tp2 == null) Debug.LogError($"RoomTeleporterIA: Missing action '{teleport2ActionName}'.");

        if (tp0 != null) { tp0.performed += OnTp0; tp0.Enable(); }
        if (tp1 != null) { tp1.performed += OnTp1; tp1.Enable(); }
        if (tp2 != null) { tp2.performed += OnTp2; tp2.Enable(); }
    }

    private void OnDisable()
    {
        if (tp0 != null) { tp0.performed -= OnTp0; tp0.Disable(); }
        if (tp1 != null) { tp1.performed -= OnTp1; tp1.Disable(); }
        if (tp2 != null) { tp2.performed -= OnTp2; tp2.Disable(); }
    }

    private void OnTp0(InputAction.CallbackContext ctx) => Teleport(0);
    private void OnTp1(InputAction.CallbackContext ctx) => Teleport(1);
    private void OnTp2(InputAction.CallbackContext ctx) => Teleport(2);

    private void Teleport(int roomIndex)
    {
        if (teleporter == null)
        {
            Debug.LogError("RoomTeleporterIA: RoomRelativeTeleporter not found/assigned.");
            return;
        }

        if (logRoomIndexOnPress)
        {
            Debug.Log($"[RoomTeleporterIA] Pressed teleport -> currentRoom={teleporter.CurrentRoomIndex}, targetRoom={roomIndex}");
        }

        bool ok = teleporter.TeleportToRoom(roomIndex);
        if (!ok)
            Debug.LogWarning($"RoomTeleporterIA: TeleportToRoom({roomIndex}) failed.");
    }
}
