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

        float distance = GetClosestDistanceToTarget(user, target);

        // Check range against nearest occupied cell, not just target origin
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

    private float GetClosestDistanceToTarget(ICombatant user, ICombatant target)
    {
        if(user == null || target == null)
            return float.MaxValue;

        Vector3 userWorld = user.GetWorldPosition();
        List<Vector3Int> occupiedCells = target.GetOccupiedCells();

        if(occupiedCells == null || occupiedCells.Count == 0)
        {
            return Vector3.Distance(userWorld, target.GetWorldPosition());
        }

        float closestDistance = float.MaxValue;

        foreach(Vector3Int cell in occupiedCells)
        {
            Vector3 cellWorld = new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);
            float dist = Vector3.Distance(userWorld, cellWorld);

            if(dist < closestDistance)
            {
                closestDistance = dist;
            }
        }

        return closestDistance;
    }
}
