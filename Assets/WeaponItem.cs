using UnityEngine;

public enum DamageType
{
   Slashing,
   Piercing,
   Bludgeoning
}

[CreateAssetMenu(menuName = "RPG/Items/Weapon")]
public class WeaponItem : Item
{
   [Header("Attack")]
   public WeaponType weaponType = WeaponType.Melee;
   public int range = 1;
   public int damageBonus = 0;

   [Header("5e Weapon")]
   public string damageDice = "1d8";
   public string versatileDamageDice = "1d10";
   public DamageType damageType = DamageType.Slashing;
   public bool isMartial = true;

   // inline methods 
   public override bool CanEquip => true;
   public override EquipmentSlot? EquipSlot => EquipmentSlot.Weapon;

   public override void Use(ICombatant user, ICombatant target)
   {
       // Equipment system handles equipping weapons.
   }
}
