using UnityEngine;

public class Enemy : MonoBehaviour{

  DynamicObstacle dynamicObstacle;

  void Awake(){
    dynamicObstacle = GetComponent<DynamicObstacle>();
  }

  public void start(){

  }

  public void MoveTo(Vector3 worldTagetPosition){
    transform.position = worldTagetPosition;
    dynamicObstacle.UpdateCell(transform.position);
  }

  public void OnMouseDown(){
    Debug.Log("OnMouseDown Fired");

    // Tell the game this is an attack target
    // This might later call an IntentManager or CombatManager

  }
}
