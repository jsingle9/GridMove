using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class Enemy : MonoBehaviour, ICombatant
{
    protected DynamicObstacle dynamicObstacle;
    [SerializeField] protected GridController grid;
    [SerializeField] protected int maxHP = 10;
    protected int currentHP;

    public int Initiative { get; set; }
    public bool HasMove { get; set; }
    public bool HasAction { get; set; }
    public bool HasBonusAction { get; set; }
    public int RemainingMovement { get; set; }

    [SerializeField] protected int armorClass = 12;
    [SerializeField] protected int attackBonus = 4;
    [SerializeField] protected string damageDice = "1d6";
    [SerializeField] protected int damageModifier = 0;
    [SerializeField] protected int speed = 6;
    public int Speed => speed;
    public string Name => name;
    public int MaxHP => maxHP;

    protected IntentResolver resolver;
    protected UnitMover mover;
    protected IntentExecutor intentExecutor;
    protected StatusManager statusManager;
    protected Weapon equippedWeapon;
    public Weapon EquippedWeapon {
        get => equippedWeapon;
        set => equippedWeapon = value;
    }

    protected List<Ability> abilities = new List<Ability>();

    protected virtual void Awake()
    {
        currentHP = maxHP;

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

        Debug.Log("✅ Enemy base initialized correctly");
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

    protected void EndMyTurn()
    {
        CombatManager.Instance.EndTurn();
    }

    public List<Ability> GetAbilities()
    {
        return abilities;
    }

    // ABSTRACT - Subclasses MUST implement their own turn behavior
    protected abstract System.Collections.IEnumerator EnemyTurnRoutine();

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

    protected virtual void Die()
    {
        Debug.Log($"{name} died");
        Vector3Int deathCell = grid.WorldToGrid(transform.position);
        grid.UnregisterOccupant(deathCell);

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

    public void MoveTo(Vector3 worldTargetPosition)
    {
        transform.position = worldTargetPosition;
        dynamicObstacle.UpdateCell(transform.position);
    }
}
