using UnityEngine;

public static class SavingThrowUtility
{
    public enum AbilityScore { STR, DEX, CON, INT, WIS, CHA }

    public static int RollSaveTotal(ICombatant target, AbilityScore ability)
    {
        int roll = DiceRoller.RollD20();
        int mod = GetAbilityMod(target, ability);
        return roll + mod;
    }

    public static bool PassesSave(ICombatant target, int dc, AbilityScore ability, out int roll, out int mod, out int total)
    {
        roll = DiceRoller.RollD20();
        mod = GetAbilityMod(target, ability);
        total = roll + mod;
        return total >= dc;
    }

    // Keep old STR-specific methods for backwards compatibility
    public static int RollStrSaveTotal(ICombatant target)
    {
        return RollSaveTotal(target, AbilityScore.STR);
    }

    public static bool PassesStrSave(ICombatant target, int dc, out int roll, out int mod, out int total)
    {
        return PassesSave(target, dc, AbilityScore.STR, out roll, out mod, out total);
    }

    private static int GetAbilityMod(ICombatant target, AbilityScore ability)
    {
        int abilityScore = GetAbilityScore(target, ability);
        return Mathf.FloorToInt((abilityScore - 10) / 2f);
    }

    private static int GetAbilityScore(ICombatant target, AbilityScore ability)
    {
        // Real value when combatant is BoxMover
        if (target is BoxMover bm && bm.Sheet != null)
        {
            return ability switch
            {
                AbilityScore.STR => bm.Sheet.Scores.STR,
                AbilityScore.DEX => bm.Sheet.Scores.DEX,
                AbilityScore.CON => bm.Sheet.Scores.CON,
                AbilityScore.INT => bm.Sheet.Scores.INT,
                AbilityScore.WIS => bm.Sheet.Scores.WIS,
                AbilityScore.CHA => bm.Sheet.Scores.CHA,
                _ => 10
            };
        }

        // Fallback for enemies/NPCs until they expose sheet or save stats
        return 10;
    }
}
