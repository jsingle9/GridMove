using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Classes/Fighter Loadout")]
public class FighterLoadout : ScriptableObject
{
    public WeaponItem weapon;
    public ArmorItem armor;
    public ShieldItem shield;

    public Sprite classSprite;
}
