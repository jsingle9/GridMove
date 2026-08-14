using UnityEngine;

[System.Serializable]
public class CharacterSheet
{
    [Header("Identity")]
    public string CharacterName = "New Hero";
    public string Ancestry;   // later: enum if you want strict typing
    public string Background; // later: enum/scriptable data

    [Header("Progression")]
    public int Level = 1;
    public int Experience = 0;

    [Header("Core Stats")]
    public AbilityScores Scores = new AbilityScores();

    [Header("Combat Snapshot")]
    public int MaxHP = 1;
    public int CurrentHP = 1;
    public int ArmorClass = 10;
    public int Speed = 6; // your grid-oriented movement budget

    // 5e progression: 2 at lv1-4, 3 at 5-8, 4 at 9-12, etc.
    public int ProficiencyBonus => 2 + ((Level - 1) / 4);
    public CharacterFeatureFlags Flags = new CharacterFeatureFlags();
}
