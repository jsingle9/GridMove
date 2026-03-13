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
      Debug.Log($"TARGET WORLD POSITION: {target.GetWorldPosition()}");
      Debug.Log($"CALCULATED TARGET GRID: {targetNode.gridPos}");

      if(grid.IsTileOccupied(targetNode.gridPos) == false){
        Debug.LogWarning("Resolver thinks target tile is empty!");
      }
        Debug.Log("Target passed to resolver: " + target);
        Debug.Log("Target node instance ID: " + targetNode.GetHashCode());
        if(targetNode == null)
            return null;

        List<GridNode> neighbors = grid.GetNeighbors(targetNode);
        //Debug.Log($"Found {neighbors.Count} neighbor tiles");
        GridNode bestTile = null;
        List<GridNode> bestPath = null;
        int bestCost = int.MaxValue;

        foreach (GridNode tile in neighbors){
            Debug.Log($"Checking neighbor: {tile.gridPos}, walkable: {tile.walkable}, occupied: {grid.IsTileOccupied(tile.gridPos)}");

            if(tile.gridPos == targetNode.gridPos) continue;
            if(tile == null) continue;
            if(!tile.walkable) continue;
            //if (grid.IsTileOccupied(tile.gridPos)) continue;
            if(grid.IsTileOccupied(tile.gridPos)){
                // Allow if this tile contains only the target
                ICombatant occupant = grid.GetOccupant(tile.gridPos);
                if (occupant != target) continue;
            }

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
