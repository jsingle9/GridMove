using UnityEngine;

public class Weapon
{
    public string WeaponName;
    public int DamageBonus;
    public string DamageDice;
    public WeaponType WeaponType;

    public Weapon(string name, int bonus, string dice, WeaponType type = WeaponType.Melee)
    {
        WeaponName = name;
        DamageBonus = bonus;
        DamageDice = dice;
        WeaponType = type;
    }

    public static Weapon CreateRandomWeapon()
    {
        int roll = Random.Range(0, 3);

        switch(roll)
        {
            case 0:
                return new Weapon("Two Handed Sword", 3, "1d12", WeaponType.Melee);
            case 1:
                return new Weapon("Steel Axe", 3, "1d10", WeaponType.Melee);
            case 2:
                return new Weapon("Longsword", 2, "1d10", WeaponType.Melee);
            default:
                return new Weapon("Iron Sword", 2, "1d8", WeaponType.Melee);
        }
    }
}
