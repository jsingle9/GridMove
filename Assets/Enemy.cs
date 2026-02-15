using UnityEngine;

public class Enemy : MonoBehaviour, ICombatant{

  DynamicObstacle dynamicObstacle;
  public int Initiative { get; set; }

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

  public void StartTurn(){
    Debug.Log("Enemy turn started");
    // AI combat logic goes here
    Invoke(nameof(FinishTurn), 1f);
  }

  public void EndTurn(){
    Debug.Log("Enemy turn ended");
    // pass control back to initiative or combat manager here

  }

  void FinishTurn(){
    CombatManager.Instance.EndTurn();
  }

}
