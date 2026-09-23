using System.Collections.Generic;
using UnityEngine;

public class AttackAbility : Ability
{
    private const int DefaultMeleeTargetingRange = 1;
    private const int DefaultRangedRange = 6;

    public AttackAbility()
    {
        AbilityName = "Attack";
        CostType = AbilityCostType.Action;
        targetingMode = TargetingMode.Enemy;

        // Updated when the player begins targeting.
        Range = DefaultMeleeTargetingRange;
        range = DefaultMeleeTargetingRange;
    }

    private WeaponItem GetEquippedWeapon(ICombatant user)
    {
        if (user is BoxMover boxMover)
            return boxMover.EquippedWeapon;

        return null;
    }

    public bool UsesRangedWeapon(ICombatant user)
    {
        WeaponItem weapon = GetEquippedWeapon(user);

        return weapon != null &&
               weapon.weaponType == WeaponType.Ranged;
    }

    public bool UsesMeleeWeapon(ICombatant user)
    {
        WeaponItem weapon = GetEquippedWeapon(user);

        return weapon == null ||
               weapon.weaponType == WeaponType.Melee;
    }

    public void RefreshTargetingRange(ICombatant user)
    {
        if (user == null)
            return;

        WeaponItem weapon = GetEquippedWeapon(user);

        if (weapon != null &&
            weapon.weaponType == WeaponType.Ranged)
        {
            Range = Mathf.Max(
                DefaultRangedRange,
                weapon.range
            );

            range = Mathf.CeilToInt(Range);
            return;
        }

        /*
         * For melee weapons, allow the player to select targets that
         * may be reachable using remaining movement. The final path
         * check is performed by AttackAbility/IntentExecutor.
         */
        Range = user.RemainingMovement + 1f;
        range = Mathf.CeilToInt(Range);
    }

    public bool CanUseRanged(
        ICombatant user,
        ICombatant target
    )
    {
        if (user == null || target == null)
            return false;

        WeaponItem weapon = GetEquippedWeapon(user);

        if (weapon == null ||
            weapon.weaponType != WeaponType.Ranged)
        {
            return false;
        }

        GridController grid = GridRegistry.Grid;

        if (grid == null)
            return false;

        float distance = GetClosestCombatDistance(
            user,
            target
        );

        int weaponRange = weapon.range > 0
            ? weapon.range
            : DefaultRangedRange;

        if (distance > weaponRange)
            return false;

        Vector3Int fromCell =
            grid.WorldToGrid(user.GetWorldPosition());

        Vector3Int toCell =
            grid.WorldToGrid(target.GetWorldPosition());

        return grid.HasLineOfSight(fromCell, toCell);
    }

    public bool CanReachMeleeTarget(
        ICombatant user,
        ICombatant target
    )
    {
        if (user == null || target == null)
            return false;

        if (!UsesMeleeWeapon(user))
            return false;

        GridController grid = GridRegistry.Grid;

        if (grid == null)
            return false;

        if (!AttackReachabilityUtility.TryGetMeleeApproach(
            grid,
            user,
            target,
            out _,
            out int movementCost
        ))
        {
            return false;
        }

        return movementCost <= user.RemainingMovement;
    }

    public bool TryGetMeleeApproach(
        ICombatant user,
        ICombatant target,
        out List<GridNode> path,
        out int movementCost
    )
    {
        path = null;
        movementCost = -1;

        if (user == null || target == null)
            return false;

        if (!UsesMeleeWeapon(user))
            return false;

        GridController grid = GridRegistry.Grid;

        if (grid == null)
            return false;

        return AttackReachabilityUtility.TryGetMeleeApproach(
            grid,
            user,
            target,
            out path,
            out movementCost
        );
    }

    public override AbilityResult TryUse(
        ICombatant user,
        TargetData targetData
    )
    {
        if (!CanUse(user))
        {
            return AbilityResult.CreateFailure(
                "No action available"
            );
        }

        if (targetData?.primaryTarget == null)
        {
            return AbilityResult.CreateFailure(
                "No target"
            );
        }

        ICombatant target = targetData.primaryTarget;

        if (UsesRangedWeapon(user))
        {
            if (!CanUseRanged(user, target))
            {
                return AbilityResult.CreateFailure(
                    "Target out of ranged weapon range or line of sight"
                );
            }

            SpendCost(user);
            Execute(user, target);

            return AbilityResult.CreateSuccess();
        }

        /*
         * Melee movement is handled by IntentExecutor. AttackAbility
         * only resolves the attack after the actor reaches an adjacent
         * attack position.
         */
        if (!TryGetMeleeApproach(
            user,
            target,
            out _,
            out int movementCost
        ))
        {
            return AbilityResult.CreateFailure(
                "Target cannot be reached with a melee weapon"
            );
        }

        if (movementCost > 0)
        {
            return AbilityResult.CreateFailure(
                "Target out of melee range"
            );
        }

        SpendCost(user);
        Execute(user, target);

        return AbilityResult.CreateSuccess();
    }

    protected override void Execute(
        ICombatant user,
        ICombatant target
    )
    {
        if (user == null || target == null)
            return;

        WeaponItem weapon = GetEquippedWeapon(user);

        string damageDice = weapon != null &&
                            !string.IsNullOrEmpty(weapon.damageDice)
            ? weapon.damageDice
            : user.DamageDice;

        int damageModifier = weapon != null
            ? weapon.damageBonus
            : user.DamageModifier;

        bool isRanged = weapon != null &&
                        weapon.weaponType == WeaponType.Ranged;

        string attackVerb = isRanged
            ? "shoots"
            : "attacks";

        TryPlayActionFlash(user);

        int roll = DiceRoller.RollD20();
        int total = roll + user.AttackBonus;

        bool critical = roll == 20;
        bool hit = critical || total >= target.ArmorClass;

        Debug.Log(
            $"{user} {attackVerb} {target} | " +
            $"roll {roll} + {user.AttackBonus} = {total} " +
            $"vs AC {target.ArmorClass}"
        );

        if (!hit)
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

            return;
        }

        int damage =
            DiceRoller.Roll(damageDice) +
            damageModifier;

        if (critical)
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

    private float GetClosestCombatDistance(
        ICombatant user,
        ICombatant target
    )
    {
        if (user == null || target == null)
            return float.MaxValue;

        List<Vector3Int> userCells =
            user.GetOccupiedCells();

        List<Vector3Int> targetCells =
            target.GetOccupiedCells();

        if (userCells == null ||
            userCells.Count == 0 ||
            targetCells == null ||
            targetCells.Count == 0)
        {
            return Vector3.Distance(
                user.GetWorldPosition(),
                target.GetWorldPosition()
            );
        }

        float closestDistance = float.MaxValue;

        foreach (Vector3Int userCell in userCells)
        {
            Vector3 userWorld = new Vector3(
                userCell.x + 0.5f,
                userCell.y + 0.5f,
                0f
            );

            foreach (Vector3Int targetCell in targetCells)
            {
                Vector3 targetWorld = new Vector3(
                    targetCell.x + 0.5f,
                    targetCell.y + 0.5f,
                    0f
                );

                float distance = Vector3.Distance(
                    userWorld,
                    targetWorld
                );

                if (distance < closestDistance)
                    closestDistance = distance;
            }
        }

        return closestDistance;
    }

    private void TryPlayActionFlash(ICombatant user)
    {
        MonoBehaviour monoBehaviour = user as MonoBehaviour;

        if (monoBehaviour == null)
            return;

        CombatantActionFlash flash =
            monoBehaviour.GetComponent<CombatantActionFlash>();

        flash?.PlayFlash();
    }
}
