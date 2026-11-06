using UnityEngine;

// ArcaneFortitudeAbility.cs - Mage bonus action for defensive boost
public class ArcaneFortitudeAbility : Ability
{
    private int acBoost;
    private int durationTurns;
    private bool usedThisCombat;

    public ArcaneFortitudeAbility(int boost = 2, int duration = 3)
    {
        AbilityName = "Arcane Fortitude";
        CostType = AbilityCostType.BonusAction;
        Range = 0f;
        targetingMode = TargetingMode.Self;
        acBoost = boost;
        durationTurns = duration;
    }

    public override AbilityResult TryUse(ICombatant user, TargetData targetData)
    {
        if (!CanUse(user))
            return AbilityResult.CreateFailure("No bonus action available");

        if (usedThisCombat)
            return AbilityResult.CreateFailure("Arcane Fortitude already used this combat");

        // Apply AC buff to self
        StatusEffect buff = new ArmorClassBuff(acBoost, durationTurns);
        user.AddStatus(buff);

        usedThisCombat = true;
        SpendCost(user);

        Debug.Log($"{user.Name} gains {acBoost} AC for {durationTurns} turns");
        return AbilityResult.CreateSuccess();
    }

    protected override void Execute(ICombatant user, ICombatant target)
    {
        // Already handled in TryUse
    }

    // Reset at end of combat
    public void ResetForNewCombat()
    {
        usedThisCombat = false;
    }
}
