using UnityEngine;
using System.Collections.Generic;

public class IntentExecutor
{
    private GridController grid;
    private IntentResolver intentResolver;
    private UnitMover unitMover;

    private ICombatant pendingUser;
    private Ability pendingAbility;
    private TargetData pendingTargetData;

    private bool awaitingMovementCompletion;

    public void Initialize(
        GridController gridController,
        UnitMover mover
    )
    {
        grid = gridController;
        unitMover = mover;
        intentResolver = new IntentResolver(grid);
    }

    public AbilityResult ExecuteAbilityWithMovement(
        ICombatant user,
        Ability ability,
        TargetData targetData
    )
    {
        if (user == null ||
            ability == null ||
            targetData == null)
        {
            return AbilityResult.CreateFailure(
                "Invalid ability or target"
            );
        }

        MonoBehaviour userObject = user as MonoBehaviour;

        if (userObject == null)
        {
            return AbilityResult.CreateFailure(
                "User is not a MonoBehaviour"
            );
        }

        UnitMover actorMover =
            userObject.GetComponent<UnitMover>();

        if (actorMover == null)
        {
            return AbilityResult.CreateFailure(
                "User has no UnitMover"
            );
        }

        AbilityResult result =
            ability.TryUse(user, targetData);

        if (result.Success)
            return result;

        /*
         * AttackAbility has its own movement-aware melee logic.
         * Ranged attacks must never trigger automatic movement.
         */
        if (ability is AttackAbility attack)
        {
            if (attack.UsesRangedWeapon(user))
                return result;

            if (result.FailureReason != "Target out of melee range")
                return result;

            return TryQueueMeleeAttackMovement(
                user,
                attack,
                targetData,
                actorMover,
                result
            );
        }

        /*
         * Other abilities can still use generic move-then-retry
         * behavior, such as touch spells.
         */
        if (IsGenericMovementFailure(result.FailureReason))
        {
            return TryQueueGenericAbilityMovement(
                user,
                ability,
                targetData,
                actorMover,
                result
            );
        }

        return result;
    }

    private AbilityResult TryQueueMeleeAttackMovement(
        ICombatant user,
        AttackAbility attack,
        TargetData targetData,
        UnitMover actorMover,
        AbilityResult originalFailure
    )
    {
        if (targetData.primaryTarget == null)
            return originalFailure;

        if (!user.HasMove ||
            user.RemainingMovement <= 0)
        {
            return originalFailure;
        }

        if (!attack.TryGetMeleeApproach(
            user,
            targetData.primaryTarget,
            out List<GridNode> path,
            out int movementCost
        ))
        {
            return AbilityResult.CreateFailure(
                "Cannot reach target with remaining movement"
            );
        }

        if (movementCost <= 0)
        {
            // The attack should have resolved already if the actor
            // was adjacent. Preserve the original failure.
            return originalFailure;
        }

        if (movementCost > user.RemainingMovement)
        {
            return AbilityResult.CreateFailure(
                "Cannot reach target with remaining movement"
            );
        }

        if (path == null || path.Count == 0)
        {
            return AbilityResult.CreateFailure(
                "No melee movement path available"
            );
        }

        QueuePendingAbility(
            user,
            attack,
            targetData
        );

        SpendMovement(
            user,
            movementCost
        );

        actorMover.StartPath(path);

        Debug.Log(
            $"[IntentExecutor] Moving {user.Name} into melee range. " +
            $"spent={movementCost}, " +
            $"remaining={user.RemainingMovement}"
        );

        return AbilityResult.CreateSuccess();
    }

    private AbilityResult TryQueueGenericAbilityMovement(
        ICombatant user,
        Ability ability,
        TargetData targetData,
        UnitMover actorMover,
        AbilityResult originalFailure
    )
    {
        if (!user.HasMove ||
            user.RemainingMovement <= 0)
        {
            return originalFailure;
        }

        if (grid == null ||
            intentResolver == null)
        {
            return AbilityResult.CreateFailure(
                "Movement system is not initialized"
            );
        }

        GridNode startNode =
            grid.GetNodeFromWorld(
                user.GetWorldPosition()
            );

        if (startNode == null)
        {
            return AbilityResult.CreateFailure(
                "Cannot find start position"
            );
        }

        Vector3Int targetCell =
            GetPreferredOrClosestTargetCell(
                user,
                targetData
            );

        GridNode targetNode =
            grid.GetNodeFromWorld(
                grid.GridToWorld(targetCell)
            );

        if (targetNode == null)
        {
            return AbilityResult.CreateFailure(
                "Cannot find target position"
            );
        }

        /*
         * Preserve the existing generic movement behavior.
         * AttackIntent should resolve a path toward the target.
         */
        targetData.tile = targetNode;

        AttackIntent moveIntent =
            new AttackIntent(targetData);

        List<GridNode> path =
            intentResolver.Resolve(
                moveIntent,
                startNode
            );

        if (path == null ||
            path.Count == 0)
        {
            return AbilityResult.CreateFailure(
                "Cannot reach target"
            );
        }

        int spent;

        List<GridNode> allowedPath =
            MovementCostUtility.TrimPathToBudget(
                grid,
                path,
                user.RemainingMovement,
                out spent
            );

        if (allowedPath == null ||
            allowedPath.Count == 0 ||
            spent <= 0)
        {
            return AbilityResult.CreateFailure(
                "No reachable movement within budget"
            );
        }

        QueuePendingAbility(
            user,
            ability,
            targetData
        );

        SpendMovement(
            user,
            spent
        );

        actorMover.StartPath(allowedPath);

        Debug.Log(
            $"[IntentExecutor] Moving {user.Name} for " +
            $"{ability.AbilityName}. " +
            $"spent={spent}, " +
            $"remaining={user.RemainingMovement}"
        );

        return AbilityResult.CreateSuccess();
    }

