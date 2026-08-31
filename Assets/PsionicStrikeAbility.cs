using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Mystic level 1 – Psionic melee attack.
/// Costs an Action. Deals 1d6 + WIS modifier psychic damage.
/// Uses INT modifier for the attack roll (psionic accuracy).
/// </summary>
public class PsionicStrikeAbility : Ability
{
    public PsionicStrikeAbility()
    {
        AbilityName = "Psionic Strike";
        CostType = AbilityCostType.Action;
        Range = 1.5f;
        targetingMode = TargetingMode.Enemy;
    }

    public override AbilityResult TryUse(ICombatant user, TargetData targetData)
    {
        if (!CanUse(user))
            return AbilityResult.CreateFailure("No action available");

        if (targetData?.primaryTarget == null)
            return AbilityResult.CreateFailure("No target");

        ICombatant target = targetData.primaryTarget;

        float distance = GetClosestCombatDistance(user, target);
        if (distance > Range)
            return AbilityResult.CreateFailure("Target out of melee range");

        SpendCost(user);
        Execute(user, target);
        return AbilityResult.CreateSuccess();
    }

    protected override void Execute(ICombatant user, ICombatant target)
    {
        if (user == null || target == null) return;

        TryPlayActionFlash(user);

        // Attack roll uses INT modifier (psionic accuracy)
        int intMod = GetIntMod(user);
        int roll = DiceRoller.RollD20();
        int total = roll + intMod;

        bool crit = roll == 20;
        bool hit = (total >= target.ArmorClass) || crit;

        Debug.Log($"[PsionicStrike] {user.Name} rolls {roll} + {intMod} = {total} vs AC {target.ArmorClass}");

        if (hit)
        {
            int wisMod = GetWisMod(user);
            int damage = DiceRoller.Roll("1d6") + wisMod;
            if (crit)
                damage *= 2;

            Debug.Log($"[PsionicStrike] Hit! {damage} psychic damage");
            target.TakeDamage(damage);

            CombatUIManager.Instance?.LogAttack(
                user.Name, target.Name, true, roll, total, target.ArmorClass, damage);
        }
        else
        {
            Debug.Log("[PsionicStrike] Miss");
            CombatUIManager.Instance?.LogAttack(
                user.Name, target.Name, false, roll, total, target.ArmorClass);
        }
    }

    private int GetIntMod(ICombatant user)
    {
        BoxMover bm = user as BoxMover;
        return bm != null ? bm.Sheet.Scores.ModINT : 0;
    }

    private int GetWisMod(ICombatant user)
    {
        BoxMover bm = user as BoxMover;
        return bm != null ? bm.Sheet.Scores.ModWIS : 0;
    }

    private float GetClosestCombatDistance(ICombatant user, ICombatant target)
    {
        List<Vector3Int> userCells = user.GetOccupiedCells();
        List<Vector3Int> targetCells = target.GetOccupiedCells();

        if (userCells == null || userCells.Count == 0 ||
            targetCells == null || targetCells.Count == 0)
            return Vector3.Distance(user.GetWorldPosition(), target.GetWorldPosition());

        float closest = float.MaxValue;
        foreach (Vector3Int uc in userCells)
        {
            Vector3 uWorld = new Vector3(uc.x + 0.5f, uc.y + 0.5f, 0f);
            foreach (Vector3Int tc in targetCells)
            {
                Vector3 tWorld = new Vector3(tc.x + 0.5f, tc.y + 0.5f, 0f);
                float d = Vector3.Distance(uWorld, tWorld);
                if (d < closest) closest = d;
            }
        }
        return closest;
    }

    private void TryPlayActionFlash(ICombatant user)
    {
        MonoBehaviour mb = user as MonoBehaviour;
        CombatantActionFlash flash = mb?.GetComponent<CombatantActionFlash>();
        flash?.PlayFlash();
    }
}
