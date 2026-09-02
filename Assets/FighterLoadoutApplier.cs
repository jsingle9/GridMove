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

        if (loadout.longsword != null)
        {
            eq.AddItem(loadout.longsword);
            eq.EquipWeapon(loadout.longsword);
        }

        if (loadout.scaleMail != null)
        {
            eq.AddItem(loadout.scaleMail);
            eq.EquipArmor(loadout.scaleMail);
        }

        if (loadout.shield != null)
        {
            eq.AddItem(loadout.shield);
            eq.EquipShield(loadout.shield);
        }
    }
}
