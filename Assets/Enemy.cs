using UnityEngine;

public class Enemy : MonoBehaviour{
  public void OnMouseDown(){
          Debug.Log("OnMouseDown Fired");

          // Tell the game this is an attack target
          // This might later call an IntentManager or CombatManager

  }
}
