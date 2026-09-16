using UnityEngine;

public static class MysticLoadoutApplier
{
    // Optional spriteRenderer lets you give immediate visual feedback
    public static void ApplyTo(ICombatant combatant, MysticLoadout loadout, SpriteRenderer spriteRenderer = null)
    {
        if (combatant == null || loadout == null) return;

        if (combatant is not IEquipmentUser eq)
        {
            Debug.LogWarning($"{combatant.Name} does not implement IEquipmentUser.");
            return;
        }

        if (loadout.startingWeapon != null)
        {
            eq.AddItem(loadout.startingWeapon);
            eq.EquipWeapon(loadout.startingWeapon);
        }

        if (loadout.startingArmor != null)
        {
            eq.AddItem(loadout.startingArmor);
            eq.EquipArmor(loadout.startingArmor);
        }

        if (loadout.startingShield != null)
        {
            eq.AddItem(loadout.startingShield);
            eq.EquipShield(loadout.startingShield);
        }

        // Visual feedback
        if (spriteRenderer != null && loadout.classSprite != null)
        {
            spriteRenderer.sprite = loadout.classSprite;
        }
    }
}
