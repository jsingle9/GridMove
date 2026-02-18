using UnityEngine;
using System.Collections.Generic;

public class BoxMover : MonoBehaviour, ICombatant
{
    [SerializeField] GridController grid;
    [SerializeField] UnitMover mover;

    IntentResolver resolver;
    Intent currentIntent;
    public int Initiative { get; set; }
    public bool HasMove { get; set; }
    public bool HasAction { get; set; }
    public bool HasBonusAction { get; set; }

    void Start()
    {
        mover = GetComponent<UnitMover>();
        mover.Initialize(grid);

        Vector3Int startCell = grid.WorldToGrid(transform.position);
        grid.RegisterOccupant(startCell, this);

        resolver = new IntentResolver(grid);

        if(grid == null)
        {
            Debug.LogError("BoxMover has no GridController assigned!", this);
            return;
        }
    }

    void Update(){
        mover.Tick();
        CheckIntentCompletion();
        CheckForProximityCombat();
        if(GameStateManager.Instance.CurrentState == GameState.Combat){
          if(CombatManager.Instance.IsPlayersTurn(this)){
              if(UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame){
                  Debug.Log("Manual end turn");
                  FinishTurn();
              }
          }
        }
    }


    public void HandleLeftClick(){
    // Only current combatant can act
      if(GameStateManager.Instance.CurrentState == GameState.Combat){
          if(!CombatManager.Instance.IsPlayersTurn(this))
              return;

          if(!HasMove && !HasAction){
            Debug.Log("No actions left");
            return;
          }
      }

      Debug.Log("HandleLeftClick - Current State: " +
        GameStateManager.Instance.CurrentState);

      Enemy enemy = GetClickedEnemy();

      // 🗡 ATTACK CLICKED

      if(enemy != null){
        Debug.Log("Enemy clicked");

    // Enter combat if in explore
      if(GameStateManager.Instance.CurrentState == GameState.FreeExplore){
          Debug.Log("Entering combat from attack");
          GameStateManager.Instance.EnterCombat();
      }

      // In combat → attack ability will handle action spending later
      if(GameStateManager.Instance.CurrentState == GameState.Combat){
        Debug.Log("Using AttackAbility");   // test log

        AttackAbility attack = new AttackAbility(enemy);
        attack.TryUse(this);

        return;
      }

    }

      // 🚶 MOVEMENT
      if(GameStateManager.Instance.CurrentState == GameState.FreeExplore){
          HandleExploreClick();
      }
      else if(GameStateManager.Instance.CurrentState == GameState.Combat){

        if(!HasMove){
            Debug.Log("Move already used");
            return;
        }

          HandleCombatClick();
      }
    }



    void ResolveIntent(){
        if(currentIntent == null)
            return;

        GridNode startNode = grid.GetNodeFromWorld(transform.position);
        if(startNode == null)
            return;

        List<GridNode> path = resolver.Resolve(currentIntent, startNode);

        if(path == null || path.Count == 0)
            return;

        mover.StartPath(path);
    }

    void CheckIntentCompletion(){
      if(currentIntent == null)
        return;

      if(!mover.IsMoving){

        if(currentIntent is AttackIntent attack){
          if(GameStateManager.Instance.CurrentState == GameState.FreeExplore){
            Debug.Log("Attack resolved → entering combat");
            GameStateManager.Instance.EnterCombat();
            float combatRadius = 4f;

            List<ICombatant> participants = new List<ICombatant>();

            // Always add player
            participants.Add(GetComponent<ICombatant>());

            // Find nearby enemies
            Collider2D[] hits = Physics2D.OverlapCircleAll(
              transform.position,
              combatRadius
            );

            foreach(Collider2D hit in hits){
              Enemy enemy = hit.GetComponent<Enemy>();
              if(enemy == null) continue;

              ICombatant combatant = enemy.GetComponent<ICombatant>();
                if(combatant != null && !participants.Contains(combatant)){
                  participants.Add(combatant);
                }
              }CombatManager.Instance.StartCombat(participants);
          }


      }

        currentIntent = null;
      }
      if(GameStateManager.Instance.CurrentState == GameState.Combat){
        if(!HasMove && !HasAction && !mover.IsMoving && currentIntent == null){
            Debug.Log("Player turn complete → ending turn");
            FinishTurn();
        }
      }

    }

    Vector3 GetMouseWorld(){
        if (Camera.main == null)
            return Vector3.zero;

        Vector3 mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        mousePos.z = -Camera.main.transform.position.z;

        Vector3 world = Camera.main.ScreenToWorldPoint(mousePos);
        world.z = 0;

        return world;
    }

    Enemy GetClickedEnemy(){
        if(Camera.main == null)
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
      if(!CombatManager.Instance.IsPlayersTurn(this))
        return;

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

    public void StartTurn(){
        Debug.Log("Player turn started");

        HasMove = true;
        HasAction = true;
        HasBonusAction = true;
        //Invoke(nameof(FinishTurn), 1f);
    }

    public void EndTurn(){
        Debug.Log("Player turn ended");
        // Disable input
    }

    void CheckForProximityCombat(){
      // If already in combat return
      if(GameStateManager.Instance.CurrentState != GameState.FreeExplore)
          return;

      float combatRadius = 4f;

      Collider2D[] hits = Physics2D.OverlapCircleAll(
          transform.position,
          combatRadius
      );

      List<ICombatant> participants = new List<ICombatant>();
      bool enemyFound = false;

      participants.Add(GetComponent<ICombatant>());

      foreach (Collider2D hit in hits){
          Enemy enemy = hit.GetComponent<Enemy>();
          if(enemy == null) continue;

          enemyFound = true;

          ICombatant combatant = enemy.GetComponent<ICombatant>();
          if(combatant != null && !participants.Contains(combatant))
              participants.Add(combatant);
      }

      if(!enemyFound)
        return;

      Debug.Log("Proximity combat triggered");

      mover.Stop();
      currentIntent = null;
      GameStateManager.Instance.EnterCombat();
      CombatManager.Instance.StartCombat(participants);
    }


    void FinishTurn(){
      CombatManager.Instance.EndTurn();
    }

    public void SetIntent(Intent intent){
      currentIntent = intent;
      ResolveIntent();
    }

    void OnDrawGizmosSelected(){
      Gizmos.color = Color.green;
      Gizmos.DrawWireSphere(transform.position, 4f);
    }
}
