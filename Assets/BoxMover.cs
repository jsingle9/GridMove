using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BoxMover : MonoBehaviour, ICombatant
{
    [SerializeField] GridController grid;
    [SerializeField] UnitMover mover;
    List<Ability> abilities = new List<Ability>();
    TargetingSystem targeting;

    IntentResolver resolver;
    Intent currentIntent;

    [SerializeField] int maxHP = 45;
    int currentHP;
    //bool isMyTurn = false;
    public int Initiative { get; set; }
    public bool HasMove { get; set; }
    public bool HasAction { get; set; }
    public bool HasBonusAction { get; set; }
    public int RemainingMovement { get; set; }
    public bool turnStarted = false;
    //public string name;
    [SerializeField] int armorClass = 16;
    [SerializeField] int attackBonus = 5;
    [SerializeField] string damageDice = "1d8";
    [SerializeField] int damageModifier = 3;
    [SerializeField] int speed = 6;
    ICombatant pendingAttackTarget;
    public string Name => name;
    public int Speed => speed;
    public int ArmorClass => armorClass;
    public int AttackBonus => attackBonus;
    public string DamageDice => damageDice;
    public int DamageModifier => damageModifier;
    //private List<StatusEffect> activeStatuses = new List<StatusEffect>();
    private StatusManager statusManager;

    void Awake(){
        currentHP = maxHP;
        abilities.Add(new AttackAbility());        // slot 1
        abilities.Add(new RangedAttackAbility());  // slot 2
        abilities.Add(new HealAbility());          // slot 3
        Debug.Log("Player abilities: " + abilities.Count);
        statusManager = new StatusManager(this);

    }

    void Start(){
      if(grid == null){
          grid = FindFirstObjectByType<GridController>();
      }

      if(grid == null){
          Debug.LogError("BoxMover has no GridController!", this);
          return;
      }

      targeting = new TargetingSystem(grid);
      mover = GetComponent<UnitMover>();
      mover.Initialize(grid);

      resolver = new IntentResolver(grid);
    }

    void Update(){
        mover.Tick();
        CheckIntentCompletion();
        CheckForProximityCombat();
        if(GameStateManager.Instance.CurrentState == GameState.Combat){
          if(CombatManager.Instance.IsPlayersTurn(this)){
              if(UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame){
                if(mover.IsMoving){
                        Debug.Log("Cannot end turn while moving");
                        return;
                }

                    Debug.Log("Manual end turn");
                    FinishTurn();
              }
          }
        }
    }

    public void HandleLeftClick(){
        Debug.Log("HandleLeftClick - State: " +
            GameStateManager.Instance.CurrentState);

      // EXPLORE MODE
        if(GameStateManager.Instance.CurrentState == GameState.FreeExplore){
          HandleExploreClick();
          return;
        }

      // COMBAT MODE
        if(GameStateManager.Instance.CurrentState == GameState.Combat){
            HandleCombatClickRouter();
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

            // calculate cost
            int moveCost = path.Count - 1;

            // trim path if not enough movement
            if(moveCost > RemainingMovement)
            {
                int allowed = RemainingMovement;

                if(allowed <= 0)
                {
                    // allow free explore movement
                    if(GameStateManager.Instance.CurrentState != GameState.Combat ||
                       !CombatManager.Instance.IsPlayersTurn(this)){
                         //Debug.Log("====== FINAL PATH BEGIN ======");

                         if (path == null)
                         {
                             Debug.Log("PATH IS NULL");
                         }
                         else
                         {
                             for (int i = 0; i < path.Count; i++)
                             {
                                // Debug.Log($"Step {i}: {path[i].gridPos}");
                             }

                            // Debug.Log($"FINAL STEP SHOULD BE: {path[path.Count - 1].gridPos}");
                         }

                         //Debug.Log("====== FINAL PATH END ======");
                         Debug.Log("START PATH (FreeExplore bypass)");
                        mover.StartPath(path);
                        return;
                    }

                    Debug.Log("No movement left");
                    return;
                }

                path = path.GetRange(0, allowed + 1);
                moveCost = allowed;
            }

            // spend movement
            RemainingMovement -= moveCost;

            if(RemainingMovement < 0)
                RemainingMovement = 0;

            HasMove = RemainingMovement > 0;

            Debug.Log($"Movement spent: {moveCost}, remaining: {RemainingMovement}");

            //Debug.Log("====== FINAL PATH BEGIN ======");

            if (path == null)
            {
                Debug.Log("PATH IS NULL");
            }
            else
            {
                for (int i = 0; i < path.Count; i++)
                {
                    Debug.Log($"Step {i}: {path[i].gridPos}");
                }

                Debug.Log($"FINAL STEP SHOULD BE: {path[path.Count - 1].gridPos}");
            }

            //Debug.Log("====== FINAL PATH END ======");
            //Debug.Log("START PATH (ResolveIntent)");
            mover.StartPath(path);

    }

    void CheckIntentCompletion()
    {
        if(currentIntent == null)
            return;

        if(mover.IsMoving)
            return;

        // ===============================
        // FINISHED MOVING FOR ATTACK
        // ===============================
        if(currentIntent is AttackIntent){
            // Enter combat if coming from explore
            if(GameStateManager.Instance.CurrentState == GameState.FreeExplore){
                Debug.Log("Attack resolved → entering combat");

                GameStateManager.Instance.EnterCombat();
                float combatRadius = 4f;

                List<ICombatant> participants = new List<ICombatant>();
                participants.Add(GetComponent<ICombatant>());

                Collider2D[] hits = Physics2D.OverlapCircleAll(
                    transform.position,
                    combatRadius
                );

                foreach(Collider2D hit in hits){
                    Enemy enemy = hit.GetComponent<Enemy>();
                    if(enemy == null) continue;

                    ICombatant c = enemy.GetComponent<ICombatant>();
                    if(c != null && !participants.Contains(c))
                        participants.Add(c);
                }

                CombatManager.Instance.StartCombat(participants);
            }

            // ===============================
            // EXECUTE QUEUED ATTACK
            // ===============================
            if(pendingAttackTarget != null &&
               GameStateManager.Instance.CurrentState == GameState.Combat){
                float dist = Vector3.Distance(
                    GetWorldPosition(),
                    pendingAttackTarget.GetWorldPosition()
                );

                if(dist <= 1.6f){
                    Debug.Log("Reached target → executing queued attack");

                    AttackAbility attack = new AttackAbility();
                    attack.TryUse(this, new TargetData(pendingAttackTarget));
                }
                else{
                    Debug.Log("Arrived but not in range — no attack");
                }

                pendingAttackTarget = null;
            }
        }

        currentIntent = null;

        // ===============================
        // AUTO END TURN
        // ===============================
        if(GameStateManager.Instance.CurrentState == GameState.Combat &&
           CombatManager.Instance.IsPlayersTurn(this))
        {
            if(!HasMove && !HasAction && !mover.IsMoving && currentIntent == null)
            {
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

    Enemy GetClickedEnemy()
    {
        if (Camera.main == null)
            return null;

        // Get mouse world position
        Vector2 screenPos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        // Convert to grid cell
        Vector3Int clickedCell = grid.WorldToGrid(worldPos);

        Debug.Log("Clicked world position: " + worldPos);
        Debug.Log("Clicked grid cell: " + clickedCell);

        // Ask grid who occupies this tile
        ICombatant occupant = grid.GetOccupant(clickedCell);

        if (occupant == null)
        {
            Debug.Log("No occupant at this cell.");
            return null;
        }

        if (occupant is Enemy enemy)
        {
            Debug.Log("Grid selected enemy: " + enemy.name);
            Debug.Log("Enemy instance ID: " + enemy.GetInstanceID());
            return enemy;
        }

        Debug.Log("Occupant is not an enemy: " + occupant);
        return null;
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
        if(turnStarted) return;

        Debug.Log("Player turn started");
        turnStarted = true;

        HasMove = true;
        HasAction = true;
        HasBonusAction = true;
        RemainingMovement = Speed;

        AbilityUI.Instance.CurrentPhase = PlayerTurnPhase.WaitingForAction;

        Debug.Log("Choose Action: [1] Melee  [2] Ranged  [3] Item");

        statusManager.ProcessTurnStart();
    }

    public void EndTurn(){
        Debug.Log("Player turn ended");
        turnStarted = false;
        statusManager.ProcessTurnEnd();
        //isMyTurn = false;
        // Disable input
    }

    void CheckForProximityCombat(){
      // If already in combat return
      if(GameStateManager.Instance.CurrentState != GameState.FreeExplore)
          return;

      if(currentIntent is AttackIntent)
          return;

      if(mover.IsMoving)
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

      Vector3Int cell = grid.WorldToGrid(transform.position);
      transform.position = grid.GridToWorld(cell);
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

    public List<Ability> GetAbilities(){
      return abilities;
    }

    public Vector3 GetWorldPosition(){
      return transform.position;
    }

    void OnDrawGizmosSelected(){
      Gizmos.color = Color.green;
      Gizmos.DrawWireSphere(transform.position, 4f);
    }

    public int CurrentHP => currentHP;

    public void TakeDamage(int amount){
      currentHP -= amount;
      Debug.Log($"{name} took {amount} damage. HP: {currentHP}");

      if(currentHP <= 0)
          Die();
    }

    public bool IsDead(){
        return currentHP <= 0;
    }

    int ICombatant.CalculateMoveCost(List<GridNode> path){
        if(path == null || path.Count <= 1)
            return 0;

        return path.Count - 1;
    }

    public int PreviewMoveCost(Intent intent){
        GridNode startNode = grid.GetNodeFromWorld(transform.position);
        if(startNode == null) return -1;

        List<GridNode> path = resolver.Resolve(intent, startNode);
        if(path == null || path.Count == 0) return -1;

        return path.Count - 1;
    }

    public void AddStatus(StatusEffect status){
        statusManager.AddStatus(status);
    }

    public void RemoveStatus(StatusEffect status){
        statusManager.RemoveStatus(status);
    }

    void Die(){
        Debug.Log($"{name} died");
        statusManager.Clear();
        CombatManager.Instance.NotifyDeath(this);
        gameObject.SetActive(false);
    }

    public Ability GetAbility(int slot){
        if (slot < 0 || slot >= abilities.Count)
            return null;

        return abilities[slot];
    }

    void HandleCombatClickRouter(){
        if(!CombatManager.Instance.IsPlayersTurn(this))
            return;

        if(mover.IsMoving)
            return;

        var phase = AbilityUI.Instance.CurrentPhase;

        if(phase == PlayerTurnPhase.WaitingForTarget){
            HandleAbilityTargetClick();
        }
        else{
            HandleCombatMovementClick();
        }
    }

    void HandleAbilityTargetClick(){
        var ability = AbilityUI.Instance.selectedAbility;

        if(ability == null){
            Debug.Log("No ability selected");
            return;
        }

        Vector3 click = GetMouseWorld();

        TargetData target = targeting.ResolveTarget(
            ability,
            this,
            click
        );

        if(target == null){
            Debug.Log("Invalid target");
            return;
        }

        AbilityUI.Instance.TryUseSelected(target);
    }

    void HandleCombatMovementClick(){
        if(!HasMove){
            Debug.Log("Move already used");
            return;
        }

        Vector3 worldClick = GetMouseWorld();
        Vector3Int gridPos = grid.WorldToGrid(worldClick);

        if(!grid.IsWalkable(gridPos))
            return;

        currentIntent = new MoveIntent(gridPos);
        ResolveIntent();
    }

    public void Heal(int amount){
        currentHP += amount;

        if(currentHP > maxHP)
            currentHP = maxHP;

        Debug.Log($"{this} healed to {currentHP}/{maxHP}");
    }

    public void ShowTargetingHighlights(Ability ability)
    {
        targeting.HighlightValidTargets(ability, this);
    }

    public void ClearTargetingHighlights()
    {
        targeting.ClearTargetHighlights();
    }

    public bool IsPlayerControlled(){
        return true;
    }

}
