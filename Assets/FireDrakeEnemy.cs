using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireDrakeEnemy : Enemy
{
    [Header("Fire Drake")]
    [SerializeField] private int breathEveryNTurns = 4;
    [SerializeField] private int breathRange = 6;
    [SerializeField] private int breathDamage = 8;

    private int turnCounter = 0;

    protected override void Awake()
    {
        base.Awake();

        maxHP = 55;
        currentHP = maxHP;
        armorClass = 16;
        attackBonus = 6;
        damageDice = "2d8";
        damageModifier = 4;
        speed = 4;

        abilities.Clear();
        abilities.Add(new AttackAbility());

        Debug.Log("FireDrakeEnemy initialized");
    }

    public override List<Vector3Int> GetOccupiedCells()
    {
        Vector3Int origin = grid.WorldToGrid(transform.position);

        return new List<Vector3Int>
        {
            origin,
            origin + Vector3Int.right,
            origin + Vector3Int.up,
            origin + Vector3Int.right + Vector3Int.up
        };
    }

    protected override IEnumerator EnemyTurnRoutine()
    {
        if (IsDead())
            yield break;

        yield return new WaitForSeconds(0.15f);

        BoxMover player = FindFirstObjectByType<BoxMover>();
        if (player == null)
        {
            EndMyTurn();
            yield break;
        }

        turnCounter++;

        if (IsBreathTurn())
        {
            yield return ExecuteBreathTurn(player);
        }
        else
        {
            yield return ExecuteNormalTurn(player);
        }

        yield return new WaitForSeconds(0.1f);
        EndMyTurn();
    }

    private bool IsBreathTurn()
    {
        return turnCounter % breathEveryNTurns == 0;
    }

    private IEnumerator ExecuteNormalTurn(BoxMover player)
    {
        if (!HasAction && !HasMove)
            yield break;

        Ability melee = abilities[0];
        TargetData targetData = new TargetData(player);

        intentExecutor.ExecuteAbilityWithMovement(this, melee, targetData);

        while (mover.IsMoving)
            yield return null;

        yield return new WaitForSeconds(0.1f);
    }

    private IEnumerator ExecuteBreathTurn(BoxMover player)
    {
        Debug.Log("Fire Drake uses breath turn");

        if (HasMove)
        {
            Ability melee = abilities[0];
            TargetData moveTowardTarget = new TargetData(player);

            intentExecutor.ExecuteAbilityWithMovement(this, melee, moveTowardTarget);

            while (mover.IsMoving)
                yield return null;

            yield return new WaitForSeconds(0.1f);
        }

        Vector3Int drakeOrigin = grid.WorldToGrid(transform.position);
        Vector3Int playerCell = grid.WorldToGrid(player.GetWorldPosition());
        Vector3Int breathDir = GetBreathDirection(drakeOrigin, playerCell);

        if (breathDir == Vector3Int.right)
        {
            ResolveBreathLane(drakeOrigin + new Vector3Int(2, 0, 0), Vector3Int.right);
            ResolveBreathLane(drakeOrigin + new Vector3Int(2, 1, 0), Vector3Int.right);
        }
        else if (breathDir == Vector3Int.left)
        {
            ResolveBreathLane(drakeOrigin + new Vector3Int(-1, 0, 0), Vector3Int.left);
            ResolveBreathLane(drakeOrigin + new Vector3Int(-1, 1, 0), Vector3Int.left);
        }
        else if (breathDir == Vector3Int.up)
        {
            ResolveBreathLane(drakeOrigin + new Vector3Int(0, 2, 0), Vector3Int.up);
            ResolveBreathLane(drakeOrigin + new Vector3Int(1, 2, 0), Vector3Int.up);
        }
        else if (breathDir == Vector3Int.down)
        {
            ResolveBreathLane(drakeOrigin + new Vector3Int(0, -1, 0), Vector3Int.down);
            ResolveBreathLane(drakeOrigin + new Vector3Int(1, -1, 0), Vector3Int.down);
        }

        HasAction = false;

        Debug.Log($"Fire Drake breathed {breathDir}");
        yield return new WaitForSeconds(0.2f);
    }

    private void ResolveBreathLane(Vector3Int start, Vector3Int step)
    {
        HashSet<ICombatant> hitTargets = new HashSet<ICombatant>();

        for (int i = 0; i < breathRange; i++)
        {
            Vector3Int cell = start + (step * i);

            if (!grid.IsInBounds(cell))
                break;

            GoldPileObstacle goldPile = grid.GetGoldPileAt(cell);
            if (goldPile != null && !goldPile.IsMelted)
            {
                goldPile.Melt();
                break;
            }

            ICombatant occupant = grid.GetOccupant(cell);
            if (occupant != null && occupant != this && !hitTargets.Contains(occupant))
            {
                occupant.TakeDamage(breathDamage);
                hitTargets.Add(occupant);
            }
        }
    }

    private Vector3Int GetBreathDirection(Vector3Int from, Vector3Int to)
    {
        int dx = to.x - from.x;
        int dy = to.y - from.y;

        if (Mathf.Abs(dx) >= Mathf.Abs(dy))
        {
            return dx >= 0 ? Vector3Int.right : Vector3Int.left;
        }

        return dy >= 0 ? Vector3Int.up : Vector3Int.down;
    }

    protected override void Die()
    {
        Debug.Log($"{name} died");

        BossEncounterScoreManager scoreManager = FindFirstObjectByType<BossEncounterScoreManager>();
        if (scoreManager != null)
        {
            Debug.Log($"Boss defeated! Gold remaining: {scoreManager.GetGoldRemaining()}/{scoreManager.GetTotalGold()}");
        }

        grid.UnregisterCombatant(this);

        statusManager.Clear();
        CombatManager.Instance.NotifyDeath(this);
        gameObject.SetActive(false);
    }
}
