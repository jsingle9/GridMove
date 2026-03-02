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

        // all adjacent tiles to clicked enemy
        List<GridNode> adjacent = grid.GetNeighbors(enemyNode);

        foreach(GridNode neighbor in adjacent)
        {
            if(!neighbor.walkable)
                continue;

            // cannot stand on occupied tile
            if(GameStateManager.Instance.CurrentState == GameState.Combat)
            {
                if(grid.IsTileOccupied(neighbor.gridPos))
                    continue;
            }

            List<GridNode> path = pathfinder.FindPath(actorNode, neighbor);
            if(path == null || path.Count == 0)
                continue;

            int length = path.Count;

            if(length < bestLength)
            {
                bestLength = length;
                bestPath = path;
            }
        }

        // ✔ Found reachable adjacent tile
        if(bestPath != null)
        {
            Debug.Log("Attack path chosen (adjacent to clicked enemy):");
            foreach(var n in bestPath)
                Debug.Log(n.gridPos);

            return bestPath;
        }

        // ❗ No adjacent tile reachable
        // Move toward enemy instead (5e behavior)
        Debug.Log("No adjacent tile reachable → moving toward enemy");

        List<GridNode> fallback = pathfinder.FindPath(actorNode, enemyNode);

        if(fallback == null || fallback.Count == 0)
            return null;

        return fallback;
    }

}
