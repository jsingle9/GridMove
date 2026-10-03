using UnityEngine;

/// <summary>
/// A menu ability that opens the spell selection panel.
/// Used as slot 5 for Mage and Mystic to access their full spell list.
/// </summary>
public class CastSpellAbility : Ability
{
    public CastSpellAbility()
    {
        AbilityName = "Cast Spell";
        CostType = AbilityCostType.Action;
        targetingMode = TargetingMode.None;  // Opens menu instead
        Range = 0f;
    }

    public override AbilityResult TryUse(ICombatant user, TargetData target)
    {
        // This ability opens the spell menu, doesn't actually cast
        if (user is BoxMover caster)
        {
            SpellSelectionPanel panel = Object.FindFirstObjectByType<SpellSelectionPanel>();
            if (panel != null)
            {
                panel.Open(caster);
                Debug.Log($"[CastSpellAbility] Opened spell selection panel for {caster.Name}");
            }
            else
            {
                Debug.LogError("[CastSpellAbility] SpellSelectionPanel not found in scene!");
                return AbilityResult.CreateFailure("Spell menu not available.");
            }
        }

        return AbilityResult.CreateSuccess();
    }

    protected override void Execute(ICombatant user, ICombatant myTarget)
    {
        // Not used for this ability
    }
}
