using UnityEngine;

public static class AbilityResolver
{
    public static void Cast(BoxMover caster, BoxMover target, AbilityData ability, RollMode mode)
    {
        if (caster == null || target == null || ability == null) return;

        CombatEvents.Log($"{caster.Sheet.CharacterName} uses {ability.abilityName} on {target.Sheet.CharacterName}.");

        if (ability.usesAttackRoll)
        {
            var roll = DiceRoller.RollD20(mode);
            int total = roll.KeptDie + ability.attackBonus;

            if (roll.Mode == RollMode.Advantage)
                CombatEvents.Log($"Advantage roll: {roll.FirstDie}/{roll.SecondDie} -> {roll.KeptDie} (+{ability.attackBonus}) = {total}");
            else if (roll.Mode == RollMode.Disadvantage)
                CombatEvents.Log($"Disadvantage roll: {roll.FirstDie}/{roll.SecondDie} -> {roll.KeptDie} (+{ability.attackBonus}) = {total}");
            else
                CombatEvents.Log($"Attack roll: {roll.KeptDie} (+{ability.attackBonus}) = {total}");

            // Replace with your real AC check:
            bool hit = total >= 10;

            if (!hit)
            {
                CombatEvents.Log("Miss.");
                return;
            }
        }

        int dmg = DiceRoller.Roll(ability.damageDice);
        target.TakeDamage(dmg); // assumes you already have this
        CombatEvents.Log($"Hit for {dmg} damage.");
    }
}
