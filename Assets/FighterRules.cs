using UnityEngine;

public class FighterRules : IClassRules
{
    public CharacterClassType ClassType => CharacterClassType.Fighter;

    public void ApplyLevel1(CharacterSheet sheet)
    {
        sheet.Level = 1;
        sheet.MaxHP = Mathf.Max(1, 10 + sheet.Scores.ConMod);
        sheet.CurrentHP = sheet.MaxHP;
        sheet.ArmorClass = Mathf.Max(1, 10 + sheet.Scores.DexMod);
        if (sheet.Speed <= 0) sheet.Speed = 6;

        sheet.Flags.HasSecondWind = true;   // level 1
        sheet.Flags.HasActionSurge = false; // not yet
    }

    public void ApplyLevelUp(CharacterSheet sheet, int newLevel)
    {
        if (sheet == null) return;
        if (newLevel <= sheet.Level) return;

        for (int lvl = sheet.Level + 1; lvl <= newLevel; lvl++)
        {
            int hpGain = Mathf.Max(1, 6 + sheet.Scores.ConMod);
            sheet.MaxHP += hpGain;

            if (lvl == 2)
                sheet.Flags.HasActionSurge = true;
            // additional features gained at higher levels to follow 
        }

        sheet.Level = newLevel;
        sheet.CurrentHP = sheet.MaxHP;
    }
}
