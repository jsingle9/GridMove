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
        if(intent is MoveIntent move)
        {
            Debug.Log("intent = move intent");
            GridNode target = grid.GetNodeFromWorld(
                grid.GridToWorld(move.targetCell)
            );

            return pathfinder.FindPath(actorNode, target);
        }

        if(intent is AttackIntent attack)
        {
            Debug.Log("intent = attack intent");
            return ResolveAttackMove(actorNode, attack.data);
        }

        return null;
    }

    public List<GridNode> ResolveAttackMove(GridNode actorNode, TargetData targetData)
    {
        if(targetData == null || targetData.primaryTarget == null)
            return null;

        ICombatant attacker = grid.GetOccupant(actorNode.gridPos);
        ICombatant target = targetData.primaryTarget;

        if(attacker == null)
            return null;

        Vector2Int attackerFootprint = GetFootprintSize(attacker);
        if(attackerFootprint.x <= 0 || attackerFootprint.y <= 0)
            return null;

        List<Vector3Int> targetCells = target.GetOccupiedCells();
        if(targetCells == null || targetCells.Count == 0)
            return null;

        HashSet<Vector3Int> candidateOrigins = new HashSet<Vector3Int>();

        // Search a local area around the target for valid attacker origin positions.
        // If the footprint fits there and can melee the target from there, it's a candidate.
        for(int tx = -attackerFootprint.x - 1; tx <= 1; tx++)
        {
            for(int ty = -attackerFootprint.y - 1; ty <= 1; ty++)
            {
                foreach(Vector3Int targetCell in targetCells)
                {
                    Vector3Int origin = new Vector3Int(
                        targetCell.x + tx,
                        targetCell.y + ty,
                        0
                    );

                    if(!grid.CanOccupyFootprint(origin, attackerFootprint.x, attackerFootprint.y, attacker))
                        continue;

                    if(CanMeleeTargetFromOrigin(origin, attackerFootprint, targetCells))
                    {
                        candidateOrigins.Add(origin);
                    }
                }
            }
        }

        GridNode bestTile = null;
        List<GridNode> bestPath = null;
        int bestCost = int.MaxValue;

        foreach(Vector3Int origin in candidateOrigins)
        {
            GridNode tile = grid.GetNodeFromWorld(grid.GridToWorld(origin));
            if(tile == null)
                continue;

            List<GridNode> path = pathfinder.FindPath(actorNode, tile);
            if(path == null || path.Count == 0)
                continue;

            int cost = path.Count;

            if(cost < bestCost)
            {
                bestCost = cost;
                bestTile = tile;
                bestPath = path;
            }
        }

        if(bestPath != null)
        {
            return bestPath;
        }

        Debug.Log("No reachable melee origin for attacker footprint");
        return null;
    }

    private Vector2Int GetFootprintSize(ICombatant combatant)
    {
        List<Vector3Int> cells = combatant.GetOccupiedCells();
        if(cells == null || cells.Count == 0)
            return Vector2Int.one;

        int minX = cells[0].x;
        int maxX = cells[0].x;
        int minY = cells[0].y;
        int maxY = cells[0].y;

        foreach(Vector3Int cell in cells)
        {
            if(cell.x < minX) minX = cell.x;
            if(cell.x > maxX) maxX = cell.x;
            if(cell.y < minY) minY = cell.y;
            if(cell.y > maxY) maxY = cell.y;
        }

        return new Vector2Int(
            (maxX - minX) + 1,
            (maxY - minY) + 1
        );
    }

    private bool CanMeleeTargetFromOrigin(
        Vector3Int attackerOrigin,
        Vector2Int footprint,
        List<Vector3Int> targetCells
    )
    {
        List<Vector3Int> attackerCells = new List<Vector3Int>();

        for(int x = 0; x < footprint.x; x++)
        {
            for(int y = 0; y < footprint.y; y++)
            {
                attackerCells.Add(attackerOrigin + new Vector3Int(x, y, 0));
            }
        }

        foreach(Vector3Int attackerCell in attackerCells)
        {
            Vector3 attackerWorld = new Vector3(attackerCell.x + 0.5f, attackerCell.y + 0.5f, 0f);

            foreach(Vector3Int targetCell in targetCells)
            {
                Vector3 targetWorld = new Vector3(targetCell.x + 0.5f, targetCell.y + 0.5f, 0f);
                float dist = Vector3.Distance(attackerWorld, targetWorld);

                if(dist <= 1f)
                    return true;
            }
        }

        return false;
    }
}
