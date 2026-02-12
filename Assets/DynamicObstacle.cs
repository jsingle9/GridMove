using UnityEngine;

public class DynamicObstacle : MonoBehaviour
{
    [SerializeField] GridController gridController;

    Vector3Int currentCell;

    void Start()
    {
        currentCell = gridController.WorldToGrid(transform.position);
        gridController.SetWalkable(currentCell, false);
    }

    public void UpdateCell(Vector3 newWorldPos)
    {
        Vector3Int newCell = gridController.WorldToGrid(newWorldPos);

        if (newCell == currentCell)
            return;

        // free old cell
        gridController.SetWalkable(currentCell, true);

        // block new cell
        gridController.SetWalkable(newCell, false);

        currentCell = newCell;
    }

    void OnDestroy()
    {
        gridController.SetWalkable(currentCell, true);
    }
}
