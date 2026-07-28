using UnityEngine;

public class BossEncounterScoreManager : MonoBehaviour
{
    private int totalGold = 0;
    private int destroyedGold = 0;
    private bool scoreFinalized = false;

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

        // Optional UI feedback
        CombatUIManager.Instance?.AddLog($"Boss defeated! Gold preserved: {remaining}/{total}");

        // TODO: Hook into your broader progression/score system here if you have one:
        // ScoreManager.Instance?.AddGoldScore(remaining);
    }

    // Optional: call this when encounter is reset/restarted
    public void ResetEncounterScore()
    {
        totalGold = 0;
        destroyedGold = 0;
        scoreFinalized = false;
    }
}
