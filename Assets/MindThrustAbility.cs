using UnityEngine;

/// <summary>
/// Mystic level 1 – Mind Thrust (Tier I).
/// Costs an Action. Ranged psychic attack (range 6 tiles).
/// Attack roll: INT modifier. Damage: 1d8 + INT modifier psychic.
/// Requires line of sight (mirrors RangedAttackAbility).
/// </summary>
public class MindThrustAbility : Ability
{
    public MindThrustAbility()
    {
        AbilityName = "Mind Thrust";
        CostType = AbilityCostType.Action;
        Range = 6f;
        targetingMode = TargetingMode.Enemy;
    }

    public override AbilityResult TryUse(ICombatant user, TargetData targetData)
    {
        if (!CanUse(user))
            return AbilityResult.CreateFailure("No action available");

        if (targetData?.primaryTarget == null)
            return AbilityResult.CreateFailure("No target");

        ICombatant target = targetData.primaryTarget;

        float distance = Vector3.Distance(user.GetWorldPosition(), target.GetWorldPosition());
        if (distance > Range)
            return AbilityResult.CreateFailure("Target out of range");

        GridController grid = Object.FindFirstObjectByType<GridController>();
        if (grid == null)
            return AbilityResult.CreateFailure("Grid not found");

        Vector3Int fromCell = grid.WorldToGrid(user.GetWorldPosition());
        Vector3Int toCell   = grid.WorldToGrid(target.GetWorldPosition());
        if (!grid.HasLineOfSight(fromCell, toCell))
            return AbilityResult.CreateFailure("No line of sight");

        SpendCost(user);
        Execute(user, target);
        return AbilityResult.CreateSuccess();
    }

    protected override void Execute(ICombatant user, ICombatant target)
    {
        if (user == null || target == null) return;

        TryPlayActionFlash(user);

        int intMod = GetIntMod(user);
        int roll   = DiceRoller.RollD20();
        int total  = roll + intMod;

        bool crit = roll == 20;
        bool hit  = (total >= target.ArmorClass) || crit;

        Debug.Log($"[MindThrust] {user.Name} rolls {roll} + {intMod} = {total} vs AC {target.ArmorClass}");

        if (hit)
        {
            int damage = DiceRoller.Roll("1d8") + intMod;
            if (crit) damage *= 2;

            Debug.Log($"[MindThrust] Hit! {damage} psychic damage");
            target.TakeDamage(damage);

            CombatUIManager.Instance?.LogAttack(
                user.Name, target.Name, true, roll, total, target.ArmorClass, damage);
        }
        else
        {
            Debug.Log("[MindThrust] Miss");
            CombatUIManager.Instance?.LogAttack(
                user.Name, target.Name, false, roll, total, target.ArmorClass);
        }
    }

    private int GetIntMod(ICombatant user)
    {
        BoxMover bm = user as BoxMover;
        return bm != null ? bm.Sheet.Scores.ModINT : 0;
    }

    private void TryPlayActionFlash(ICombatant user)
    {
        MonoBehaviour mb = user as MonoBehaviour;
        CombatantActionFlash flash = mb?.GetComponent<CombatantActionFlash>();
        flash?.PlayFlash();
    }
}
