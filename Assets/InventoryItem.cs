using UnityEngine;

public class InventoryItem
{
    public Weapon weapon;
    public HealingPotion potion;
    public bool isWeapon;

    public InventoryItem(Weapon w)
    {
        weapon = w;
        isWeapon = true;
    }

    public InventoryItem(HealingPotion p)
    {
        potion = p;
        isWeapon = false;
    }

    public string GetDisplayName()
    {
        return isWeapon ? weapon.WeaponName : "Healing Potion";
    }
}
