using UnityEngine;
using System.Collections.Generic;

public interface ICombatant{
    int Initiative { get; set; }
    void StartTurn();
    void EndTurn();
    List<Ability> GetAbilities();

    int CurrentHP { get;}
    void TakeDamage(int amount);
    bool IsDead();

    bool HasMove { get; set; }
    bool HasAction { get; set; }
    bool HasBonusAction { get; set; }

    int ArmorClass { get; }
    int AttackBonus { get; }
    string DamageDice { get; }
    int DamageModifier { get; }

    void SetIntent(Intent intent);
    Vector3 GetWorldPosition();

}
