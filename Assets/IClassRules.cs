public interface IClassRules
{
    CharacterClassType ClassType { get; }
    void ApplyLevel1(CharacterSheet sheet);
    void ApplyLevelUp(CharacterSheet sheet, int newLevel);
}
