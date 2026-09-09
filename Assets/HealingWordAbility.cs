using UnityEngine;

/// <summary>
/// Mystic 1st-level spell – Healing Word.
/// Action. Ranged (5 grid units). Target one ally.
/// Restore 1d6 + WIS modifier HP.
/// Requires 1st-level spell slot.
/// </summary>
public class HealingWordAbility : Ability
{
    private const int SpellLevel = 1;

    public HealingWordAbility()
    {
        AbilityName = "Healing Word";
        CostType = AbilityCostType.Action;
        Range = 5f;
        targetingMode = TargetingMode.Ally;
    }

    public override AbilityResult TryUse(ICombatant user, TargetData targetData)
    {
        if (!CanUse(user))
            return AbilityResult.CreateFailure("No action available");

        BoxMover bm = user as BoxMover;
        if (bm?.Sheet == null)
            return AbilityResult.CreateFailure("Cannot cast spells");

        if (!bm.Sheet.SpellSlots.HasSlot(SpellLevel))
            return AbilityResult.CreateFailure($"No {SpellLevel}st-level spell slots remaining");

        if (targetData?.primaryTarget == null)
            return AbilityResult.CreateFailure("No target");

        ICombatant target = targetData.primaryTarget;

        float distance = Vector3.Distance(user.GetWorldPosition(), target.GetWorldPosition());
        if (distance > Range)
            return AbilityResult.CreateFailure("Target out of range");

        SpendCost(user);

        if (!bm.Sheet.SpellSlots.TrySpendSlot(SpellLevel))
            return AbilityResult.CreateFailure("Failed to spend spell slot");

        Execute(user, target);
        return AbilityResult.CreateSuccess();
    }

    protected override void Execute(ICombatant user, ICombatant target)
    {
        if (target == null) return;

        int wisMod = GetWisMod(user);
        int healing = Mathf.Max(1, DiceRoller.Roll("1d6") + wisMod);

        target.Heal(healing);

        Debug.Log($"[HealingWord] {user.Name} heals {target.Name} for {healing} HP (1d6 + {wisMod})");

        CombatUIManager.Instance?.AddLog(
            $"{user.Name} casts Healing Word. {target.Name} recovers {healing} HP.");
    }

    private int GetWisMod(ICombatant user)
    {
        BoxMover bm = user as BoxMover;
        return bm != null ? bm.Sheet.Scores.ModWIS : 0;
    }
}
