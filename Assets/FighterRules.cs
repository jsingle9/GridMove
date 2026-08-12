using UnityEngine;

public class FighterRules : IClassRules
{
    public CharacterClassType ClassType => CharacterClassType.Fighter;

    public void ApplyLevel1(CharacterSheet sheet)
    {
        // 5e Fighter level 1 baseline:
        // HP = 10 + CON mod
        sheet.MaxHP = Mathf.Max(1, 10 + sheet.Scores.ConMod);
        sheet.CurrentHP = sheet.MaxHP;

        // Temporary baseline AC (until armor system fully drives this)
        sheet.ArmorClass = Mathf.Max(1, 10 + sheet.Scores.DexMod);

        // Your grid movement default
        if (sheet.Speed <= 0) sheet.Speed = 6;
    }

    public void ApplyLevelUp(CharacterSheet sheet, int newLevel)
    {
        if (sheet == null) return;
        if (newLevel <= sheet.Level) return;

        for (int lvl = sheet.Level + 1; lvl <= newLevel; lvl++)
        {
            // Fighter hit die d10; using fixed average gain for predictability
            int hpGain = Mathf.Max(1, 6 + sheet.Scores.ConMod);
            sheet.MaxHP += hpGain;

            // Feature unlocks
            if (lvl == 2)
            {
                // Track in your own feature system/list when you add it
                // e.g., sheet.Features.Add("Action Surge");
            }
            else if (lvl == 3)
            {
                // e.g., sheet.Features.Add("Martial Archetype");
            }
        }

        sheet.Level = newLevel;
        sheet.CurrentHP = sheet.MaxHP; // common on level-up for demo feel
    }
}
