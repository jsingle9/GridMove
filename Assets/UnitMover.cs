using UnityEngine;
using System.Collections.Generic;

public class UnitMover : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;

    List<GridNode> currentPath;
    int pathIndex;

    Vector3 targetPosition;
    bool isMoving;

    GridController grid;

    public bool IsMoving => isMoving;

    public void Initialize(GridController grid){
        this.grid = grid;
        targetPosition = transform.position;
    }

    public void StartPath(List<GridNode> path){
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
            pathIndex++;

            if (pathIndex >= currentPath.Count){
                isMoving = false;
                currentPath = null;
            }
            else{
                SetNextTarget();
            }
        }
    }

    public void Stop(){
      isMoving = false;
      currentPath = null;
    }

    void SetNextTarget(){
        GridNode nextNode = currentPath[pathIndex];
        targetPosition = grid.GridToWorld(nextNode.gridPos);
        isMoving = true;
    }
}
