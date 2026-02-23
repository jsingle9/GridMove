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

    List<GridNode> ResolveAttackMove(
      GridNode actorNode,
      ICombatant enemy
    ){
        GridNode enemyNode = grid.GetNodeFromWorld(enemy.GetWorldPosition());

        List<GridNode> bestPath = null;
        int bestLength = int.MaxValue;

        foreach(GridNode neighbor in grid.GetNeighbors(enemyNode)){

        // must be walkable
        if(!neighbor.walkable)
            continue;

        // cannot stand on another combatant
        if(GameStateManager.Instance.CurrentState == GameState.Combat){
          if(grid.IsTileOccupied(neighbor.gridPos))
            continue;
        }

        List<GridNode> path = pathfinder.FindPath(actorNode, neighbor);

        if(path == null || path.Count == 0)
            continue;

        // choose shortest valid path
        if(path.Count < bestLength){
            bestLength = path.Count;
            bestPath = path;
        }
    }

    if(bestPath == null){
        Debug.Log("No valid adjacent path to enemy");
    }

    return bestPath;
  }


}
