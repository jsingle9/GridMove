using UnityEngine;

public class DifficultTerrain : MonoBehaviour
{
    [SerializeField] private int movementCost = 2;

    private GridController grid;
    private Vector3Int cell;

    public int MovementCost => movementCost;
    public Vector3Int Cell => cell;

    void Start()
    {
        grid = FindFirstObjectByType<GridController>();
        if (grid == null)
        {
            Debug.LogError("DifficultTerrain could not find GridController");
            return;
        }

        cell = grid.WorldToGrid(transform.position);
        grid.RegisterDifficultTerrain(cell, this);
    }

    void OnDisable()
    {
        if (grid != null)
            grid.UnregisterDifficultTerrain(cell);
    }
}
