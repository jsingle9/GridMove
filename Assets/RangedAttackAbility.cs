using UnityEngine;

public class RangedAttackAbility : Ability
{
    public RangedAttackAbility()
    {
        AbilityName = "Bow Shot";
        CostType = AbilityCostType.Action;
        Range = 6f;
    }

    protected override void Execute(ICombatant user, ICombatant target)
    {
        if(target == null) return;

        float distance = UnityEngine.Vector3.Distance(
            user.GetWorldPosition(),
            target.GetWorldPosition()
        );

        // Move closer if somehow out of range
        if(distance > Range)
        {
            if(!user.HasMove){
                Debug.Log("Too far and no move left");
                return;
            }

            Debug.Log("Ranged unit moving into range");
            user.SetIntent(new AttackIntent(target));
            return;
        }

        int roll = DiceRoller.RollD20();
        int total = roll + user.AttackBonus;

        bool crit = roll == 20;

        if(total >= target.ArmorClass || crit)
        {
            int damage = DiceRoller.Roll(user.DamageDice) + user.DamageModifier;
            if(crit) damage *= 2;

            target.TakeDamage(damage);
            UnityEngine.Debug.Log($"{user} shoots {target} for {damage}");
        }
        else
        {
            UnityEngine.Debug.Log($"{user} missed ranged attack");
        }
    }
}
