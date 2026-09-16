using UnityEngine;

[CreateAssetMenu(fileName = "MysticLoadout", menuName = "RPG/Classes/Mystic Loadout")]
public class MysticLoadout : ScriptableObject
{
    public WeaponItem startingWeapon;
    public ArmorItem startingArmor;
    public ShieldItem startingShield;

    public Sprite classSprite; // add this
}
