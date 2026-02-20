using UnityEngine;

public class DraggableState : MonoBehaviour
{
    [SerializeField] private bool canDrag = true;
    public bool CanDrag => canDrag;

    public void SetCanDrag(bool value) => canDrag = value;
}