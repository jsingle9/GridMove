using System.Collections.Generic;

public static class MovementCostUtility
{
    public static int CalculatePathCost(GridController grid, List<GridNode> path)
    {
        if (grid == null || path == null || path.Count == 0)
            return 0;

        int total = 0;

        for (int i = 0; i < path.Count; i++)
        {
            total += grid.GetMovementCost(path[i].gridPos);
        }

        return total;
    }

    public static List<GridNode> TrimPathToBudget(GridController grid, List<GridNode> path, int budget, out int spent)
    {
        spent = 0;

        List<GridNode> trimmed = new List<GridNode>();

        if (grid == null || path == null || path.Count == 0 || budget <= 0)
            return trimmed;

        for (int i = 0; i < path.Count; i++)
        {
            int stepCost = grid.GetMovementCost(path[i].gridPos);

            if (spent + stepCost > budget)
                break;

            spent += stepCost;
            trimmed.Add(path[i]);
        }

        return trimmed;
    }
}
