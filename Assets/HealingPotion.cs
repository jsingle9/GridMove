using UnityEngine;

[CreateAssetMenu(menuName = "Items/Healing Potion")]
public class HealingPotion : Item
{
    public int healAmount = 10;

    public override void Use(ICombatant user, ICombatant target)
    {
        target.Heal(healAmount);

        Debug.Log("{target} healed for " + healAmount);
    }
}
