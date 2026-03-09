public class HealAbility : Ability
{
    public int healAmount = 10;

    public HealAbility()
    {
        AbilityName = "Heal";
        CostType = AbilityCostType.BonusAction;
        range = 3;
        targetingMode = TargetingMode.Ally;
    }

    protected override void Execute(ICombatant user, ICombatant myTarget)
    {
        if(myTarget == null)
        {
            UnityEngine.Debug.Log("Heal failed: no target.");
            return;
        }

        myTarget.Heal(healAmount);

        UnityEngine.Debug.Log($"{user} heals {myTarget} for {healAmount}");
    }
}
