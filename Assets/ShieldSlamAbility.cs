using UnityEngine;

public class ShieldSlamAbility : Ability
{
    private const int SaveDC = 13;
    private const int DamageNumDice = 2;
    private const int DamageDiceSize = 8;
    private const string DamageDice = "2d8";

    public ShieldSlamAbility()
    {
        AbilityName = "Shield Slam";
        CostType = AbilityCostType.Action;
        Range = 1.5f;   // melee feel, consistent with AttackAbility
        targetingMode = TargetingMode.Enemy;
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

        int strMod = SaveStatUtility.GetEstimatedStrMod(target);
        int roll = DiceRoller.RollD20();
        int total = roll + strMod;

        bool saveSuccess = total >= SaveDC;
        int pushTiles = saveSuccess ? 1 : 2;

        // Roll 2d8 damage
        int baseDamage = DiceRoller.Roll(DamageDice);
        int damage = saveSuccess ? baseDamage / 2 : baseDamage;

        bool pushed = KnockbackUtility.TryPushAway(user, target, pushTiles);

        // Apply damage
        target.TakeDamage(damage);

        Debug.Log(
            $"{AbilityName}: {target.Name} STR save {roll} + {strMod} = {total} vs DC {SaveDC} " +
            $"=> {(saveSuccess ? "SUCCESS (half damage)" : "FAIL")} | push {pushTiles} | damage {damage} (base 2d8={baseDamage}) | moved={pushed}"
        );
    }
}
