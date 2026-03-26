using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Enemy : MonoBehaviour, ICombatant
{
    DynamicObstacle dynamicObstacle;
    [SerializeField] GridController grid;
    [SerializeField] int maxHP = 10;
    int currentHP;

    public int Initiative { get; set; }
    public bool HasMove { get; set; }
    public bool HasAction { get; set; }
    public bool HasBonusAction { get; set; }
    public int RemainingMovement { get; set; }

    [SerializeField] int armorClass = 12;
    [SerializeField] int attackBonus = 4;
    [SerializeField] string damageDice = "1d6";
    [SerializeField] int damageModifier = 0;
    [SerializeField] int speed = 6;
    public int Speed => speed;
    public string Name => name;
    public int MaxHP => maxHP;

    IntentResolver resolver;
    UnitMover mover;
    IntentExecutor intentExecutor;
    StatusManager statusManager;
    private Weapon equippedWeapon;
    public Weapon EquippedWeapon {
      get => equippedWeapon;
      set => EquippedWeapon = value;
    }

    List<Ability> abilities = new List<Ability>();

    void Awake()
    {
        currentHP = maxHP;
        equippedWeapon = new Weapon("Short Sword", 0, "1d6");

        abilities.Add(new AttackAbility());
        abilities.Add(new RangedAttackAbility());

        Debug.Log("Enemy abilities: " + abilities.Count);
        dynamicObstacle = GetComponent<DynamicObstacle>();
        mover = GetComponent<UnitMover>();

        if(grid == null)
            grid = FindFirstObjectByType<GridController>();

        if(grid == null)
        {
            Debug.LogError(" ENEMY FAILED INIT: No GridController in scene");
            enabled = false;
            return;
        }

        if(mover == null)
        {
            Debug.LogError(" ENEMY FAILED INIT: Missing UnitMover");
            enabled = false;
            return;
        }

        mover.Initialize(grid);
        resolver = new IntentResolver(grid);
        intentExecutor = new IntentExecutor();
        intentExecutor.Initialize(grid, mover);
        statusManager = new StatusManager(this);

        Debug.Log("✅ Enemy initialized correctly");
    }

    void Start()
    {
        if(grid == null)
            grid = FindFirstObjectByType<GridController>();

        Vector3Int startCell = grid.WorldToGrid(transform.position);
        grid.RegisterOccupant(startCell, this);
    }

    void Update()
    {
        mover.Tick();

        // Check if a queued ability is ready to execute after movement
        intentExecutor.CheckPendingAbilityExecution();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger entered by: " + other.name);
        if(other.GetComponent<BoxMover>())
        {
            GameStateManager.Instance.EnterCombat();
        }
    }

    public void StartTurn()
    {
        Debug.Log("Enemy turn started");

        if(!gameObject.activeInHierarchy)
            return;

        HasMove = true;
        HasAction = true;
        HasBonusAction = true;
        RemainingMovement = Speed;
        statusManager.ProcessTurnStart();

        if(IsDead())
        {
            CombatManager.Instance.EndTurn();
            return;
        }

        StartCoroutine(EnemyTurnRoutine());
    }

    public void EndTurn()
    {
        Debug.Log("Enemy EndTurn() called");
        statusManager.ProcessTurnEnd();
    }

    void EndMyTurn()
    {
        CombatManager.Instance.EndTurn();
    }

    public List<Ability> GetAbilities()
    {
        return abilities;
    }

    System.Collections.IEnumerator EnemyTurnRoutine()
    {
        if(IsDead()) yield break;

        yield return new WaitForSeconds(0.15f);

        BoxMover player = FindFirstObjectByType<BoxMover>();
        if(player == null)
        {
            EndMyTurn();
            yield break;
        }

        // Try attack first
        Ability chosen = ChooseAbility(player);
        TargetData targetData = new TargetData(player);

        AbilityResult result = intentExecutor.ExecuteAbilityWithMovement(this, chosen, targetData);

        // If we moved, wait until movement finishes
        while(mover.IsMoving)
        {
            yield return null;
        }

        // Wait a bit for pending ability execution
        yield return new WaitForSeconds(0.1f);

        // If still have action, try attack again
        if(HasAction)
        {
            chosen = ChooseAbility(player);
            targetData = new TargetData(player);
            result = intentExecutor.ExecuteAbilityWithMovement(this, chosen, targetData);
        }

        // Wait for any final movement
        while(mover.IsMoving)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        Debug.Log("Enemy finished turn");
        EndMyTurn();
    }

    Ability ChooseAbility(ICombatant target)
    {
        float dist = Vector3.Distance(
            GetWorldPosition(),
            target.GetWorldPosition()
        );

        Ability melee = null;
        Ability ranged = null;

        foreach(var a in abilities)
        {
            if(a is RangedAttackAbility) ranged = a;
            if(a is AttackAbility) melee = a;
        }

        // PRIORITY 1: If ranged exists and usable → use it
        if(ranged != null && ranged.CanUse(this))
        {
            Debug.Log("Choosing ranged");
            return ranged;
        }

        // PRIORITY 2: fallback melee
        if(melee != null && melee.CanUse(this))
        {
            Debug.Log("Choosing melee fallback");
            return melee;
        }

        Debug.Log("No valid ability");
        return melee;
    }

    public Vector3 GetWorldPosition()
    {
        return transform.position;
    }

    public int CurrentHP => currentHP;
    public int ArmorClass => armorClass;
    public int AttackBonus => attackBonus;
    public string DamageDice => damageDice;
    public int DamageModifier => damageModifier;

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        Debug.Log($"{name} took {amount} damage. HP: {currentHP}");

        if(currentHP <= 0)
            Die();
    }

    public bool IsDead()
    {
        return currentHP <= 0;
    }

    int ICombatant.CalculateMoveCost(List<GridNode> path)
    {
        if(path == null || path.Count <= 1)
            return 0;

        return path.Count - 1;
    }

    public int PreviewMoveCost(Intent intent)
    {
        GridNode startNode = grid.GetNodeFromWorld(transform.position);
        if(startNode == null) return -1;

        List<GridNode> path = resolver.Resolve(intent, startNode);
        if(path == null || path.Count == 0) return -1;

        return path.Count - 1;
    }

    public void AddStatus(StatusEffect status)
    {
        statusManager.AddStatus(status);
    }

    public void RemoveStatus(StatusEffect status)
    {
        statusManager.RemoveStatus(status);
    }

    void Die()
    {
        Debug.Log($"{name} died");
        // Unregister dead occupant from grid
        Vector3Int deathCell = grid.WorldToGrid(transform.position);
        grid.UnregisterOccupant(deathCell);
        // Drop weapon
        if(equippedWeapon != null)
        {
            GameObject dropObj = new GameObject($"Loot_{equippedWeapon.WeaponName}");
            dropObj.transform.position = transform.position;

            LootDrop loot = dropObj.AddComponent<LootDrop>();
            loot.SetWeapon(equippedWeapon);
        }
                
        statusManager.Clear();
        CombatManager.Instance.NotifyDeath(this);
        gameObject.SetActive(false);
    }

    public void Heal(int amount)
    {
        currentHP += amount;

        if(currentHP > maxHP)
            currentHP = maxHP;

        Debug.Log($"{this} healed to {currentHP}/{maxHP}");
    }

    public bool IsPlayerControlled()
    {
        return false;
    }

  /*  public void SetIntent(Intent intent)
    {
        // DEPRECATED: No longer used. Abilities now use IntentExecutor.
        Debug.LogWarning("SetIntent() called but no longer used. Use IntentExecutor instead.");
    }*/

    public void MoveTo(Vector3 worldTargetPosition)
    {
        transform.position = worldTargetPosition;
        dynamicObstacle.UpdateCell(transform.position);
    }
}
