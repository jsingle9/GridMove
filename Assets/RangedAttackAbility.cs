using UnityEngine;

public class RangedAttackAbility : Ability
{
    public RangedAttackAbility()
    {
        AbilityName = "Bow Shot";
        CostType = AbilityCostType.Action;
        Range = 6f;
        targetingMode = TargetingMode.Enemy;
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

        float distance = Vector3.Distance(
            user.GetWorldPosition(),
            target.GetWorldPosition()
        );

        if(distance > Range)
        {
            return AbilityResult.CreateFailure("Target out of range");
        }

        GridController grid = Object.FindFirstObjectByType<GridController>();
        if(grid == null)
        {
            return AbilityResult.CreateFailure("No GridController found");
        }

        Vector3Int fromCell = grid.WorldToGrid(user.GetWorldPosition());
        Vector3Int toCell = grid.WorldToGrid(target.GetWorldPosition());

        if(!grid.HasLineOfSight(fromCell, toCell))
        {
            return AbilityResult.CreateFailure("No line of sight");
        }

        SpendCost(user);
        Execute(user, target);
        return AbilityResult.CreateSuccess();
    }

    protected override void Execute(ICombatant user, ICombatant target)
    {
        if(target == null || user == null)
            return;

        TryPlayActionFlash(user);

        int roll = DiceRoller.RollD20();
        int total = roll + user.AttackBonus;
        bool crit = roll == 20;
        bool hit = (total >= target.ArmorClass) || crit;

        if(hit)
        {
            string damageDice = user.DamageDice;
            int damageModifier = user.DamageModifier;

            BoxMover boxMover = user as BoxMover;
            if(boxMover != null)
            {
                Weapon rangedWeapon = Inventory.Instance.GetEquippedRangedWeapon();
                if(rangedWeapon != null)
                {
                    damageDice = rangedWeapon.DamageDice;
                    damageModifier = rangedWeapon.DamageBonus;
                }
            }

            int damage = DiceRoller.Roll(damageDice) + damageModifier;
            if(crit)
                damage *= 2;

            Debug.Log($"{user} shoots {target} for {damage} damage");
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
            Debug.Log($"{user} missed ranged attack");

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
