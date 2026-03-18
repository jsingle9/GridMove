using System.Collections.Generic;

public class TargetData
{
    public GridNode tile;

    public ICombatant primaryTarget;
    public ICombatant user;

    public List<ICombatant> unitsInArea = new List<ICombatant>();

    public TargetData(){

    }

    public TargetData(ICombatant target){
        primaryTarget = target;
        unitsInArea.Add(target);
    }
}
