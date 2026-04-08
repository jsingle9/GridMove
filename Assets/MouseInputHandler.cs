using UnityEngine;
using UnityEngine.InputSystem;

public class MouseInputHandler : MonoBehaviour
{
    public BoxMover boxMover;
    private GridController grid;
    private TargetingSystem targetingSystem;

    void Start()
    {
        grid = FindFirstObjectByType<GridController>();
        if (grid != null)
        {
            targetingSystem = new TargetingSystem(grid);
        }
    }

    void Update()
    {
        // Show AOE preview on hover (only during targeting phase)
        if (GameStateManager.Instance.CurrentState == GameState.Combat &&
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

        if (GameStateManager.Instance.CurrentState != GameState.FreeExplore &&
            GameStateManager.Instance.CurrentState != GameState.Combat)
            return;

        boxMover.HandleLeftClick();
    }
}
