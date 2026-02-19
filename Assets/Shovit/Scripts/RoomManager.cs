using System;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Serializable]
    public class Room
    {
        public string name = "Room";
        public Transform plane;
        public float height = 3f;
        public float yOffset = 0f;
    }

    [Header("Rooms (order = room index)")]
    public Room[] rooms;

    [Header("Debug")]
    public bool drawGizmos = true;

    public int RoomCount => rooms != null ? rooms.Length : 0;

    public int GetRoomIndex(Vector3 worldPos)
    {
        if (rooms == null) return -1;

        for (int i = 0; i < rooms.Length; i++)
        {
            if (IsInsideRoom(i, worldPos))
                return i;
        }

        return -1;
    }

    public bool IsInsideRoom(int roomIndex, Vector3 worldPos)
    {
        if (!TryGetRoomBounds(roomIndex, out Bounds b))
            return false;

        return b.Contains(worldPos);
    }

    public bool WorldToRoomNormalizedXZ(int roomIndex, Vector3 worldPos, out Vector2 normalizedXZ)
    {
        normalizedXZ = default;

        if (!TryGetRoomBounds(roomIndex, out Bounds b))
            return false;

        float nx = Mathf.InverseLerp(b.min.x, b.max.x, worldPos.x);
        float nz = Mathf.InverseLerp(b.min.z, b.max.z, worldPos.z);

        normalizedXZ = new Vector2(nx, nz);
        return true;
    }

    public bool RoomNormalizedXZToWorldXZ(int roomIndex, Vector2 normalizedXZ, out Vector3 worldXZ)
    {
        worldXZ = default;

        if (!TryGetRoomBounds(roomIndex, out Bounds b))
            return false;

        float x = Mathf.Lerp(b.min.x, b.max.x, normalizedXZ.x);
        float z = Mathf.Lerp(b.min.z, b.max.z, normalizedXZ.y);

        worldXZ = new Vector3(x, 0f, z);
        return true;
    }

    public bool TryGetRoomBounds(int roomIndex, out Bounds bounds)
    {
        bounds = default;

        if (rooms == null || roomIndex < 0 || roomIndex >= rooms.Length)
            return false;

        Room r = rooms[roomIndex];
        if (r == null || r.plane == null)
            return false;

        Renderer rend = r.plane.GetComponentInChildren<Renderer>();
        if (rend == null)
        {
            Debug.LogWarning($"RoomManager: plane '{r.plane.name}' has no Renderer (can't auto-size).");
            return false;
        }

        Bounds rb = rend.bounds;

        float sizeX = rb.size.x;
        float sizeZ = rb.size.z;

        Vector3 planePos = r.plane.position;

        float yMin = planePos.y + r.yOffset;
        float yMax = yMin + Mathf.Max(0.01f, r.height);

        Vector3 bCenter = new Vector3(planePos.x, (yMin + yMax) * 0.5f, planePos.z);
        Vector3 bSize = new Vector3(sizeX, (yMax - yMin), sizeZ);

        bounds = new Bounds(bCenter, bSize);
        return true;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || rooms == null) return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < rooms.Length; i++)
        {
            if (TryGetRoomBounds(i, out Bounds b))
            {
                Gizmos.DrawWireCube(b.center, b.size);

#if UNITY_EDITOR
                UnityEditor.Handles.Label(b.center + Vector3.up * (b.extents.y + 0.2f), $"{i}: {rooms[i].name}");
#endif
            }
        }
    }
}
