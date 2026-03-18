public class HealAbility : Ability
{
    public int healAmount = 10;

    public HealAbility(){
        AbilityName = "Heal";
        CostType = AbilityCostType.BonusAction;
        range = 3;
        targetingMode = TargetingMode.Self;
    }

    protected override void Execute(TargetData targetData){
        UnityEngine.Debug.Log("HealAbility Execute fired");

        if(targetData.primaryTarget == null){
            UnityEngine.Debug.Log("Heal failed: no target.");
            return;
        }

        targetData.primaryTarget.Heal(healAmount);

        UnityEngine.Debug.Log($"{targetData.user} heals {targetData.primaryTarget} for {healAmount}");
    }
}
