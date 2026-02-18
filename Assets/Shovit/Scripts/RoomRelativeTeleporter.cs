using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class RoomRelativeTeleporter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoomManager roomManager;

    [Header("Teleport Options")]
    [SerializeField] private bool preserveRelativeXZ = true;
    [SerializeField] private float yAfterTeleportOffset = 0.05f;
    [SerializeField] private bool snapToGround = true;
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float groundRayStartHeight = 5f;
    [SerializeField] private float groundRayDistance = 20f;

    private CharacterController controller;

    public int CurrentRoomIndex { get; private set; } = -1;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (roomManager == null) return;
        CurrentRoomIndex = roomManager.GetRoomIndex(transform.position);
    }

    public bool TeleportToRoom(int targetRoomIndex)
    {
        if (roomManager == null)
        {
            Debug.LogError("RoomRelativeTeleporter: roomManager not assigned.");
            return false;
        }

        if (targetRoomIndex < 0 || targetRoomIndex >= roomManager.RoomCount)
        {
            Debug.LogWarning($"RoomRelativeTeleporter: invalid targetRoomIndex {targetRoomIndex}.");
            return false;
        }

        int fromRoom = roomManager.GetRoomIndex(transform.position);
        if (fromRoom == -1)
        {
            Debug.LogWarning("RoomRelativeTeleporter: player is not inside any room.");
            return false;
        }

        Vector3 targetPos = transform.position;

        if (preserveRelativeXZ)
        {
            if (!roomManager.WorldToRoomNormalizedXZ(fromRoom, transform.position, out Vector2 nXZ))
                return false;

            if (!roomManager.RoomNormalizedXZToWorldXZ(targetRoomIndex, nXZ, out Vector3 targetXZ))
                return false;

            targetPos.x = targetXZ.x;
            targetPos.z = targetXZ.z;
        }

        targetPos.y = transform.position.y + yAfterTeleportOffset;

        if (snapToGround)
        {
            Vector3 rayStart = new Vector3(targetPos.x, targetPos.y + groundRayStartHeight, targetPos.z);
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundRayDistance, groundMask, QueryTriggerInteraction.Ignore))
            {
                targetPos.y = hit.point.y + yAfterTeleportOffset;
            }
        }

        controller.enabled = false;
        transform.position = targetPos;
        controller.enabled = true;

        CurrentRoomIndex = targetRoomIndex;
        return true;
    }
}
