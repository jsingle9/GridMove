using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class BoxMover : MonoBehaviour
{
    [SerializeField] GridController grid;
    [SerializeField] float moveSpeed = 5f;

    Pathfinder pathfinder;

    List<GridNode> currentPath;
    int pathIndex;

    Vector3 targetPosition;
    bool isMoving;

    void Start()
    {
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

    void HandleInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && !isMoving)
        {
            Vector3 worldClick =
                Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            worldClick.z = 0;

            GridNode startNode = grid.GetNodeFromWorld(transform.position);
            GridNode targetNode = grid.GetNodeFromWorld(worldClick);

            if (startNode == null || targetNode == null)
                return;

            if (!targetNode.walkable)
                return;

            currentPath = pathfinder.FindPath(startNode, targetNode);

            if (currentPath == null || currentPath.Count == 0)
                return;

            pathIndex = 0;
            SetNextTarget();
        }
    }

    void HandleMovement()
    {
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            pathIndex++;

            if (pathIndex >= currentPath.Count)
            {
                isMoving = false;
                currentPath = null;
            }
            else
            {
                SetNextTarget();
            }
        }
    }

    void SetNextTarget()
    {
        GridNode nextNode = currentPath[pathIndex];
        targetPosition = grid.GridToWorld(nextNode.gridPos);
        isMoving = true;
    }
}
