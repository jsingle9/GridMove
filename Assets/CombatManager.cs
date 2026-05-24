using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CombatManager : MonoBehaviour{
  public static CombatManager Instance;
  public CombatState currentCombatState;
  public PlayerTurnPhase currentPlayerPhase;
  //public BoxMover Player;

  List<ICombatant> combatants = new List<ICombatant>();
  int currentIndex = 0;
  bool turnAdvancing = false;

  public void Awake(){
    Instance = this;
  }

  public void StartCombat(List<ICombatant> participants){
      combatants = participants;

      GameStateManager.Instance.EnterCombat();

      RollInitiative();

      if(CombatUIManager.Instance != null)
          CombatUIManager.Instance.OnCombatStart();

      currentIndex = 0;

      ICombatant current = combatants[currentIndex];

      if(current.IsPlayerControlled()){
          SetCombatState(CombatState.PlayerTurn);
          SetPlayerPhase(PlayerTurnPhase.WaitingForAction);
      }
      else{
          SetCombatState(CombatState.EnemyTurn);
      }

      current.StartTurn();

      if(current.IsPlayerControlled()){
          if(CombatUIManager.Instance != null)
              CombatUIManager.Instance.OnPlayerTurnStart();
      }
      else{
          if(CombatUIManager.Instance != null)
              CombatUIManager.Instance.OnEnemyTurnStart();
      }
  }

  void RollInitiative(){
      foreach(var c in combatants){
          c.Initiative = Random.Range(1, 21);
      }

      combatants = combatants
          .OrderByDescending(c => c.Initiative)
          .ToList();
  }

  public void EndTurn(){
      if(turnAdvancing){
          Debug.Log("Turn advance blocked (already advancing)");
          return;
      }

      turnAdvancing = true;

      if(combatants == null || combatants.Count == 0){
          turnAdvancing = false;
          return;
      }

      if(currentIndex >= 0 && currentIndex < combatants.Count)
      {
          var current = combatants[currentIndex];
          if(current != null && !current.IsDead())
              current.EndTurn();
      }

      CheckCombatEnd();
      if(combatants == null || combatants.Count == 0){
          turnAdvancing = false;
          return;
      }

      int safety = 0;

      do
      {
          currentIndex++;

          if(currentIndex >= combatants.Count)
              currentIndex = 0;

          safety++;

          if(safety > combatants.Count){
              Debug.LogWarning("No valid combatants left.");
              turnAdvancing = false;
              return;
          }

      }
      while(
          combatants[currentIndex] == null ||
          combatants[currentIndex].IsDead() ||
          !((MonoBehaviour)combatants[currentIndex]).gameObject.activeInHierarchy
      );

      ICombatant next = combatants[currentIndex];

      if(next.IsPlayerControlled()){
          SetCombatState(CombatState.PlayerTurn);
          SetPlayerPhase(PlayerTurnPhase.WaitingForAction);
      }
      else{
          SetCombatState(CombatState.EnemyTurn);
      }

      Debug.Log(">>> NEW TURN: " + next);

      next.StartTurn();

      if(next.IsPlayerControlled()){
          if(CombatUIManager.Instance != null)
              CombatUIManager.Instance.OnPlayerTurnStart();
      }
      else{
          if(CombatUIManager.Instance != null)
              CombatUIManager.Instance.OnEnemyTurnStart();
      }

      turnAdvancing = false;
  }

  public bool IsPlayersTurn(ICombatant combatant){
    if(combatants == null || combatants.Count == 0)
        return false;

    if(currentIndex < 0 || currentIndex >= combatants.Count)
        return false;

    return combatants[currentIndex] == combatant;
  }

  public void NotifyDeath(ICombatant dead){

      int deadIndex = combatants.IndexOf(dead);

      if(deadIndex >= 0){
          combatants.RemoveAt(deadIndex);

          if(deadIndex <= currentIndex && currentIndex > 0)
              currentIndex--;
      }
      else{
        Debug.LogWarning("Dead combatant not found in combatants list!");
      }
      Debug.Log($"Combatants after removal: {combatants.Count}");
      CheckCombatEnd();
  }

  void CheckCombatEnd(){
      bool anyPlayersAlive = false;
      bool anyEnemiesAlive = false;

      foreach(var c in combatants)
      {
          if(c == null || c.IsDead())
              continue;

          if(c.IsPlayerControlled())
              anyPlayersAlive = true;
          else
              anyEnemiesAlive = true;
      }

      Debug.Log($"CheckCombatEnd -> playersAlive={anyPlayersAlive}, enemiesAlive={anyEnemiesAlive}, combatantCount={combatants.Count}");

      if(!anyPlayersAlive || !anyEnemiesAlive)
          EndCombat();
  }

  void EndCombat(){
      Debug.Log("Combat ended");

      turnAdvancing = false;
      currentIndex = 0;

      if(CombatUIManager.Instance != null)
          CombatUIManager.Instance.OnCombatEnd();

      combatants.Clear();

      GameStateManager.Instance.ExitCombat();
  }

  public void ExitCombat()
  {
    GameStateManager.Instance.ExitCombat();
  }

  public void SetCombatState(CombatState newState)
  {
      currentCombatState = newState;
      //Debug.Log("Combat State → " + newState);
  }

  public void SetPlayerPhase(PlayerTurnPhase newPhase)
  {
      currentPlayerPhase = newPhase;
      Debug.Log("Player Phase → " + newPhase);
  }

  public bool IsPlayerActive()
  {
      return GameStateManager.Instance != null &&
             GameStateManager.Instance.CurrentState == GameState.Combat &&
             currentCombatState == CombatState.PlayerTurn;
  }

  public List<ICombatant> GetCombatants()
  {
      return new List<ICombatant>(combatants);
  }

}
