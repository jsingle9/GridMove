public class HealAbility : Ability
{
    public int healAmount = 10;

    public HealAbility(){
        AbilityName = "Heal";
        CostType = AbilityCostType.BonusAction;
        range = 3;
        targetingMode = TargetingMode.Self;
    }

    protected override void Execute(ICombatant user, ICombatant myTarget){
        UnityEngine.Debug.Log("HealAbility Execute fired");

        if(myTarget == null){
            UnityEngine.Debug.Log("Heal failed: no target.");
            return;
        }

        myTarget.Heal(healAmount);

        UnityEngine.Debug.Log($"{user} heals {myTarget} for {healAmount}");
    }
}
