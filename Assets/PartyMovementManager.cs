using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages simultaneous movement of all party members to a common target.
/// Coordinates pathfinding, movement resolution, and grid occupancy for the entire party.
/// </summary>
public class PartyMovementManager : MonoBehaviour
{
    public static PartyMovementManager Instance { get; private set; }

    private class MemberMovement
    {
        public BoxMover Member;
        public List<GridNode> Path;
        public int RemainingMovement;
        public bool IsMoving;
    }

    private List<MemberMovement> currentMovements = new List<MemberMovement>();
    private GridController grid;
    private IntentResolver resolver;
    private bool isPartyMoving;

    public bool IsPartyMoving => isPartyMoving;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        grid = FindFirstObjectByType<GridController>();
        if (grid != null)
        {
            resolver = new IntentResolver(grid);
        }
    }

    /// <summary>
    /// Attempt to move all party members to the target cell.
    /// Each member will find their own path and only move if they have movement available.
    /// </summary>
    public void MovePartyToCell(Vector3Int targetCell)
    {
        if (grid == null || resolver == null)
        {
            Debug.LogError("PartyMovementManager: Grid or Resolver not initialized");
            return;
        }

        if (!grid.IsWalkable(targetCell))
        {
            Debug.Log($"[PartyMovement] Target cell {targetCell} is not walkable");
            return;
        }

        currentMovements.Clear();
        List<BoxMover> partyMembers = GetActivePartyMembers();

        if (partyMembers.Count == 0)
        {
            Debug.LogWarning("[PartyMovement] No active party members");
            return;
        }

        bool isExplorationMode = GameStateManager.Instance.CurrentGameState == GameState.FreeExplore;

        // Leader goes to target, followers get spaced behind in formation
        for (int memberIndex = 0; memberIndex < partyMembers.Count; memberIndex++)
        {
            var member = partyMembers[memberIndex];

            if (member == null || member.IsDead())
                continue;

            GridNode startNode = grid.GetNodeFromWorld(member.GetWorldPosition());
            if (startNode == null)
                continue;

            // Determine destination: leader goes to click, followers get formation positions
            Vector3Int destination = targetCell;

            if (memberIndex > 0)
            {
                // Followers: space them out in a line behind the leader
                // Calculate direction from leader's current position to target
                Vector3Int leaderPos = grid.WorldToGrid(partyMembers[0].GetWorldPosition());
                Vector3Int direction = targetCell - leaderPos;

                // Normalize direction to get a direction vector
                Vector3Int normalized = new Vector3Int(
                    direction.x != 0 ? (direction.x > 0 ? 1 : -1) : 0,
                    direction.y != 0 ? (direction.y > 0 ? 1 : -1) : 0,
                    0
                );

                // Place each follower offset from leader based on their index
                // Follower 1 goes 1 cell back, Follower 2 goes 2 cells back, etc.
                destination = targetCell - (normalized * memberIndex);

                // If that position isn't walkable, try to find nearby alternative
                if (!grid.IsWalkable(destination))
                {
                    // Try adjacent cells
                    Vector3Int[] adjacent = new Vector3Int[]
                    {
                        destination + Vector3Int.left,
                        destination + Vector3Int.right,
                        destination + Vector3Int.up,
                        destination + Vector3Int.down
                    };

                    bool foundAlternative = false;
                    foreach (var alt in adjacent)
                    {
                        if (grid.IsWalkable(alt))
                        {
                            destination = alt;
                            foundAlternative = true;
                            break;
                        }
                    }

                    if (!foundAlternative)
                    {
                        Debug.Log($"[PartyMovement] {member.Name} has no valid formation position near {targetCell}");
                        continue;
                    }
                }
            }

            // Create movement intent for this member
            MoveIntent intent = new MoveIntent(destination);
            List<GridNode> path = resolver.Resolve(intent, startNode);

            if (path == null || path.Count == 0)
            {
                Debug.Log($"[PartyMovement] {member.Name} has no path to {destination}");
                continue;
            }

            int moveCost = MovementCostUtility.CalculatePathCost(grid, path);
            int remainingMovement = member.RemainingMovement;

            // In exploration mode, don't limit by movement budget
            if (!isExplorationMode)
            {
                // Clamp path to available movement (combat mode only)
                if (moveCost > remainingMovement)
                {
                    int allowed = remainingMovement;

                    if (allowed <= 0)
                    {
                        Debug.Log($"[PartyMovement] {member.Name} has no movement remaining");
                        continue;
                    }

                    int spent;
                    path = MovementCostUtility.TrimPathToBudget(grid, path, allowed, out spent);
                    moveCost = spent;

                    if (path == null || path.Count == 0)
                    {
                        Debug.Log($"[PartyMovement] {member.Name} cannot reach target within movement budget");
                        continue;
                    }

                    remainingMovement -= moveCost;
                }
                else
                {
                    remainingMovement -= moveCost;
                }
            }

            // Queue this member's movement
            var movement = new MemberMovement
            {
                Member = member,
                Path = path,
                RemainingMovement = remainingMovement,
                IsMoving = false
            };

            currentMovements.Add(movement);

            Debug.Log($"[PartyMovement] {member.Name} (index {memberIndex}) -> destination {destination}");
        }

        if (currentMovements.Count == 0)
        {
            Debug.Log("[PartyMovement] No party members could move to target");
            return;
        }

        // Start all movements simultaneously
        isPartyMoving = true;
        foreach (var movement in currentMovements)
        {
            movement.IsMoving = true;
            movement.Member.mover.StartPath(movement.Path);
        }

        Debug.Log($"[PartyMovement] Started movement for {currentMovements.Count} party members to {targetCell}");
    }

    private void Update()
    {
        if (!isPartyMoving || currentMovements.Count == 0)
            return;

        // Check if all party members have finished moving
        bool anyMoving = false;
        foreach (var movement in currentMovements)
        {
            if (movement.Member.mover.IsMoving)
            {
                anyMoving = true;
                break;
            }
        }

        if (!anyMoving)
        {
            FinishPartyMovement();
        }
    }

    private void FinishPartyMovement()
    {
        // Update remaining movement for all members
        foreach (var movement in currentMovements)
        {
            if (movement.Member != null)
            {
                movement.Member.RemainingMovement = movement.RemainingMovement;
                movement.Member.HasMove = movement.RemainingMovement > 0;
            }
        }

        isPartyMoving = false;
        currentMovements.Clear();

        Debug.Log("[PartyMovement] All party members finished moving");
    }

    /// <summary>
    /// Gets all active player-controlled party members
    /// </summary>
    private List<BoxMover> GetActivePartyMembers()
    {
        List<BoxMover> members = new List<BoxMover>();

        if (PartyManager.Instance != null)
        {
            foreach (var member in PartyManager.Instance.Members)
            {
                if (member != null && !member.IsDead())
                {
                    members.Add(member as BoxMover);
                }
            }
        }

        return members;
    }

    public List<Vector3Int> GetPartyOccupiedCells()
    {
        List<Vector3Int> occupied = new List<Vector3Int>();
        List<BoxMover> members = GetActivePartyMembers();

        foreach (var member in members)
        {
            if (member != null && !member.IsDead())
            {
                Vector3Int cell = grid.WorldToGrid(member.GetWorldPosition());
                occupied.Add(cell);
            }
        }

        return occupied;
    }

    public void StopPartyMovement()
    {
        foreach (var movement in currentMovements)
        {
            if (movement.Member != null)
            {
                movement.Member.mover.Stop();
            }
        }

        isPartyMoving = false;
        currentMovements.Clear();
    }
}
