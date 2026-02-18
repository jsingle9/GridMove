
using UnityEngine;
using System.Collections.Generic;

public class Pathfinder{
    private GridController grid;

    public Pathfinder(GridController inGrid){
      this.grid = inGrid;
    }

    public List<GridNode> FindPath(GridNode startNode, GridNode targetNode){
      if (startNode == null || targetNode == null)
            return null;

        if (!targetNode.walkable)
            return null;

        List<GridNode> openSet = new List<GridNode>();
        HashSet<GridNode> closedSet = new HashSet<GridNode>();

        openSet.Add(startNode);

        // Reset start node
        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);
        startNode.parent = null;

        while (openSet.Count > 0){
            GridNode currentNode = openSet[0];

            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                    openSet[i].fCost == currentNode.fCost &&
                    openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            foreach (GridNode neighbor in grid.GetNeighbors(currentNode))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                    continue;

                //  block tiles occupied by combatants DURING COMBAT
               if(GameStateManager.Instance.CurrentState == GameState.Combat){

                  // allow pathing to FINAL target even if occupied
                  bool isTargetTile = neighbor == targetNode;

                  if(!isTargetTile && grid.IsTileOccupied(neighbor.gridPos)){
                  continue;
                }
              }
              int newCostToNeighbor =
                    currentNode.gCost + GetDistance(currentNode, neighbor);

                if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null;
    }

    private List<GridNode> RetracePath(GridNode startNode, GridNode endNode)
    {
        List<GridNode> path = new List<GridNode>();
        GridNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    private int GetDistance(GridNode a, GridNode b)
    {
        int dstX = Mathf.Abs(a.gridPos.x - b.gridPos.x);
        int dstY = Mathf.Abs(a.gridPos.y - b.gridPos.y);

        // 4-direction grid
        return dstX + dstY;
    }
}
