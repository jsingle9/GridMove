using UnityEngine;
using System.Collections.Generic;

public class IntentExecutor
{
    private GridController grid;
    private IntentResolver intentResolver;
    private UnitMover unitMover;

    // Pending state for "move then retry"
    private ICombatant pendingUser;
    private Ability pendingAbility;
    private TargetData pendingTargetData;
    private bool awaitingMovementCompletion = false;

    public void Initialize(GridController gridController, UnitMover mover)
    {
        this.grid = gridController;
        this.unitMover = mover;
        this.intentResolver = new IntentResolver(grid);
    }

    /// <summary>
    /// Execute an ability with movement support.
    /// If ability is out of range and unit has movement, will move then retry.
    /// </summary>
    public AbilityResult ExecuteAbilityWithMovement(ICombatant user, Ability ability, TargetData targetData)
    {
        if(ability == null || targetData == null)
            return AbilityResult.CreateFailure("Invalid ability or target");

        // IMPORTANT: use the acting unit's mover, not a shared cached mover
        MonoBehaviour userMb = user as MonoBehaviour;
        if(userMb == null)
            return AbilityResult.CreateFailure("User is not a MonoBehaviour");

        UnitMover actorMover = userMb.GetComponent<UnitMover>();
        if(actorMover == null)
            return AbilityResult.CreateFailure("User has no UnitMover");

        AbilityResult result = ability.TryUse(user, targetData);
        if(result.Success)
            return result;

        if(result.FailureReason == "Target out of melee range" ||
           result.FailureReason == "Target out of range")
        {
            if(!user.HasMove)
                return result;

            GridNode startNode = grid.GetNodeFromWorld(user.GetWorldPosition());
            if(startNode == null)
                return AbilityResult.CreateFailure("Cannot find start position");

            Vector3Int targetCell = GetPreferredOrClosestTargetCell(user, targetData);
            Vector3 targetWorld = grid.GridToWorld(targetCell);
            targetData.tile = grid.GetNodeFromWorld(targetWorld);

            AttackIntent moveIntent = new AttackIntent(targetData);
            List<GridNode> path = intentResolver.Resolve(moveIntent, startNode);

            if(path == null || path.Count <= 1)
                return AbilityResult.CreateFailure("Cannot reach target");

            int spent;
            path = MovementCostUtility.TrimPathToBudget(grid, path, user.RemainingMovement, out spent);

            if(path == null || path.Count <= 1 || spent <= 0)
                return AbilityResult.CreateFailure("No reachable movement within budget");

            // queue retry
            pendingUser = user;
            pendingAbility = ability;
            pendingTargetData = targetData;
            awaitingMovementCompletion = true;

            // start movement on correct mover
            actorMover.StartPath(path);

            user.RemainingMovement -= spent;
            if(user.RemainingMovement < 0) user.RemainingMovement = 0;
            user.HasMove = user.RemainingMovement > 0;

            Debug.Log($"[IntentExecutor] {user.Name} move started via actorMover. spent={spent}, remaining={user.RemainingMovement}");

            return AbilityResult.CreateSuccess();
        }

        return result;
    }

    /// <summary>
    /// Call this from BoxMover/Enemy's Update() to check if movement finished
    /// </summary>
    public void CheckPendingAbilityExecution()
    {
        if(!awaitingMovementCompletion)
            return;

        if(pendingUser == null || pendingAbility == null || pendingTargetData == null)
        {
            awaitingMovementCompletion = false;
            pendingUser = null;
            pendingAbility = null;
            pendingTargetData = null;
            return;
        }

        MonoBehaviour mb = pendingUser as MonoBehaviour;
        if(mb == null)
        {
            Debug.LogWarning("[IntentExecutor] pendingUser is not a MonoBehaviour. Clearing pending state.");
            awaitingMovementCompletion = false;
            pendingUser = null;
            pendingAbility = null;
            pendingTargetData = null;
            return;
        }

        UnitMover pendingMover = mb.GetComponent<UnitMover>();
        if(pendingMover == null)
        {
            Debug.LogWarning($"[IntentExecutor] {pendingUser.Name} has no UnitMover. Retrying ability immediately.");
        }
        else if(pendingMover.IsMoving)
        {
            return; // still moving, wait
        }

        // Movement finished (or no mover) -> retry ability once
        Debug.Log($"[IntentExecutor] Movement finished for {pendingUser.Name} -> retrying ability");
        AbilityResult retryResult = pendingAbility.TryUse(pendingUser, pendingTargetData);

        if(!retryResult.Success)
            Debug.Log($"[IntentExecutor] Ability failed after movement: {retryResult.FailureReason}");

        // Clear pending state
        pendingUser = null;
        pendingAbility = null;
        pendingTargetData = null;
        awaitingMovementCompletion = false;
    }

    /// <summary>
    /// Returns true if we're waiting for movement to complete
    /// </summary>
    public bool IsExecutingAbilityWithMovement()
    {
        return awaitingMovementCompletion;
    }

    /// <summary>
    /// Cancel any pending ability execution
    /// </summary>
    public void CancelPendingAbility()
    {
        pendingUser = null;
        pendingAbility = null;
        pendingTargetData = null;
        awaitingMovementCompletion = false;
    }

    private Vector3Int GetPreferredOrClosestTargetCell(ICombatant user, TargetData targetData)
    {
        if (targetData != null && targetData.preferredTargetCell.HasValue)
            return targetData.preferredTargetCell.Value;

        if (targetData == null || targetData.primaryTarget == null)
            return grid.WorldToGrid(user.GetWorldPosition());

        List<Vector3Int> occupiedCells = targetData.primaryTarget.GetOccupiedCells();

        if (occupiedCells == null || occupiedCells.Count == 0)
            return grid.WorldToGrid(targetData.primaryTarget.GetWorldPosition());

        Vector3Int userCell = grid.WorldToGrid(user.GetWorldPosition());
        Vector3Int bestCell = occupiedCells[0];
        int bestDist = ManhattanDistance(userCell, bestCell);

        for (int i = 1; i < occupiedCells.Count; i++)
        {
            int dist = ManhattanDistance(userCell, occupiedCells[i]);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestCell = occupiedCells[i];
            }
        }

        return bestCell;
    }

    private int ManhattanDistance(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private int CalculateEnteredTileCost(GridController grid, List<GridNode> path)
    {
        if (path == null || path.Count <= 1) return 0;

        int cost = 0;
        for (int i = 1; i < path.Count; i++) // skip start node
        {
            int step = grid.GetMovementCost(path[i].gridPos);
            if (step <= 0) step = 1;
            cost += step;
        }
        return cost;
    }
}
