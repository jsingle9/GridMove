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


    /// <summary>
    /// Track which spell slot level was used to cast this spell.
    /// 0 = cantrip, 1-9 = spell slot level.
    /// Allows upcasting and proper resource tracking.
    /// </summary>
    public int spellSlotLevelUsed = 0;

    public TargetData()
    {
    }

    public TargetData(ICombatant target)
    {
        primaryTarget = target;
        unitsInArea.Add(target);
    }

    public TargetData(ICombatant target, int slotLevel)
    {
        primaryTarget = target;
        unitsInArea.Add(target);
        spellSlotLevelUsed = slotLevel;
    }
}
