using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

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

    [SerializeField] private int maxLogLines = 8;
    private readonly Queue<string> logQueue = new Queue<string>();

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
        if (combatUIPanel != null)
            combatUIPanel.SetActive(false);

        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(OnEndTurnClicked);
    }

    void Update()
    {
        if (CombatManager.Instance == null || !CombatManager.Instance.IsPlayerActive())
            return;

        UpdateResourceDisplay();
        UpdateAbilityButtons();
    }

    public void OnCombatStart()
    {
        if (combatUIPanel != null)
            combatUIPanel.SetActive(true);

        currentPlayer = FindFirstObjectByType<BoxMover>();
        RefreshCombatUI();
        AddLog("Combat begins!");
    }

    public void OnPlayerTurnStart()
    {
        if (turnIndicatorText != null)
        {
            turnIndicatorText.text = "YOUR TURN";
            turnIndicatorText.color = Color.green;
        }

        RefreshCombatUI();
    }

    public void OnEnemyTurnStart()
    {
        if (turnIndicatorText != null)
        {
            turnIndicatorText.text = "ENEMY TURN";
            turnIndicatorText.color = Color.red;
        }
    }

    public void OnCombatEnd()
    {
        AddLog("Combat ended!");

        if (combatUIPanel != null)
            combatUIPanel.SetActive(false);
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
        if (currentPlayer == null || abilityButtons == null || abilityButtons.Length == 0)
            return;

        for (int i = 0; i < abilityButtons.Length && i < 4; i++)
        {
            Ability ability = currentPlayer.GetAbility(i);
            abilityButtons[i].SetAbility(ability, i);
        }
    }

    void OnEndTurnClicked()
    {
        if (CombatManager.Instance != null && CombatManager.Instance.IsPlayerActive())
        {
            AddLog($"{currentPlayer.name} ends turn.");
            CombatManager.Instance.EndTurn();
        }
    }

    public void AddLog(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        logQueue.Enqueue(message);

        while (logQueue.Count > maxLogLines)
            logQueue.Dequeue();

        UpdateLogDisplay();
    }

    void UpdateLogDisplay()
    {
        if (combatLogText == null)
            return;

        combatLogText.text = string.Join("\n", logQueue);
    }

    public void OnAbilitySelected(int slot)
    {
        if (currentPlayer == null) return;

        Ability selected = currentPlayer.GetAbility(slot);
        if (selected != null)
            AddLog($"Selected: {selected.AbilityName}");
    }

    // --- New helper methods for richer combat logging ---

    public void LogTurnStart(string unitName)
    {
        AddLog($"-- {unitName}'s turn starts --");
    }

    public void LogTurnEnd(string unitName)
    {
        AddLog($"-- {unitName}'s turn ends --");
    }

    public void LogAttack(
        string attacker,
        string target,
        bool hit,
        int roll,
        int total,
        int targetAC,
        int damage = 0)
    {
        if (hit)
            AddLog($"{attacker} attacks {target}. Hit! ({roll} -> {total} vs AC {targetAC}) {target} takes {damage} damage.");
        else
            AddLog($"{attacker} attacks {target}. Miss! ({roll} -> {total} vs AC {targetAC})");
    }
    
    public void LogAbilityDamage(string source, string abilityName, string target, int damage, string damageType = "")
    {
        string dtype = string.IsNullOrWhiteSpace(damageType) ? "" : $" {damageType}";
        AddLog($"{source} uses {abilityName} on {target}. {target} takes {damage}{dtype} damage.");
    }

}
