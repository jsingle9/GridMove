using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Items/Shield")]
public class ShieldItem : Item
{
    [Header("5e Shield")]
    public int acBonus = 2;

    public override bool CanEquip => true;
    public override EquipmentSlot? EquipSlot => EquipmentSlot.Shield;

    public override void Use(ICombatant user, ICombatant target)
    {
        // Route to Equip(user, this) later.
    }
}
