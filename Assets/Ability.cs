public enum AbilityCostType
{
    Action,
    BonusAction,
    Free
}

public abstract class Ability
{
    public string AbilityName;
    public AbilityCostType CostType;
    public float Range = 1.5f;
    public TargetingMode targetingMode;
    public int range = 1;
    public int radius = 0;

    public virtual bool CanUse(ICombatant user)
    {
        switch(CostType)
        {
            case AbilityCostType.Action:
                return user.HasAction;

            case AbilityCostType.BonusAction:
                return user.HasBonusAction;

            case AbilityCostType.Free:
                return true;
        }

        return false;
    }

    public virtual void SpendCost(ICombatant user)
    {
        switch(CostType)
        {
            case AbilityCostType.Action:
                user.HasAction = false;
                break;

            case AbilityCostType.BonusAction:
                user.HasBonusAction = false;
                break;
        }
    }

    // CONTRACT: TargetData in → AbilityResult out
    // Ability returns success or failure reason
    // NO MOVEMENT LOGIC IN ABILITY
    public abstract AbilityResult TryUse(ICombatant user, TargetData targetData);

    protected abstract void Execute(ICombatant user, ICombatant myTarget);
}
