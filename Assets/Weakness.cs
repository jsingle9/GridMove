using UnityEngine;

/// <summary>
/// Weakness debuff - reduces damage output for N turns.
/// Stores the original damage modifier and restores it when expired.
/// </summary>
public class Weakness : StatusEffect
{
    private readonly int damagePenalty;
    private int originalDamageModifier;

    public Weakness(int penalty, int duration)
        : base($"Weakness (-{penalty} DMG)", duration)
    {
        damagePenalty = penalty;
    }

    public override void OnApply(ICombatant target)
    {
        MonoBehaviour mb = target as MonoBehaviour;
        if (mb != null && mb.TryGetComponent<BoxMover>(out var bm))
        {
            originalDamageModifier = bm.BaseDamageModifier;
            bm.BaseDamageModifier -= damagePenalty;
            Debug.Log($"{target.Name} is weakened for {RemainingTurns} turns (damage modifier reduced by {damagePenalty})");
        }
    }

    public override void OnExpire(ICombatant target)
    {
        MonoBehaviour mb = target as MonoBehaviour;
        if (mb != null && mb.TryGetComponent<BoxMover>(out var bm))
        {
            bm.BaseDamageModifier = originalDamageModifier;
            Debug.Log($"{target.Name}'s weakness expired (damage modifier restored)");
        }
    }
}
