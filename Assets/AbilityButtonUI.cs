using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private int abilitySlot;

    private Ability currentAbility;
    private BoxMover player;

    void Awake()
    {
        // Determine which button this is (0-3)
        Transform parent = transform.parent;
        for(int i = 0; i < parent.childCount; i++)
        {
            if(parent.GetChild(i) == transform)
            {
                abilitySlot = i;
                break;
            }
        }
    }


    void Start()
    {
        player = AbilityUI.Instance != null ? AbilityUI.Instance.player : null;

        if(button != null)
            button.onClick.AddListener(OnButtonClicked);
    }

    public void SetAbility(Ability ability, int slot)
    {
        player = AbilityUI.Instance != null ? AbilityUI.Instance.player : null;

        currentAbility = ability;
        abilitySlot = slot;

        if (ability != null)
        {
            if(abilityNameText != null)
                abilityNameText.text = $"[{slot + 1}] {ability.AbilityName}";

            if(costText != null)
                costText.text = ability.CostType.ToString();

            if(button != null)
                button.interactable = player != null && ability.CanUse(player);
        }
        else
        {
            if(abilityNameText != null)
                abilityNameText.text = $"[{slot + 1}] Empty";

            if(costText != null)
                costText.text = "";

            if(button != null)
                button.interactable = false;
        }
    }

    public void OnButtonClicked()
    {
        Debug.Log($"Button clicked. slot={abilitySlot}, ability={(currentAbility != null ? currentAbility.AbilityName : "null")}");

        if(currentAbility == null)
            return;

        AbilityUI.Instance.SelectAbility(abilitySlot);
        AbilityUI.Instance.CurrentPhase = PlayerTurnPhase.WaitingForTarget;

        GridController grid = FindFirstObjectByType<GridController>();
        if(grid == null)
            return;

        grid.ClearAllHighlights();

        if(currentAbility.targetingMode != TargetingMode.Area)
        {
            grid.HighlightEnemyTiles();
        }
    }
}
