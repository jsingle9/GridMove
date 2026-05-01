using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CombatUIManager : MonoBehaviour
{
    public static CombatUIManager Instance { get; private set; }

    [SerializeField] private GameObject combatUIPanel;
    [SerializeField] private TextMeshProUGUI turnIndicatorText;
    [SerializeField] private TextMeshProUGUI actionPointsText;
    [SerializeField] private TextMeshProUGUI bonusActionText;
    [SerializeField] private TextMeshProUGUI movementText;
    [SerializeField] private TextMeshProUGUI combatLogText;
    [SerializeField] private AbilityButtonUI[] abilityButtons;
    [SerializeField] private Button endTurnButton;

    private BoxMover currentPlayer;
    private int maxLogLines = 5;
    private string[] logBuffer;
    private int logIndex = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        logBuffer = new string[maxLogLines];

        if (combatUIPanel != null)
            combatUIPanel.SetActive(false);

        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(OnEndTurnClicked);
    }

    void Update()
    {
        if (!CombatManager.Instance.IsPlayerActive())
            return;

        UpdateResourceDisplay();
    }

    public void OnCombatStart()
    {
        if (combatUIPanel != null)
            combatUIPanel.SetActive(true);

        currentPlayer = FindFirstObjectByType<BoxMover>();
        RefreshCombatUI();
    }

    public void OnPlayerTurnStart()
    {
        if (turnIndicatorText != null)
        {
            turnIndicatorText.text = "YOUR TURN";
            turnIndicatorText.color = Color.green;
        }

        RefreshCombatUI();
        AddLog("Your turn started!");
    }

    public void OnEnemyTurnStart()
    {
        if (turnIndicatorText != null)
        {
            turnIndicatorText.text = "ENEMY TURN";
            turnIndicatorText.color = Color.red;
        }

        AddLog("Enemy's turn...");
    }

    public void OnCombatEnd()
    {
        if (combatUIPanel != null)
            combatUIPanel.SetActive(false);

        AddLog("Combat ended!");
    }

    void RefreshCombatUI()
    {
        if (currentPlayer == null)
            return;

        UpdateResourceDisplay();
        UpdateAbilityButtons();
    }

    void UpdateResourceDisplay()
    {
        if (currentPlayer == null)
            return;

        if (actionPointsText != null)
            actionPointsText.text = $"Action: {(currentPlayer.HasAction ? "✓" : "✗")}";

        if (bonusActionText != null)
            bonusActionText.text = $"Bonus: {(currentPlayer.HasBonusAction ? "✓" : "✗")}";

        if (movementText != null)
            movementText.text = $"Move: {currentPlayer.RemainingMovement}/{currentPlayer.Speed}";
    }

    void UpdateAbilityButtons()
    {
        if (abilityButtons == null || abilityButtons.Length == 0)
            return;

        for (int i = 0; i < abilityButtons.Length && i < 4; i++)
        {
            Ability ability = currentPlayer.GetAbility(i);
            abilityButtons[i].SetAbility(ability, i);
        }
    }

    void OnEndTurnClicked()
    {
        if (CombatManager.Instance.IsPlayerActive())
        {
            AddLog("Turn ended.");
            CombatManager.Instance.EndTurn();
        }
    }

    public void AddLog(string message)
    {
        logBuffer[logIndex] = message;
        logIndex = (logIndex + 1) % maxLogLines;

        UpdateLogDisplay();
    }

    void UpdateLogDisplay()
    {
        if (combatLogText == null)
            return;

        combatLogText.text = "";
        for (int i = 0; i < maxLogLines; i++)
        {
            if (logBuffer[i] != null)
                combatLogText.text += logBuffer[i] + "\n";
        }
    }

    public void OnAbilitySelected(int slot)
    {
        AddLog($"Selected: {currentPlayer.GetAbility(slot).AbilityName}");
    }
}
