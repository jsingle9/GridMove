using UnityEngine;

/// <summary>
/// Mystic level 1 – Psionic Focus.
/// Bonus Action. Self only. Once per combat.
/// Grants temporary HP equal to 1d6 + WIS modifier and resets
/// the "psionic focus" concentration for the current combat.
/// </summary>
public class PsionicFocusAbility : Ability
{
    public bool UsedThisCombat { get; private set; }

    public PsionicFocusAbility()
    {
        AbilityName = "Psionic Focus";
        CostType = AbilityCostType.BonusAction;
        Range = 0f;
        targetingMode = TargetingMode.Self;
    }

    public void ResetForNewCombat()
    {
        UsedThisCombat = false;
    }

    public override bool CanUse(ICombatant user)
    {
        return base.CanUse(user) && !UsedThisCombat;
    }

    public override AbilityResult TryUse(ICombatant user, TargetData targetData)
    {
        if (!base.CanUse(user))
            return AbilityResult.CreateFailure("No bonus action available");

        if (UsedThisCombat)
            return AbilityResult.CreateFailure("Psionic Focus already used this combat");

        SpendCost(user);
        Execute(user, user);
        UsedThisCombat = true;
        return AbilityResult.CreateSuccess();
    }

    protected override void Execute(ICombatant user, ICombatant myTarget)
    {
        if (user == null) return;

        int wisMod = GetWisMod(user);
        int tempHP = Mathf.Max(1, DiceRoller.Roll("1d6") + wisMod);

        // Add temp HP by healing up to max (temp HP stacks on top of current HP
        // via the existing Heal path, which is the closest approximation until
        // a dedicated TempHP system is built).
        user.Heal(tempHP);

        Debug.Log($"[PsionicFocus] {user.Name} focuses: gains {tempHP} temp HP (1d6 + {wisMod})");

        CombatUIManager.Instance?.AddLog(
            $"{user.Name} enters Psionic Focus and gains {tempHP} temp HP.");
    }

    private int GetWisMod(ICombatant user)
    {
        BoxMover bm = user as BoxMover;
        return bm != null ? bm.Sheet.Scores.ModWIS : 0;
    }
}
