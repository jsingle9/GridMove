using UnityEngine;
using System.Collections.Generic;

public class Enemy : MonoBehaviour, ICombatant
{
    DynamicObstacle dynamicObstacle;
    [SerializeField] GridController grid;
    [SerializeField] int maxHP = 18;
    int currentHP;

    public int Initiative { get; set; }
    public bool HasMove { get; set; }
    public bool HasAction { get; set; }
    public bool HasBonusAction { get; set; }

    /*int ArmorClass { get; }
    int AttackBonus { get; }
    string DamageDice { get; }
    int DamageModifier { get; }*/

    [SerializeField] int armorClass = 12;
    [SerializeField] int attackBonus = 4;
    [SerializeField] string damageDice = "1d6";
    [SerializeField] int damageModifier = 2;



    List<Ability> abilities = new List<Ability>();

    void Awake(){
        currentHP = maxHP;
        dynamicObstacle = GetComponent<DynamicObstacle>();

        // Give enemy a default attack ability
        abilities.Add(new AttackAbility(null));
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

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger entered by: " + other.name);
        if (other.GetComponent<BoxMover>())
        {
            GameStateManager.Instance.EnterCombat();
        }
    }

    // =========================
    // TURN SYSTEM
    // =========================

    public void StartTurn()
    {
        Debug.Log("Enemy turn started");

        HasMove = true;
        HasAction = true;
        HasBonusAction = true;

        // TEMP basic AI: wait then end turn
        Invoke(nameof(FinishTurn), 1f);
    }

    public void EndTurn()
    {
        Debug.Log("Enemy turn ended");
    }

    void FinishTurn()
    {
        CombatManager.Instance.EndTurn();
    }

    // =========================
    // ABILITIES
    // =========================

    public List<Ability> GetAbilities()
    {
        return abilities;
    }

    // =========================
    // INTENT SYSTEM
    // =========================

    public void SetIntent(Intent intent)
    {
        // later AI will use this
        // for now you can ignore
    }

    // =========================
    // POSITION
    // =========================

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

    void Die()
    {
      Debug.Log($"{name} died");

      CombatManager.Instance.NotifyDeath(this);
      gameObject.SetActive(false);
    }
}
