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
    [SerializeField] private AbilityButtonUI[] abilityButtons;

    void Awake(){

      Instance = this;
      grid = FindFirstObjectByType<GridController>();
      targetingSystem = new TargetingSystem(grid);
      CurrentPhase = PlayerTurnPhase.WaitingForAction;
      Debug.Log("AbilityUI Awake() fired");

      aoeVisualizer = GetComponent<AOEVisualizer>();
      if (aoeVisualizer == null){
          GameObject aoeObj = new GameObject("AOEVisualizer");
          aoeObj.transform.parent = transform;
          aoeVisualizer = aoeObj.AddComponent<AOEVisualizer>();
      }
    }

    void Start(){
      RefreshAbilityButtons();
    }

    void Update(){


      // NEW: Add cancel ability input during targeting phase
      if(CurrentPhase == PlayerTurnPhase.WaitingForTarget &&
             Keyboard.current.qKey.wasPressedThisFrame)
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
          BeginTargetingForSelectedAbility("KEYBIND");
          //grid.HighlightEnemyTiles();
      }

      if(Keyboard.current.digit2Key.wasPressedThisFrame){
          selectedAbility = player.GetAbility(1);
          CurrentPhase = PlayerTurnPhase.WaitingForTarget;
          BeginTargetingForSelectedAbility("KEYBIND");
          //grid.HighlightEnemyTiles();
      }
      if(Keyboard.current.digit3Key.wasPressedThisFrame){
          selectedAbility = player.GetAbility(2);
          CurrentPhase = PlayerTurnPhase.WaitingForTarget;
          BeginTargetingForSelectedAbility("KEYBIND");
        //  player.ShowTargetingHighlights(selectedAbility);

      }
      if(Keyboard.current.digit4Key.wasPressedThisFrame){
          selectedAbility = player.GetAbility(3);
          CurrentPhase = PlayerTurnPhase.WaitingForTarget;
          Debug.Log($"Selected: {selectedAbility.AbilityName}");

      }
    }

    public void SelectAbility(int slot){

        // the following line is commented out because the context of player changed
        //var player = CombatManager.Instance.CurrentPlayer;
        Debug.Log("SelectAbilityFired");
        selectedAbility = player.GetAbility(slot);

        if (selectedAbility == null){
          Debug.Log("No ability in that slot");
          return;
        }

        if (selectedAbility != null && selectedAbility.targetingMode == TargetingMode.Area)
        {
            // Show AOE preview at player position
            Vector3Int playerGridPos = grid.WorldToGrid(player.GetWorldPosition());
            //aoeVisualizer.DrawAOE(playerGridPos, selectedAbility.radius);
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
        //var player = CombatManager.Instance.CurrentPlayer;
        if(ability == null){
                ability = player.GetAbility(0); // default attack
        }
        if(ability == null){
                Debug.LogError("No ability available");
                return;
        }

        CombatEvents.Log($"{player.Name} uses {ability.AbilityName}.");

        grid.ClearAllHighlights();
        AbilityResult result = selectedAbility.TryUse(player, target);

        CombatEvents.Log(result.Success
            ? $"{ability.AbilityName} resolved successfully."
            : $"{ability.AbilityName} failed: {result.FailureReason}");

        CurrentPhase = PlayerTurnPhase.WaitingForAction;
        selectedAbility = null;
    }

    public void BeginTargetingForSelectedAbility(string sourceTag)
    {
        if (selectedAbility == null || player == null || grid == null)
            return;

        grid.ClearAllHighlights();

        Debug.Log(
            $"[ABILITY ENTRY] source={sourceTag} " +
            $"abilityName={selectedAbility.AbilityName} " +
            $"abilityType={selectedAbility.GetType().Name} " +
            $"targetingMode={selectedAbility.targetingMode} " +
            $"user={player.Name}"
        );

        if (selectedAbility.targetingMode == TargetingMode.Area)
        {
            // Let your AOE preview/template system handle visuals.
            // Example if needed:
            // Vector3Int p = grid.WorldToGrid(player.GetWorldPosition());
            // aoeVisualizer.DrawAOE(p, selectedAbility.radius);
            return;
        }

        targetingSystem.HighlightValidTargets(selectedAbility, player);
    }

    public void TryActivateAbilitySlot(int slot, string sourceTag)
    {
        if (player == null)
        {
            Debug.LogWarning("TryActivateAbilitySlot: player is null.");
            return;
        }

        Ability ability = player.GetAbility(slot);
        if (ability == null)
        {
            Debug.Log($"No ability in slot {slot}");
            return;
        }

        if (!ability.CanUse(player))
        {
            Debug.Log($"{ability.AbilityName} cannot be used right now.");
            return;
        }

        selectedAbility = ability;

        if (grid != null)
            grid.ClearAllHighlights();

        Debug.Log(
            $"[ABILITY ACTIVATE] source={sourceTag} " +
            $"slot={slot} ability={ability.AbilityName} mode={ability.targetingMode}"
        );

        bool selfCast =
            ability.targetingMode == TargetingMode.Self ||
            ability.Range <= 0f;

        if (selfCast)
        {
            TargetData selfTarget = new TargetData
            {
                primaryTarget = player,
                user = player
            };
            selfTarget.unitsInArea.Add(player);

            AbilityResult result = ability.TryUse(player, selfTarget);

            if (!result.Success)
                Debug.Log($"Ability failed: {result.FailureReason}");

            CurrentPhase = PlayerTurnPhase.WaitingForAction;
            selectedAbility = null;
            return;
        }

        CurrentPhase = PlayerTurnPhase.WaitingForTarget;
        BeginTargetingForSelectedAbility(sourceTag);
    }

    public void RefreshAbilityButtons()
    {
        if (player == null || abilityButtons == null) return;

        for (int i = 0; i < abilityButtons.Length; i++)
        {
            Ability a = player.GetAbility(i);
            abilityButtons[i].SetAbility(a, i);
        }
    }
}
