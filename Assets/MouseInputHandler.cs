using UnityEngine;
using UnityEngine.InputSystem;

public class MouseInputHandler : MonoBehaviour
{
    private BoxMover boxMover;
    private GridController grid;
    private TargetingSystem targetingSystem;

    private void Start()
    {
        grid = FindFirstObjectByType<GridController>();
        if (grid == null)
        {
            //grid = FindFirstObjectByType<GridController>();
            Debug.Log($"GridController found: {grid}");
        }

        targetingSystem = null;
        //if (targetingSystem == null)
        /*{
            //targetingSystem = FindFirstObjectByType<TargetingSystem>();
            Debug.Log($"TargetingSystem found: {targetingSystem}");
        }*/

        if (PartyManager.Instance != null)
        {
            boxMover = PartyManager.Instance.ActiveMember;
            Debug.Log($"boxMover set to: {boxMover}");
            PartyManager.Instance.OnPartyMemberChanged += OnPartyMemberChanged;
        }
        else
        {
            Debug.LogError("PartyManager.Instance is NULL!");
        }
    }

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            // Only allow party member switching during combat if it's a player turn
            // (the system will handle advancing turns properly through initiative)
            if (PartyManager.Instance != null && PartyManager.Instance.Members.Count > 0)
            {
                // Check if currently in combat during a player turn
                bool inCombat = GameStateManager.Instance?.CurrentGameState == GameState.Combat;
                bool isPlayerTurn = inCombat && CombatManager.Instance != null && CombatManager.Instance.IsPlayerActive();
                
                if (isPlayerTurn)
                {
                    Debug.Log("Cannot switch party members during combat. Each party member has their own turn in initiative order.");
                }
                else
                {
                    // Allow switching in exploration or between turns
                    int nextIndex = (PartyManager.Instance.GetActiveIndex() + 1) % PartyManager.Instance.Members.Count;
                    PartyManager.Instance.SetActiveByIndex(nextIndex);
                    Debug.Log($"Switched to party member {nextIndex}");
                }
            }
        }
        // Show AOE preview on hover (only during targeting phase)
        if (GameStateManager.Instance.CurrentGameState == GameState.Combat &&
            CombatManager.Instance.IsPlayersTurn(boxMover) &&
            AbilityUI.Instance.CurrentPhase == PlayerTurnPhase.WaitingForTarget &&
            AbilityUI.Instance.selectedAbility != null &&
            targetingSystem != null)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mouseWorld.z = 0;
            Vector3Int gridPos = grid.WorldToGrid(mouseWorld);

            // Show AOE preview if ability has radius
            if (AbilityUI.Instance.selectedAbility.radius > 0)
            {
                targetingSystem.ShowAOEPreview(
                    AbilityUI.Instance.selectedAbility,
                    gridPos
                );
            }
        }

        // Handle left click (original functionality)
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (boxMover == null)
        {
            Debug.LogWarning("boxMover is null, cannot handle click");
            return;
        }

        if (GameStateManager.Instance.CurrentGameState != GameState.FreeExplore &&
            GameStateManager.Instance.CurrentGameState != GameState.Combat)
            return;

        boxMover.HandleLeftClick();
    }

    private void OnDestroy()
    {
        if (PartyManager.Instance != null)
        {
            PartyManager.Instance.OnPartyMemberChanged -= OnPartyMemberChanged;
        }
    }

    private void OnPartyMemberChanged(BoxMover newMember)
    {
        boxMover = newMember;
    }

}
