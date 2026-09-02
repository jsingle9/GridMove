using UnityEngine;

public enum ArmorCategory { Light, Medium, Heavy }

[CreateAssetMenu(menuName = "RPG/Items/Armor")]
public class ArmorItem : Item
{
    [Header("5e Armor")]
    public ArmorCategory category = ArmorCategory.Light;
    public string armorId = "leather_armor";
    public int baseAC = 11; // leather default
    public int maxDexBonus = 99;
    public bool stealthDisadvantage = false;
    public int strengthRequirement = 0;

    public override void Use(ICombatant user, ICombatant target)
    {
        // Route to Equip(user, this) once your equipment manager exists.
    }
}
