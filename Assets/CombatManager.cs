using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CombatManager : MonoBehaviour{
  public static CombatManager Instance;

  List<ICombatant> combatants = new List<ICombatant>();
  int currentIndex = 0;

  public void Awake(){
    Instance = this;
  }

  public void StartCombat(List<ICombatant> participants){
    combatants = participants;

    RollInitiative();
    currentIndex = 0;

    combatants[currentIndex].StartTurn();
  }

  void RollInitiative(){
    foreach(var c in combatants){
      c.Initiative = Random.Range(1, 21);
      combatants = combatants
            .OrderByDescending(c => c.Initiative)
            .ToList();
    }
  }

  public void EndTurn(){
    combatants[currentIndex].EndTurn();

        currentIndex++;

        if (currentIndex >= combatants.Count)
            currentIndex = 0;

        combatants[currentIndex].StartTurn();
  }

  public bool IsPlayersTurn(ICombatant combatant){
    if(combatants == null || combatants.Count == 0)
        return false;

    if(currentIndex < 0 || currentIndex >= combatants.Count)
        return false;

    return combatants[currentIndex] == combatant;
  }
}
