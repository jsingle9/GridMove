using System;
using UnityEngine;

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
        c.Scores.DEX = 12;
        c.Scores.CON = 15;
        c.Scores.INT = 10;
        c.Scores.WIS = 10;
        c.Scores.CHA = 12;

        var classDef = RulesLookups.GetClassDef(c.ClassId);
        c.CurrentHP = RulesService.CalculateMaxHP(c, classDef);

        // TEMP compat fields
        c.MaxHP = c.CurrentHP;
        c.ArmorClass = RulesService.CalculateAC(c, RulesLookups.GetArmorDefOrNull((ArmorItem)null));
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
            BackgroundId = "sage",
            Level = 1,
            Experience = 0,
            BaseSpeed = 6,
            EquippedArmorId = "",
            HasShieldEquipped = false
        };

        c.Scores.STR = 10;
        c.Scores.DEX = 14;
        c.Scores.CON = 13;
        c.Scores.INT = 12;
        c.Scores.WIS = 16;
        c.Scores.CHA = 11;

        var classDef = RulesLookups.GetClassDef(c.ClassId);
        c.CurrentHP = RulesService.CalculateMaxHP(c, classDef);

        // TEMP compat fields
        c.MaxHP = c.CurrentHP;
        c.ArmorClass = RulesService.CalculateAC(c, RulesLookups.GetArmorDefOrNull((ArmorItem)null));
        c.Speed = RulesService.CalculateSpeed(c);

        c.LastUpdatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return c;
    }

    public static CharacterSheet CreateMage_Example()
    {
        var c = new CharacterSheet
        {
            CharacterName = "Mage One",
            ClassId = "mage",
            SpeciesId = "human",
            BackgroundId = "scholar",
            Level = 1,
            Experience = 0,
            BaseSpeed = 6,
            EquippedArmorId = "",
            HasShieldEquipped = false
        };

        c.Scores.STR = 8;
        c.Scores.DEX = 14;
        c.Scores.CON = 13;
        c.Scores.INT = 16;
        c.Scores.WIS = 12;
        c.Scores.CHA = 11;

        var classDef = RulesLookups.GetClassDef(c.ClassId);
        c.CurrentHP = RulesService.CalculateMaxHP(c, classDef);

        // TEMP compat fields
        c.MaxHP = c.CurrentHP;
        c.ArmorClass = RulesService.CalculateAC(c, RulesLookups.GetArmorDefOrNull((ArmorItem)null));
        c.Speed = RulesService.CalculateSpeed(c);

        c.LastUpdatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return c;
    }

    public static CharacterSheet CreateCharacterByClass(string classId)
    {
        return classId switch
        {
            "fighter" => CreateFighter_Example(),
            "mystic" => CreateMystic_Example(),
            "mage" => CreateMage_Example(),
            _ => CreateFighter_Example() // Default fallback
        };
    }
}
