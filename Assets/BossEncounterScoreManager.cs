using UnityEngine;

public class BossEncounterScoreManager : MonoBehaviour
{
    private int totalGold = 0;
    private int destroyedGold = 0;
    private bool scoreFinalized = false;

    [Header("Optional Direct Reference (falls back to singleton)")]
    [SerializeField] private BossVictoryPanelUI victoryPanel;

    public void RegisterGoldPile(int value)
    {
        totalGold += value;
    }

    public void NotifyGoldDestroyed(int value)
    {
        destroyedGold += value;
        Debug.Log($"Gold destroyed: {destroyedGold}/{totalGold}");
    }

    public int GetGoldRemaining()
    {
        return totalGold - destroyedGold;
    }

    public int GetTotalGold()
    {
        return totalGold;
    }

    public bool IsScoreFinalized()
    {
        return scoreFinalized;
    }

    // Call this when drake dies
    public void FinalizeBossEncounterScore()
    {
        if (scoreFinalized)
            return;

        scoreFinalized = true;

        int remaining = GetGoldRemaining();
        int total = GetTotalGold();

        Debug.Log($"Boss defeated! Gold preserved: {remaining}/{total}");
        CombatUIManager.Instance?.AddLog($"Boss defeated! Gold preserved: {remaining}/{total}");

        // Prefer inspector reference, fallback to singleton
        if (victoryPanel == null)
            victoryPanel = BossVictoryPanelUI.Instance;

        if (victoryPanel != null)
        {
            victoryPanel.ShowVictory(remaining, total);
            Debug.Log("[BossEncounterScoreManager] Victory panel shown.");
        }
        else
        {
            Debug.LogError("[BossEncounterScoreManager] No BossVictoryPanelUI found. Add it to scene or assign reference.");
        }
    }

    // Optional: call this when encounter is reset/restarted
    public void ResetEncounterScore()
    {
        totalGold = 0;
        destroyedGold = 0;
        scoreFinalized = false;
    }
}
