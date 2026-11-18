using UnityEngine;
public static class MageLoadoutApplier
{
    public static void ApplyTo(ICombatant combatant, MageLoadout loadout)
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
        // Mages don't typically have special spell setup here
        // Spells are handled via RebuildAbilities() in BoxMover
    }
}
