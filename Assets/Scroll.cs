using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Items/Consumables/Scroll")]
public abstract class Scroll : Item
{
    [Header("Scroll Effect")]
    public int potency = 10;  // Generic strength value

    public override bool CanEquip => false;
    public override EquipmentSlot? EquipSlot => null;

    public abstract override void Use(ICombatant user, ICombatant target);
}
