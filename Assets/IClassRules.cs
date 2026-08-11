public interface IClassRules
{
    CharacterClassType ClassType { get; }
    void ApplyLevel1(CharacterSheet sheet);
}
