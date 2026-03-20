using UnityEngine;
using System.Collections.Generic;

public interface ICombatant
{
    int Initiative { get; set; }
    void StartTurn();
    void EndTurn();
    List<Ability> GetAbilities();

    int CurrentHP { get; }
    void TakeDamage(int amount);
    void Heal(int amount);
    bool IsDead();
    string Name { get; }
    bool HasMove { get; set; }
    bool HasAction { get; set; }
    bool HasBonusAction { get; set; }

    int ArmorClass { get; }
    int AttackBonus { get; }
    string DamageDice { get; }
    int DamageModifier { get; }
    int Speed { get; }
    int RemainingMovement { get; set; }

    void AddStatus(StatusEffect status);
    void RemoveStatus(StatusEffect status);
    int PreviewMoveCost(Intent intent);
    int CalculateMoveCost(List<GridNode> path);
    Vector3 GetWorldPosition();
    bool IsPlayerControlled();
}
