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
        Debug.Log($"[ApplyBuff] Called for spell: {definition.displayName}");

        if (target.primaryTarget == null)
        {
            Debug.Log("[ApplyBuff] No primary target!");
            return AbilityResult.CreateFailure("No target to buff.");
        }

        Debug.Log($"[ApplyBuff] Target: {target.primaryTarget.Name}");

        string buffType = string.IsNullOrEmpty(definition.damageType)
            ? "unknown"
            : definition.damageType.ToLowerInvariant();

        Debug.Log($"[ApplyBuff] Buff type from spell: {buffType}");

        int duration = definition.Duration;

        StatusEffect buff = buffType switch
        {
            "movement_bonus" => new MovementBuff(definition.MovementBonus, duration),
            "armor_bonus" => new ArmorClassBuff(definition.ArmorBonus, duration),
            _ => null
        };

        if (buff == null)
        {
            Debug.Log($"[ApplyBuff] Unknown buff type: {buffType}");
            return AbilityResult.CreateFailure($"Unknown buff type: {buffType}");
        }

        Debug.Log($"[ApplyBuff] Applying {buff.Name} to {target.primaryTarget.Name}");
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
        Debug.Log($"[ApplyDebuff] Called for spell: {definition.displayName}");

        if (target.primaryTarget == null)
        {
            Debug.Log("[ApplyDebuff] No primary target!");
            return AbilityResult.CreateFailure("No target to debuff.");
        }

        Debug.Log($"[ApplyDebuff] Target: {target.primaryTarget.Name}, Requires Save: {definition.requiresSave}");

        // Check if this debuff requires a save
        if (definition.requiresSave)
        {
            int dc = SavingThrowUtility.CalculateCasterDC(user);
            Debug.Log($"[ApplyDebuff] Calculated DC from caster {user.Name}: {dc}");

            if (SavingThrowUtility.PassesSave(target.primaryTarget, dc, definition.saveAbility, out int roll, out int mod, out int total))
            {
                // Save succeeded - no effect
                Debug.Log($"[ApplyDebuff] {target.primaryTarget.Name} saved against {definition.displayName} (rolled {roll}+{mod}={total}, needed {dc})");
                return AbilityResult.CreateSuccess();  // Success but no debuff applied
            }

            Debug.Log($"[ApplyDebuff] {target.primaryTarget.Name} failed save against {definition.displayName} (rolled {roll}+{mod}={total}, needed {dc})");
        }

        // Apply the debuff
        string debuffType = string.IsNullOrEmpty(definition.damageType)
            ? "weakness"
            : definition.damageType.ToLowerInvariant();

        Debug.Log($"[ApplyDebuff] Debuff type from spell: {debuffType}");

        int duration = definition.Duration;

        StatusEffect debuff = debuffType switch
        {
            "weakness" => new Weakness(definition.DamageAmount, duration),
            "movement_reduction" => new MovementReduction(definition.MovementReduction, duration),
            _ => null
        };

        if (debuff == null)
        {
            Debug.Log($"[ApplyDebuff] Unknown debuff type: {debuffType}");
            return AbilityResult.CreateFailure($"Unknown debuff type: {debuffType}");
        }

        Debug.Log($"[ApplyDebuff] Applying {debuff.Name} to {target.primaryTarget.Name}");
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
