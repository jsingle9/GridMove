using UnityEngine;
using System.Collections.Generic;

public class UnitMover : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    Vector3Int currentCell;
    List<GridNode> currentPath;
    int pathIndex;

    Vector3 targetPosition;
    bool isMoving;

    GridController grid;

    public bool IsMoving => isMoving;

    public void Initialize(GridController grid){
        this.grid = grid;
        targetPosition = transform.position;
        currentCell = grid.WorldToGrid(transform.position);
        grid.RegisterOccupant(currentCell, GetComponent<ICombatant>());
    }

    public void StartPath(List<GridNode> path){
      Debug.Log("STARTPATH CALLED. Path length: " + path.Count);
        if (path == null || path.Count == 0)
            return;

        currentPath = path;
        pathIndex = 0;
        SetNextTarget();
    }

    public void Tick(){
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime

        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f){
            transform.position = targetPosition;
            //Debug.Log("---- SNAP EVENT ----");
            //Debug.Log("Snapped world position: " + transform.position);
            //Debug.Log("Target position: " + targetPosition);

            Vector3Int newCell = grid.WorldToGrid(transform.position);
            //Debug.Log("WorldToGrid result: " + newCell);
            //Debug.Log("CurrentCell before update: " + currentCell);
            Debug.Log("--------------------");

            if(newCell != currentCell){
              grid.UnregisterOccupant(currentCell);
              grid.RegisterOccupant(newCell, GetComponent<ICombatant>());
              currentCell = newCell;
            }
            pathIndex++;

            if (pathIndex >= currentPath.Count){
                isMoving = false;
                currentPath = null;
                return;
            }
            else{
                SetNextTarget();
            }
        }

    }

    public void Stop(){
      isMoving = false;
      currentPath = null;
      Vector3Int newCell = grid.WorldToGrid(transform.position);

      if(newCell != currentCell){
        grid.UnregisterOccupant(currentCell);
        grid.RegisterOccupant(newCell, GetComponent<ICombatant>());
        currentCell = newCell;
      }
    }

    void SetNextTarget(){
        GridNode nextNode = currentPath[pathIndex];
        targetPosition = grid.GridToWorld(nextNode.gridPos);
        isMoving = true;
        Debug.Log("Setting next target to: " + currentPath[pathIndex]);
    }
}
