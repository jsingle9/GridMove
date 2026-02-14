using UnityEngine;
using UnityEngine.InputSystem;

public class MouseInputHandler : MonoBehaviour{
  public BoxMover boxMover;

  void Update(){
    if(!Mouse.current.leftButton.wasPressedThisFrame)
      return;

    if (GameStateManager.Instance.CurrentState != GameState.FreeExplore &&
          GameStateManager.Instance.CurrentState != GameState.Combat)
          return;


    boxMover.HandleLeftClick();
  }
}
