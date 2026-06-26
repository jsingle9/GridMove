using System.Collections.Generic;
using UnityEngine;

public class TargetData
{
    public GridNode tile;

    public ICombatant primaryTarget;
    public ICombatant user;

    public List<ICombatant> unitsInArea = new List<ICombatant>();

    // For multi-tile targets, this is the specific occupied cell we want to attack toward.
    public Vector3Int? preferredTargetCell;

    public TargetData()
    {
    }

    public TargetData(ICombatant target)
    {
        primaryTarget = target;
        unitsInArea.Add(target);
    }
}
