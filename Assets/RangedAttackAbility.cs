using UnityEngine;

public class RangedAttackAbility : Ability
{
    public RangedAttackAbility()
    {
        AbilityName = "Bow Shot";
        CostType = AbilityCostType.Action;
        Range = 6f;
    }

    public override AbilityResult TryUse(ICombatant user, TargetData targetData)
    {
        if(!CanUse(user))
        {
            return AbilityResult.CreateFailure("No action available");
        }

        if(targetData?.primaryTarget == null)
        {
            return AbilityResult.CreateFailure("No target");
        }

        ICombatant target = targetData.primaryTarget;
        float distance = Vector3.Distance(
            user.GetWorldPosition(),
            target.GetWorldPosition()
        );

        // Check range
        if(distance > Range)
        {
            return AbilityResult.CreateFailure("Target out of range");
        }

        // In range → execute
        SpendCost(user);
        Execute(user, target);
        return AbilityResult.CreateSuccess();
    }

    protected override void Execute(ICombatant user, ICombatant target)
    {
        if(target == null) return;

        int roll = DiceRoller.RollD20();
        int total = roll + user.AttackBonus;

        bool crit = roll == 20;

        if(total >= target.ArmorClass || crit)
        {
            int damage = DiceRoller.Roll(user.DamageDice) + user.DamageModifier;
            if(crit) damage *= 2;

            target.TakeDamage(damage);
            Debug.Log($"{user} shoots {target} for {damage}");
        }
        else
        {
            Debug.Log($"{user} missed ranged attack");
        }
    }
}
