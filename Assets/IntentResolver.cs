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
      GridNode targetNode = grid.GetNodeFromWorld(
            grid.GridToWorld(grid.WorldToGrid(target.GetWorldPosition()))
      );
      Debug.Log("Target passed to resolver: " + target);
      Debug.Log("Target node instance ID: " + targetNode.GetHashCode());
        if (targetNode == null)
            return null;

        List<GridNode> neighbors = grid.GetNeighbors(targetNode);

        GridNode bestTile = null;
        List<GridNode> bestPath = null;
        int bestCost = int.MaxValue;

        foreach (GridNode tile in neighbors){

            if (tile == null) continue;
            if (!tile.walkable) continue;
            if (grid.IsTileOccupied(tile.gridPos)) continue;

            List<GridNode> path = pathfinder.FindPath(actorNode, tile);
            if (path == null || path.Count == 0) continue;

            int cost = path.Count;

            if (cost < bestCost){
                bestCost = cost;
                bestTile = tile;
                bestPath = path;
            }
        }

        if (bestPath != null){
            Debug.Log($"Attack move chosen tile: {bestTile.gridPos} with cost {bestCost}");
            return bestPath;
        }

        Debug.Log("No reachable adjacent tile to target");
        return null;
    }
}
