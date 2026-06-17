using UnityEngine;

public class BossEncounterScoreManager : MonoBehaviour
{
    private int totalGold = 0;
    private int destroyedGold = 0;

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
}
