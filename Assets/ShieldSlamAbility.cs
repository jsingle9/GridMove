using UnityEngine;

public class ShieldSlamAbility : Ability
{
    private const int SaveDC = 13;

    public ShieldSlamAbility()
    {
        AbilityName = "Shield Slam";
        CostType = AbilityCostType.Action;
        Range = 1.5f;   // melee feel, consistent with AttackAbility
    }

    public override AbilityResult TryUse(ICombatant user, TargetData targetData)
    {
        if (!CanUse(user))
            return AbilityResult.CreateFailure("No action available");

        if (targetData?.primaryTarget == null)
            return AbilityResult.CreateFailure("No target");

        ICombatant target = targetData.primaryTarget;

        float distance = Vector3.Distance(user.GetWorldPosition(), target.GetWorldPosition());
        if (distance > Range)
            return AbilityResult.CreateFailure("Target out of range");

        SpendCost(user);
        Execute(user, target);
        return AbilityResult.CreateSuccess();
    }

    protected override void Execute(ICombatant user, ICombatant target)
    {
        if (user == null || target == null) return;

        int strMod = SaveStatUtility.GetEstimatedStrMod(target); // temp compatible
        int roll = DiceRoller.RollD20();
        int total = roll + strMod;

        bool saveSuccess = total >= SaveDC;
        int pushTiles = saveSuccess ? 1 : 2;

        bool pushed = KnockbackUtility.TryPushAway(user, target, pushTiles);

        Debug.Log(
            $"{AbilityName}: {target.Name} STR save {roll} + {strMod} = {total} vs DC {SaveDC} " +
            $"=> {(saveSuccess ? "SUCCESS" : "FAIL")} | push {pushTiles} | moved={pushed}"
        );
    }
}
