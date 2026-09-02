public interface IEquipmentUser
{
    void AddItem(Item item);

    void EquipWeapon(WeaponItem weapon);
    void EquipArmor(ArmorItem armor);
    void EquipShield(ShieldItem shield);
}
