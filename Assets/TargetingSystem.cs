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

    public TargetData ResolveTarget(Ability ability, ICombatant user, Vector3 worldClick)
    {
        TargetData data = new TargetData();
        data.user = user;

        Vector3Int gridPos = grid.WorldToGrid(worldClick);
        data.preferredTargetCell = gridPos;

        // Heal special case (keep if you added it)
        if (ability is HealAbility)
        {
            ICombatant player = FindPlayerBoxCombatant();
            if (player != null && !player.IsDead())
            {
                data.primaryTarget = player;
                data.unitsInArea.Add(player);
                data.preferredTargetCell = grid.WorldToGrid(player.GetWorldPosition());
            }
            return data;
        }

        ICombatant occupant = grid.GetOccupant(gridPos);

        // NEW: multi-tile fallback
        if (occupant == null)
        {
            List<ICombatant> valid = GetValidTargets(ability, user);
            foreach (var t in valid)
            {
                var occ = t.GetOccupiedCells();
                if (occ != null && occ.Contains(gridPos))
                {
                    occupant = t;
                    break;
                }
            }
        }

        if (occupant != null)
        {
            switch (ability.targetingMode)
            {
                case TargetingMode.Self:
                    if (occupant == user) data.primaryTarget = occupant;
                    break;
                case TargetingMode.Ally:
                    if (IsAlly(user, occupant)) data.primaryTarget = occupant;
                    break;
                case TargetingMode.Enemy:
                    if (IsEnemy(user, occupant)) data.primaryTarget = occupant;
                    break;
                case TargetingMode.Area:
                    data.primaryTarget = occupant;
                    break;
            }
        }

        if (ability.radius > 0)
            data.unitsInArea = GetUnitsInRadius(gridPos, ability.radius);
        else if (data.primaryTarget != null)
            data.unitsInArea.Add(data.primaryTarget);

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

        // Hard rule: Heal highlights only player Box
        if (ability is HealAbility)
        {
            ICombatant player = FindPlayerBoxCombatant();
            if (player != null && !player.IsDead())
                validTargets.Add(player);
            return validTargets;
        }

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

    private ICombatant FindPlayerBoxCombatant()
    {
        BoxMover box = Object.FindFirstObjectByType<BoxMover>();
        return box as ICombatant;
    }

    public void HighlightValidTargets(Ability ability, ICombatant user)
    {
        ClearTargetHighlights();

        List<ICombatant> targets = GetValidTargets(ability, user);

        foreach (ICombatant target in targets)
        {
            if (target == null || target.IsDead())
                continue;

            // Multi-tile safe
            List<Vector3Int> cells = target.GetOccupiedCells();
            if (cells == null || cells.Count == 0)
            {
                // fallback for older units
                cells = new List<Vector3Int> { grid.WorldToGrid(target.GetWorldPosition()) };
            }

            foreach (Vector3Int pos in cells)
            {
                TileVisual visual = grid.GetTileVisual(pos);
                if (visual != null && !highlightedVisuals.Contains(visual))
                {
                    visual.Highlight();
                    highlightedVisuals.Add(visual);
                }
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

    public void ShowAOEPreview(Ability ability, Vector3Int centerGridPos)
    {
        if (ability.radius <= 0)
            return; // No AOE

        // Get all nodes in radius
        List<GridNode> nodesInRadius = grid.GetNodesInRadius(centerGridPos, ability.radius);

        // Clear previous highlights
        ClearTargetHighlights();

        // Highlight all affected tiles
        foreach (GridNode node in nodesInRadius)
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
