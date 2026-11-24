using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PartyMovementController : MonoBehaviour
{
    public static PartyMovementController Instance { get; private set; }
    public bool IsPartyMovingTogether => false; // Placeholder for future use

    private class MemberTrail
    {
        public BoxMover Member;
        public Queue<Vector3Int> PositionHistory = new Queue<Vector3Int>();
    }

    private List<MemberTrail> trails = new List<MemberTrail>();
    private GridController grid;
    private const int TRAIL_LENGTH = 3; // Leader + 2 followers

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
    }

    public void InitializeParty(List<BoxMover> members)
    {
        trails.Clear();

        foreach (var member in members)
        {
            if (member != null)
            {
                trails.Add(new MemberTrail
                {
                    Member = member,
                    PositionHistory = new Queue<Vector3Int>()
                });
            }
        }

        Debug.Log($"[PartyMovementController] Initialized {trails.Count} members");
    }

    private void Update()
    {
        if (trails.Count == 0 || grid == null)
            return;

        // Only update during exploration
        if (GameStateManager.Instance != null &&
            GameStateManager.Instance.CurrentGameState == GameState.Combat)
            return;

        UpdateFormation();
    }

    private void UpdateFormation()
    {
        if (trails.Count == 0)
            return;

        // Leader is always the active party member
        BoxMover leader = PartyManager.Instance?.ActiveMember;
        if (leader == null)
            return;

        Vector3Int leaderCell = grid.WorldToGrid(leader.GetWorldPosition());

        // Record leader's current position
        MemberTrail leaderTrail = trails.FirstOrDefault(t => t.Member == leader);
        if (leaderTrail != null)
        {
            leaderTrail.PositionHistory.Enqueue(leaderCell);
            if (leaderTrail.PositionHistory.Count > TRAIL_LENGTH)
                leaderTrail.PositionHistory.Dequeue();
        }

        // Move followers to previous leader positions
        for (int i = 1; i < trails.Count; i++)
        {
            MemberTrail followerTrail = trails[i];

            // Look back in leader's history for where this follower should be
            if (leaderTrail?.PositionHistory.Count > i)
            {
                Vector3Int targetCell = leaderTrail.PositionHistory
                    .Skip(leaderTrail.PositionHistory.Count - i - 1)
                    .First();

                Vector3 targetWorld = grid.GridToWorld(targetCell);

                // Teleport follower to follow position
                followerTrail.Member.transform.position = targetWorld;

                // Update grid occupancy
                grid.TryRelocateCombatant(followerTrail.Member, targetCell);

                Debug.Log($"[Formation] {followerTrail.Member.Name} moved to {targetCell}");
            }
        }
    }

    public List<Vector3Int> GetPartyOccupiedCells()
    {
        List<Vector3Int> occupied = new List<Vector3Int>();

        foreach (var trail in trails)
        {
            if (trail.Member != null && !trail.Member.IsDead())
            {
                Vector3Int cell = grid.WorldToGrid(trail.Member.GetWorldPosition());
                occupied.Add(cell);
            }
        }

        return occupied;
    }
}
