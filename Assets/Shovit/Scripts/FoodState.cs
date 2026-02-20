using UnityEngine;

public class FoodState : MonoBehaviour
{
    public enum State
    {
        Raw,
        Aged,
        Spoiled
    }

    [Header("Assign the 3 visuals (only one active at a time)")]
    [SerializeField] private GameObject rawObject;
    [SerializeField] private GameObject agedObject;
    [SerializeField] private GameObject spoiledObject;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    [SerializeField] private State currentState = State.Raw;

    private void Start()
    {
        ApplyState(currentState);
    }

    public void SetRaw() => ApplyState(State.Raw);
    public void SetAged() => ApplyState(State.Aged);
    public void SetSpoiled() => ApplyState(State.Spoiled);

    public State GetState() => currentState;

    private void ApplyState(State state)
    {
        currentState = state;

        if (rawObject != null) rawObject.SetActive(state == State.Raw);
        if (agedObject != null) agedObject.SetActive(state == State.Aged);
        if (spoiledObject != null) spoiledObject.SetActive(state == State.Spoiled);

        if (debugLogs)
            Debug.Log($"FoodState: {name} -> {currentState}");
    }
}
