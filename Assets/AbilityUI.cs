using UnityEngine;
using UnityEngine.InputSystem;

public class AbilityUI : MonoBehaviour
{
    private GridController grid;
    private TargetingSystem targetingSystem;

    public static AbilityUI Instance;

    public PlayerTurnPhase CurrentPhase;
    public Ability selectedAbility;
    public BoxMover player;

    private AOEVisualizer aoeVisualizer;

    [SerializeField] private AbilityButtonUI[] abilityButtons;

    private void Awake()
    {
        Instance = this;

        grid = FindFirstObjectByType<GridController>();
        targetingSystem = new TargetingSystem(grid);

        CurrentPhase = PlayerTurnPhase.WaitingForAction;

        Debug.Log("AbilityUI Awake() fired");

        aoeVisualizer = GetComponent<AOEVisualizer>();

        if (aoeVisualizer == null)
        {
            GameObject aoeObject = new GameObject("AOEVisualizer");
            aoeObject.transform.SetParent(transform);
            aoeVisualizer = aoeObject.AddComponent<AOEVisualizer>();
        }
    }

    private void Start()
    {
        RefreshAbilityButtons();
    }

    private void Update()
    {
        // Cancel targeting with Q.
        if (CurrentPhase == PlayerTurnPhase.WaitingForTarget &&
            Keyboard.current.qKey.wasPressedThisFrame)
        {
            CancelAbility();
            grid?.ClearAllHighlights();
            CurrentPhase = PlayerTurnPhase.WaitingForAction;
            return;
        }

        if (CurrentPhase != PlayerTurnPhase.WaitingForAction)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            TryActivateAbilitySlot(0, "KEYBIND");
            return;
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            TryActivateAbilitySlot(1, "KEYBIND");
            return;
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            TryActivateAbilitySlot(2, "KEYBIND");
            return;
        }

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            TryActivateAbilitySlot(3, "KEYBIND");
            return;
        }

        if (Keyboard.current.digit5Key.wasPressedThisFrame)
        {
            TryActivateAbilitySlot(4, "KEYBIND");
        }
    }

    public void SelectAbility(int slot)
    {
        if (player == null)
        {
            Debug.LogWarning("SelectAbility: player is null.");
            return;
        }

        selectedAbility = player.GetAbility(slot);

        if (selectedAbility == null)
        {
            Debug.Log($"No ability in slot {slot}");
            return;
        }

        Debug.Log($"Selected ability: {selectedAbility.AbilityName}");
    }

    public void CancelAbility()
    {
        aoeVisualizer?.HideAOE();

        selectedAbility = null;

        Debug.Log("Ability canceled");
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
            // Area-target preview belongs here when enabled.
            return;
        }

        /*
         * Attack range now depends on the currently equipped weapon:
         *
         * - Melee weapon: remaining movement + 1 tile.
         * - Ranged weapon: equipped weapon range.
         *
         * BoxMover/IntentExecutor still perform the authoritative
         * pathfinding, movement-cost, range, and line-of-sight checks
         * after the target is clicked.
         */
        if (selectedAbility is AttackAbility attackAbility)
        {
            attackAbility.RefreshTargetingRange(player);
        }

        targetingSystem.HighlightValidTargets(
            selectedAbility,
            player
        );
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

        grid?.ClearAllHighlights();

        Debug.Log(
            $"[ABILITY ACTIVATE] source={sourceTag} " +
            $"slot={slot} " +
            $"ability={ability.AbilityName} " +
            $"mode={ability.targetingMode}"
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

            AbilityResult result = ability.TryUse(
                player,
                selfTarget
            );

            if (!result.Success)
            {
                Debug.Log(
                    $"Ability failed: {result.FailureReason}"
                );
            }

            CurrentPhase = PlayerTurnPhase.WaitingForAction;
            selectedAbility = null;

            return;
        }

        CurrentPhase = PlayerTurnPhase.WaitingForTarget;

        BeginTargetingForSelectedAbility(sourceTag);
    }

    public void RefreshAbilityButtons()
    {
        if (player == null || abilityButtons == null)
            return;

        for (int i = 0; i < abilityButtons.Length; i++)
        {
            Ability ability = player.GetAbility(i);

            abilityButtons[i].gameObject.SetActive(
                ability != null
            );

            if (ability != null)
            {
                abilityButtons[i].SetAbility(ability, i);
            }
        }
    }
}
