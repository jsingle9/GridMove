
using UnityEngine;
using System.Collections.Generic;

public class Pathfinder{
    private GridController grid;

    public Pathfinder(GridController inGrid){
      this.grid = inGrid;
    }

    public List<GridNode> FindPath(GridNode startNode, GridNode targetNode)
    {
        return FindPath(startNode, targetNode, null, Vector2Int.one);
    }

    public List<GridNode> FindPath(GridNode startNode, GridNode targetNode, ICombatant moverCombatant, Vector2Int footprint)
    {
        if (startNode == null || targetNode == null)
            return null;

        for (int x = 0; x < grid.grid.GetLength(0); x++){
            for (int y = 0; y < grid.grid.GetLength(1); y++){
                GridNode node = grid.grid[x, y];
                node.gCost = int.MaxValue;
                node.hCost = 0;
                node.parent = null;
            }
        }

        if (!targetNode.walkable)
            return null;

        List<GridNode> openSet = new List<GridNode>();
        HashSet<GridNode> closedSet = new HashSet<GridNode>();

        openSet.Add(startNode);
        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);
        startNode.parent = null;

        while (openSet.Count > 0){
            GridNode currentNode = openSet[0];

            for (int i = 1; i < openSet.Count; i++){
                if (openSet[i].fCost < currentNode.fCost ||
                   (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
                return RetracePath(startNode, targetNode);

            foreach (GridNode neighbor in grid.GetNeighbors(currentNode))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                // FOOTPRINT-AWARE occupancy/walkability check
                if (!grid.CanOccupyFootprint(neighbor.gridPos, footprint.x, footprint.y, moverCombatant))
                    continue;

                int stepCost = grid.GetMovementCost(neighbor.gridPos);
                if (stepCost <= 0) stepCost = 1;

                int newCostToNeighbor = currentNode.gCost + stepCost;

                if (newCostToNeighbor < neighbor.gCost)
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
