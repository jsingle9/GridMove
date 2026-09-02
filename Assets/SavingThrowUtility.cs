using UnityEngine;

public static class SavingThrowUtility
{
    public static int RollSave(ICombatant target, AbilityScoreType stat)
    {
        if (target == null || target.Sheet == null)
            return Random.Range(1, 21); // fallback d20

        int score = GetScore(target.Sheet.Scores, stat);
        int mod = Mathf.FloorToInt((score - 10) / 2f);

        // TODO: add proficiency for class saves later
        int d20 = Random.Range(1, 21);
        return d20 + mod;
    }

    private static int GetScore(AbilityScores scores, AbilityScoreType stat)
    {
        return stat switch
        {
            AbilityScoreType.STR => scores.STR,
            AbilityScoreType.DEX => scores.DEX,
            AbilityScoreType.CON => scores.CON,
            AbilityScoreType.INT => scores.INT,
            AbilityScoreType.WIS => scores.WIS,
            AbilityScoreType.CHA => scores.CHA,
            _ => 10
        };
    }
}
