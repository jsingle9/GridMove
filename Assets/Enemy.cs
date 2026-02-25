using UnityEngine;
using System.Collections.Generic;

public class Enemy : MonoBehaviour, ICombatant
{
    DynamicObstacle dynamicObstacle;
    [SerializeField] GridController grid;
    [SerializeField] int maxHP = 18;
    int currentHP;
    bool turnEnding = false;

    public int Initiative { get; set; }
    public bool HasMove { get; set; }
    public bool HasAction { get; set; }
    public bool HasBonusAction { get; set; }

    [SerializeField] int armorClass = 12;
    [SerializeField] int attackBonus = 4;
    [SerializeField] string damageDice = "1d6";
    [SerializeField] int damageModifier = 2;

    IntentResolver resolver;
    UnitMover mover;
    Intent currentIntent;

    List<Ability> abilities = new List<Ability>();

    void Awake()
    {
      currentHP = maxHP;

      dynamicObstacle = GetComponent<DynamicObstacle>();
      mover = GetComponent<UnitMover>();

      // 🔍 Always auto-find grid if missing
      if (grid == null)
          grid = FindFirstObjectByType<GridController>();

      if(grid == null){
          Debug.LogError(" ENEMY FAILED INIT: No GridController in scene");
          enabled = false;
          return;
      }

      if (mover == null){
          Debug.LogError(" ENEMY FAILED INIT: Missing UnitMover");
          enabled = false;
          return;
      }

      mover.Initialize(grid);
      resolver = new IntentResolver(grid);

      Debug.Log("✅ Enemy initialized correctly");
    }



    void Start(){
        if (grid == null)
            grid = FindFirstObjectByType<GridController>();

        Vector3Int startCell = grid.WorldToGrid(transform.position);
        grid.RegisterOccupant(startCell, this);
    }

    public void MoveTo(Vector3 worldTargetPosition)
    {
        transform.position = worldTargetPosition;
        dynamicObstacle.UpdateCell(transform.position);
    }

    void Update(){
        mover.Tick();

    }


    void OnTriggerEnter2D(Collider2D other){
        Debug.Log("Trigger entered by: " + other.name);
        if (other.GetComponent<BoxMover>())
        {
            GameStateManager.Instance.EnterCombat();
        }
    }

    // =========================
    // TURN SYSTEM
    // =========================

    public void StartTurn(){
        Debug.Log("Enemy turn started");

        HasMove = true;
        HasAction = true;
        HasBonusAction = true;

        StartCoroutine(EnemyTurnRoutine());
    }




    public void EndTurn(){ // announce turn end
        Debug.Log("Enemy EndTurn() called");
    }

    void EndMyTurn(){ // make turn end
      CombatManager.Instance.EndTurn();
    }

    // =========================
    // ABILITIES
    // =========================

    public List<Ability> GetAbilities(){
        return abilities;
    }

    System.Collections.IEnumerator EnemyTurnRoutine(){

      yield return new WaitForSeconds(0.15f);

      BoxMover player = FindFirstObjectByType<BoxMover>();
      if(player == null){
          EndMyTurn();
          yield break;
      }

      // Try attack first
      AttackAbility attack = new AttackAbility(player);
      attack.TryUse(this);

      // If we moved, wait until movement finishes
      while(mover.IsMoving){
          yield return null;
      }

      // If still have action, try attack again
      if(HasAction){
          attack = new AttackAbility(player);
          attack.TryUse(this);
      }
      // Wait for any final movement
      while(mover.IsMoving){
        yield return null;
      }
      // small delay so logs readable
      yield return new WaitForSeconds(0.05f);

      Debug.Log("Enemy finished turn");
      EndMyTurn();
    }


    // =========================
    // INTENT SYSTEM
    // =========================

  /*  public void SetIntent(Intent intent){
      currentIntent = intent;

      GridNode startNode = grid.GetNodeFromWorld(transform.position);

      List<GridNode> path = resolver.Resolve(intent, startNode);

      if(path != null && path.Count > 0)
        mover.StartPath(path);
    }*/

    public void SetIntent(Intent intent){
      Debug.Log("ENEMY SET INTENT CALLED");

      currentIntent = intent;

      if(GameStateManager.Instance.CurrentState == GameState.Combat){
        HasMove = false; // 👈 ADD THIS
      }

      GridNode startNode = grid.GetNodeFromWorld(transform.position);
      if(startNode == null){
        Debug.LogError("Enemy start node null");
        return;
      }

      List<GridNode> path = resolver.Resolve(intent, startNode);

      if(path == null){
        Debug.LogError("Enemy path returned NULL");
        return;
      }

      Debug.Log("Enemy path length: " + path.Count);

      if(path.Count == 0){
        Debug.LogError("Enemy path empty");
        return;
      }

      Debug.Log("Calling mover.StartPath()");
      mover.StartPath(path);
    }

    // =========================
    // POSITION
    // =========================

    public Vector3 GetWorldPosition(){
        return transform.position;
    }

    public int CurrentHP => currentHP;
    public int ArmorClass => armorClass;
    public int AttackBonus => attackBonus;
    public string DamageDice => damageDice;
    public int DamageModifier => damageModifier;

    public void TakeDamage(int amount){
      currentHP -= amount;
      Debug.Log($"{name} took {amount} damage. HP: {currentHP}");

      if(currentHP <= 0)
        Die();
    }

    public bool IsDead(){
      return currentHP <= 0;
    }

    void Think()
    {
        Debug.Log("ENEMY THINKING");

        BoxMover player = FindFirstObjectByType<BoxMover>();

        if(player == null){
            EndMyTurn();
            return;
        }

        // Try attack
        AttackAbility attack = new AttackAbility(player);
        attack.TryUse(this);

        // If we attacked successfully, action will be spent
        // If we had to move, SetIntent() already started movement
    }


    /*void Update(){

        mover.Tick();

        if(GameStateManager.Instance.CurrentState != GameState.Combat)
            return;

        if(!CombatManager.Instance.IsPlayersTurn(this)){

            Debug.Log($"ENEMY STATE → moving:{mover.IsMoving} | hasMove:{HasMove} | hasAction:{HasAction}");

            // 🔥 When movement finishes, try attack
            if(!mover.IsMoving && HasAction){
                Debug.Log(">>> MOVEMENT COMPLETE → TRYING ATTACK");

                BoxMover player = FindFirstObjectByType<BoxMover>();

                if(player != null){
                    AttackAbility attack = new AttackAbility(player);
                    attack.TryUse(this);
                }
            }

            // 🔥 End turn when fully done
            if(!HasMove && !HasAction && !mover.IsMoving){
                Debug.Log(">>> ENEMY TURN FINISHED → END TURN");
                EndMyTurn();
            }
        }
    }*/





    void Die(){
      Debug.Log($"{name} died");

      CombatManager.Instance.NotifyDeath(this);
      gameObject.SetActive(false);
    }
}
