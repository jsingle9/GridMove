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
          Enemy enemy
        ){
          GridNode enemyNode =
              grid.GetNodeFromWorld(enemy.transform.position);

              foreach(GridNode neighbor in grid.GetNeighbors(enemyNode)){
              if(!neighbor.walkable) continue;

              return pathfinder.FindPath(actorNode, neighbor);
          }

        return null;
    }
}
