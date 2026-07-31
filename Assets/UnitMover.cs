using UnityEngine;
using System.Collections.Generic;

public class UnitMover : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    Vector3Int currentCell;
    List<GridNode> currentPath;
    int pathIndex;
    private int _lastTickFrame = -1;
    Vector3 targetPosition;
    bool isMoving;

    GridController grid;

    public bool IsMoving => isMoving;

    public void Initialize(GridController grid){
        this.grid = grid;
        targetPosition = transform.position;
        currentCell = grid.WorldToGrid(transform.position);

        ICombatant combatant = GetComponent<ICombatant>();
        if(combatant != null)
        {
            grid.RegisterCombatant(combatant);
        }
    }

    public void StartPath(List<GridNode> path){
        if (path == null || path.Count == 0)
            return;

        currentPath = path;
        isMoving = false;

        Vector3Int myCell = grid.WorldToGrid(transform.position);

        // Find first node that is actually different from current cell
        pathIndex = 0;
        while (pathIndex < currentPath.Count && currentPath[pathIndex].gridPos == myCell)
            pathIndex++;

        if (pathIndex >= currentPath.Count)
        {
            // No actual displacement
            currentPath = null;
            return;
        }

        SetNextTarget();

        // If first target resolves to same world position, abort safely
        if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            isMoving = false;
            currentPath = null;
            return;
        }

        Debug.Log($"[UnitMover] {name} start move: from={myCell} toFirst={currentPath[pathIndex].gridPos} pathCount={currentPath.Count}");
    }

    public void Tick(){
      // Guard: prevent double-tick in same frame
      if (_lastTickFrame == Time.frameCount)
          return;
      _lastTickFrame = Time.frameCount;

      if (!isMoving || currentPath == null || pathIndex >= currentPath.Count)
          return;

      if (!isMoving)
          return;

        Debug.Log($"[UnitMover.Tick] {name} pos={transform.position} target={targetPosition} speed={moveSpeed}");
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

            if(newCell != currentCell)
            {
                ICombatant combatant = GetComponent<ICombatant>();
                if(combatant != null)
                {
                    // IMPORTANT: clear old tile explicitly
                    grid.UnregisterOccupant(currentCell);

                    // update tracked cell
                    currentCell = newCell;

                    // then occupy new tile explicitly
                    grid.RegisterOccupant(currentCell, combatant);
                }
                else
                {
                    currentCell = newCell;
                }
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
          ICombatant combatant = GetComponent<ICombatant>();
          if(combatant != null){
              Debug.Log($"LEAVE {currentCell}: occupiedBefore={grid.IsOccupied(currentCell)} walkableBefore={grid.IsWalkable(currentCell)}");
              grid.UnregisterCombatant(combatant);
              Debug.Log($"LEAVE {currentCell}: occupiedAfter={grid.IsOccupied(currentCell)} walkableAfter={grid.IsWalkable(currentCell)}");
              currentCell = newCell;
              grid.RegisterCombatant(combatant);
          }
          else{
              currentCell = newCell;
          }
      }
    }

    void SetNextTarget(){
        GridNode nextNode = currentPath[pathIndex];
        targetPosition = grid.GridToWorld(nextNode.gridPos);
        isMoving = true;
        Debug.Log("Setting next target to: " + currentPath[pathIndex]);
    }

    private void Update()
    {
        Tick();
    }
}
