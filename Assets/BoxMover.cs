using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BoxMover : MonoBehaviour, ICombatant
{
    [SerializeField] GridController grid;
    [SerializeField] UnitMover mover;
    List<Ability> abilities = new List<Ability>();
    TargetingSystem targeting;
    IntentExecutor intentExecutor;
    // To telegraph fireball for now
    [SerializeField] TelegraphStyle fireballTelegraphStyle;
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
    private ClassDef classDef;
    private ArmorDef armorDef;
    public string Name => name;
    public int Speed => speed;
    public int ArmorClass => armorClass;
    public int AttackBonus => attackBonus;
    [SerializeField] private CharacterSheet characterSheet = new CharacterSheet();
    public CharacterSheet Sheet => characterSheet;
    public bool SecondWindUsedThisCombat { get; set; }
    public bool ActionSurgeUsedThisCombat { get; set; }

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
        set => equippedWeapon = value;
    }

    void Awake()
    {
        if (characterSheet == null)
            characterSheet = new CharacterSheet();

        if (characterSheet.Level < 1) characterSheet.Level = 1;

        // Apply class rules at level 1 (fighter for now)
        var rules = ClassRulesFactory.Create(CharacterClassType.Fighter);
        rules?.ApplyLevel1(characterSheet);
        rules?.ApplyLevelUp(characterSheet, 2);

        // Resolve defs (temp lookup approach)
        classDef = RulesLookups.GetClassDef(characterSheet.ClassId);
        armorDef = RulesLookups.GetArmorDefOrNull(characterSheet.EquippedArmorId);

        // Rules-authoritative sync
        maxHP = RulesService.CalculateMaxHP(characterSheet, classDef);
        armorClass = RulesService.CalculateAC(characterSheet, armorDef);
        speed = RulesService.CalculateSpeed(characterSheet);

        // Keep current HP valid
        if (characterSheet.CurrentHP <= 0)
            characterSheet.CurrentHP = maxHP;

        currentHP = Mathf.Clamp(characterSheet.CurrentHP, 0, maxHP);

        // TEMP compat backfill (safe during migration)
        characterSheet.MaxHP = maxHP;
        characterSheet.ArmorClass = armorClass;
        characterSheet.Speed = speed;

        equippedWeapon = new Weapon("Long Sword", 3, "1d8");

        abilities.Add(new AttackAbility());
        abilities.Add(new RangedAttackAbility());
      //abilities.Add(new HealAbility());

      /*  var fireball = new FireballAbility();
        fireball.SetTelegraphStyle(fireballTelegraphStyle);
        abilities.Add(fireball); */
        abilities.Add(new SecondWindAbility(new DefaultSecondWindConfig()));
        abilities.Add(new ActionSurgeAbility(new DefaultActionSurgeConfig()));

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
        // Wire UI to this player and refresh ability buttons
        if (AbilityUI.Instance != null)
        {
            AbilityUI.Instance.player = this;
            AbilityUI.Instance.RefreshAbilityButtons();
        }
    }

    void Update()
    {
        mover.Tick();

        if(UnityEngine.InputSystem.Keyboard.current.fKey.wasPressedThisFrame)
        {
            Debug.Log("F key detected in BoxMover!");
            //InteractionSystem.Instance?.AttemptInteraction();
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
        if(EventSystem.current.IsPointerOverGameObject())
            return;
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

        int moveCost = MovementCostUtility.CalculatePathCost(grid, path);

        if (moveCost > RemainingMovement)
        {
                int allowed = RemainingMovement;

                if (allowed <= 0)
                {
                    if (GameStateManager.Instance.CurrentState != GameState.Combat ||
                        !CombatManager.Instance.IsPlayersTurn(this))
                    {
                        mover.StartPath(path);
                        return;
                    }

                    Debug.Log("No movement left");
                    return;
                }

                int spent;
                path = MovementCostUtility.TrimPathToBudget(grid, path, allowed, out spent);
                moveCost = spent;

                if (path == null || path.Count == 0)
                {
                    Debug.Log("No reachable movement within budget");
                    return;
                }
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
        if (GameStateManager.Instance.CurrentState != GameState.FreeExplore)
            return;

        if (mover.IsMoving)
            return;

        // 1) Trigger range: can start combat if an enemy is seen within this range.
        float triggerRadius = 12f;
        float triggerRadiusSqr = triggerRadius * triggerRadius;

        // 2) Join range: same-encounter enemies can join without LoS if this close.
        float joinRadius = 6f;
        float joinRadiusSqr = joinRadius * joinRadius;

        // Snap player first so LoS uses the same final position combat starts from.
        Vector3Int snappedCell = grid.WorldToGrid(transform.position);
        transform.position = grid.GridToWorld(snappedCell);
        Vector3Int playerCell = snappedCell;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, triggerRadius);

        ICombatant self = GetComponent<ICombatant>();
        if (self == null)
            return;

        // Find one valid trigger enemy (alive + active + within trigger radius + LoS)
        Enemy triggerEnemy = null;
        ICombatant triggerCombatant = null;

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy == null) continue;
            if (!enemy.gameObject.activeInHierarchy) continue;

            ICombatant enemyCombatant = enemy.GetComponent<ICombatant>();
            if (enemyCombatant == null) continue;
            if (enemyCombatant.IsDead()) continue;

            Vector3 delta = enemy.transform.position - transform.position;
            if (delta.sqrMagnitude > triggerRadiusSqr) continue;

            Vector3Int enemyCell = grid.WorldToGrid(enemy.transform.position);
            if (!grid.HasLineOfSight(playerCell, enemyCell)) continue;

            triggerEnemy = enemy;
            triggerCombatant = enemyCombatant;
            break;
        }

        // No valid trigger => no combat
        if (triggerEnemy == null || triggerCombatant == null)
            return;

        string encounterId = triggerEnemy.EncounterId;

        List<ICombatant> participants = new List<ICombatant> { self };

        // Build participants from SAME encounter only
        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy == null) continue;
            if (!enemy.gameObject.activeInHierarchy) continue;
            if (enemy.EncounterId != encounterId) continue;

            ICombatant enemyCombatant = enemy.GetComponent<ICombatant>();
            if (enemyCombatant == null) continue;
            if (enemyCombatant.IsDead()) continue;

            Vector3 delta = enemy.transform.position - transform.position;
            float sqrDist = delta.sqrMagnitude;
            if (sqrDist > triggerRadiusSqr) continue; // hard cap from overlap query

            Vector3Int enemyCell = grid.WorldToGrid(enemy.transform.position);
            bool hasLos = grid.HasLineOfSight(playerCell, enemyCell);

            // Include if close OR visible
            if (sqrDist > joinRadiusSqr && !hasLos)
                continue;

            if (!participants.Contains(enemyCombatant))
                participants.Add(enemyCombatant);
        }

        // Must have at least one enemy
        if (participants.Count <= 1)
            return;

        Debug.Log($"Proximity combat triggered. EncounterId={encounterId}, Participants={participants.Count}");

        mover.Stop();
        currentMoveIntent = null;

        GameStateManager.Instance.EnterCombat();

        // Reset per-combat feature usage
        SecondWindUsedThisCombat = false;
        ActionSurgeUsedThisCombat = false;

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
        RefreshDerivedCombatStats();

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
        characterSheet.CurrentHP = Mathf.Max(0, currentHP);
        Debug.Log($"{name} took {amount} damage. HP: {currentHP}");

        if (DamagePopupManager.Instance != null)
        {
            DamagePopupManager.Instance.ShowDamage(amount, transform.position);
        }

        // Flash red on hit
        StartCoroutine(FlashRed());

        if(currentHP <= 0)
            Die();
    }

    public bool IsDead()
    {
        return currentHP <= 0;
    }

    int ICombatant.CalculateMoveCost(List<GridNode> path)
    {
        if(path == null || path.Count == 0)
            return 0;

        return MovementCostUtility.CalculatePathCost(grid, path);
    }

    public int PreviewMoveCost(Intent intent)
    {
        GridNode startNode = grid.GetNodeFromWorld(transform.position);
        if(startNode == null) return -1;

        List<GridNode> path = resolver.Resolve(intent, startNode);
        if(path == null || path.Count == 0) return -1;

        return MovementCostUtility.CalculatePathCost(grid, path);
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
        Debug.Log($"{name} died at position: {transform.position}");
        SaveLoadService.SetLastDeathPosition(transform.position);

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

        characterSheet.CurrentHP = currentHP;

        Debug.Log($"{this} healed to {currentHP}/{maxHP}");
    }

    public void EquipWeapon(Weapon weapon)
    {
        if(weapon == null) return;

        equippedWeapon = weapon;
        Debug.Log($"Equipped: {weapon.WeaponName} (+{weapon.DamageBonus} damage)");
    }

    private IEnumerator FlashRed()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if(sr != null)
        {
            Color originalColor = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = originalColor;
        }
    }

    public virtual List<Vector3Int> GetOccupiedCells()
    {
        Vector3Int origin = grid.WorldToGrid(transform.position);

        return new List<Vector3Int>
        {
            origin
        };
    }

    public List<ICombatant> GetEnemiesInProximityWithLineOfSight(float combatRadius)
    {
        List<ICombatant> found = new List<ICombatant>();

        if(grid == null)
            return found;

        float combatRadiusSqr = combatRadius * combatRadius;
        Vector3Int playerCell = grid.WorldToGrid(transform.position);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, combatRadius);

        foreach(Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if(enemy == null)
                continue;

            Vector3 delta = enemy.transform.position - transform.position;
            if(delta.sqrMagnitude > combatRadiusSqr)
                continue;

            Vector3Int enemyCell = grid.WorldToGrid(enemy.transform.position);

            if(!grid.HasLineOfSight(playerCell, enemyCell))
                continue;

            ICombatant c = enemy.GetComponent<ICombatant>();
            if(c != null && !found.Contains(c))
                found.Add(c);
        }

        return found;
    }
    private void RefreshDerivedCombatStats()
    {
        classDef = RulesLookups.GetClassDef(characterSheet.ClassId);
        armorDef = RulesLookups.GetArmorDefOrNull(characterSheet.EquippedArmorId);

        maxHP = RulesService.CalculateMaxHP(characterSheet, classDef);
        armorClass = RulesService.CalculateAC(characterSheet, armorDef);
        speed = RulesService.CalculateSpeed(characterSheet);

        currentHP = Mathf.Clamp(currentHP, 0, maxHP);


        // TEMP compat backfill
        characterSheet.MaxHP = maxHP;
        characterSheet.ArmorClass = armorClass;
        characterSheet.Speed = speed;
    }

    public void SetCharacterSheet(CharacterSheet loadedSheet)
    {
        if (loadedSheet == null) return;

        characterSheet = loadedSheet;
        Debug.Log($"SetCharacterSheet called. Incoming HP={characterSheet.CurrentHP}");

        // Recompute maxHP/AC/speed from loaded sheet
        RefreshDerivedCombatStats();

        // Final authoritative load: sheet -> runtime (clamped)
        currentHP = Mathf.Clamp(characterSheet.CurrentHP, 0, maxHP);

        // Keep sheet and runtime in sync after clamp
        characterSheet.CurrentHP = currentHP;

        Debug.Log($"After SetCharacterSheet: SheetHP={characterSheet.CurrentHP}, CurrentHP={CurrentHP}, MaxHP={maxHP}");

        if (AbilityUI.Instance != null)
        {
            AbilityUI.Instance.player = this;
            AbilityUI.Instance.RefreshAbilityButtons();
        }
    }

    public void ReviveFromLoad(bool resetCombatResources = true)
    {
        // 1) Ensure actor is active
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        // 2) Recompute derived stats from loaded sheet
        RefreshDerivedCombatStats();

        // 3) Ensure runtime HP is valid/alive
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        if (currentHP <= 0)
            currentHP = Mathf.Max(1, maxHP); // full heal fallback for death-load

        characterSheet.CurrentHP = currentHP;

        // 4) Clear transient statuses/effects
        statusManager?.Clear();

        // 5) Normalize turn state
        turnStarted = false;
        HasMove = false;
        HasAction = false;
        HasBonusAction = false;
        RemainingMovement = 0;

        // 6) Reset per-combat feature usage (optional but recommended for death-load)
        if (resetCombatResources)
        {
            SecondWindUsedThisCombat = false;
            ActionSurgeUsedThisCombat = false;
        }

        Debug.Log($"ReviveFromLoad: active={gameObject.activeSelf}, HP={currentHP}/{maxHP}, SWUsed={SecondWindUsedThisCombat}, ASUsed={ActionSurgeUsedThisCombat}");
    }
}
