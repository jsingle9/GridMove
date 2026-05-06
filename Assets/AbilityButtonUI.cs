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
        player = FindFirstObjectByType<BoxMover>();

        if (button != null)
            button.onClick.AddListener(OnButtonClicked);
    }

    public void SetAbility(Ability ability, int slot)
    {
        currentAbility = ability;
        abilitySlot = slot;

        if (ability != null)
        {
            if (abilityNameText != null)
                abilityNameText.text = $"[{slot + 1}] {ability.AbilityName}";

            if (costText != null)
                costText.text = ability.CostType.ToString();

            // Grey out if can't use
            if (button != null)
                button.interactable = ability.CanUse(player);
        }
        else
        {
            if (abilityNameText != null)
                abilityNameText.text = $"[{slot + 1}] Empty";

            if (costText != null)
                costText.text = "";

            if (button != null)
                button.interactable = false;
        }
    }

    public void OnButtonClicked()
    {
        if (currentAbility == null)
            return;

            AbilityUI.Instance.SelectAbility(abilitySlot);
            AbilityUI.Instance.CurrentPhase = PlayerTurnPhase.WaitingForTarget;
            GridController grid = FindFirstObjectByType<GridController>();
            if(grid != null)
              grid.HighlightEnemyTiles();
    }
}
