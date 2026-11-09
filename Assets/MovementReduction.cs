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
    }

    public override void OnApply(ICombatant target)
    {
        MonoBehaviour mb = target as MonoBehaviour;
        if (mb != null && mb.TryGetComponent<BoxMover>(out var bm))
        {
            originalSpeed = bm.Speed;
            bm.Speed = Mathf.Max(0, bm.Speed - movementReduction);
            Debug.Log($"{target.Name} is immobilized for {RemainingTurns} turn(s) (speed: {originalSpeed} → {bm.Speed})");
        }
    }

    public override void OnExpire(ICombatant target)
    {
        MonoBehaviour mb = target as MonoBehaviour;
        if (mb != null && mb.TryGetComponent<BoxMover>(out var bm))
        {
            bm.Speed = originalSpeed;
            Debug.Log($"{target.Name}'s immobilization expired (speed restored to {originalSpeed})");
        }
    }
}
