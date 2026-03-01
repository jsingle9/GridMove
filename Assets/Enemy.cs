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
    public int RemainingMovement { get; set; }

    [SerializeField] int armorClass = 12;
    [SerializeField] int attackBonus = 4;
    [SerializeField] string damageDice = "1d6";
    [SerializeField] int damageModifier = 2;
    [SerializeField] int speed = 6;
    public int Speed => speed;

    IntentResolver resolver;
    UnitMover mover;
    Intent currentIntent;

    List<Ability> abilities = new List<Ability>();

    void Awake()
    {
      currentHP = maxHP;
      abilities.Add(new AttackAbility());
      abilities.Add(new RangedAttackAbility());

      Debug.Log("Enemy abilities: " + abilities.Count);
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
        RemainingMovement = Speed;

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
      Ability chosen = ChooseAbility(player);
      chosen.TryUse(this, player);

      // If we moved, wait until movement finishes
      while(mover.IsMoving){
          yield return null;
      }

      // If still have action, try attack again
      if(HasAction){
      //  Ability attack = abilities[0];
        chosen.TryUse(this, player);
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

        GridNode startNode = grid.GetNodeFromWorld(transform.position);
        if(startNode == null){
            Debug.LogError("Enemy start node null");
            return;
        }

        List<GridNode> path = resolver.Resolve(currentIntent, startNode);

        if(path == null || path.Count == 0)
            return;

        // calculate cost
        int moveCost = path.Count - 1;

        // trim if not enough movement
        if(moveCost > RemainingMovement)
        {
            int allowed = RemainingMovement;

            if(allowed <= 0){
                Debug.Log("No movement left");
                return;
            }

            path = path.GetRange(0, allowed + 1);
            moveCost = allowed;
        }

        // spend movement
        RemainingMovement -= moveCost;

        // HARD CLAMP
        if(RemainingMovement < 0)
            RemainingMovement = 0;

        HasMove = RemainingMovement > 0;

        Debug.Log($"Movement spent: {moveCost}, remaining: {RemainingMovement}");

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
        Ability chosen = ChooseAbility(player);
        chosen.TryUse(this, player);


        // If we attacked successfully, action will be spent
        // If we had to move, SetIntent() already started movement
    }

    /*Ability ChooseAbility(ICombatant target){
        float dist = Vector3.Distance(
            GetWorldPosition(),
            target.GetWorldPosition()
        );

        foreach(var a in abilities){
            if(a.Range >= dist && a.CanUse(this))
                return a;
        }

        return abilities[0]; // fallback melee
    }*/

    Ability ChooseAbility(ICombatant target)
    {
        float dist = Vector3.Distance(
            GetWorldPosition(),
            target.GetWorldPosition()
        );

        Ability melee = null;
        Ability ranged = null;

        foreach(var a in abilities){
            if(a is RangedAttackAbility) ranged = a;
            if(a is AttackAbility) melee = a;
        }

        // PRIORITY 1: If ranged exists and usable → use it
        if(ranged != null && ranged.CanUse(this)){
            Debug.Log("Choosing ranged");
            return ranged;
        }

        // PRIORITY 2: fallback melee
        if(melee != null && melee.CanUse(this)){
            Debug.Log("Choosing melee fallback");
            return melee;
        }

        Debug.Log("No valid ability");
        return melee; // final fallback
    }

    int CalculateMoveCost(List<GridNode> path){
        if(path == null || path.Count <= 1)
            return 0;

        return path.Count - 1;
    }

    void Die(){
      Debug.Log($"{name} died");

      CombatManager.Instance.NotifyDeath(this);
      gameObject.SetActive(false);
    }
}
