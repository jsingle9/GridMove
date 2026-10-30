using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MageLoadout", menuName = "RPG/Classes/Mage Loadout")]
public class MageLoadout : ScriptableObject
{
    public WeaponItem startingWeapon;
    public ArmorItem startingArmor;
    public ShieldItem startingShield;

    [Header("Spells")]
    public List<SpellDefinition> level0Spells = new();
    public List<SpellDefinition> level1Spells = new();

    public Sprite classSprite;
}
