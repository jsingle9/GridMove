using UnityEngine;

public static class ClassLoadoutApplier
{
    public static void ApplyTo(ICombatant combatant, ClassTemplate template, SpriteRenderer spriteRenderer = null)
    {
        if (combatant == null || template == null) return;

        if (combatant is not IEquipmentUser eq)
        {
            Debug.LogWarning($"{combatant.Name} does not implement IEquipmentUser.");
            return;
        }

        if (template.startingWeapon != null)
        {
            eq.AddItem(template.startingWeapon);
            eq.EquipWeapon(template.startingWeapon);
        }

        if (template.startingArmor != null)
        {
            eq.AddItem(template.startingArmor);
            eq.EquipArmor(template.startingArmor);
        }

        if (template.startingShield != null)
        {
            eq.AddItem(template.startingShield);
            eq.EquipShield(template.startingShield);
        }

        if (template.classSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = template.classSprite;
    }
}
