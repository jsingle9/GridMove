using UnityEngine;

public class Weapon
{
    public string WeaponName;
    public int DamageBonus;
    public string DamageDice;

    public Weapon(string name, int bonus, string dice)
    {
        WeaponName = name;
        DamageBonus = bonus;
        DamageDice = dice;
    }

    public static Weapon CreateRandomWeapon()
    {
        int roll = Random.Range(0, 3);

        switch(roll)
        {
            case 0:
                return new Weapon("Two Handed Sword", 3, "1d12");
            case 1:
                return new Weapon("Steel Axe", 3, "1d10");
            case 2:
                return new Weapon("Longsword", 2, "1d10");
            default:
                return new Weapon("Iron Sword", 2, "1d8");
        }
    }
}
