using UnityEngine;

public static class SavingThrowUtility
{
    public static int RollStrSaveTotal(ICombatant target)
    {
        int roll = DiceRoller.RollD20();
        int mod = GetStrMod(target);
        return roll + mod;
    }

    public static bool PassesStrSave(ICombatant target, int dc, out int roll, out int mod, out int total)
    {
        roll = DiceRoller.RollD20();
        mod = GetStrMod(target);
        total = roll + mod;
        return total >= dc;
    }

    private static int GetStrMod(ICombatant target)
    {
        // Real value when combatant is BoxMover
        if (target is BoxMover bm && bm.Sheet != null)
        {
            int str = bm.Sheet.Scores.STR;
            return Mathf.FloorToInt((str - 10) / 2f);
        }

        // Fallback for enemies/NPCs until they expose sheet or save stats
        return 1;
    }
}
