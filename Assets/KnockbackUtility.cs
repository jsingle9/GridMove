using UnityEngine;

public static class KnockbackUtility
{
    /// <summary>
    /// Pushes defender away from attacker along the cardinal direction from attacker->defender.
    /// Stops at first blocked tile. Returns true if moved at least 1 tile.
    /// </summary>
    public static bool TryPush(ICombatant defender, Vector3Int attackerCell, Vector3Int defenderCell, int maxTiles)
    {
        var grid = GridRegistry.Grid;
        if (grid == null || defender == null || maxTiles <= 0)
            return false;

        Vector3Int dir = new Vector3Int(
            Mathf.Clamp(defenderCell.x - attackerCell.x, -1, 1),
            Mathf.Clamp(defenderCell.y - attackerCell.y, -1, 1),
            0
        );

        // Keep knockback cardinal for cleaner behavior
        if (Mathf.Abs(dir.x) > 0 && Mathf.Abs(dir.y) > 0)
            dir.y = 0;

        Vector3Int current = defenderCell;
        Vector3Int lastValid = defenderCell;

        for (int i = 0; i < maxTiles; i++)
        {
            Vector3Int next = current + dir;

            if (!grid.IsWalkable(next))
                break;

            if (grid.GetOccupant(next) != null)
                break;

            lastValid = next;
            current = next;
        }

        if (lastValid == defenderCell)
            return false;

        defender.transform.position = grid.GridToWorld(lastValid);
        return true;
    }
}
