public class ActionSurgeAbility : Ability
{
    private readonly IActionSurgeConfig config;

    public ActionSurgeAbility(IActionSurgeConfig config)
    {
        this.config = config;
        AbilityName = "Action Surge";
        CostType = AbilityCostType.Free; // treat as feature activation
        Range = 0f;
        targetingMode = TargetingMode.Self;
    }

    public override AbilityResult TryUse(ICombatant user, TargetData targetData)
    {
        if (user is not BoxMover player) return AbilityResult.CreateFailure("Only player can use this");
        if (player.ActionSurgeUsedThisCombat) return AbilityResult.CreateFailure("Action Surge already used");

        player.HasAction = true;
        player.ActionSurgeUsedThisCombat = true;

        return AbilityResult.CreateSuccess();
    }

    protected override void Execute(ICombatant user, ICombatant myTarget) { }
}
