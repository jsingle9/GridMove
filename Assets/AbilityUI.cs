using UnityEngine;
using UnityEngine.InputSystem;

public class AbilityUI : MonoBehaviour
{
    GridController grid;
    TargetingSystem targetingSystem;
    public static AbilityUI Instance;
    public PlayerTurnPhase CurrentPhase;
    public Ability selectedAbility;
    public BoxMover player;   // drag player object here in inspector
    private AOEVisualizer aoeVisualizer;
    private CombatActionUIDisplay actionUIDisplay;

    void Awake(){

      Instance = this;
      grid = FindFirstObjectByType<GridController>();
      targetingSystem = new TargetingSystem(grid);
      CurrentPhase = PlayerTurnPhase.WaitingForAction;
      Debug.Log("AbilityUI Awake() fired");

      aoeVisualizer = GetComponent<AOEVisualizer>();
      if(aoeVisualizer == null){
          GameObject aoeObj = new GameObject("AOEVisualizer");
          aoeObj.transform.parent = transform;
          aoeVisualizer = aoeObj.AddComponent<AOEVisualizer>();
      }

      actionUIDisplay = GetComponent<CombatActionUIDisplay>();
      if(actionUIDisplay == null)
      {
        Debug.LogError("CombatActionUIDisplay not found on AbilityUI! Make sure it's added as a component.");
        return;
      }
    }

    void Update(){

      // cancel ability input during targeting phase
      if(CurrentPhase == PlayerTurnPhase.WaitingForTarget &&
             Keyboard.current.escapeKey.wasPressedThisFrame)
      {
          CancelAbility();
          grid.ClearAllHighlights();
          CurrentPhase = PlayerTurnPhase.WaitingForAction;
          return;
      }

      if(CurrentPhase != PlayerTurnPhase.WaitingForAction)
          return;

      if(Keyboard.current.digit1Key.wasPressedThisFrame){
          selectedAbility = player.GetAbility(0);
          CurrentPhase = PlayerTurnPhase.WaitingForTarget;
          grid.HighlightEnemyTiles();
      }

      if(Keyboard.current.digit2Key.wasPressedThisFrame){
          selectedAbility = player.GetAbility(1);
          CurrentPhase = PlayerTurnPhase.WaitingForTarget;
          grid.HighlightEnemyTiles();
      }

      if(Keyboard.current.digit3Key.wasPressedThisFrame){
          selectedAbility = player.GetAbility(2);
          CurrentPhase = PlayerTurnPhase.WaitingForTarget;
          grid.HighlightEnemyTiles();
      }

      if(Keyboard.current.digit4Key.wasPressedThisFrame){
          selectedAbility = player.GetAbility(3);
          CurrentPhase = PlayerTurnPhase.WaitingForTarget;
          Debug.Log($"Selected: {selectedAbility.AbilityName}");
      }
    }

    public void SelectAbility(int slot){

        selectedAbility = player.GetAbility(slot);

        if (selectedAbility == null){
          Debug.Log("No ability in that slot");
          return;
        }

        if (selectedAbility != null && selectedAbility.targetingMode == TargetingMode.Area)
        {
            // Show AOE preview at player position
            Vector3Int playerGridPos = grid.WorldToGrid(player.GetWorldPosition());
            aoeVisualizer.DrawAOE(playerGridPos, selectedAbility.radius);
        }

        Debug.Log($"Selected ability: {selectedAbility.AbilityName}");
    }

    public void CancelAbility(){
        aoeVisualizer.HideAOE();
        selectedAbility = null;
        Debug.Log("Ability canceled");
    }

    public void TryUseSelected(TargetData target){
        if(selectedAbility == null)
            return;

        Ability ability = selectedAbility;
        if(ability == null){
                ability = player.GetAbility(0); // default attack
        }
        if(ability == null){
                Debug.LogError("No ability available");
                return;
        }
        grid.ClearAllHighlights();
        selectedAbility.TryUse(player, target);

        CurrentPhase = PlayerTurnPhase.WaitingForAction;
        selectedAbility = null;
    }

    public void OnPlayerTurnStart()
    {
        actionUIDisplay.UpdateActionDisplay(player);
        actionUIDisplay.ShowActionUI();
    }

    public void OnPlayerTurnEnd()
    {
        actionUIDisplay.HideActionUI();
    }
}
