using UnityEngine;

public class GoldPileObstacle : MonoBehaviour
{
    [SerializeField] private int goldValue = 1;

    private GridController grid;
    private Vector3Int cell;
    private bool melted = false;

    public int GoldValue => goldValue;
    public bool IsMelted => melted;
    public Vector3Int Cell => cell;

    void Start()
    {
        grid = FindFirstObjectByType<GridController>();

        if (grid == null)
        {
            Debug.LogError("GoldPileObstacle could not find GridController");
            return;
        }

        cell = grid.WorldToGrid(transform.position);
        grid.RegisterGoldPile(cell, this);

        BossEncounterScoreManager scoreManager = FindFirstObjectByType<BossEncounterScoreManager>();
        if (scoreManager != null)
            scoreManager.RegisterGoldPile(goldValue);
    }

    public void Melt()
    {
        if (melted)
            return;

        melted = true;

        if (grid != null)
            grid.UnregisterGoldPile(cell);

        Debug.Log($"{name} melted at {cell}");

        BossEncounterScoreManager scoreManager = FindFirstObjectByType<BossEncounterScoreManager>();
        if (scoreManager != null)
            scoreManager.NotifyGoldDestroyed(goldValue);

        gameObject.SetActive(false);
    }
}
