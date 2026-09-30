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

        int slotLevel = 0;
        int baseSlotLevel = definition.spellLevel;
        int baseDamage = DiceRoller.Roll(definition.damageDice);
        int upcastDamage = 0;

        if (slotLevel > baseSlotLevel && !string.IsNullOrEmpty(definition.upcastDamageBonus))
        {
            int extraLevels = slotLevel - baseSlotLevel;
            for (int i = 0; i < extraLevels; i++)
                upcastDamage += DiceRoller.Roll(definition.upcastDamageBonus);
        }

        int totalDamage = baseDamage + upcastDamage;
        target.primaryTarget.TakeDamage(totalDamage);
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

        int amount = DiceRoller.Roll(definition.damageDice);
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

        int amount = DiceRoller.Roll(definition.damageDice);

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

    public static AbilityResult ApplyAttackWithSave(
        ICombatant user,
        TargetData target,
        SpellDefinition definition
    )
    {
        if (target.primaryTarget == null)
        {
            return AbilityResult.CreateFailure(
                "No target to attack."
            );
        }

        ICombatant targetCombatant = target.primaryTarget;

        // Attack roll
        int roll = DiceRoller.RollD20();
        int attackBonus = user.AttackBonus;
        int total = roll + attackBonus;

        Debug.Log(
            $"[AttackWithSave] {user.Name} attacks {targetCombatant.Name}: " +
            $"roll {roll} + {attackBonus} = {total} vs AC {targetCombatant.ArmorClass}"
        );

        if (total < targetCombatant.ArmorClass)
        {
            Debug.Log($"[AttackWithSave] Miss");
            return AbilityResult.CreateSuccess(); // Miss but spell succeeds
        }

        // Hit - now target makes save
        int saveDamage = DiceRoller.Roll(definition.damageDice);
        int damage = saveDamage;

        bool saveSuccess = SavingThrowUtility.PassesSave(
            targetCombatant,
            definition.saveDC,
            definition.saveAbility,
            out int saveRoll,
            out int saveMod,
            out int saveTotal
        );

        Debug.Log(
            $"[AttackWithSave] Hit! {targetCombatant.Name} saves: " +
            $"roll {saveRoll} + {saveMod} = {saveTotal} vs DC {definition.saveDC} " +
            $"=> {(saveSuccess ? "SUCCESS" : "FAIL")}"
        );

        // Apply damage (full if fail, half if pass)
        if (saveSuccess)
            damage = Mathf.Max(1, damage / 2);

        targetCombatant.TakeDamage(damage);

        // Apply failure effect if save failed
        if (!saveSuccess && !string.IsNullOrEmpty(definition.failureEffect))
        {
            Debug.Log($"[AttackWithSave] {definition.failureEffect} applied to {targetCombatant.Name}");
            // TODO: implement failureEffect (e.g., consume Reaction)
        }

        return AbilityResult.CreateSuccess();
    }    

}
