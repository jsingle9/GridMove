using UnityEngine;

/// <summary>
/// Armor Class Buff - increases AC for N turns.
/// Stores the original AC and restores it when expired.
/// </summary>
public class ArmorClassBuff : StatusEffect
{
    private readonly int acBonus;
    private int originalAC;

    public ArmorClassBuff(int bonus, int duration)
        : base($"AC Bonus +{bonus}", duration)
    {
        acBonus = bonus;
    }

    public override void OnApply(ICombatant target)
    {
        MonoBehaviour mb = target as MonoBehaviour;
        if (mb != null && mb.TryGetComponent<BoxMover>(out var bm))
        {
            originalAC = bm.ArmorClass;
            bm.ArmorClass += acBonus;
            Debug.Log($"{target.Name} gains +{acBonus} AC for {RemainingTurns} turns (now {bm.ArmorClass})");
        }
    }

    public override void OnExpire(ICombatant target)
    {
        MonoBehaviour mb = target as MonoBehaviour;
        if (mb != null && mb.TryGetComponent<BoxMover>(out var bm))
        {
            bm.ArmorClass = originalAC;
            Debug.Log($"{target.Name}'s AC bonus expired (back to {originalAC})");
        }
    }
}
