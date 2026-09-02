using UnityEngine;

public enum DamageType { Slashing, Piercing, Bludgeoning }

[CreateAssetMenu(menuName = "RPG/Items/Weapon")]
public class WeaponItem : Item
{
    [Header("5e Weapon")]
    public string damageDice = "1d8";           // longsword 1H
    public string versatileDamageDice = "1d10"; // longsword 2H
    public DamageType damageType = DamageType.Slashing;
    public bool isMartial = true;

    public override void Use(ICombatant user, ICombatant target)
    {
        // Usually not "used" like a potion.
        // You can leave empty for now or route to Equip system later.
    }
}
