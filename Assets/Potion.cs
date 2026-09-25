using UnityEngine;

public enum PotionType
{
    Healing,
    Mana,
    Poison,
    Strength,
    Speed
}

[CreateAssetMenu(menuName = "RPG/Items/Consumables/Potion")]
public class Potion : Item
{
    [Header("Potion Effect")]
    public PotionType potionType = PotionType.Healing;
    public int potency = 10;

    public override bool CanEquip => false;
    public override EquipmentSlot? EquipSlot => null;

    public override void Use(ICombatant user, ICombatant target)
    {
        switch (potionType)
        {
            case PotionType.Healing:
                target.Heal(potency);
                Debug.Log($"{target} healed for {potency}");
                break;

            case PotionType.Mana:
                Debug.Log($"{target} restored {potency} mana");
                break;

            case PotionType.Poison:
                target.TakeDamage(potency);
                Debug.Log($"{target} poisoned for {potency} damage");
                break;

            case PotionType.Strength:
                Debug.Log($"{target} gained +{potency} strength");
                break;

            case PotionType.Speed:
                Debug.Log($"{target} gained +{potency} speed");
                break;
        }
    }
}
