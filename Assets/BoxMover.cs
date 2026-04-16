using UnityEngine;
using System.Collections.Generic;

public class BoxMover : MonoBehaviour, ICombatant
{
    [SerializeField] GridController grid;
    [SerializeField] UnitMover mover;
    List<Ability> abilities = new List<Ability>();
    TargetingSystem targeting;
    IntentExecutor intentExecutor;

    IntentResolver resolver;
    MoveIntent currentMoveIntent; // Only for exploration movement
    [SerializeField] int maxHP = 45;
    int currentHP;
    public int Initiative { get; set; }
    public bool HasMove { get; set; }
    public bool HasAction { get; set; }
    public bool HasBonusAction { get; set; }
    public int RemainingMovement { get; set; }
    public bool turnStarted = false;
    [SerializeField] int armorClass = 16;
    [SerializeField] int attackBonus = 5;
    [SerializeField] string baseDamageDice = "1d8";      // Base damage without weapon
    [SerializeField] int baseDamageModifier = 3;         // Base modifier without weapon
    [SerializeField] int speed = 6;
    public string Name => name;
    public int Speed => speed;
    public int ArmorClass => armorClass;
    public int AttackBonus => attackBonus;

    // These now factor in equipped weapons
    public string DamageDice
    {
        get
        {
            Weapon equippedMelee = Inventory.Instance.GetEquippedMeleeWeapon();
            if (equippedMelee != null)
                return equippedMelee.DamageDice;
            return baseDamageDice;
        }
    }

    public int DamageModifier
    {
        get
        {
            Weapon equippedMelee = Inventory.Instance.GetEquippedMeleeWeapon();
            if (equippedMelee != null)
                return equippedMelee.DamageBonus;
            return baseDamageModifier;
        }
    }

    private StatusManager statusManager;
    private Weapon equippedWeapon;
    public Weapon EquippedWeapon {
        get => equippedWeapon;
        set => EquippedWeapon = value;
    }

    void Awake()
    {
        currentHP = maxHP;
        equippedWeapon = new Weapon("Iron Sword", 3, "1d8");

        abilities.Add(new AttackAbility());
        abilities.Add(new RangedAttackAbility());
        abilities.Add(new HealAbility());
        abilities.Add(new FireballAbility());
        Debug.Log("Player abilities: " + abilities.Count);
        statusManager = new StatusManager(this);
    }

    void Start()
    {
        if(grid == null)
        {
            grid = FindFirstObjectByType<GridController>();
        }

        if(grid == null)
        {
            Debug.LogError("BoxMover has no GridController!", this);
            return;
        }

        targeting = new TargetingSystem(grid);
        mover = GetComponent<UnitMover>();
        mover.Initialize(grid);

        resolver = new IntentResolver(grid);

        // Initialize IntentExecutor
        intentExecutor = new IntentExecutor();
        intentExecutor.Initialize(grid, mover);
    }

    void Update()
    {
        mover.Tick();

        if(UnityEngine.InputSystem.Keyboard.current.fKey.wasPressedThisFrame)
        {
            Debug.Log("F key detected in BoxMover!");
            InteractionSystem.Instance?.AttemptInteraction();
        }

        // Check if a queued ability is ready to execute after movement
        intentExecutor.CheckPendingAbilityExecution();

        CheckForProximityCombat();

        if(GameStateManager.Instance.CurrentState == GameState.Combat)
        {
            if(CombatManager.Instance.IsPlayersTurn(this))
            {
                if(UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    if(mover.IsMoving)
                    {
                        Debug.Log("Cannot end turn while moving");
                        return;
                    }

                    Debug.Log("Manual end turn");
                    FinishTurn();
                }
            }
        }
      //  Debug.Log($"anyKeyDown: {Input.anyKeyDown}");
      //  Debug.Log($"F: {Input.GetKeyDown(KeyCode.F)}");
      //  Debug.Log($"Space: {Input.GetKeyDown(KeyCode.Space)}");
    }

    public void HandleLeftClick()
    {
        Debug.Log("HandleLeftClick - State: " + GameStateManager.Instance.CurrentState);

        // EXPLORE MODE
        if(GameStateManager.Instance.CurrentState == GameState.FreeExplore)
        {
            HandleExploreClick();
            return;
        }

        // COMBAT MODE
        if(GameStateManager.Instance.CurrentState == GameState.Combat)
        {
            HandleCombatClickRouter();
        }
    }

    void HandleExploreClick()
    {
        if(mover.IsMoving)
            return;

        Vector3 worldClick = GetMouseWorld();
        Vector3Int gridPos = grid.WorldToGrid(worldClick);

        if(!grid.IsWalkable(gridPos))
            return;

        currentMoveIntent = new MoveIntent(gridPos);
        ResolveMoveIntent();
    }

    /// <summary>
    /// Handle pure movement intents (exploration mode only)
    /// </summary>
    void ResolveMoveIntent()
    {
        if(currentMoveIntent == null)
            return;

        GridNode startNode = grid.GetNodeFromWorld(transform.position);
        if(startNode == null)
            return;

        List<GridNode> path = resolver.Resolve(currentMoveIntent, startNode);

        if(path == null || path.Count == 0)
            return;

        int moveCost = path.Count - 1;

        if(moveCost > RemainingMovement)
        {
            int allowed = RemainingMovement;

            if(allowed <= 0)
            {
                if(GameStateManager.Instance.CurrentState != GameState.Combat ||
                   !CombatManager.Instance.IsPlayersTurn(this))
                {
                    mover.StartPath(path);
                    return;
                }

                Debug.Log("No movement left");
                return;
            }

            path = path.GetRange(0, allowed + 1);
            moveCost = allowed;
        }

        RemainingMovement -= moveCost;

        if(RemainingMovement < 0)
            RemainingMovement = 0;

        HasMove = RemainingMovement > 0;

        //Debug.Log($"Movement spent: {moveCost}, remaining: {RemainingMovement}");

        mover.StartPath(path);
        currentMoveIntent = null;
    }

