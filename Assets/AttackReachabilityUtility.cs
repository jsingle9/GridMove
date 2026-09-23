using System.Collections.Generic;
using UnityEngine;

public static class AttackReachabilityUtility
{
    private static readonly Vector3Int[] AdjacentOffsets =
    {
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right
    };

    /// <summary>
    /// Finds the cheapest path from the user to any valid cell adjacent
    /// to the target.
    ///
    /// Returns true when an adjacent attack position is reachable.
    /// movementCost is zero when the user is already adjacent.
    /// </summary>
    public static bool TryGetMeleeApproach(
        GridController grid,
        ICombatant user,
        ICombatant target,
        out List<GridNode> bestPath,
        out int movementCost
    )
    {
        bestPath = null;
        movementCost = -1;

        if (grid == null || user == null || target == null)
            return false;

        GridNode startNode =
            grid.GetNodeFromWorld(user.GetWorldPosition());

        if (startNode == null)
            return false;

        List<Vector3Int> targetCells =
            target.GetOccupiedCells();

        if (targetCells == null || targetCells.Count == 0)
            return false;

        Pathfinder pathfinder = new Pathfinder(grid);

        Vector2Int footprint =
            grid.GetCombatantFootprintSize(user);

        foreach (Vector3Int targetCell in targetCells)
        {
            foreach (Vector3Int offset in AdjacentOffsets)
            {
                Vector3Int candidateCell =
                    targetCell + offset;

                if (!grid.IsInBounds(candidateCell))
                    continue;

                /*
                 * Already adjacent. An empty path is valid and has zero
                 * movement cost.
                 */
                if (candidateCell == startNode.gridPos)
                {
                    if (bestPath == null || movementCost > 0)
                    {
                        bestPath = new List<GridNode>();
                        movementCost = 0;
                    }

                    continue;
                }

                if (!grid.CanOccupyFootprint(
                    candidateCell,
                    footprint.x,
                    footprint.y,
                    user
                ))
                {
                    continue;
                }

                GridNode candidateNode =
                    grid.GetNodeFromWorld(
                        grid.GridToWorld(candidateCell)
                    );

                if (candidateNode == null)
                    continue;

                List<GridNode> path =
                    pathfinder.FindPath(
                        startNode,
                        candidateNode,
                        user,
                        footprint
                    );

                if (path == null)
                    continue;

                int cost =
                    MovementCostUtility.CalculatePathCost(
                        grid,
                        path
                    );

                if (bestPath == null ||
                    cost < movementCost)
                {
                    bestPath = path;
                    movementCost = cost;
                }
            }
        }

        return bestPath != null;
    }

    /// <summary>
    /// Returns only the cheapest movement cost to melee range.
    /// Returns -1 when no adjacent attack position is reachable.
    /// </summary>
    public static int GetMovementCostToMeleeRange(
        GridController grid,
        ICombatant user,
        ICombatant target
    )
    {
        if (!TryGetMeleeApproach(
            grid,
            user,
            target,
            out _,
            out int movementCost
        ))
        {
            return -1;
        }

        return movementCost;
    }
}
