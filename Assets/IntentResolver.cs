using UnityEngine;
using System.Collections.Generic;

public class IntentResolver
{
    GridController grid;
    Pathfinder pathfinder;

    public IntentResolver(GridController grid)
    {
        this.grid = grid;
        pathfinder = new Pathfinder(grid);
    }

    public List<GridNode> Resolve(
        Intent intent,
        GridNode actorNode
    )
    {
        if (intent is MoveIntent move)
        {
            Debug.Log("intent = move intent");
            GridNode target = grid.GetNodeFromWorld(
                grid.GridToWorld(move.targetCell)
            );

            return pathfinder.FindPath(actorNode, target);
        }

        if (intent is AttackIntent attack)
        {
            Debug.Log("intent = attack intent");
            return ResolveAttackMove(actorNode, attack.data);
        }

        return null;
    }

    public List<GridNode> ResolveAttackMove(GridNode actorNode, TargetData targetData)
    {
        if (targetData == null || targetData.primaryTarget == null)
            return null;

        ICombatant target = targetData.primaryTarget;
        List<Vector3Int> occupiedCells = target.GetOccupiedCells();

        if (occupiedCells == null || occupiedCells.Count == 0)
            return null;

        HashSet<Vector3Int> candidatePositions = new HashSet<Vector3Int>();

        foreach (Vector3Int occupied in occupiedCells)
        {
            Vector3Int[] directions =
            {
                Vector3Int.up,
                Vector3Int.down,
                Vector3Int.left,
                Vector3Int.right
            };

            foreach (Vector3Int dir in directions)
            {
                Vector3Int adjacent = occupied + dir;

                if (occupiedCells.Contains(adjacent))
                    continue;

                candidatePositions.Add(adjacent);
            }
        }

        GridNode bestTile = null;
        List<GridNode> bestPath = null;
        int bestCost = int.MaxValue;

        foreach (Vector3Int cell in candidatePositions)
        {
            if (!grid.IsInBounds(cell))
                continue;

            GridNode tile = grid.GetNodeFromWorld(grid.GridToWorld(cell));
            if (tile == null)
                continue;

            if (!tile.walkable)
                continue;

            if (grid.IsTileOccupied(cell))
                continue;

            List<GridNode> path = pathfinder.FindPath(actorNode, tile);
            if (path == null || path.Count == 0)
                continue;

            int cost = path.Count;

            if (cost < bestCost)
            {
                bestCost = cost;
                bestTile = tile;
                bestPath = path;
            }
        }

        if (bestPath != null)
        {
            return bestPath;
        }

        Debug.Log("No reachable adjacent tile to target");
        return null;
    }
}
