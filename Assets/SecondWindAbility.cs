using UnityEngine;

public class SecondWindAbility : Ability
{
    private readonly ISecondWindConfig config;

    public SecondWindAbility(ISecondWindConfig config)
    {
        this.config = config;
        AbilityName = "Second Wind";
        CostType = AbilityCostType.BonusAction;
        Range = 0f;
        targetingMode = TargetingMode.Self;
    }

    public override AbilityResult TryUse(ICombatant user, TargetData targetData)
    {
        if (user is not BoxMover player) return AbilityResult.CreateFailure("Only player can use this");
        if (!CanUse(user)) return AbilityResult.CreateFailure("No bonus action available");
        if (player.SecondWindUsedThisCombat) return AbilityResult.CreateFailure("Second Wind already used");

        int heal = Random.Range(1, config.BaseDieSides + 1) + player.Sheet.Level; // 1d10 + level
        player.Heal(heal);
        player.SecondWindUsedThisCombat = true;

        SpendCost(user);
        return AbilityResult.CreateSuccess();
    }

    protected override void Execute(ICombatant user, ICombatant myTarget) { }
}
