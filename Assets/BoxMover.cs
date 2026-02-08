using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class BoxMover : MonoBehaviour{
    [SerializeField] GridController grid;
    [SerializeField] float moveSpeed = 5f;
    IntentResolver resolver;
    Intent currentIntent;

    Pathfinder pathfinder;

    List<GridNode> currentPath;
    int pathIndex;

    Vector3 targetPosition;
    bool isMoving;

    void Start()
    {
        resolver = new IntentResolver(grid);
        if (grid == null)
        {
            Debug.LogError("BoxMover has no GridController assigned!", this);
            return;
        }

        pathfinder = new Pathfinder(grid);
        targetPosition = transform.position;
    }

    void Update()
    {
        HandleInput();
        HandleMovement();
    }

    void HandleInput(){
      // Guard clause: bail out early
      if (!Mouse.current.leftButton.wasPressedThisFrame || isMoving)
      return;

      if (Camera.main == null)
      return;

      Enemy enemy = GetClickedEnemy();

      if (enemy != null){
        currentIntent = new AttackIntent(enemy);
      }
      else{
        Vector3 worldClick = GetMouseWorld();
        Vector3Int gridPos = grid.WorldToGrid(worldClick);

        if (!grid.IsWalkable(gridPos))
          return;

        currentIntent = new MoveIntent(gridPos);
      }

      ResolveIntent();
    }

    Vector3 GetMouseWorld()
    {
      if (Camera.main == null)
          return Vector3.zero;

      Vector3 mousePos = Mouse.current.position.ReadValue();
      mousePos.z = -Camera.main.transform.position.z;

      Vector3 world = Camera.main.ScreenToWorldPoint(mousePos);
      world.z = 0;

      return world;
    }


  void HandleMovement(){
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

  void SetNextTarget(){
        GridNode nextNode = currentPath[pathIndex];
        targetPosition = grid.GridToWorld(nextNode.gridPos);
        isMoving = true;
  }

  public Enemy GetClickedEnemy(){
      if (Camera.main == null) return null;

      Ray ray = Camera.main.ScreenPointToRay(
          Mouse.current.position.ReadValue()
      );

      RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

      if(hit.collider == null)
        return null;

    return hit.collider.GetComponent<Enemy>();
  }

  void ResolveIntent(){
    if (currentIntent == null)
        return;

    GridNode startNode = grid.GetNodeFromWorld(transform.position);
    if (startNode == null)
        return;

    currentPath = resolver.Resolve(currentIntent, startNode);

    if (currentPath == null || currentPath.Count == 0)
        return;

    pathIndex = 0;
    SetNextTarget();
  }

}
