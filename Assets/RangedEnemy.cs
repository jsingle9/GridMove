using UnityEngine;
using System.Collections.Generic;

public class RangedEnemy : Enemy
{
    protected override void Awake()
    {
        base.Awake();

        // Override to ranged-specific stats
        maxHP = 8;
        currentHP = maxHP;
        equippedWeapon = new Weapon("Bow", 1, "1d6");
        armorClass = 11;
        attackBonus = 3;
        damageDice = "1d6";
        damageModifier = 1;
        speed = 5;

        // Ranged enemy only has ranged attack
        abilities.Clear();
        abilities.Add(new RangedAttackAbility());

        Debug.Log("RangedEnemy initialized");
    }

    protected override System.Collections.IEnumerator EnemyTurnRoutine()
    {
        if(IsDead()) yield break;

        yield return new WaitForSeconds(0.15f);

        BoxMover player = FindFirstObjectByType<BoxMover>();
        if(player == null)
        {
            CombatManager.Instance.EndTurn();
            yield break;
        }

        // Ranged enemy prefers to keep distance
        float distToPlayer = Vector3.Distance(GetWorldPosition(), player.GetWorldPosition());

        if(distToPlayer < 4f && HasMove)
        {
            // Too close - back away
            GridNode startNode = grid.GetNodeFromWorld(transform.position);
            List<GridNode> pathAway = resolver.Resolve(new AttackIntent(new TargetData(player)), startNode);

            if(pathAway != null && pathAway.Count > 2)
            {
                // Move away (take last node instead of first)
                pathAway = pathAway.GetRange(pathAway.Count - 2, 2);
                mover.StartPath(pathAway);
                RemainingMovement -= 2;
                HasMove = RemainingMovement > 0;
            }

            while(mover.IsMoving)
                yield return null;
        }

        // Attack with ranged
        if(HasAction)
        {
            Ability rangedAttack = abilities[0];
            TargetData targetData = new TargetData(player);
            intentExecutor.ExecuteAbilityWithMovement(this, rangedAttack, targetData);

            while(mover.IsMoving)
                yield return null;

            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.1f);
        CombatManager.Instance.EndTurn();
    }
}
