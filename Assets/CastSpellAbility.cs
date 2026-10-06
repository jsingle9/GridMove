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
        targetingMode = TargetingMode.AllyOrSelf;  // Opens menu instead
        Range = 0f;
    }

    public override AbilityResult TryUse(ICombatant user, TargetData target)
    {
        Debug.Log("[CastSpellAbility] Searching for SpellSelectionPanel...");

        // Find the panel EVEN IF INACTIVE
        SpellSelectionPanel panel = Object.FindFirstObjectByType<SpellSelectionPanel>(FindObjectsInactive.Include);
        Debug.Log($"[CastSpellAbility] FindFirstObjectByType result: {(panel != null ? "FOUND" : "NULL")}");

        // If that fails, try GetComponent on all objects
        if (panel == null)
        {
            SpellSelectionPanel[] allPanels = Object.FindObjectsOfType<SpellSelectionPanel>();
            Debug.Log($"[CastSpellAbility] FindObjectsOfType result: {allPanels.Length} panels found");
            if (allPanels.Length > 0)
                panel = allPanels[0];
        }

        if (panel == null)
        {
            Debug.Log("[CastSpellAbility] SpellSelectionPanel still not found!");
            return AbilityResult.CreateFailure("Spell menu not available.");
        }

        Debug.Log($"[CastSpellAbility] Panel found: {panel.name}");

        if (user is BoxMover caster)
        {
            panel.Open(caster);
        }

        return AbilityResult.CreateSuccess();
    }

    protected override void Execute(ICombatant user, ICombatant myTarget)
    {
        // Not used for this ability
    }
}
