using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputRouter : MonoBehaviour
{
    [SerializeField] BoxMover player;

    void Update(){
        if(Mouse.current.leftButton.wasPressedThisFrame){
            player.HandleLeftClick();
        }
    }
}
