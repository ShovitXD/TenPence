using UnityEngine;

public interface IDropTarget
{
    bool CanAccept(GameObject dragged);
    void Accept(GameObject dragged);
}