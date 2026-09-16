using System;
using System.Collections.Generic;
using UnityEngine;

public static class CharacterFactory
{
    // Assign once at runtime from a bootstrap MonoBehaviour
    public static ClassTemplateDatabase ClassDb { get; set; }

    public static CharacterSheet CreateCharacterByClass(string classId)
    {
        classId = string.IsNullOrWhiteSpace(classId) ? "fighter" : classId.ToLowerInvariant();

        // 1) Preferred: ScriptableObject template path
        if (ClassDb != null)
        {
            var template = ClassDb.Get(classId);
            if (template != null)
            {
                var fromTemplate = CreateFromTemplate(template);
                if (fromTemplate != null)
                {
                    Debug.Log($"CharacterFactory: created '{classId}' from ClassTemplate.");
                    return fromTemplate;
                }

                Debug.LogWarning($"CharacterFactory: template existed for '{classId}' but CreateFromTemplate returned null. Falling back.");
            }
            else
            {
                Debug.LogWarning($"CharacterFactory: no template found for '{classId}'. Falling back.");
            }
        }
        else
        {
            Debug.LogWarning("CharacterFactory: ClassDb is null. Falling back to hardcoded examples.");
        }

        // 2) Safe fallback: existing hardcoded creators
        return classId switch
        {
            "fighter" => CreateFighter_Example(),
            "mystic"  => CreateMystic_Example(),
            "mage"    => CreateMage_Example(),
            _         => CreateFighter_Example()
        };
    }

    public static CharacterSheet CreateFromTemplate(ClassTemplate t)
    {
        if (t == null)
        {
            Debug.LogError("CreateFromTemplate called with null template.");
            return null;
        }

        var c = new CharacterSheet
        {
            CharacterName = string.IsNullOrWhiteSpace(t.defaultName) ? "New Hero" : t.defaultName,
            ClassId = string.IsNullOrWhiteSpace(t.classId) ? "fighter" : t.classId.ToLowerInvariant(),
            SpeciesId = string.IsNullOrWhiteSpace(t.speciesId) ? "human" : t.speciesId,
            BackgroundId = string.IsNullOrWhiteSpace(t.backgroundId) ? "soldier" : t.backgroundId,

            Level = 1,
            Experience = 0,

            BaseSpeed = t.baseSpeed > 0 ? t.baseSpeed : 6,
            EquippedArmorId = "",
            HasShieldEquipped = false,

            SkillProficiencyIds = t.skillProficiencyIds != null
                ? new List<string>(t.skillProficiencyIds)
                : new List<string>(),

            SaveProficiencyIds = t.saveProficiencyIds != null
                ? new List<string>(t.saveProficiencyIds)
                : new List<string>(),

            FeatureIds = t.featureIds != null
                ? new List<string>(t.featureIds)
                : new List<string>(),

            LastUpdatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        // Ability scores
        c.Scores.STR = t.str;
        c.Scores.DEX = t.dex;
        c.Scores.CON = t.con;
        c.Scores.INT = t.intel;
        c.Scores.WIS = t.wis;
        c.Scores.CHA = t.cha;

        // Derived stats
        var classDef = RulesLookups.GetClassDef(c.ClassId);
        c.CurrentHP = RulesService.CalculateMaxHP(c, classDef);

        // TEMP compatibility shims
        c.MaxHP = c.CurrentHP;
        c.ArmorClass = RulesService.CalculateAC(c, RulesLookups.GetArmorDefOrNull((ArmorItem)null));
        c.Speed = RulesService.CalculateSpeed(c);

        return c;
    }

    // Keep/replace with your existing implemented versions:
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

        c.Scores.STR = 17; c.Scores.DEX = 12; c.Scores.CON = 15;
        c.Scores.INT = 10; c.Scores.WIS = 10; c.Scores.CHA = 12;

        var classDef = RulesLookups.GetClassDef(c.ClassId);
        c.CurrentHP = RulesService.CalculateMaxHP(c, classDef);

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

        c.Scores.STR = 10; c.Scores.DEX = 14; c.Scores.CON = 13;
        c.Scores.INT = 12; c.Scores.WIS = 16; c.Scores.CHA = 11;

        var classDef = RulesLookups.GetClassDef(c.ClassId);
        c.CurrentHP = RulesService.CalculateMaxHP(c, classDef);

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

        c.Scores.STR = 8; c.Scores.DEX = 14; c.Scores.CON = 13;
        c.Scores.INT = 16; c.Scores.WIS = 12; c.Scores.CHA = 11;

        var classDef = RulesLookups.GetClassDef(c.ClassId);
        c.CurrentHP = RulesService.CalculateMaxHP(c, classDef);

        c.MaxHP = c.CurrentHP;
        c.ArmorClass = RulesService.CalculateAC(c, RulesLookups.GetArmorDefOrNull((ArmorItem)null));
        c.Speed = RulesService.CalculateSpeed(c);

        c.LastUpdatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return c;
    }
}
