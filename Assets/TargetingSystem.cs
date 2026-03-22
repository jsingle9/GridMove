using UnityEngine;
using System.Collections.Generic;

public class TargetingSystem
{
    GridController grid;
    List<TileVisual> highlightedVisuals = new List<TileVisual>();

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
        data.user = user;

        Vector3Int gridPos = grid.WorldToGrid(worldClick);
        // SELF TARGET SHORTCUT
        if(ability.targetingMode == TargetingMode.Self)
        {
            data.primaryTarget = user;
            data.unitsInArea.Add(user);
            return data;
        }

        Vector3 world = grid.GridToWorld(gridPos);
        GridNode node = grid.GetNodeFromWorld(world);

        data.tile = node;

        ICombatant occupant = grid.GetOccupant(gridPos);

        if(occupant != null)
        {
            switch(ability.targetingMode)
            {
                case TargetingMode.Self:
                    if(occupant == user)
                        data.primaryTarget = occupant;
                    break;

                case TargetingMode.Ally:
                    if(IsAlly(user, occupant))
                        data.primaryTarget = occupant;
                    break;

                case TargetingMode.Enemy:
                    if(IsEnemy(user, occupant))
                        data.primaryTarget = occupant;
                    break;

            }
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

    bool IsEnemy(ICombatant a, ICombatant b)
    {
        return a.GetType() != b.GetType();
    }

    bool IsAlly(ICombatant a, ICombatant b)
    {
        return a.GetType() == b.GetType();
    }

    public List<ICombatant> GetValidTargets(Ability ability, ICombatant user)
    {
        List<ICombatant> validTargets = new List<ICombatant>();
        List<ICombatant> all = CombatManager.Instance.GetCombatants();

        foreach (ICombatant c in all)
        {
            if (c == null || c.IsDead()) continue;

            switch (ability.targetingMode)
            {
                case TargetingMode.Self:
                    if (c == user) validTargets.Add(c);
                    break;

                case TargetingMode.Ally:
                    if (IsAlly(user, c)) validTargets.Add(c);
                    break;

                case TargetingMode.Enemy:
                    if (IsEnemy(user, c)) validTargets.Add(c);
                    break;

                case TargetingMode.Area:
                    validTargets.Add(c);
                    break;
            }
        }

        return validTargets;
    }

    public void HighlightValidTargets(Ability ability, ICombatant user)
    {
        ClearTargetHighlights();

        List<ICombatant> targets = GetValidTargets(ability, user);

        foreach (ICombatant target in targets)
        {
            if(target == null || target.IsDead())
              continue;

            Vector3Int pos = grid.WorldToGrid(target.GetWorldPosition());
            TileVisual visual = grid.GetTileVisual(pos);

            if (visual != null)
            {
                visual.Highlight();
                highlightedVisuals.Add(visual);
            }
        }
    }

    public void ClearTargetHighlights()
    {
        foreach (TileVisual visual in highlightedVisuals)
        {
              visual.ClearHighlight();
        }

        highlightedVisuals.Clear();
    }

    public void RemoveHighlightAt(Vector3Int gridPos)
    {
        TileVisual visual = grid.GetTileVisual(gridPos);

        if (visual == null)
            return;

        if (highlightedVisuals.Contains(visual))
        {
            visual.ClearHighlight();
            highlightedVisuals.Remove(visual);
        }
    }

    public void HighlightAOE(Vector3Int center, int radius)
    {
        ClearTargetHighlights();

        List<GridNode> nodes = grid.GetNodesInRadius(center, radius);

        foreach (GridNode node in nodes)
        {
            TileVisual visual = grid.GetTileVisual(node.gridPos);

            if (visual != null)
            {
                visual.Highlight();
                highlightedVisuals.Add(visual);
            }
        }
    }
}