    private bool IsGenericMovementFailure(
        string failureReason
    )
    {
        return failureReason == "Target out of range" ||
               failureReason == "Target is out of range" ||
               failureReason == "Target out of reach" ||
               failureReason == "Target is out of reach";
    }

    private void QueuePendingAbility(
        ICombatant user,
        Ability ability,
        TargetData targetData
    )
    {
        pendingUser = user;
        pendingAbility = ability;
        pendingTargetData = targetData;
        awaitingMovementCompletion = true;
    }

    private void SpendMovement(
        ICombatant user,
        int amount
    )
    {
        user.RemainingMovement -= amount;
        user.RemainingMovement =
            Mathf.Max(0, user.RemainingMovement);

        user.HasMove =
            user.RemainingMovement > 0;
    }

    public void CheckPendingAbilityExecution()
    {
        if (!awaitingMovementCompletion)
            return;

        if (pendingUser == null ||
            pendingAbility == null ||
            pendingTargetData == null)
        {
            ClearPendingState();
            return;
        }

        MonoBehaviour userObject =
            pendingUser as MonoBehaviour;

        if (userObject == null)
        {
            Debug.LogWarning(
                "[IntentExecutor] Pending user is not a MonoBehaviour."
            );

            ClearPendingState();
            return;
        }

        UnitMover pendingMover =
            userObject.GetComponent<UnitMover>();

        if (pendingMover != null &&
            pendingMover.IsMoving)
        {
            return;
        }

        Debug.Log(
            $"[IntentExecutor] Movement finished for " +
            $"{pendingUser.Name}; retrying " +
            $"{pendingAbility.AbilityName}"
        );

        AbilityResult retryResult =
            pendingAbility.TryUse(
                pendingUser,
                pendingTargetData
            );

        if (!retryResult.Success)
        {
            Debug.Log(
                $"[IntentExecutor] Ability failed after movement: " +
                $"{retryResult.FailureReason}"
            );
        }

        ClearPendingState();
    }

    public bool IsExecutingAbilityWithMovement()
    {
        return awaitingMovementCompletion;
    }

    public void CancelPendingAbility()
    {
        ClearPendingState();
    }

    private void ClearPendingState()
    {
        pendingUser = null;
        pendingAbility = null;
        pendingTargetData = null;
        awaitingMovementCompletion = false;
    }

    private Vector3Int GetPreferredOrClosestTargetCell(
        ICombatant user,
        TargetData targetData
    )
    {
        if (targetData != null &&
            targetData.preferredTargetCell.HasValue)
        {
            return targetData.preferredTargetCell.Value;
        }

        if (targetData == null ||
            targetData.primaryTarget == null)
        {
            return grid.WorldToGrid(
                user.GetWorldPosition()
            );
        }

        List<Vector3Int> occupiedCells =
            targetData.primaryTarget.GetOccupiedCells();

        if (occupiedCells == null ||
            occupiedCells.Count == 0)
        {
            return grid.WorldToGrid(
                targetData.primaryTarget.GetWorldPosition()
            );
        }

        Vector3Int userCell =
            grid.WorldToGrid(
                user.GetWorldPosition()
            );

        Vector3Int closestCell =
            occupiedCells[0];

        int closestDistance =
            ManhattanDistance(
                userCell,
                closestCell
            );

        for (int i = 1; i < occupiedCells.Count; i++)
        {
            int distance =
                ManhattanDistance(
                    userCell,
                    occupiedCells[i]
                );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCell = occupiedCells[i];
            }
        }

        return closestCell;
    }

    private int ManhattanDistance(
        Vector3Int a,
        Vector3Int b
    )
    {
        return Mathf.Abs(a.x - b.x) +
               Mathf.Abs(a.y - b.y);
    }
}
