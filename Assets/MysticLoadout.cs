using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MysticLoadout", menuName = "RPG/Classes/Mystic Loadout")]
public class MysticLoadout : ScriptableObject
{
    public WeaponItem startingWeapon;
    public ArmorItem startingArmor;
    public ShieldItem startingShield;

    [Header("Spells")]
    public List<SpellDefinition> level0Spells = new();
    public List<SpellDefinition> level1Spells = new();

    public Sprite classSprite; // add this
}
