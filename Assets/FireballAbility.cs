using UnityEngine;

public class FireballAbility : Ability
{
    public int damagePerTarget = 8;
    public int radiusSize = 2;

    public FireballAbility()
    {
        AbilityName = "Fireball";
        CostType = AbilityCostType.Action;
        Range = 10f;
        radius = radiusSize;
        targetingMode = TargetingMode.Area;
    }

    public override AbilityResult TryUse(ICombatant user, TargetData targetData)
    {
        if(!CanUse(user))
        {
            return AbilityResult.CreateFailure("No action available");
        }

        if(targetData?.tile == null)
        {
            return AbilityResult.CreateFailure("No target tile");
        }

        Vector3 tileWorldPos = targetData.tile.gridPos;
        float distance = Vector3.Distance(
            user.GetWorldPosition(),
            tileWorldPos
        );

        // Check range to center of AoE
        if(distance > Range)
        {
            return AbilityResult.CreateFailure("Target out of range");
        }

        // In range → execute
        SpendCost(user);
        Execute(user, null); // Fireball doesn't use primaryTarget

        // Damage all units in area
        if(targetData.unitsInArea != null)
        {
            foreach(ICombatant target in targetData.unitsInArea)
            {
                if(target != null && target != user)
                {
                    int damage = DiceRoller.Roll("2d6");
                    target.TakeDamage(damage);
                    Debug.Log($"Fireball hits {target} for {damage} damage");
                }
            }
        }

        return AbilityResult.CreateSuccess();
    }

    protected override void Execute(ICombatant user, ICombatant myTarget)
    {
        // Fireball doesn't have single-target execute logic
        Debug.Log($"{user} casts Fireball!");
    }
}
