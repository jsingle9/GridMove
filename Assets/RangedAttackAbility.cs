using UnityEngine;

public class RangedAttackAbility : Ability
{
    public RangedAttackAbility()
    {
        AbilityName = "Bow Shot";
        CostType = AbilityCostType.Action;
        Range = 6f;
    }

    protected override void Execute(TargetData targetData)
    {
      if (targetData == null)
      {
          Debug.LogError("TargetData is null");
          return;
      }

      if (targetData.user == null)
      {
          Debug.LogError("TargetData.user is null");
          return;
      }

      if (targetData.primaryTarget == null)
      {
          Debug.LogError("TargetData.primaryTarget is null");
          return;
      }
        if(targetData.primaryTarget == null) return;

        float distance = UnityEngine.Vector3.Distance(
            targetData.user.GetWorldPosition(),
            targetData.primaryTarget.GetWorldPosition()
        );

        // Move closer if somehow out of range
        if(distance > Range)
        {
            if(!targetData.user.HasMove){
                Debug.Log("Too far and no move left");
                return;
            }

            Debug.Log("Ranged unit moving into range");
            targetData.user.SetIntent(new AttackIntent(targetData.primaryTarget));
            return;
        }

        int roll = DiceRoller.RollD20();
        int total = roll + targetData.user.AttackBonus;

        bool crit = roll == 20;

        if(total >= targetData.primaryTarget.ArmorClass || crit)
        {
            int damage = DiceRoller.Roll(targetData.user.DamageDice) +
              targetData.user.DamageModifier;
            if(crit) damage *= 2;

            targetData.primaryTarget.TakeDamage(damage);
            UnityEngine.Debug.Log($"{targetData.user} shoots {targetData.primaryTarget} for {damage}");
        }
        else
        {
            UnityEngine.Debug.Log($"{targetData.user} missed ranged attack");
        }
    }
}
