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

        // First, move toward the player using existing movement support.
        if (HasMove)
        {
            Ability melee = abilities[0];
            TargetData moveTowardTarget = new TargetData(player);

            // This may move closer if out of melee range.
            intentExecutor.ExecuteAbilityWithMovement(this, melee, moveTowardTarget);

            while (mover.IsMoving)
                yield return null;

            yield return new WaitForSeconds(0.1f);
        }

        Vector3Int drakeOrigin = grid.WorldToGrid(transform.position);
        Vector3Int playerCell = grid.WorldToGrid(player.GetWorldPosition());
        Vector3Int breathDir = GetBreathDirection(drakeOrigin, playerCell);

        List<Vector3Int> breathTiles = GetBreathArea(drakeOrigin, breathDir, breathRange);

        foreach (Vector3Int cell in breathTiles)
        {
            ICombatant occupant = grid.GetOccupant(cell);

            if (occupant != null && occupant != this)
            {
                occupant.TakeDamage(breathDamage);
            }

            // Later:
            // Check for gold pile / destructible cover here.
            // If gold pile present: destroy it and stop advancing that lane.
        }

        HasAction = false;

        Debug.Log($"Fire Drake breathed {breathDir} affecting {breathTiles.Count} tiles");
        yield return new WaitForSeconds(0.2f);
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

    private List<Vector3Int> GetBreathArea(Vector3Int origin, Vector3Int dir, int range)
    {
        List<Vector3Int> cells = new List<Vector3Int>();

        // 2-tile-wide breath in the chosen direction.
        // origin is bottom-left of the drake footprint.

        Vector3Int startA;
        Vector3Int startB;

        if (dir == Vector3Int.right)
        {
            startA = origin + new Vector3Int(2, 0, 0);
            startB = origin + new Vector3Int(2, 1, 0);

            for (int i = 0; i < range; i++)
            {
                cells.Add(startA + new Vector3Int(i, 0, 0));
                cells.Add(startB + new Vector3Int(i, 0, 0));
            }
        }
        else if (dir == Vector3Int.left)
        {
            startA = origin + new Vector3Int(-1, 0, 0);
            startB = origin + new Vector3Int(-1, 1, 0);

            for (int i = 0; i < range; i++)
            {
                cells.Add(startA + new Vector3Int(-i, 0, 0));
                cells.Add(startB + new Vector3Int(-i, 0, 0));
            }
        }
        else if (dir == Vector3Int.up)
        {
            startA = origin + new Vector3Int(0, 2, 0);
            startB = origin + new Vector3Int(1, 2, 0);

            for (int i = 0; i < range; i++)
            {
                cells.Add(startA + new Vector3Int(0, i, 0));
                cells.Add(startB + new Vector3Int(0, i, 0));
            }
        }
        else // down
        {
            startA = origin + new Vector3Int(0, -1, 0);
            startB = origin + new Vector3Int(1, -1, 0);

            for (int i = 0; i < range; i++)
            {
                cells.Add(startA + new Vector3Int(0, -i, 0));
                cells.Add(startB + new Vector3Int(0, -i, 0));
            }
        }

        List<Vector3Int> validCells = new List<Vector3Int>();

        foreach (Vector3Int cell in cells)
        {
            if (grid.IsInBounds(cell))
                validCells.Add(cell);
        }

        return validCells;
    }

    protected override void Die()
    {
        Debug.Log($"{name} died");

        grid.UnregisterCombatant(this);

        statusManager.Clear();
        CombatManager.Instance.NotifyDeath(this);
        gameObject.SetActive(false);
    }

    private void ResolveBreathLane(Vector3Int start, Vector3Int step){
      
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

}
