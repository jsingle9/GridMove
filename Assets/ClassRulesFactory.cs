public static class ClassRulesFactory
{
    public static IClassRules Create(CharacterClassType classType)
    {
        switch (classType)
        {
            case CharacterClassType.Fighter: return new FighterRules();
            default: return null;
        }
    }
}
