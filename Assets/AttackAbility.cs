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

    protected override void Execute(ICombatant user)
    {
        Debug.Log("Attack ability executed");

        BoxMover player = user as BoxMover;
        if(player == null) return;

        player.SetIntent(new AttackIntent(target));
    }
}
