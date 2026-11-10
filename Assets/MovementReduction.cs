// MovementReduction.cs
using UnityEngine;

public class MovementReduction : StatusEffect
{
    private readonly int movementReduction;
    private int originalSpeed;

    public MovementReduction(int reduction, int duration)
        : base($"Immobilized", duration)
    {
        movementReduction = reduction;
        Debug.Log($"[MovementReduction] Created with reduction value: {reduction}");
    }

    public override void OnApply(ICombatant target)
    {
        originalSpeed = target.Speed;
        target.Speed = Mathf.Max(0, target.Speed - movementReduction);
        Debug.Log($"[MovementReduction.OnApply] {target.Name} immobilized. Speed: {originalSpeed} → {target.Speed}, Duration: {RemainingTurns}");
    }

    public override void OnTurnStart(ICombatant target)
    {
        Debug.Log($"[MovementReduction.OnTurnStart] {target.Name} turn start, immobilized for {RemainingTurns} more turns");
    }

    public override void OnExpire(ICombatant target)
    {
        target.Speed = originalSpeed;
        Debug.Log($"[MovementReduction.OnExpire] {target.Name}'s immobilization expired. Speed restored to {originalSpeed}");
    }
}
