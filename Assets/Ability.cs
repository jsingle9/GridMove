

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

    /*public virtual void TryUse(ICombatant user, ICombatant myTarget)
    {
        UnityEngine.Debug.Log($"Using Ability {AbilityName}");
        if(!CanUse(user)){
            UnityEngine.Debug.Log($"Cannot use {AbilityName}");
            return;
        }
        UnityEngine.Debug.Log($"SUCCESS: {AbilityName} allowed → spending cost");
        SpendCost(user);
        Execute(user, myTarget);
    }*/
    public virtual void TryUse(ICombatant user, TargetData targetData){
        UnityEngine.Debug.Log($"Using Ability {AbilityName}");

        if(!CanUse(user)){
            UnityEngine.Debug.Log($"Cannot use {AbilityName}");
            return;
        }

        if(targetData == null){
            UnityEngine.Debug.Log("Invalid target");
            return;
        }

        // Most abilities still use a primary target
        ICombatant myTarget = targetData.primaryTarget;

        UnityEngine.Debug.Log($"SUCCESS: {AbilityName} allowed → spending cost");

        SpendCost(user);

        Execute(user, myTarget);
    }


    protected abstract void Execute(ICombatant user, ICombatant myTarget);
}
