using UnityEngine;

public class AttackAbility : Ability{
    Enemy target;

    public AttackAbility(Enemy targetEnemy)
    {
        AbilityName = "Attack";
        CostType = AbilityCostType.Action;
        target = targetEnemy;
    }

    /*public override void SetTarget(object t){
      target = t as ICombatant;
    }*/

    protected override void Execute(ICombatant user){
      if(target == null) return;

      Debug.Log($"{user} attacks {target}");

      int roll = DiceRoller.RollD20();
      int total = roll + user.AttackBonus;

      // Log the numbers so we can see what is happening
      // with the math
      Debug.Log($"Attack roll: {roll} + {user.AttackBonus} = {total} vs AC {target.ArmorClass}");

        // NAT 20 crit
        bool crit = roll == 20;

        if(total >= target.ArmorClass || crit){
          int damage = DiceRoller.Roll(user.DamageDice) + user.DamageModifier;

        if(crit){
            Debug.Log("CRITICAL HIT!");
            damage *= 2;
        }

          Debug.Log($"Hit for {damage} damage");
          target.TakeDamage(damage);
      }
      else{
          Debug.Log("Miss");
      }
  }

}
