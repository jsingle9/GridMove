using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    public AbilityUI abilityUI;

    void Update()
    {
        if(Keyboard.current.digit1Key.wasPressedThisFrame)
            abilityUI.SelectAbility(0);

        if(Keyboard.current.digit2Key.wasPressedThisFrame)
            abilityUI.SelectAbility(1);

        if(Keyboard.current.digit3Key.wasPressedThisFrame)
            abilityUI.SelectAbility(2);

        if(Keyboard.current.escapeKey.wasPressedThisFrame)
            abilityUI.CancelAbility();
    }
}
