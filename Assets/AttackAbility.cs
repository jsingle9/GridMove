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

      int damage = Random.Range(3, 9);

      Debug.Log($"{user} hits {target} for {damage}");

      target.TakeDamage(damage);
    }
}
