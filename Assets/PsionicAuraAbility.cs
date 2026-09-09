using UnityEngine;

/// <summary>
/// Mystic level 3 class feature – Psionic Aura.
/// Bonus Action. Self (radiates around user). Once per combat.
/// Heals both party members within 3 grid squares for 2d6 + WIS modifier.
/// </summary>
public class PsionicAuraAbility : Ability
{
    private bool usedThisCombat = false;
    private const float AuraRadius = 3f;

    public PsionicAuraAbility()
    {
        AbilityName = "Psionic Aura";
        CostType = AbilityCostType.BonusAction;
        Range = 0f;
        targetingMode = TargetingMode.Self;
    }

    public void ResetForNewCombat()
    {
        usedThisCombat = false;
    }

    public override bool CanUse(ICombatant user)
    {
        return base.CanUse(user) && !usedThisCombat;
    }

    public override AbilityResult TryUse(ICombatant user, TargetData targetData)
    {
        if (!base.CanUse(user))
            return AbilityResult.CreateFailure("No bonus action available");

        if (usedThisCombat)
            return AbilityResult.CreateFailure("Psionic Aura already used this combat");

        SpendCost(user);
        Execute(user, user);
        usedThisCombat = true;
        return AbilityResult.CreateSuccess();
    }

    protected override void Execute(ICombatant user, ICombatant target)
    {
        if (user == null) return;

        int wisMod = GetWisMod(user);
        int baseHealing = DiceRoller.Roll("2d6") + wisMod;

        // Heal the caster
        user.Heal(baseHealing);
        Debug.Log($"[PsionicAura] {user.Name} activates Psionic Aura and heals for {baseHealing} HP");

        CombatUIManager.Instance?.AddLog(
            $"{user.Name} activates Psionic Aura and recovers {baseHealing} HP.");

        // Find and heal nearby allies in the combatant list
        CombatManager combatManager = CombatManager.Instance;
        if (combatManager != null)
        {
            // Get all combatants and filter for allies in range
            foreach (ICombatant ally in combatManager.GetCombatants())
            {
                if (ally == null || ally.IsDead())
                    continue;

                if (ally == user)
                    continue; // Already healed the user

                if (!ally.IsPlayerControlled())
                    continue; // Skip enemies

                float distance = Vector3.Distance(user.GetWorldPosition(), ally.GetWorldPosition());
                if (distance <= AuraRadius)
                {
                    ally.Heal(baseHealing);
                    Debug.Log($"[PsionicAura] {ally.Name} heals for {baseHealing} HP from {user.Name}'s aura");

                    CombatUIManager.Instance?.AddLog(
                        $"{ally.Name} recovers {baseHealing} HP from {user.Name}'s Psionic Aura.");
                }
            }
        }
    }

    private int GetWisMod(ICombatant user)
    {
        BoxMover bm = user as BoxMover;
        return bm != null ? bm.Sheet.Scores.ModWIS : 0;
    }
}
