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
        if(target == null || user == null) return;

        Debug.Log($"{user} attacks {target}");
        TryPlayActionFlash(user);

        int roll = DiceRoller.RollD20();
        int total = roll + user.AttackBonus;

        Debug.Log($"Attack roll: {roll} + {user.AttackBonus} = {total} vs AC {target.ArmorClass}");

        bool crit = roll == 20;
        bool hit = (total >= target.ArmorClass) || crit;

        if(hit)
        {
            int damage = DiceRoller.Roll(user.DamageDice) + user.DamageModifier;

            if(crit)
            {
                Debug.Log("CRITICAL HIT!");
                damage *= 2;
            }

            Debug.Log($"Hit for {damage} damage");
            target.TakeDamage(damage);

            CombatUIManager.Instance?.LogAttack(
                user.Name,
                target.Name,
                true,
                roll,
                total,
                target.ArmorClass,
                damage
            );
        }
        else
        {
            Debug.Log("Miss");

            CombatUIManager.Instance?.LogAttack(
                user.Name,
                target.Name,
                false,
                roll,
                total,
                target.ArmorClass
            );
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

    private void TryPlayActionFlash(ICombatant user)
    {
        MonoBehaviour mb = user as MonoBehaviour;
        if (mb == null)
            return;

        CombatantActionFlash flash = mb.GetComponent<CombatantActionFlash>();
        if (flash != null)
            flash.PlayFlash();
    }
}
