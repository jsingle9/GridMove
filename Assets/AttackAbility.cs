using UnityEngine;

public class AttackAbility : Ability
{
    ICombatant target;

    public AttackAbility(){
        AbilityName = "Attack";
        CostType = AbilityCostType.Action;
    }

/*    protected override void Execute(ICombatant user, ICombatant target){
        if(target == null) return;

        float distance = Vector3.Distance(
            user.GetWorldPosition(),
            target.GetWorldPosition()
        );

        // =========================
        // OUT OF RANGE → MOVE ONLY
        // =========================
        // =========================
        // OUT OF RANGE → TRY MOVE INTO RANGE
        // =========================
        if(distance > Range)
        {
            // no movement at all
            if(!user.HasMove){
                Debug.Log("Out of range but no move left → cannot reach target");
                return;
            }

            // preview path cost
            AttackIntent previewIntent = new AttackIntent(target);
            int cost = user.PreviewMoveCost(previewIntent);

            if(cost <= 0){
                Debug.Log("No valid path to target");
                return;
            }

            if(cost > user.RemainingMovement){
                Debug.Log($"Target too far. Need {cost} movement, have {user.RemainingMovement}");
                return;
            }

            Debug.Log("Target out of range → moving into range");
            user.SetIntent(previewIntent);
            return;
        }

        SpendCost(user);
        // =========================
        // IN RANGE → ATTACK
        // =========================
        Debug.Log($"{user} attacks {target}");

        int roll = DiceRoller.RollD20();
        int total = roll + user.AttackBonus;

        Debug.Log($"Attack roll: {roll} + {user.AttackBonus} = {total} vs AC {target.ArmorClass}");

        bool crit = roll == 20;

        if(total >= target.ArmorClass || crit)
        {
            int damage = DiceRoller.Roll(user.DamageDice) + user.DamageModifier;

            if(crit){
                Debug.Log("CRITICAL HIT!");
                damage *= 2;
            }

            Debug.Log($"Hit for {damage} damage");
            target.TakeDamage(damage);
        }
        else
        {
            Debug.Log("Miss");
        }
    }*/

    protected override void Execute(ICombatant user, ICombatant target){
        if(target == null) return;

        float distance = Vector3.Distance(
            user.GetWorldPosition(),
            target.GetWorldPosition()
        );

        // Not in melee range → do nothing
        /*if(distance > Range + 0.1f){
            Debug.Log("Target not in melee range");
            return;
        }*/
        if(distance > Range){
            if(!user.HasMove){
                Debug.Log("Out of range and no movement");
                return;
            }

            AttackIntent intent = new AttackIntent(target);
            user.SetIntent(intent);
            // DO NOT attack yet
            return;
        }

        SpendCost(user);

        Debug.Log($"{user} attacks {target}");

        int roll = DiceRoller.RollD20();
        int total = roll + user.AttackBonus;

        Debug.Log($"Attack roll: {roll} + {user.AttackBonus} = {total} vs AC {target.ArmorClass}");

        bool crit = roll == 20;

        if(total >= target.ArmorClass || crit){
            int damage = DiceRoller.Roll(user.DamageDice) + user.DamageModifier;

            if(crit){
                Debug.Log("CRITICAL HIT!");
                damage *= 2;
            }

            Debug.Log($"Hit for {damage} damage");
            target.TakeDamage(damage);
            if (user is BoxMover){
                target.AddStatus(new PoisonStatus(3, 2));
            }
        }
        else{
            Debug.Log("Miss");
        }
    }

    public override void TryUse(ICombatant user, ICombatant myTarget){
        if(!user.HasAction){
            Debug.Log("No action available");
            return;
        }

        // spend action
        //user.HasAction = false;

        Execute(user, myTarget);
    }


}
