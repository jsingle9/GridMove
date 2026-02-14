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

  void OnTriggerEnter2D(Collider2D other){
    Debug.Log("Trigger entered by: " + other.name);
    if(other.GetComponent<BoxMover>()){
        GameStateManager.Instance.EnterCombat();
    }
  }

}
