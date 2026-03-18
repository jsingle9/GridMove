public class FireballAbility : Ability
{
    public int damage = 7;

    public FireballAbility(){
        AbilityName = "Fireball";
        CostType = AbilityCostType.Action;
        range = 5;

        targetingMode = TargetingMode.Area;

        radius = 2; // AOE size
    }

    protected override void Execute(TargetData data){
        foreach(ICombatant unit in data.unitsInArea){
            unit.TakeDamage(damage);
        }
    }
}
