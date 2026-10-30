public static class ClassRulesFactory
{
    public static IClassRules Create(CharacterClassType classType)
    {
        switch (classType)
        {
            case CharacterClassType.Fighter: return new FighterRules();
            case CharacterClassType.Mystic:  return new MysticRules();
            case CharacterClassType.Mage: return new MageRules();
            default: return null;
        }
    }
}
