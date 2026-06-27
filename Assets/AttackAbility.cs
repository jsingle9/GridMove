using UnityEngine;
using System.Collections.Generic;

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

        float distance = GetClosestCombatDistance(user, target);

        // Check range using all occupied cells of attacker and target
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

    private float GetClosestCombatDistance(ICombatant user, ICombatant target)
    {
        if(user == null || target == null)
            return float.MaxValue;

        List<Vector3Int> userCells = user.GetOccupiedCells();
        List<Vector3Int> targetCells = target.GetOccupiedCells();

        if(userCells == null || userCells.Count == 0 ||
           targetCells == null || targetCells.Count == 0)
        {
            return Vector3.Distance(user.GetWorldPosition(), target.GetWorldPosition());
        }

        float closestDistance = float.MaxValue;

        foreach(Vector3Int userCell in userCells)
        {
            Vector3 userWorld = new Vector3(userCell.x + 0.5f, userCell.y + 0.5f, 0f);

            foreach(Vector3Int targetCell in targetCells)
            {
                Vector3 targetWorld = new Vector3(targetCell.x + 0.5f, targetCell.y + 0.5f, 0f);
                float dist = Vector3.Distance(userWorld, targetWorld);

                if(dist < closestDistance)
                {
                    closestDistance = dist;
                }
            }
        }

        return closestDistance;
    }
}
