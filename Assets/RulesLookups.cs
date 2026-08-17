public static class RulesLookups
{
    public static ClassDef GetClassDef(string classId)
    {
        // temp hardcoded defs for demo phase
        switch (classId)
        {
            case "fighter":
                return new ClassDef
                {
                    ClassId = "fighter",
                    HitDie = 10,
                    Level1BaseHP = 10
                };

            // sensible fallback
            default:
                return new ClassDef
                {
                    ClassId = string.IsNullOrWhiteSpace(classId) ? "unknown" : classId,
                    HitDie = 8,
                    Level1BaseHP = 8
                };
        }
    }

    public static ArmorDef GetArmorDefOrNull(string armorId)
    {
        if (string.IsNullOrWhiteSpace(armorId))
            return null; // unarmored

        switch (armorId)
        {
            // Light armor: full DEX
            case "leather":
                return new ArmorDef
                {
                    ArmorId = "leather",
                    BaseAC = 11,
                    DexRule = DexContribution.Full,
                    DexCap = 99,
                    IsHeavy = false
                };

            // Medium armor: DEX capped at +2
            case "chain_shirt":
                return new ArmorDef
                {
                    ArmorId = "chain_shirt",
                    BaseAC = 13,
                    DexRule = DexContribution.Capped,
                    DexCap = 2,
                    IsHeavy = false
                };

            // Heavy armor: no DEX
            case "chain_mail":
                return new ArmorDef
                {
                    ArmorId = "chain_mail",
                    BaseAC = 16,
                    DexRule = DexContribution.None,
                    DexCap = 0,
                    IsHeavy = true
                };

            default:
                // unknown armor ID => treat as unarmored for safety
                return null;
        }
    }
}