    void CheckForProximityCombat()
    {
        if(GameStateManager.Instance.CurrentState != GameState.FreeExplore)
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

        foreach(Collider2D hit in hits)
        {
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
        currentMoveIntent = null;
        GameStateManager.Instance.EnterCombat();
        CombatManager.Instance.StartCombat(participants);
    }

    void FinishTurn()
    {
        CombatManager.Instance.EndTurn();
    }

    public void StartTurn()
    {
        if(turnStarted) return;

        Debug.Log("Player turn started");
        turnStarted = true;

        HasMove = true;
        HasAction = true;
        HasBonusAction = true;
        RemainingMovement = Speed;

        AbilityUI.Instance.CurrentPhase = PlayerTurnPhase.WaitingForAction;

        Debug.Log("Choose Action: [1] Melee  [2] Ranged  [3] HealSpell [4] Fireball");

        statusManager.ProcessTurnStart();
    }

    public void EndTurn()
    {
        Debug.Log("Player turn ended");
        turnStarted = false;
        statusManager.ProcessTurnEnd();
    }

    Vector3 GetMouseWorld()
    {
        if(Camera.main == null)
            return Vector3.zero;

        Vector3 mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        mousePos.z = -Camera.main.transform.position.z;

        Vector3 world = Camera.main.ScreenToWorldPoint(mousePos);
        world.z = 0;

        return world;
    }

    Enemy GetClickedEnemy()
    {
        if(Camera.main == null)
            return null;

        Vector2 screenPos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        Vector3Int clickedCell = grid.WorldToGrid(worldPos);

        ICombatant occupant = grid.GetOccupant(clickedCell);

        if(occupant == null)
        {
            Debug.Log("No occupant at this cell.");
            return null;
        }

        if(occupant is Enemy enemy)
        {
            Debug.Log("Grid selected enemy: " + enemy.name);
            return enemy;
        }

        Debug.Log("Occupant is not an enemy: " + occupant);
        return null;
    }

    void HandleCombatClickRouter()
    {
        if(!CombatManager.Instance.IsPlayersTurn(this))
            return;

        if(mover.IsMoving)
            return;

        var phase = AbilityUI.Instance.CurrentPhase;

        if(phase == PlayerTurnPhase.WaitingForTarget)
        {
            HandleAbilityTargetClick();
        }
        else
        {
            HandleCombatMovementClick();
        }
    }

    void HandleAbilityTargetClick()
    {
        var ability = AbilityUI.Instance.selectedAbility;

        if(ability == null)
        {
            Debug.Log("No ability selected");
            return;
        }

        Vector3 click = GetMouseWorld();

        TargetData target = targeting.ResolveTarget(
            ability,
            this,
            click
        );

        if(target == null)
        {
            Debug.Log("Invalid target");
            return;
        }

        // Use IntentExecutor to handle ability with movement support
        AbilityResult result = intentExecutor.ExecuteAbilityWithMovement(this, ability, target);

        if(!result.Success && !intentExecutor.IsExecutingAbilityWithMovement())
        {
            Debug.Log($"Ability failed: {result.FailureReason}");
        }

        grid.ClearAllHighlights();

        if(result.Success && !intentExecutor.IsExecutingAbilityWithMovement())
        {
            AbilityUI.Instance.CurrentPhase = PlayerTurnPhase.WaitingForAction;
            AbilityUI.Instance.selectedAbility = null;
        }
    }

    void HandleCombatMovementClick()
    {
        if(!HasMove)
        {
            Debug.Log("Move already used");
            return;
        }

        Vector3 worldClick = GetMouseWorld();
        Vector3Int gridPos = grid.WorldToGrid(worldClick);

        if(!grid.IsWalkable(gridPos))
            return;

        currentMoveIntent = new MoveIntent(gridPos);
        ResolveMoveIntent();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 4f);
    }

    public int CurrentHP => currentHP;

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
        statusManager.Clear();
        CombatManager.Instance.NotifyDeath(this);
        gameObject.SetActive(false);
    }

    public List<Ability> GetAbilities()
    {
        return abilities;
    }

    public Ability GetAbility(int slot)
    {
        if(slot < 0 || slot >= abilities.Count)
            return null;

        return abilities[slot];
    }

    public Vector3 GetWorldPosition()
    {
        return transform.position;
    }

    public void ShowTargetingHighlights(Ability ability)
    {
        targeting.HighlightValidTargets(ability, this);
    }

    public void ClearTargetingHighlights()
    {
        targeting.ClearTargetHighlights();
    }

    public bool IsPlayerControlled()
    {
        return true;
    }

    public void Heal(int amount)
    {
        currentHP += amount;

        if(currentHP > maxHP)
            currentHP = maxHP;

        Debug.Log($"{this} healed to {currentHP}/{maxHP}");
    }

    public void EquipWeapon(Weapon weapon)
    {
        if(weapon == null) return;

        equippedWeapon = weapon;
        Debug.Log($"Equipped: {weapon.WeaponName} (+{weapon.DamageBonus} damage)");
    }
}
