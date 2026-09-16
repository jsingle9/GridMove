using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "RPG/Classes/Class Template")]
public class ClassTemplate : ScriptableObject
{
    [Header("Identity")]
    public string classId;              // fighter/mystic/mage
    public string defaultName;
    public string speciesId = "human";
    public string backgroundId;

    [Header("Starting Stats")]
    public int str = 10, dex = 10, con = 10, intel = 10, wis = 10, cha = 10;
    public int baseSpeed = 6;

    [Header("Proficiencies/Features")]
    public List<string> skillProficiencyIds = new();
    public List<string> saveProficiencyIds = new();
    public List<string> featureIds = new();

    [Header("Starting Equipment")]
    public WeaponItem startingWeapon;
    public ArmorItem startingArmor;
    public ShieldItem startingShield;

    [Header("Visual")]
    public Sprite classSprite;
}
