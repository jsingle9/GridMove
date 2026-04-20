using UnityEngine;


public class AttackAbility : Ability
{
    public AttackAbility()
    {
        AbilityName = "Attack";
        CostType = AbilityCostType.Action;
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
            return AbilityResult.CreateFailure("Target out of melee range");
        }

        // In range → execute
        SpendCost(user);
        Execute(user, target);
        return AbilityResult.CreateSuccess();
    }

    protected override void Execute(ICombatant user, ICombatant target)
    {
        if(target == null) return;

        Debug.Log($"{user} attacks {target}");

        int roll = DiceRoller.RollD20();
        int total = roll + user.AttackBonus;

        Debug.Log($"Attack roll: {roll} + {user.AttackBonus} = {total} vs AC {target.ArmorClass}");

        bool crit = roll == 20;

        if(total >= target.ArmorClass || crit)
        {
            int damage = DiceRoller.Roll(user.DamageDice) + user.DamageModifier;

            if(crit)
            {
                Debug.Log("CRITICAL HIT!");
                damage *= 2;
            }

            Debug.Log($"Hit for {damage} damage");
            target.TakeDamage(damage);

        }
        else
        {
            Debug.Log("Miss");
        }
    }

}
