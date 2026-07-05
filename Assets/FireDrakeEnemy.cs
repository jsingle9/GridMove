using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireDrakeEnemy : Enemy
{
    [Header("Fire Drake")]
    [SerializeField] private int breathEveryNTurns = 4;
    [SerializeField] private int breathRange = 6;
    [SerializeField] private int breathDamage = 8;
    [SerializeField] private float telegraphDuration = 1.8f;

    [Header("References")]
    [SerializeField] private BreathWeaponTelegrapher breathTelegrapher;

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

        if (breathTelegrapher == null)
            breathTelegrapher = GetComponent<BreathWeaponTelegrapher>();

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
            yield return ExecuteBreathTurn(player);
        else
            yield return ExecuteNormalTurn(player);

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
            yield return MoveTowardPlayerForBreath(player);
            yield return new WaitForSeconds(0.1f);
        }

        if (!HasAction)
        {
            Debug.Log("Fire Drake has no action left for breath attack.");
            yield break;
        }

        Vector3Int drakeOrigin = grid.WorldToGrid(transform.position);
        Vector3Int playerCell = grid.WorldToGrid(player.GetWorldPosition());
        Vector3Int breathDir = GetBreathDirection(drakeOrigin, playerCell);

        List<BreathLane> lanes = GetBreathLanes(drakeOrigin, breathDir);
        List<Vector3Int> previewCells = GetPreviewCells(lanes);

        Debug.Log($"Fire Drake breath direction: {breathDir}, preview cell count: {previewCells.Count}");

        if (breathTelegrapher != null && previewCells.Count > 0)
        {
            Debug.Log($"[FireDrake] Preview cells count: {previewCells.Count}, telegrapher assigned: {breathTelegrapher != null}");          
            breathTelegrapher.ShowTelegraph(grid, previewCells, breathDir);
        }

        yield return new WaitForSeconds(telegraphDuration);

        if (breathTelegrapher != null)
        {
            breathTelegrapher.ClearTelegraph();
        }

        ResolveBreathAttack(lanes);

        HasAction = false;

        Debug.Log($"Fire Drake breathed {breathDir}");
        yield return new WaitForSeconds(0.2f);
    }

    private List<BreathLane> GetBreathLanes(Vector3Int drakeOrigin, Vector3Int breathDir)
    {
        List<BreathLane> lanes = new List<BreathLane>();

        if (breathDir == Vector3Int.right)
        {
            lanes.Add(new BreathLane(drakeOrigin + new Vector3Int(2, 0, 0), Vector3Int.right));
            lanes.Add(new BreathLane(drakeOrigin + new Vector3Int(2, 1, 0), Vector3Int.right));
        }
        else if (breathDir == Vector3Int.left)
        {
            lanes.Add(new BreathLane(drakeOrigin + new Vector3Int(-1, 0, 0), Vector3Int.left));
            lanes.Add(new BreathLane(drakeOrigin + new Vector3Int(-1, 1, 0), Vector3Int.left));
        }
        else if (breathDir == Vector3Int.up)
        {
            lanes.Add(new BreathLane(drakeOrigin + new Vector3Int(0, 2, 0), Vector3Int.up));
            lanes.Add(new BreathLane(drakeOrigin + new Vector3Int(1, 2, 0), Vector3Int.up));
        }
        else if (breathDir == Vector3Int.down)
        {
            lanes.Add(new BreathLane(drakeOrigin + new Vector3Int(0, -1, 0), Vector3Int.down));
            lanes.Add(new BreathLane(drakeOrigin + new Vector3Int(1, -1, 0), Vector3Int.down));
        }

        return lanes;
    }

    private List<Vector3Int> GetPreviewCells(List<BreathLane> lanes)
    {
        List<Vector3Int> cells = new List<Vector3Int>();

        foreach (BreathLane lane in lanes)
        {
            for (int i = 0; i < breathRange; i++)
            {
                Vector3Int cell = lane.start + (lane.step * i);

                if (!grid.IsInBounds(cell))
                    break;

                cells.Add(cell);

                GoldPileObstacle goldPile = grid.GetGoldPileAt(cell);
                if (goldPile != null && !goldPile.IsMelted)
                    break;
            }
        }

        return cells;
    }

    private void ResolveBreathAttack(List<BreathLane> lanes)
    {
        Debug.Log($"Resolving breath attack with {lanes.Count} lanes");

        HashSet<ICombatant> hitTargets = new HashSet<ICombatant>();

        foreach (BreathLane lane in lanes)
        {
            Debug.Log($"Resolving lane from {lane.start} step {lane.step}");
            ResolveBreathLane(lane.start, lane.step, hitTargets);
        }
    }

    private void ResolveBreathLane(Vector3Int start, Vector3Int step, HashSet<ICombatant> hitTargets)
    {
        for (int i = 0; i < breathRange; i++)
        {
            Vector3Int cell = start + (step * i);

            if (!grid.IsInBounds(cell))
            {
                Debug.Log($"Breath stopped: out of bounds at {cell}");
                break;
            }

            Debug.Log($"Breath checking cell {cell}");

            GoldPileObstacle goldPile = grid.GetGoldPileAt(cell);
            if (goldPile != null && !goldPile.IsMelted)
            {
                Debug.Log($"Breath hit gold pile at {cell} and melted it");
                goldPile.Melt();
                break;
            }

            ICombatant occupant = grid.GetOccupant(cell);
            if (occupant != null && occupant != this && !hitTargets.Contains(occupant))
            {
                Debug.Log($"Breath hit {occupant.Name} at {cell} for {breathDamage}");
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
            return dx >= 0 ? Vector3Int.right : Vector3Int.left;

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

        if (breathTelegrapher != null)
            breathTelegrapher.ClearTelegraph();

        grid.UnregisterCombatant(this);

        statusManager.Clear();
        CombatManager.Instance.NotifyDeath(this);
        gameObject.SetActive(false);
    }

    private IEnumerator MoveTowardPlayerForBreath(BoxMover player)
    {
        GridNode startNode = grid.GetNodeFromWorld(transform.position);
        if (startNode == null)
            yield break;

        List<GridNode> path = resolver.Resolve(new AttackIntent(new TargetData(player)), startNode);

        if (path == null || path.Count <= 1)
            yield break;

        int maxSteps = Mathf.Min(RemainingMovement, path.Count - 1);
        if (maxSteps <= 0)
            yield break;

        List<GridNode> trimmedPath = path.GetRange(0, maxSteps + 1);

        mover.StartPath(trimmedPath);

        RemainingMovement -= maxSteps;
        if (RemainingMovement < 0)
            RemainingMovement = 0;

        HasMove = RemainingMovement > 0;

        while (mover.IsMoving)
            yield return null;
    }

    private struct BreathLane
    {
        public Vector3Int start;
        public Vector3Int step;

        public BreathLane(Vector3Int start, Vector3Int step)
        {
            this.start = start;
            this.step = step;
        }
    }
}
