using UnityEngine;
using System.Collections.Generic;

public class IceDrakeEnemy : Enemy
{
    protected override void Awake()
    {
        base.Awake();

        maxHP = 40;
        currentHP = maxHP;
        armorClass = 15;
        attackBonus = 6;
        damageDice = "2d8";
        damageModifier = 4;
        speed = 4;

        abilities.Clear();
        abilities.Add(new AttackAbility());
        abilities.Add(new RangedAttackAbility()); // replace later with IceBreathAbility etc.

        Debug.Log("IceDrakeEnemy initialized");
    }

    public override List<Vector3Int> GetOccupiedCells()
    {
        Vector3Int origin = grid.WorldToGrid(transform.position);

        // origin is bottom-left of the 2x2 footprint
        return new List<Vector3Int>
        {
            origin,
            origin + Vector3Int.right,
            origin + Vector3Int.up,
            origin + Vector3Int.right + Vector3Int.up
        };
    }

    protected override System.Collections.IEnumerator EnemyTurnRoutine()
    {
        if(IsDead()) yield break;

        yield return new WaitForSeconds(0.15f);

        BoxMover player = FindFirstObjectByType<BoxMover>();
        if(player == null)
        {
            EndMyTurn();
            yield break;
        }

        // placeholder AI
        float distToPlayer = Vector3.Distance(GetWorldPosition(), player.GetWorldPosition());

        if(distToPlayer < 2.5f && HasAction)
        {
            Ability melee = abilities[0];
            TargetData targetData = new TargetData(player);
            intentExecutor.ExecuteAbilityWithMovement(this, melee, targetData);

            while(mover.IsMoving)
                yield return null;
        }

        yield return new WaitForSeconds(0.1f);
        EndMyTurn();
    }

    protected override void Die()
    {
        Debug.Log($"{name} died");

        grid.UnregisterCombatant(this);

        statusManager.Clear();
        CombatManager.Instance.NotifyDeath(this);
        gameObject.SetActive(false);
    }
}
