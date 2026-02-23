using UnityEngine;

public class AttackAbility : Ability
{
    ICombatant target;

    public AttackAbility(ICombatant targetCombatant)
    {
        AbilityName = "Attack";
        CostType = AbilityCostType.Action;
        target = targetCombatant;
    }

    protected override void Execute(ICombatant user)
    {
        if (target == null) return;

        float distance = Vector3.Distance(
            user.GetWorldPosition(),
            target.GetWorldPosition()
        );

        // melee range check
        if (distance > 1.5f)
        {
            Debug.Log("Target out of range → moving into range");
            user.SetIntent(new AttackIntent(target));
            return;
        }

        Debug.Log($"{user} attacks {target}");

        int roll = DiceRoller.RollD20();
        int total = roll + user.AttackBonus;

        Debug.Log($"Attack roll: {roll} + {user.AttackBonus} = {total} vs AC {target.ArmorClass}");

        bool crit = roll == 20;

        if (total >= target.ArmorClass || crit)
        {
            int damage = DiceRoller.Roll(user.DamageDice) + user.DamageModifier;

            if (crit)
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
