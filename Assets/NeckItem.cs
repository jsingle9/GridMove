using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Items/Neck")]
public class NeckItem : Item
{
    [Header("5e Neck")]
    //public string itemId = "amulet_basic";
    // Add any neck-specific stats here (e.g., AC bonus, resistance, etc.)

    public override bool CanEquip => true;
    public override EquipmentSlot? EquipSlot => EquipmentSlot.Neck;

    public override void Use(ICombatant user, ICombatant target)
    {
        // Route to equipment system
    }
}
