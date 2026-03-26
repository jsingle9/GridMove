using UnityEngine;
using System.Collections.Generic;

public class MiniBossEnemy : MonoBehaviour, ICombatant
{
    DynamicObstacle dynamicObstacle;
    [SerializeField] GridController grid;
    [SerializeField] int maxHP = 25;
    int currentHP;

    public int Initiative { get; set; }
    public bool HasMove { get; set; }
    public bool HasAction { get; set; }
    public bool HasBonusAction { get; set; }
    public int RemainingMovement { get; set; }

    [SerializeField] int armorClass = 14;
    [SerializeField] int attackBonus = 5;
    [SerializeField] string damageDice = "1d8";
    [SerializeField] int damageModifier = 3;
    [SerializeField] int speed = 6;
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
        equippedWeapon = new Weapon("Legendary Blade", 4, "1d10");

        // Mini boss has multiple abilities
        abilities.Add(new AttackAbility());
        abilities.Add(new RangedAttackAbility());
        abilities.Add(new FireballAbility());

        Debug.Log("MiniBoss abilities: " + abilities.Count);
        dynamicObstacle = GetComponent<DynamicObstacle>();
        mover = GetComponent<UnitMover>();

        if(grid == null)
            grid = FindFirstObjectByType<GridController>();

        if(grid == null)
        {
            Debug.LogError("MiniBoss FAILED INIT: No GridController in scene");
            enabled = false;
            return;
        }

        if(mover == null)
        {
            Debug.LogError("MiniBoss FAILED INIT: Missing UnitMover");
            enabled = false;
            return;
        }

        mover.Initialize(grid);
        resolver = new IntentResolver(grid);
        intentExecutor = new IntentExecutor();
        intentExecutor.Initialize(grid, mover);
        statusManager = new StatusManager(this);

        Debug.Log("✅ MiniBoss initialized correctly");
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

        StartCoroutine(MiniBossTurnRoutine());
    }

    public void EndTurn()
    {
        statusManager.ProcessTurnEnd();
    }

    System.Collections.IEnumerator MiniBossTurnRoutine()
    {
        if(IsDead()) yield break;

        yield return new WaitForSeconds(0.15f);

        BoxMover player = FindFirstObjectByType<BoxMover>();
        if(player == null)
        {
            CombatManager.Instance.EndTurn();
            yield break;
        }

        float distToPlayer = Vector3.Distance(GetWorldPosition(), player.GetWorldPosition());

        // AI: Use Fireball if > 3 units away and lots of HP
        if(distToPlayer > 3f && currentHP > maxHP / 2)
        {
            if(HasAction)
            {
                Vector3 playerPos = player.GetWorldPosition();
                GridNode playerNode = grid.GetNodeFromWorld(playerPos);
                TargetData fireballTarget = new TargetData(player);
                fireballTarget.tile = playerNode;

                Ability fireball = abilities[2];
                intentExecutor.ExecuteAbilityWithMovement(this, fireball, fireballTarget);

                while(mover.IsMoving)
                    yield return null;

                yield return new WaitForSeconds(0.1f);
            }
        }
        // Otherwise use melee if close
        else if(distToPlayer < 2f && HasAction)
        {
            Ability melee = abilities[0];
            TargetData targetData = new TargetData(player);
            intentExecutor.ExecuteAbilityWithMovement(this, melee, targetData);

            while(mover.IsMoving)
                yield return null;

            yield return new WaitForSeconds(0.1f);
        }
        // Move closer if too far
        else if(HasMove)
        {
            GridNode startNode = grid.GetNodeFromWorld(transform.position);
            List<GridNode> path = resolver.Resolve(new AttackIntent(new TargetData(player)), startNode);

            if(path != null && path.Count > 0)
            {
                int moveCost = path.Count - 1;
                if(moveCost > RemainingMovement)
                    path = path.GetRange(0, RemainingMovement + 1);

                mover.StartPath(path);
                RemainingMovement -= (path.Count - 1);
                HasMove = RemainingMovement > 0;

                while(mover.IsMoving)
                    yield return null;
            }
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
        Debug.Log($"{name} took {amount} damage. HP: {currentHP}/{maxHP}");
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
