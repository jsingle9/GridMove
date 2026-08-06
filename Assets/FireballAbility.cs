using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballAbility : Ability
{
    public int damagePerTarget = 8;
    public int radiusSize = 2;

    [Header("Telegraph")]
    [SerializeField] private TelegraphStyle telegraphStyle;

    public FireballAbility()
    {
        AbilityName = "Fireball";
        CostType = AbilityCostType.Action;
        Range = 10f;
        radius = radiusSize;
        targetingMode = TargetingMode.Area;
    }

    public override AbilityResult TryUse(ICombatant user, TargetData targetData)
    {
        if (!CanUse(user))
            return AbilityResult.CreateFailure("No action available");

        if (targetData?.tile == null)
            return AbilityResult.CreateFailure("No target tile");

        // IMPORTANT: compare world-to-world, not world-to-gridpos directly
        GridController grid = Object.FindFirstObjectByType<GridController>();
        if (grid == null)
            return AbilityResult.CreateFailure("Grid not found");

        Vector3 centerWorld = grid.GridToWorld(targetData.tile.gridPos);
        float distance = Vector3.Distance(user.GetWorldPosition(), centerWorld);

        if (distance > Range)
            return AbilityResult.CreateFailure("Target out of range");

        SpendCost(user);

        if (AbilityUI.Instance == null)
            return AbilityResult.CreateFailure("AbilityUI runner missing");

        AbilityUI.Instance.StartCoroutine(ExecuteWithTelegraph(user, targetData));
        return AbilityResult.CreateSuccess();
    }

    private IEnumerator ExecuteWithTelegraph(ICombatant user, TargetData targetData)
    {
        GridController grid = Object.FindFirstObjectByType<GridController>();
        SpellTelegrapher tele = Object.FindFirstObjectByType<SpellTelegrapher>();
        if (grid == null || targetData?.tile == null)
            yield break;

        List<Vector3Int> cells = new();
        foreach (var node in grid.GetNodesInRadius(targetData.tile.gridPos, radius))
            cells.Add(node.gridPos);

        if (tele != null && telegraphStyle != null)
            tele.Show(grid, cells, telegraphStyle);

        float delay = telegraphStyle != null ? telegraphStyle.duration : 0.8f;
        yield return new WaitForSeconds(delay);

        if (tele != null)
            tele.Clear();

        // Damage all units in area (dedupe in case multi-tile units appear multiple times)
        HashSet<ICombatant> hit = new HashSet<ICombatant>();
        foreach (ICombatant target in targetData.unitsInArea)
        {
            if (target == null || target == user || target.IsDead()) continue;
            if (!hit.Add(target)) continue;

            int damage = DiceRoller.Roll("2d8");
            target.TakeDamage(damage);
            Debug.Log($"Fireball hits {target} for {damage} damage");
        }

        Execute(user, null);
    }

    protected override void Execute(ICombatant user, ICombatant myTarget)
    {
        Debug.Log($"{user} casts Fireball!");
    }
}
