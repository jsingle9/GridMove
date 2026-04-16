using UnityEngine;

public interface IDoorInteractable
{
    Vector3Int GetGridPosition();
    float GetInteractionRange();
    string GetInteractionPrompt();
    void Interact(BoxMover player);
}
