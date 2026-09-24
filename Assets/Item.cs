using UnityEngine;

public abstract class Item : ScriptableObject
{
    [Header("Identity")]
    public string itemId;
    public string itemName;
    [TextArea] public string description;

    [Header("Behavior")]
    public bool consumable = true;

    public virtual bool CanEquip => false;
    public virtual EquipmentSlot? EquipSlot => null;

    public abstract void Use(ICombatant user, ICombatant target);
}
