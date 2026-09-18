using UnityEngine;

public static class SpellEffectExecutor
{
    public static AbilityResult ApplyDamage(
        ICombatant user,
        TargetData target,
        SpellDefinition definition
    )
    {
        if (target.primaryTarget == null)
        {
            return AbilityResult.CreateFailure(
                "No target to damage."
            );
        }

        int amount = RollDice(definition.damageDice);
        target.primaryTarget.TakeDamage(amount);

        return AbilityResult.CreateSuccess();
    }

    public static AbilityResult ApplyHeal(
        ICombatant user,
        TargetData target,
        SpellDefinition definition
    )
    {
        if (target.primaryTarget == null)
        {
            return AbilityResult.CreateFailure(
                "No target to heal."
            );
        }

        int amount = RollDice(definition.damageDice);
        target.primaryTarget.Heal(amount);

        return AbilityResult.CreateSuccess();
    }

    public static AbilityResult ApplyAreaDamage(
        ICombatant user,
        TargetData target,
        SpellDefinition definition
    )
    {
        if (target.unitsInArea == null ||
            target.unitsInArea.Count == 0)
        {
            return AbilityResult.CreateFailure(
                "No targets in the area."
            );
        }

        int amount = RollDice(definition.damageDice);

        foreach (ICombatant unit in target.unitsInArea)
        {
            if (unit != null)
                unit.TakeDamage(amount);
        }

        return AbilityResult.CreateSuccess();
    }

    public static AbilityResult ApplyBuff(
        ICombatant user,
        TargetData target,
        SpellDefinition definition
    )
    {
        return AbilityResult.CreateFailure(
            "Buff effects are not implemented yet."
        );
    }

    public static AbilityResult ApplyDebuff(
        ICombatant user,
        TargetData target,
        SpellDefinition definition
    )
    {
        return AbilityResult.CreateFailure(
            "Debuff effects are not implemented yet."
        );
    }

    public static AbilityResult ApplyStatus(
        ICombatant user,
        TargetData target,
        SpellDefinition definition
    )
    {
        return AbilityResult.CreateFailure(
            "Status effects are not implemented yet."
        );
    }

    public static AbilityResult ApplyTeleport(
        ICombatant user,
        TargetData target,
        SpellDefinition definition
    )
    {
        return AbilityResult.CreateFailure(
            "Teleport effects are not implemented yet."
        );
    }

    public static AbilityResult ApplyUtility(
        ICombatant user,
        TargetData target,
        SpellDefinition definition
    )
    {
        return AbilityResult.CreateFailure(
            "Utility effects are not implemented yet."
        );
    }

    private static int RollDice(string diceNotation)
    {
        if (string.IsNullOrWhiteSpace(diceNotation))
        {
            Debug.LogWarning(
                "SpellEffectExecutor: Empty damage dice. Using 0 damage."
            );
            return 0;
        }

        string[] parts = diceNotation.ToLower().Split('d');

        if (parts.Length != 2 ||
            !int.TryParse(parts[0], out int diceCount) ||
            !int.TryParse(parts[1], out int diceSides) ||
            diceCount <= 0 ||
            diceSides <= 0)
        {
            Debug.LogWarning(
                $"SpellEffectExecutor: Invalid dice notation '{diceNotation}'."
            );
            return 0;
        }

        int total = 0;

        for (int i = 0; i < diceCount; i++)
            total += Random.Range(1, diceSides + 1);

        return total;
    }
}
