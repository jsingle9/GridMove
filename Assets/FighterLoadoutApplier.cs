using UnityEngine;

public static class FighterLoadoutApplier
{
    public static void ApplyTo(ICombatant combatant, FighterLoadout loadout)
    {
        if (combatant == null || loadout == null) return;

        if (combatant is not IEquipmentUser eq)
        {
            Debug.LogWarning($"{combatant.Name} does not implement IEquipmentUser.");
            return;
        }

        if (loadout.weapon != null)
        {
            eq.AddItem(loadout.weapon);
            eq.EquipWeapon(loadout.weapon);
        }

        if (loadout.armor != null)
        {
            eq.AddItem(loadout.armor);
            eq.EquipArmor(loadout.armor);
        }

        if (loadout.shield != null)
        {
            eq.AddItem(loadout.shield);
            eq.EquipShield(loadout.shield);
        }
    }
}
