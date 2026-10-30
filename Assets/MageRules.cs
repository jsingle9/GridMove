using UnityEngine;

public class MageRules : IClassRules
{
    public CharacterClassType ClassType => CharacterClassType.Mage;

    public void ApplyLevel1(CharacterSheet sheet)
    {
        if (sheet == null) return;

        sheet.Level = 1;

        // HP (mage d6 at level 1)
        sheet.MaxHP = Mathf.Max(1, 6 + sheet.Scores.ModCON);
        sheet.CurrentHP = sheet.MaxHP;

        // Unarmored AC (mages typically use dex-based AC)
        sheet.ArmorClass = Mathf.Max(1, 10 + sheet.Scores.ModDEX);

        // Base speed fallback
        if (sheet.BaseSpeed <= 0) sheet.BaseSpeed = 6;

        // Mage features
        AddFeature(sheet, FeatureIds.ArcaneRecovery);
    }

    public void ApplyLevelUp(CharacterSheet sheet, int newLevel)
    {
        if (sheet == null) return;
        if (newLevel <= sheet.Level) return;

        for (int lvl = sheet.Level + 1; lvl <= newLevel; lvl++)
        {
            // Mage gain (d6 -> 4) + CON mod
            int hpGain = Mathf.Max(1, 4 + sheet.Scores.ModCON);
            sheet.MaxHP += hpGain;

            // Add higher-level mage features here later
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
