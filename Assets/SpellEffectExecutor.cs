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

      // Show damage telegraph
      DamageTelegraphSystem telegraphSystem = Object.FindFirstObjectByType<DamageTelegraphSystem>();
      if (telegraphSystem != null)
      {
          telegraphSystem.ShowDamageTelegraph(target.primaryTarget, totalDamage, definition.telegraphStyle, 1f);
      }

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
        Debug.Log($"[ApplyAreaDamage] Called with {target.unitsInArea?.Count ?? 0} units");

        if (target.unitsInArea == null ||
            target.unitsInArea.Count == 0)
        {
            return AbilityResult.CreateFailure(
                "No targets in the area."
            );
        }

        int amount = DiceRoller.Roll(definition.damageDice);
        Debug.Log($"[ApplyAreaDamage] Damage roll: {amount}");

        for (int i = 0; i < target.unitsInArea.Count; i++)
        {
            ICombatant unit = target.unitsInArea[i];
            Debug.Log($"[ApplyAreaDamage] Iteration {i}: unit={unit?.ToString() ?? "null"}");

            if (unit != null)
            {
                Debug.Log($"[ApplyAreaDamage] Damaging {unit.ToString()} for {amount}");
                unit.TakeDamage(amount);
            }
        }

        Debug.Log($"[ApplyAreaDamage] Finished");
        return AbilityResult.CreateSuccess();
    }

    public static AbilityResult ApplyBuff(
        ICombatant user,
        TargetData target,
        SpellDefinition definition
    )
    {
        if (target.primaryTarget == null)
            return AbilityResult.CreateFailure("No target to buff.");

        // Parse buff type from the spell's damageType field
        // Expected format: "ac_bonus" or "movement_bonus"
        string buffType = string.IsNullOrEmpty(definition.damageType)
            ? "ac_bonus"
            : definition.damageType.ToLowerInvariant();

        int buffAmount = 2;  // Default buff value, or parse from damageDice if needed
        int duration = 3;    // Default 3 turns

        StatusEffect buff = null;

        switch (buffType)
        {
            case "ac_bonus":
                buff = new ArmorClassBuff(buffAmount, duration);
                break;

            case "movement_bonus":
            case "speed_bonus":
                buff = new MovementBuff(buffAmount, duration);
                break;

            default:
                return AbilityResult.CreateFailure($"Unknown buff type: {buffType}");
        }

        if (buff == null)
            return AbilityResult.CreateFailure("Buff creation failed.");

        target.primaryTarget.AddStatus(buff);
        Debug.Log($"{user.Name} cast {definition.displayName} on {target.primaryTarget.Name}");

        return AbilityResult.CreateSuccess();
    }

    public static AbilityResult ApplyDebuff(
        ICombatant user,
        TargetData target,
        SpellDefinition definition
    )
    {
        if (target.primaryTarget == null)
            return AbilityResult.CreateFailure("No target to debuff.");

        // Parse debuff type from the damageType field
        // Expected format: "weakness" or "vulnerability"
        string debuffType = string.IsNullOrEmpty(definition.damageType)
            ? "weakness"
            : definition.damageType.ToLowerInvariant();

        int debuffAmount = 2;  // Default debuff value
        int duration = 2;      // Default 2 turns

        StatusEffect debuff = null;

        switch (debuffType)
        {
            case "weakness":
                debuff = new Weakness(debuffAmount, duration);
                break;

            default:
                return AbilityResult.CreateFailure($"Unknown debuff type: {debuffType}");
        }

        if (debuff == null)
            return AbilityResult.CreateFailure("Debuff creation failed.");

        target.primaryTarget.AddStatus(debuff);
        Debug.Log($"{user.Name} cast {definition.displayName} on {target.primaryTarget.Name}");

        return AbilityResult.CreateSuccess();
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
        return AbilityResult.CreateFailure(
            "Attack with save effects are not implemented yet."
        );
    }

}
