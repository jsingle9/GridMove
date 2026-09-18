public class SpellAbility : Ability
{
    private readonly SpellDefinition definition;

    public SpellAbility(SpellDefinition definition)
    {
        this.definition = definition;

        if (definition == null)
        {
            AbilityName = "Missing Spell";
            CostType = AbilityCostType.Action;
            targetingMode = TargetingMode.Enemy;
            Range = 0f;
            range = 0;
            radius = 0;
            return;
        }

        AbilityName = definition.displayName;
        CostType = AbilityCostType.Action;
        targetingMode = definition.targetingMode;
        Range = definition.range;
        range = (int)definition.range;
        radius = definition.radius;
    }

    public override AbilityResult TryUse(
        ICombatant user,
        TargetData target
    )
    {
        if (definition == null)
            return AbilityResult.CreateFailure("Spell definition is missing.");

        if (user == null)
            return AbilityResult.CreateFailure("Spell caster is missing.");

        if (target == null)
            return AbilityResult.CreateFailure("Spell target is missing.");

        if (!CanUse(user))
        {
            return AbilityResult.CreateFailure(
                $"{AbilityName} cannot be used right now."
            );
        }

        AbilityResult result;

        switch (definition.effectType)
        {
            case SpellEffectType.Damage:
                result = SpellEffectExecutor.ApplyDamage(
                    user,
                    target,
                    definition
                );
                break;

            case SpellEffectType.Heal:
                result = SpellEffectExecutor.ApplyHeal(
                    user,
                    target,
                    definition
                );
                break;

            case SpellEffectType.Buff:
                result = SpellEffectExecutor.ApplyBuff(
                    user,
                    target,
                    definition
                );
                break;

            case SpellEffectType.Debuff:
                result = SpellEffectExecutor.ApplyDebuff(
                    user,
                    target,
                    definition
                );
                break;

            case SpellEffectType.ApplyStatus:
                result = SpellEffectExecutor.ApplyStatus(
                    user,
                    target,
                    definition
                );
                break;

            case SpellEffectType.AreaDamage:
                result = SpellEffectExecutor.ApplyAreaDamage(
                    user,
                    target,
                    definition
                );
                break;

            case SpellEffectType.Teleport:
                result = SpellEffectExecutor.ApplyTeleport(
                    user,
                    target,
                    definition
                );
                break;

            case SpellEffectType.Utility:
                result = SpellEffectExecutor.ApplyUtility(
                    user,
                    target,
                    definition
                );
                break;

            default:
                result = AbilityResult.CreateFailure(
                    $"Unsupported spell effect: {definition.effectType}"
                );
                break;
        }

        if (result.Success)
            SpendCost(user);

        return result;
    }

    protected override void Execute(
        ICombatant user,
        ICombatant myTarget
    )
    {
        // Required by Ability.
        // Spell execution uses TryUse because spells use TargetData.
    }
}
