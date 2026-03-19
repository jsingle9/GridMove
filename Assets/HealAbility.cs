using UnityEngine;

public class HealAbility : Ability
{
    public int healAmount = 10;

    public HealAbility()
    {
        AbilityName = "Heal";
        CostType = AbilityCostType.BonusAction;
        range = 3;
        targetingMode = TargetingMode.Self;
    }

    public override AbilityResult TryUse(ICombatant user, TargetData targetData)
    {
        if(!CanUse(user))
        {
            return AbilityResult.CreateFailure("No bonus action available");
        }

        if(targetData?.primaryTarget == null)
        {
            return AbilityResult.CreateFailure("No target");
        }

        SpendCost(user);
        Execute(user, targetData.primaryTarget);
        return AbilityResult.CreateSuccess();
    }

    protected override void Execute(ICombatant user, ICombatant myTarget)
    {
        Debug.Log("HealAbility Execute fired");

        if(myTarget == null)
        {
            Debug.Log("Heal failed: no target.");
            return;
        }

        myTarget.Heal(healAmount);
        Debug.Log($"{user} heals {myTarget} for {healAmount}");
    }
}
