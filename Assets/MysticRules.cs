using UnityEngine;

public class MysticRules : IClassRules
{
    public CharacterClassType ClassType => CharacterClassType.Mystic;

    public void ApplyLevel1(CharacterSheet sheet)
    {
        if (sheet == null) return;

        sheet.Level = 1;

        // HP (mystic d8 at level 1)
        sheet.MaxHP = Mathf.Max(1, 8 + sheet.Scores.ModCON);
        sheet.CurrentHP = sheet.MaxHP;

        // Unarmored AC: psionic defense uses WIS modifier
        sheet.ArmorClass = Mathf.Max(1, 10 + sheet.Scores.ModWIS);

        // Base speed
        if (sheet.BaseSpeed <= 0) sheet.BaseSpeed = 6;

        // Spell slots: level 1 mystic gets 2 slots of 1st level
        sheet.SpellSlots.Slots[1].Max = 2;
        sheet.SpellSlots.Slots[1].Current = 2;

        // Level 1 features
        AddFeature(sheet, FeatureIds.PsionicFocus);
        AddFeature(sheet, FeatureIds.MindThrustI);
    }

    public void ApplyLevelUp(CharacterSheet sheet, int newLevel)
    {
        if (sheet == null) return;
        if (newLevel <= sheet.Level) return;

        for (int lvl = sheet.Level + 1; lvl <= newLevel; lvl++)
        {
            // d8 -> fixed 5 + CON mod per level
            int hpGain = Mathf.Max(1, 5 + sheet.Scores.ModCON);
            sheet.MaxHP += hpGain;

            if (lvl == 2)
            {
                AddFeature(sheet, FeatureIds.PsionicStrike);
                // Level 2: 3 slots of 1st level, 2 slots of 2nd level
                sheet.SpellSlots.Slots[1].Max = 3;
                sheet.SpellSlots.Slots[1].Current = 3;
                sheet.SpellSlots.Slots[2].Max = 2;
                sheet.SpellSlots.Slots[2].Current = 2;
            }

            if (lvl == 3)
            {
                AddFeature(sheet, FeatureIds.PsionicAura);
                // Level 3: 4 slots of 1st level, 2 slots of 2nd level
                sheet.SpellSlots.Slots[1].Max = 4;
                sheet.SpellSlots.Slots[1].Current = 4;
            }
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
