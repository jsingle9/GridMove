using System;

public static class CharacterFactory
{
    public static CharacterSheet CreateFighter_Example()
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
            EquippedArmorId = "",
            HasShieldEquipped = false
        };

        c.Scores.STR = 17;
        c.Scores.DEX = 10;
        c.Scores.CON = 15;
        c.Scores.INT = 10;
        c.Scores.WIS = 10;
        c.Scores.CHA = 10;

        if (!c.FeatureIds.Contains(FeatureIds.SecondWind))
            c.FeatureIds.Add(FeatureIds.SecondWind);

        var classDef = RulesLookups.GetClassDef(c.ClassId);
        c.CurrentHP = RulesService.CalculateMaxHP(c, classDef);

        // TEMP compat fields
        c.MaxHP = c.CurrentHP;
        c.ArmorClass = RulesService.CalculateAC(c, RulesLookups.GetArmorDefOrNull(c.EquippedArmorId));
        c.Speed = RulesService.CalculateSpeed(c);

        c.LastUpdatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return c;
    }

    public static CharacterSheet CreateMystic_Example()
    {
        var c = new CharacterSheet
        {
            CharacterName = "Mystic One",
            ClassId = "mystic",
            SpeciesId = "human",
            BackgroundId = "hermit",
            Level = 1,
            Experience = 0,
            BaseSpeed = 6,
            EquippedArmorId = "",
            HasShieldEquipped = false
        };

        c.Scores.STR = 8;
        c.Scores.DEX = 12;
        c.Scores.CON = 13;
        c.Scores.INT = 16;
        c.Scores.WIS = 15;
        c.Scores.CHA = 10;

        var rules = new MysticRules();
        rules.ApplyLevel1(c);

        var classDef = RulesLookups.GetClassDef(c.ClassId);
        c.CurrentHP = RulesService.CalculateMaxHP(c, classDef);

        // TEMP compat fields
        c.MaxHP = c.CurrentHP;
        c.ArmorClass = RulesService.CalculateAC(c, RulesLookups.GetArmorDefOrNull(c.EquippedArmorId));
        c.Speed = RulesService.CalculateSpeed(c);

        c.LastUpdatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return c;
    }
}
