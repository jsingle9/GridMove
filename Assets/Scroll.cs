using UnityEngine;

public enum ScrollType
{
    MagicMissile,
    Fireball,
    IceStorm,
    Lightning,
    Heal,
    Teleport
}

[CreateAssetMenu(menuName = "RPG/Items/Consumables/Scroll")]
public class Scroll : Item
{
    [Header("Scroll Effect")]
    public ScrollType scrollType = ScrollType.MagicMissile;
    public int spellPower = 10;
    public int spellRange = 6;

    public override bool CanEquip => false;
    public override EquipmentSlot? EquipSlot => null;

    public override void Use(ICombatant user, ICombatant target)
    {
        switch (scrollType)
        {
            case ScrollType.MagicMissile:
                target.TakeDamage(spellPower);
                Debug.Log($"{target} hit by Magic Missile for {spellPower} damage");
                break;

            case ScrollType.Fireball:
                target.TakeDamage(spellPower + 5);
                Debug.Log($"{target} hit by Fireball for {spellPower + 5} damage");
                break;

            case ScrollType.IceStorm:
                target.TakeDamage(spellPower);
                Debug.Log($"{target} hit by Ice Storm for {spellPower} damage");
                break;

            case ScrollType.Lightning:
                target.TakeDamage(spellPower + 3);
                Debug.Log($"{target} hit by Lightning for {spellPower + 3} damage");
                break;

            case ScrollType.Heal:
                target.Heal(spellPower);
                Debug.Log($"{target} healed by scroll for {spellPower} HP");
                break;

            case ScrollType.Teleport:
                Debug.Log($"{user} cast Teleport from scroll");
                break;
        }
    }
}
