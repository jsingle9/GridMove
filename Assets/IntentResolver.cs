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
    ){
        if(intent is MoveIntent move){
            Debug.Log("intent = move intent");
            GridNode target = grid.GetNodeFromWorld(
                grid.GridToWorld(move.targetCell)
            );

            return pathfinder.FindPath(actorNode, target);
        }

        if(intent is AttackIntent attack){
            Debug.Log("intent = attack intent");
            return ResolveAttackMove(actorNode, attack.target);
        }

        return null;
    }

    public List<GridNode> ResolveAttackMove(GridNode actorNode, ICombatant target){
        GridNode targetNode = grid.GetNodeFromWorld(target.GetWorldPosition());
        if(targetNode == null)
            return null;

        // Get all 4 adjacent tiles to target
        List<GridNode> neighbors = grid.GetNeighbors(targetNode);

        // 🔥 IMPORTANT: deterministic order
        // This prevents circling/overthinking
        foreach(GridNode tile in neighbors){
            if(tile == null) continue;
            if(!tile.walkable) continue;

            // cannot stand on occupied tile
            if(grid.IsTileOccupied(tile.gridPos))
                continue;

            List<GridNode> path = pathfinder.FindPath(actorNode, tile);

            if(path != null && path.Count > 0){
                Debug.Log($"Attack move chosen tile: {tile.gridPos}");
                return path; // 🔥 FIRST VALID TILE ONLY
            }
        }

        Debug.Log("No reachable adjacent tile to target");
        return null;
    }
}
