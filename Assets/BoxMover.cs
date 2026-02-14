using UnityEngine;
using System.Collections.Generic;

public class BoxMover : MonoBehaviour
{
    [SerializeField] GridController grid;
    [SerializeField] UnitMover mover;

    IntentResolver resolver;
    Intent currentIntent;

    void Start()
    {
        mover = GetComponent<UnitMover>();
        mover.Initialize(grid);

        resolver = new IntentResolver(grid);

        if (grid == null)
        {
            Debug.LogError("BoxMover has no GridController assigned!", this);
            return;
        }
    }

    void Update()
    {
        mover.Tick();
        CheckIntentCompletion();
    }

    public void HandleLeftClick(){
      Debug.Log("HandleLeftClick - Current State: " +
          GameStateManager.Instance.CurrentState);

     Enemy enemy = GetClickedEnemy();

     // If enemy clicked → always start combat
     if(enemy != null){
         Debug.Log("Enemy clicked - entering combat");

         GameStateManager.Instance.EnterCombat();
         currentIntent = new AttackIntent(enemy);
         ResolveIntent();
         return;
      }
      if(GameStateManager.Instance.CurrentState == GameState.FreeExplore){
          HandleExploreClick();
      }
      else if(GameStateManager.Instance.CurrentState == GameState.Combat){
          HandleCombatClick();
      }
    }

    void ResolveIntent(){
        if (currentIntent == null)
            return;

        GridNode startNode = grid.GetNodeFromWorld(transform.position);
        if (startNode == null)
            return;

        List<GridNode> path = resolver.Resolve(currentIntent, startNode);

        if (path == null || path.Count == 0)
            return;

        mover.StartPath(path);
    }

    void CheckIntentCompletion()
    {
      if(currentIntent == null)
        return;

      if(!mover.IsMoving)
      {
        if (currentIntent is AttackIntent attack){
            Debug.Log("Attack triggered on: " + attack.target.name);
            GameStateManager.Instance.EnterCombat();
            // Later: call CombatManager.ResolveAttack(...)
        }

      currentIntent = null;
  }
    }

    Vector3 GetMouseWorld()
    {
        if (Camera.main == null)
            return Vector3.zero;

        Vector3 mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        mousePos.z = -Camera.main.transform.position.z;

        Vector3 world = Camera.main.ScreenToWorldPoint(mousePos);
        world.z = 0;

        return world;
    }

    Enemy GetClickedEnemy(){
        if (Camera.main == null)
            return null;

        Ray ray = Camera.main.ScreenPointToRay(
            UnityEngine.InputSystem.Mouse.current.position.ReadValue()
        );

        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider == null)
            return null;

        return hit.collider.GetComponent<Enemy>();
    }

    void HandleExploreClick(){
      if(mover.IsMoving)
          return;

      Vector3 worldClick = GetMouseWorld();
      Vector3Int gridPos = grid.WorldToGrid(worldClick);

      if(!grid.IsWalkable(gridPos))
        return;

      currentIntent = new MoveIntent(gridPos);
      ResolveIntent();
    }

    void HandleCombatClick(){
      if(mover.IsMoving)
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


}
