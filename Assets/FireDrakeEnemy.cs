using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireDrakeEnemy : Enemy
{
    [Header("Fire Drake")]
    [SerializeField] private int breathEveryNTurns = 4;
    [SerializeField] private int breathRange = 4;
    [SerializeField] private string breathDamage = "3d8";
    [SerializeField] private int breathDamageBonus = 0;
    [SerializeField] private float telegraphDuration = 1.8f;
    [SerializeField] private BossEncounterScoreManager bossScoreManager;
    private bool deathHandled = false;

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
        damageDice = "1d8";
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
        Debug.Log($"[DrakeTurn] HasAction={HasAction} HasMove={HasMove} RemainingMove={RemainingMovement}");
        if (!HasAction && !HasMove)
            yield break;

        if (player == null)
            yield break;

        Ability melee = abilities[0];
        TargetData targetData = new TargetData(player);
        Debug.Log($"[DrakeTurn] Player found? {player != null}");

        bool inRange = InMeleeRange(this, player, 1);
        Debug.Log($"[DrakeTurn] InMeleeRange={inRange}");

        if (inRange)
        {
            // If you have a direct no-move execute, use it here.
            // Otherwise this still works if executor handles "already in range" cleanly.
            intentExecutor.ExecuteAbilityWithMovement(this, melee, targetData);
        }
        else
        {
            intentExecutor.ExecuteAbilityWithMovement(this, melee, targetData);
            yield return WaitForMovementOrTimeout(2.0f);
        }

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

        List<Vector3Int> drakeCells = GetOccupiedCells();
        List<Vector3Int> playerCells = player.GetOccupiedCells();

        Vector3Int breathOrigin = GetBestBreathOriginCell(drakeCells, playerCells);
        Vector3Int targetCell = GetClosestTargetCellToOrigin(breathOrigin, playerCells);
        Vector3Int breathDir = GetBreathDirection(breathOrigin, targetCell);

        List<BreathLane> lanes = GetBreathLanesFromOriginCell(breathOrigin, breathDir);
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
            var occMb = occupant as MonoBehaviour;
            if (occMb != null && occMb != this && !hitTargets.Contains(occupant))
            {
                int damage = DiceRoller.Roll(this.breathDamage) + breathDamageBonus;
                Debug.Log($"Breath hit {occupant.Name} at {cell} for {damage}");
                CombatUIManager.Instance?.LogAbilityDamage(this.Name, "Breath Weapon", occupant.Name, damage, "fire");
                occupant.TakeDamage(damage);
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

        HandleBossDeath(); // single entry point, guarded against duplicates

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

        int spent = 0;
        int lastReachableIndex = 0;

        for (int i = 1; i < path.Count; i++)
        {
            int stepCost = grid.GetMovementCost(path[i].gridPos);
            if (stepCost <= 0) stepCost = 1;

            if (spent + stepCost > RemainingMovement)
                break;

            spent += stepCost;
            lastReachableIndex = i;
        }

        if (lastReachableIndex <= 0)
        {
            HasMove = false;
            yield break;
        }

        List<GridNode> trimmedPath = path.GetRange(0, lastReachableIndex + 1);
        mover.StartPath(trimmedPath);

        RemainingMovement -= spent;
        if (RemainingMovement < 0) RemainingMovement = 0;
        HasMove = RemainingMovement > 0;

        // OLD:
        // while (mover.IsMoving) yield return null;

        // NEW:
        yield return WaitForMovementOrTimeout(2.0f);
    }

    private IEnumerator WaitForMovementOrTimeout(float timeoutSeconds = 1.5f)
    {
        float t = 0f;
        while (mover.IsMoving && t < timeoutSeconds)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (mover.IsMoving)
        {
            Debug.LogWarning($"[FireDrake] Movement timeout hit. Forcing Stop() to prevent soft lock.");
            mover.Stop();
        }
    }

    private bool CanOccupyDrakeAnchor(Vector3Int origin)
    {
        Vector3Int[] cells = new Vector3Int[]
        {
            origin,
            origin + Vector3Int.right,
            origin + Vector3Int.up,
            origin + Vector3Int.right + Vector3Int.up
        };

        foreach (var c in cells)
        {
            if (!grid.IsInBounds(c)) return false;
            if (!grid.IsWalkable(c)) return false;

            ICombatant occ = grid.GetOccupant(c);
            if (occ != null && !ReferenceEquals(occ, this)) return false;
        }

        return true;
    }

    private Vector3Int GetBestBreathOriginCell(List<Vector3Int> drakeCells, List<Vector3Int> targetCells)
    {
        Vector3Int bestOrigin = drakeCells[0];
        float bestDist = float.MaxValue;

        foreach (var dc in drakeCells)
        {
            Vector3 dWorld = new Vector3(dc.x + 0.5f, dc.y + 0.5f, 0f);

            foreach (var tc in targetCells)
            {
                Vector3 tWorld = new Vector3(tc.x + 0.5f, tc.y + 0.5f, 0f);
                float dist = Vector3.Distance(dWorld, tWorld);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestOrigin = dc;
                }
            }
        }

        return bestOrigin;
    }

    private Vector3Int GetClosestTargetCellToOrigin(Vector3Int origin, List<Vector3Int> targetCells)
    {
        if (targetCells == null || targetCells.Count == 0)
            return origin;

        Vector3Int best = targetCells[0];
        int bestDist = int.MaxValue;

        foreach (var tc in targetCells)
        {
            int dist = Mathf.Abs(origin.x - tc.x) + Mathf.Abs(origin.y - tc.y);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = tc;
            }
        }

        return best;
    }

    private List<BreathLane> GetBreathLanesFromOriginCell(Vector3Int originCell, Vector3Int breathDir)
    {
        List<BreathLane> lanes = new List<BreathLane>();

        if (breathDir == Vector3Int.right)
        {
            lanes.Add(new BreathLane(originCell + new Vector3Int(1, 0, 0), Vector3Int.right));
            lanes.Add(new BreathLane(originCell + new Vector3Int(1, 1, 0), Vector3Int.right));
        }
        else if (breathDir == Vector3Int.left)
        {
            lanes.Add(new BreathLane(originCell + new Vector3Int(-1, 0, 0), Vector3Int.left));
            lanes.Add(new BreathLane(originCell + new Vector3Int(-1, 1, 0), Vector3Int.left));
        }
        else if (breathDir == Vector3Int.up)
        {
            lanes.Add(new BreathLane(originCell + new Vector3Int(0, 1, 0), Vector3Int.up));
            lanes.Add(new BreathLane(originCell + new Vector3Int(1, 1, 0), Vector3Int.up));
        }
        else if (breathDir == Vector3Int.down)
        {
            lanes.Add(new BreathLane(originCell + new Vector3Int(0, -1, 0), Vector3Int.down));
            lanes.Add(new BreathLane(originCell + new Vector3Int(1, -1, 0), Vector3Int.down));
        }

        return lanes;
    }

    private void HandleBossDeath()
    {
        if (deathHandled) return;
        deathHandled = true;

        if (bossScoreManager == null)
            bossScoreManager = FindFirstObjectByType<BossEncounterScoreManager>();

        if (bossScoreManager != null)
            bossScoreManager.FinalizeBossEncounterScore();
        else
            Debug.LogWarning("BossEncounterScoreManager not found when drake died.");

        Debug.Log("FireDrake death handled: boss score finalized.");
    }

    private bool InMeleeRange(ICombatant a, ICombatant b, int range = 1)
    {
        if (a == null || b == null) return false;

        var aCells = a.GetOccupiedCells();
        var bCells = b.GetOccupiedCells();
        if (aCells == null || bCells == null) return false;

        for (int i = 0; i < aCells.Count; i++)
        {
            for (int j = 0; j < bCells.Count; j++)
            {
                int d = Mathf.Abs(aCells[i].x - bCells[j].x) + Mathf.Abs(aCells[i].z - bCells[j].z);
                if (d <= range) return true;
            }
        }

        return false;
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
