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
        {
            return AbilityResult.CreateFailure("Invalid ability or target");
        }

        // Try to execute ability immediately
        AbilityResult result = ability.TryUse(user, targetData);

        if(result.Success)
        {
            Debug.Log($"Ability {ability.AbilityName} executed successfully");
            return result;
        }

        // If it failed due to range and we have movement, try to move closer
        if(result.FailureReason == "Target out of melee range" ||
           result.FailureReason == "Target out of range")
        {
            if(!user.HasMove)
            {
                Debug.Log($"Out of range and no movement available");
                return result;
            }

            // Calculate path to target
            GridNode startNode = grid.GetNodeFromWorld(user.GetWorldPosition());
            if(startNode == null)
            {
                return AbilityResult.CreateFailure("Cannot find start position");
            }

            Vector3Int targetCell = GetPreferredOrClosestTargetCell(user, targetData);
            Vector3 targetWorld = grid.GridToWorld(targetCell);
            targetData.tile = grid.GetNodeFromWorld(targetWorld);

            // Create a temporary attack intent to get path to target
            AttackIntent moveIntent = new AttackIntent(targetData);
            List<GridNode> path = intentResolver.Resolve(moveIntent, startNode);

            if(path == null || path.Count == 0)
            {
                return AbilityResult.CreateFailure("Cannot reach target");
            }

            int moveCost = path.Count - 1;

            // Check if we have enough movement
            if(moveCost > user.RemainingMovement)
            {
                int allowed = user.RemainingMovement;
                if(allowed <= 0)
                {
                    return AbilityResult.CreateFailure("No movement available");
                }

                // Trim path to what we can afford
                path = path.GetRange(0, allowed + 1);
                moveCost = allowed;
            }

            // Spend movement
            user.RemainingMovement -= moveCost;
            if(user.RemainingMovement < 0)
                user.RemainingMovement = 0;

            user.HasMove = user.RemainingMovement > 0;

            Debug.Log($"Movement spent: {moveCost}, remaining: {user.RemainingMovement}");

            // Start movement and queue up ability retry
            pendingUser = user;
            pendingAbility = ability;
            pendingTargetData = targetData;
            awaitingMovementCompletion = true;

            unitMover.StartPath(path);

            return AbilityResult.CreateSuccess(); // Movement started successfully
        }

        // Other failures - just return them
        return result;
    }

    /// <summary>
    /// Call this from BoxMover/Enemy's Update() to check if movement finished
    /// </summary>
    public void CheckPendingAbilityExecution()
    {
        if(!awaitingMovementCompletion)
            return;

        if(unitMover.IsMoving)
            return; // Still moving

        // Movement finished → retry ability
        if(pendingUser != null && pendingAbility != null && pendingTargetData != null)
        {
            Debug.Log("Movement finished → retrying ability");
            AbilityResult retryResult = pendingAbility.TryUse(pendingUser, pendingTargetData);

            if(!retryResult.Success)
            {
                Debug.Log($"Ability failed after movement: {retryResult.FailureReason}");
            }

            // Clear pending state
            pendingUser = null;
            pendingAbility = null;
            pendingTargetData = null;
            awaitingMovementCompletion = false;
        }
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
}
