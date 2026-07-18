using UnityEngine;

public class DynamicObstacle : MonoBehaviour
{
    [SerializeField] GridController gridController;

    Vector3Int currentCell;

    void Start(){
        if(gridController == null)
              gridController = FindObjectOfType<GridController>();

          if(gridController == null)
          {
              Debug.LogError("NO GRID CONTROLLER FOUND for " + gameObject.name);
              return;
          }
        RegisterAtCurrentPosition();
        //currentCell = gridController.WorldToGrid(transform.position);
        //gridController.SetWalkable(currentCell, false);
    }

    // this will fire when the dynamic obstacle moves.
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

    public void RegisterAtCurrentPosition(){
        currentCell = gridController.WorldToGrid(transform.position);
        gridController.SetWalkable(currentCell, false);
    }

    void OnDestroy()
    {
        // Scene unload / domain reload safety
        if (gridController == null || gridController.grid == null)
            return;

        // Optional: also ensure lastCell was initialized if you track that
        // if (!initialized) return;

        gridController.SetWalkable(currentCell, true);
    }

}
