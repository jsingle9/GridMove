using UnityEngine;

public class AttackAbility : Ability
{
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

    protected override void Execute(ICombatant user)
    {
        Debug.Log("Attack ability executed");

        //BoxMover player = user as BoxMover;
        if(target == null) return;

        user.SetIntent(new AttackIntent(target));
    }
}
