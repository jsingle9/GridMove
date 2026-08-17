using UnityEngine;

public class FighterRules : IClassRules
{
    public CharacterClassType ClassType => CharacterClassType.Fighter;

    public void ApplyLevel1(CharacterSheet sheet)
    {
        if (sheet == null) return;

        sheet.Level = 1;

        // HP (fighter d10 at level 1)
        sheet.MaxHP = Mathf.Max(1, 10 + sheet.Scores.ModCON);
        sheet.CurrentHP = sheet.MaxHP;

        // Unarmored fallback AC for now (real AC should come from RulesService + armor later)
        sheet.ArmorClass = Mathf.Max(1, 10 + sheet.Scores.ModDEX);

        // Base speed fallback
        if (sheet.BaseSpeed <= 0) sheet.BaseSpeed = 6;

        // New feature system
        AddFeature(sheet, FeatureIds.SecondWind);

        // If you want Defense style at level 1 by default (or set via choice flow):
        // AddFeature(sheet, FeatureIds.FightingStyleDefense);
    }

    public void ApplyLevelUp(CharacterSheet sheet, int newLevel)
    {
        if (sheet == null) return;
        if (newLevel <= sheet.Level) return;

        for (int lvl = sheet.Level + 1; lvl <= newLevel; lvl++)
        {
            // Fixed fighter gain (d10 -> 6) + CON mod
            int hpGain = Mathf.Max(1, 6 + sheet.Scores.ModCON);
            sheet.MaxHP += hpGain;

            if (lvl == 2)
                AddFeature(sheet, FeatureIds.ActionSurge);

            // Add higher-level fighter features here later
        }

        sheet.Level = newLevel;
        sheet.CurrentHP = sheet.MaxHP;
    }

    private static void AddFeature(CharacterSheet sheet, string featureId)
    {
        if (string.IsNullOrWhiteSpace(featureId)) return;
        if (!sheet.FeatureIds.Contains(featureId))
            sheet.FeatureIds.Add(featureId);
    }
}
