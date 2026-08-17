using System;
using System.Collections.Generic;

[Serializable]
public class CharacterSheet
{
    public string CharacterId = Guid.NewGuid().ToString();
    public string CharacterName = "New Hero";

    public string ClassId = "fighter";
    public string SpeciesId = "human";
    public string BackgroundId = "soldier";

    public int Level = 1;
    public int Experience = 0;

    public AbilityScores Scores = new();
    public int CurrentHP = 1;
    public int TempHP = 0;

    public string EquippedArmorId = "";
    public bool HasShieldEquipped = false;
    public int BaseSpeed = 6;

    public List<string> SkillProficiencyIds = new();
    public List<string> SaveProficiencyIds = new();
    public List<string> FeatureIds = new();

    public int DataVersion = 1;
    public long LastUpdatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public int ProficiencyBonus => 2 + ((Level - 1) / 4);

    // ---- TEMP COMPAT SHIMS ----
    public int MaxHP;       // TODO remove after RulesService migration
    public int ArmorClass;  // TODO remove after RulesService migration
    public int Speed
    {
        get => BaseSpeed;
        set => BaseSpeed = value;
    }

    public CharacterFeatureFlags Flags = new CharacterFeatureFlags(); // TODO migrate to FeatureIds
}
