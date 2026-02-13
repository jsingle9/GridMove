using UnityEngine;
using UnityEngine.InputSystem;

public class MouseInputHandler : MonoBehaviour{
  public BoxMover boxMover;

  void Update(){
    if(!Mouse.current.leftButton.wasPressedThisFrame)
      return;
    Debug.Log("Mouse Input Handler Update Method Firing");
    boxMover.HandleLeftClick();
  }
}
