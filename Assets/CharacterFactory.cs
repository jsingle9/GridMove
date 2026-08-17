using System;

public static class CharacterFactory
{
    public static CharacterSheet CreateFirstFighterSave()
    {
        var c = new CharacterSheet
        {
            CharacterName = "Fighter One",
            ClassId = "fighter",
            SpeciesId = "human",
            BackgroundId = "soldier",
            Level = 1,
            Experience = 0,
            BaseSpeed = 6,
            EquippedArmorId = "",      // unarmored for now
            HasShieldEquipped = false
        };

        c.Scores.STR = 17;
        c.Scores.DEX = 10;
        c.Scores.CON = 15;
        c.Scores.INT = 10;
        c.Scores.WIS = 10;
        c.Scores.CHA = 10;

        // Level 1 fighter features
        if (!c.FeatureIds.Contains(FeatureIds.SecondWind))
            c.FeatureIds.Add(FeatureIds.SecondWind);

        // Optional if you want Defense style from the start:
        // c.FeatureIds.Add(FeatureIds.FightingStyleDefense);

        // Initialize HP from rules
        var classDef = RulesLookups.GetClassDef(c.ClassId);
        int maxHp = RulesService.CalculateMaxHP(c, classDef);
        c.CurrentHP = maxHp;

        // TEMP compat fields (while still present)
        c.MaxHP = maxHp;
        c.ArmorClass = RulesService.CalculateAC(c, RulesLookups.GetArmorDefOrNull(c.EquippedArmorId));
        c.Speed = RulesService.CalculateSpeed(c);

        c.LastUpdatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return c;
    }
}
