using UnityEngine;

public static class KnockbackUtility
{
    public static bool TryPushAway(ICombatant attacker, ICombatant target, int maxTiles)
    {
        if (attacker == null || target == null || maxTiles <= 0)
            return false;

        GridController grid = Object.FindFirstObjectByType<GridController>();
        if (grid == null)
            return false;

        Vector3Int attackerCell = grid.WorldToGrid(attacker.GetWorldPosition());
        Vector3Int targetCell = grid.WorldToGrid(target.GetWorldPosition());

        Vector3Int dir = new Vector3Int(
            Mathf.Clamp(targetCell.x - attackerCell.x, -1, 1),
            Mathf.Clamp(targetCell.y - attackerCell.y, -1, 1),
            0
        );

        if (dir == Vector3Int.zero)
            return false;

        // prefer cardinal push
        if (Mathf.Abs(dir.x) > 0 && Mathf.Abs(dir.y) > 0)
            dir.y = 0;

        Vector3Int current = targetCell;
        Vector3Int lastValid = targetCell;

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

        if (lastValid == targetCell)
            return false;

        MonoBehaviour mb = target as MonoBehaviour;
        if (mb == null)
            return false;

        if (target is BoxMover box && box.TryForceMoveToCell(lastValid))
            return true;

        // fallback (old behavior, not ideal)
        mb.transform.position = grid.GridToWorld(lastValid);
            return true;
    }
}
