using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyDefinition", menuName = "Combat/Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string enemyId = "enemy_default";
    [SerializeField] private string displayName = "Enemy";

    [Header("Base Stats")]
    [SerializeField] private int maxHP = 10;
    [SerializeField] private int armorClass = 12;
    [SerializeField] private int attackBonus = 4;
    [SerializeField] private string damageDice = "1d6";
    [SerializeField] private int damageModifier = 0;
    [SerializeField] private int speed = 6;

    [Header("Runtime Factory Keys")]
    [SerializeField] private List<string> abilityIds = new List<string>();
    [SerializeField] private List<string> lootIds = new List<string>();

    public string EnemyId => enemyId;
    public string DisplayName => displayName;
    public int MaxHP => maxHP;
    public int ArmorClass => armorClass;
    public int AttackBonus => attackBonus;
    public string DamageDice => damageDice;
    public int DamageModifier => damageModifier;
    public int Speed => speed;
    public IReadOnlyList<string> AbilityIds => abilityIds;
    public IReadOnlyList<string> LootIds => lootIds;
}
