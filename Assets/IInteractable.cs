using UnityEngine;

public interface IInteractable
{
    void Interact(ICombatant interactor);
    Vector3Int GetGridPosition();
    float GetInteractionRange();
    string GetInteractionPrompt();
}
