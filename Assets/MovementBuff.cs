using UnityEngine;

/// <summary>
/// Movement Buff - increases movement speed for N turns.
/// Stores the original speed and restores it when expired.
/// </summary>
public class MovementBuff : StatusEffect
{
    private readonly int speedBonus;
    private int originalSpeed;

    public MovementBuff(int bonus, int duration)
        : base($"Movement +{bonus}", duration)
    {
        speedBonus = bonus;
    }

    public override void OnApply(ICombatant target)
    {
        originalSpeed = target.Speed;
        target.Speed += speedBonus;
        Debug.Log($"{target.Name} gains +{speedBonus} movement for {RemainingTurns} turns (now {target.Speed})");
    }

    public override void OnExpire(ICombatant target)
    {
        target.Speed = originalSpeed;
        Debug.Log($"{target.Name}'s movement bonus expired (back to {originalSpeed})");
    }
}
