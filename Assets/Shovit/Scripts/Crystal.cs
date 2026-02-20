using UnityEngine;

public class Crystal : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoomYearDirector roomYearDirector;

    [Header("Settings")]
    [SerializeField] private string floorLayerName = "Floor";
    [SerializeField] private string roomHandlerObjectName = "RoomHandler";

    private int _floorLayerId = -1;

    private void Awake()
    {
        // Find RoomYearDirector from GameObject named "RoomHandler"
        if (roomYearDirector == null)
        {
            GameObject roomHandler = GameObject.Find(roomHandlerObjectName);

            if (roomHandler != null)
            {
                roomYearDirector = roomHandler.GetComponent<RoomYearDirector>();
                if (roomYearDirector == null)
                    Debug.LogWarning($"Crystal: '{roomHandlerObjectName}' found but RoomYearDirector component is missing on it.");
            }
            else
            {
                Debug.LogWarning($"Crystal: GameObject named '{roomHandlerObjectName}' not found.");
            }
        }

        _floorLayerId = LayerMask.NameToLayer(floorLayerName);
        if (_floorLayerId == -1)
            Debug.LogWarning($"Crystal: Layer '{floorLayerName}' not found. Check your layer name.");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_floorLayerId == -1) return;

        if (collision.collider.gameObject.layer != _floorLayerId)
            return;

        if (roomYearDirector != null)
        {
            roomYearDirector.timePaused = true;
            Debug.Log("Crystal: Hit Floor -> timePaused set TRUE on RoomYearDirector.");
        }
        else
        {
            Debug.LogWarning("Crystal: RoomYearDirector not found, couldn't set timePaused.");
        }

        // TODO: spawn particle effect here
        // Instantiate(particlePrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}