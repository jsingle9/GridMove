using System.Collections.Generic;

public interface IEquipmentUser
{
    void AddItem(Item item);
    void RemoveItem(Item item);

    IReadOnlyList<Item> GetInventoryItems();

    bool TryEquip(Item item);
    bool TryUnequip(EquipmentSlot slot);

    Item GetEquippedItem(EquipmentSlot slot);

    void EquipWeapon(WeaponItem weapon);
    void EquipArmor(ArmorItem armor);
    void EquipShield(ShieldItem shield);
}
