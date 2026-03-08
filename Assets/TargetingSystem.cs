using UnityEngine;
using System.Collections.Generic;

public class TargetingSystem
{
    GridController grid;

    public TargetingSystem(GridController grid)
    {
        this.grid = grid;
    }

    public TargetData ResolveTarget(
        Ability ability,
        ICombatant user,
        Vector3 worldClick
    )
    {
        TargetData data = new TargetData();

        Vector3Int gridPos = grid.WorldToGrid(worldClick);

        Vector3 world = grid.GridToWorld(gridPos);
        GridNode node = grid.GetNodeFromWorld(world);

        data.tile = node;

        ICombatant occupant = grid.GetOccupant(gridPos);

        if(occupant != null)
        {
            data.primaryTarget = occupant;
        }

        if(ability.radius > 0)
        {
            data.unitsInArea = GetUnitsInRadius(gridPos, ability.radius);
        }
        else if(data.primaryTarget != null)
        {
            data.unitsInArea.Add(data.primaryTarget);
        }

        return data;
    }

    List<ICombatant> GetUnitsInRadius(Vector3Int center, int radius)
    {
        List<ICombatant> units = new List<ICombatant>();

        foreach(var node in grid.GetNodesInRadius(center, radius))
        {
            ICombatant unit = grid.GetOccupant(node.gridPos);

            if(unit != null)
                units.Add(unit);
        }

        return units;
    }
}
