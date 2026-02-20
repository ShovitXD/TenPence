using UnityEngine;

public class RoomAudioSwitcher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private Transform player;

    [Header("Audio Sources (index 0=RoomS, 1=RoomN, 2=RoomF)")]
    [SerializeField] private AudioSource[] roomAudio = new AudioSource[3];

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private int _currentRoom = -1;

    private void Awake()
    {
        if (roomManager == null) roomManager = FindFirstObjectByType<RoomManager>();
    }

    private void Update()
    {
        if (roomManager == null || player == null || roomAudio == null || roomAudio.Length < 3)
            return;

        int room = roomManager.GetRoomIndex(player.position);
        if (room == _currentRoom) return;

        _currentRoom = room;

        for (int i = 0; i < roomAudio.Length; i++)
        {
            if (roomAudio[i] == null) continue;

            bool shouldEnable = (i == _currentRoom);
            roomAudio[i].enabled = shouldEnable;

            if (!shouldEnable && roomAudio[i].isPlaying)
                roomAudio[i].Stop();
            else if (shouldEnable && !roomAudio[i].isPlaying)
                roomAudio[i].Play();
        }

        if (debugLogs)
            Debug.Log($"RoomAudioSwitcher: player room = {_currentRoom}");
    }
}
