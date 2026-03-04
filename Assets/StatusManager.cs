using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatusManager
{
    private List<StatusEffect> activeStatuses = new List<StatusEffect>();
    private ICombatant owner;

    public StatusManager(ICombatant owner){
        this.owner = owner;
    }

    public void AddStatus(StatusEffect status){
      StatusEffect existing = activeStatuses
          .FirstOrDefault(s => s.Name == status.Name);

      if (existing != null){
          existing.Refresh(status.RemainingTurns);
          return;
      }
        activeStatuses.Add(status);
        status.OnApply(owner);
    }

    public void RemoveStatus(StatusEffect status){
        activeStatuses.Remove(status);
    }

    public void ProcessTurnStart(){
        foreach (var status in activeStatuses.ToList()){
            if(owner.IsDead()) return;
            status.OnTurnStart(owner);
        }
    }

    public void ProcessTurnEnd(){
        foreach (var status in activeStatuses.ToList()){
            if(owner.IsDead()) return;

            status.OnTurnEnd(owner);
            status.Tick(owner);
        }
    }

    public void Clear(){
        activeStatuses.Clear();
    }

    public IEnumerable<StatusEffect> GetStatuses(){
        return activeStatuses;
    }
}
