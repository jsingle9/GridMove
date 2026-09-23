using UnityEngine;

public static class WeaponFactory
{
    public static WeaponItem CreateRandomWeapon()
    {
        int roll = Random.Range(0, 4);

        WeaponItem weapon = ScriptableObject.CreateInstance<WeaponItem>();

        switch (roll)
        {
            case 0:
                weapon.itemName = "Two-Handed Sword";
                weapon.damageDice = "1d12";
                weapon.damageBonus = 3;
                weapon.weaponType = WeaponType.Melee;
                weapon.range = 1;
                weapon.damageType = DamageType.Slashing;
                weapon.isMartial = true;
                break;

            case 1:
                weapon.itemName = "Steel Axe";
                weapon.damageDice = "1d10";
                weapon.damageBonus = 3;
                weapon.weaponType = WeaponType.Melee;
                weapon.range = 1;
                weapon.damageType = DamageType.Slashing;
                weapon.isMartial = true;
                break;

            case 2:
                weapon.itemName = "Longsword";
                weapon.damageDice = "1d8";
                weapon.versatileDamageDice = "1d10";
                weapon.damageBonus = 2;
                weapon.weaponType = WeaponType.Melee;
                weapon.range = 1;
                weapon.damageType = DamageType.Slashing;
                weapon.isMartial = true;
                break;

            default:
                weapon.itemName = "Shortbow";
                weapon.damageDice = "1d6";
                weapon.damageBonus = 2;
                weapon.weaponType = WeaponType.Ranged;
                weapon.range = 6;
                weapon.damageType = DamageType.Piercing;
                weapon.isMartial = false;
                break;
        }

        return weapon;
    }
}
