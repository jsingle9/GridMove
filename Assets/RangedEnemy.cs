using UnityEngine;
using System.Collections.Generic;

public class RangedEnemy : MonoBehaviour, ICombatant
{
    DynamicObstacle dynamicObstacle;
    [SerializeField] GridController grid;
    [SerializeField] int maxHP = 8;
    int currentHP;

    public int Initiative { get; set; }
    public bool HasMove { get; set; }
    public bool HasAction { get; set; }
    public bool HasBonusAction { get; set; }
    public int RemainingMovement { get; set; }

    [SerializeField] int armorClass = 11;
    [SerializeField] int attackBonus = 3;
    [SerializeField] string damageDice = "1d6";
    [SerializeField] int damageModifier = 1;
    [SerializeField] int speed = 5;
    public int Speed => speed;
    public string Name => name;
    public int MaxHP => maxHP;

    IntentResolver resolver;
    UnitMover mover;
    IntentExecutor intentExecutor;
    StatusManager statusManager;

    List<Ability> abilities = new List<Ability>();
    private Weapon equippedWeapon;
    public Weapon EquippedWeapon
    {
        get => equippedWeapon;
        set => equippedWeapon = value;
    }

    void Awake()
    {
        currentHP = maxHP;
        equippedWeapon = new Weapon("Bow", 1, "1d6");

        // Ranged enemy only has ranged attack
        abilities.Add(new RangedAttackAbility());

        Debug.Log("RangedEnemy abilities: " + abilities.Count);
        dynamicObstacle = GetComponent<DynamicObstacle>();
        mover = GetComponent<UnitMover>();

        if(grid == null)
            grid = FindFirstObjectByType<GridController>();

        if(grid == null)
        {
            Debug.LogError("RangedEnemy FAILED INIT: No GridController in scene");
            enabled = false;
            return;
        }

        if(mover == null)
        {
            Debug.LogError("RangedEnemy FAILED INIT: Missing UnitMover");
            enabled = false;
            return;
        }

        mover.Initialize(grid);
        resolver = new IntentResolver(grid);
        intentExecutor = new IntentExecutor();
        intentExecutor.Initialize(grid, mover);
        statusManager = new StatusManager(this);

        Debug.Log("✅ RangedEnemy initialized correctly");
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
        if(other.GetComponent<BoxMover>())
        {
            GameStateManager.Instance.EnterCombat();
        }
    }

    public void StartTurn()
    {
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
        statusManager.ProcessTurnEnd();
    }

    System.Collections.IEnumerator EnemyTurnRoutine()
    {
        if(IsDead()) yield break;

        yield return new WaitForSeconds(0.15f);

        BoxMover player = FindFirstObjectByType<BoxMover>();
        if(player == null)
        {
            CombatManager.Instance.EndTurn();
            yield break;
        }

        // Ranged enemy prefers to keep distance
        float distToPlayer = Vector3.Distance(GetWorldPosition(), player.GetWorldPosition());

        if(distToPlayer < 4f && HasMove)
        {
            // Too close - back away
            GridNode startNode = grid.GetNodeFromWorld(transform.position);
            List<GridNode> pathAway = resolver.Resolve(new AttackIntent(new TargetData(player)), startNode);

            if(pathAway != null && pathAway.Count > 2)
            {
                // Move away (take last node instead of first)
                pathAway = pathAway.GetRange(pathAway.Count - 2, 2);
                mover.StartPath(pathAway);
                RemainingMovement -= 2;
                HasMove = RemainingMovement > 0;
            }

            while(mover.IsMoving)
                yield return null;
        }

        // Attack with ranged
        if(HasAction)
        {
            Ability rangedAttack = abilities[0];
            TargetData targetData = new TargetData(player);
            intentExecutor.ExecuteAbilityWithMovement(this, rangedAttack, targetData);

            while(mover.IsMoving)
                yield return null;

            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.1f);
        CombatManager.Instance.EndTurn();
    }

    public List<Ability> GetAbilities() => abilities;

    public Vector3 GetWorldPosition() => transform.position;

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

    public bool IsDead() => currentHP <= 0;

    public int CalculateMoveCost(List<GridNode> path) => path == null || path.Count <= 1 ? 0 : path.Count - 1;

    public int PreviewMoveCost(Intent intent)
    {
        GridNode startNode = grid.GetNodeFromWorld(transform.position);
        if(startNode == null) return -1;
        List<GridNode> path = resolver.Resolve(intent, startNode);
        return path == null || path.Count == 0 ? -1 : path.Count - 1;
    }

    public void AddStatus(StatusEffect status) => statusManager.AddStatus(status);

    public void RemoveStatus(StatusEffect status) => statusManager.RemoveStatus(status);

    void Die()
    {
        Debug.Log($"{name} died");
        statusManager.Clear();

        if(equippedWeapon != null)
        {
            GameObject dropObj = new GameObject($"Loot_{equippedWeapon.WeaponName}");
            dropObj.transform.position = transform.position;

            LootDrop loot = dropObj.AddComponent<LootDrop>();
            loot.SetWeapon(equippedWeapon);
        }

        CombatManager.Instance.NotifyDeath(this);
        gameObject.SetActive(false);
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if(currentHP > maxHP)
            currentHP = maxHP;
    }

    public bool IsPlayerControlled() => false;
}
