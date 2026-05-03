using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionInputHandler : MonoBehaviour
{
    private InteractionSystem interactionSystem;

    void Start()
    {
        interactionSystem = FindFirstObjectByType<InteractionSystem>();
        Debug.Log($"InteractionInputHandler initialized, InteractionSystem found: {interactionSystem != null}");
    }

    void Update()
    {
        //Debug.Log("InteractionInputHandler.Update() running");

        if(Keyboard.current.anyKey.isPressed)
        {
            Debug.Log("ANY key pressed!");
        }

        if(Keyboard.current.fKey.wasPressedThisFrame)
        {
            Debug.Log("F pressed!");
            if(interactionSystem != null)
            {
                Debug.Log("Calling AttemptInteraction...");
                interactionSystem.AttemptInteraction();
            }
            else
            {
                Debug.Log("ERROR: interactionSystem is null!");
            }
        }
    }
}
