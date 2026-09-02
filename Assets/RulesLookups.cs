using UnityEngine;

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

    /// <summary>
    /// Converts an equipped ArmorItem asset into an ArmorDef used by rules math.
    /// Returns null when unarmored.
    /// </summary>
    public static ArmorDef GetArmorDefOrNull(ArmorItem armorItem)
    {
        if (armorItem == null)
            return null; // unarmored

        return new ArmorDef
        {
            // keep ArmorId if you still use it elsewhere (logs/saves/debug UI)
            ArmorId = string.IsNullOrWhiteSpace(armorItem.armorId)
                ? armorItem.name.ToLowerInvariant().Replace(" ", "_")
                : armorItem.armorId,

            BaseAC = armorItem.baseAC,
            DexRule = ToDexContribution(armorItem.category),
            DexCap = GetDexCap(armorItem),
            IsHeavy = armorItem.category == ArmorCategory.Heavy
        };
    }

    private static DexContribution ToDexContribution(ArmorCategory category)
    {
        switch (category)
        {
            case ArmorCategory.Light:
                return DexContribution.Full;

            case ArmorCategory.Medium:
                return DexContribution.Capped;

            case ArmorCategory.Heavy:
                return DexContribution.None;

            default:
                return DexContribution.Full;
        }
    }

    private static int GetDexCap(ArmorItem armorItem)
    {
        switch (armorItem.category)
        {
            case ArmorCategory.Light:
                return 99; // effectively uncapped

            case ArmorCategory.Medium:
                return Mathf.Max(0, armorItem.maxDexBonus); // usually 2

            case ArmorCategory.Heavy:
                return 0;

            default:
                return 99;
        }
    }
}
