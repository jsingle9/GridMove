using UnityEngine;
using System.Collections.Generic;

public class PoisonStatus : StatusEffect
{
    int damagePerTurn;

    public PoisonStatus(int duration, int damage)
        : base("Poisoned", duration)
    {
        damagePerTurn = damage;
    }

    public override void OnApply(ICombatant target)
    {
        Debug.Log($"{target} is poisoned for {RemainingTurns} turns!");
    }

    public override void OnTurnStart(ICombatant target)
    {
        Debug.Log($"{target} takes {damagePerTurn} poison damage.");
        target.TakeDamage(damagePerTurn);
    }

    public override void OnExpire(ICombatant target)
    {
        Debug.Log($"{target} is no longer poisoned.");
    }
}
