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

    void Awake(){
      Instance = this;
      grid = FindFirstObjectByType<GridController>();
      targetingSystem = new TargetingSystem(grid);
      CurrentPhase = PlayerTurnPhase.WaitingForAction;
      Debug.Log("AbilityUI Awake() fired");
    }

    void Update(){
      if(CurrentPhase != PlayerTurnPhase.WaitingForAction)
          return;

      if(Keyboard.current.digit1Key.wasPressedThisFrame){
          selectedAbility = player.GetAbility(0);
          CurrentPhase = PlayerTurnPhase.WaitingForTarget;
          //player.ShowTargetingHighlights(selectedAbility);
          grid.HighlightEnemyTiles();
      }

      if(Keyboard.current.digit2Key.wasPressedThisFrame){
          selectedAbility = player.GetAbility(1);
          CurrentPhase = PlayerTurnPhase.WaitingForTarget;
          //player.ShowTargetingHighlights(selectedAbility);
          grid.HighlightEnemyTiles();
      }
      if(Keyboard.current.digit3Key.wasPressedThisFrame){
          selectedAbility = player.GetAbility(2);
          CurrentPhase = PlayerTurnPhase.WaitingForTarget;
        //  player.ShowTargetingHighlights(selectedAbility);

      }
      if(Keyboard.current.digit4Key.wasPressedThisFrame){
          selectedAbility = player.GetAbility(3);
          CurrentPhase = PlayerTurnPhase.WaitingForTarget;
          targetingSystem.HighlightValidTargets(selectedAbility, player);

      }
    }

    public void SelectAbility(int slot){

        // the following line is commented out because the context of player changed
        //var player = CombatManager.Instance.CurrentPlayer;
        selectedAbility = player.GetAbility(slot);
        if (selectedAbility == null){
          Debug.Log("No ability in that slot");
          return;
        }

        Debug.Log($"Selected ability: {selectedAbility.AbilityName}");
    }

    public void CancelAbility(){
        selectedAbility = null;
        Debug.Log("Ability canceled");
    }

    public void TryUseSelected(TargetData target){
        if(selectedAbility == null)
            return;

        Ability ability = selectedAbility;
        //var player = CombatManager.Instance.CurrentPlayer;
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
}
