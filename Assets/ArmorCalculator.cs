using UnityEngine;

public static class ArmorCalculator
{
    /// <summary>
    /// Computes final AC from armor + Dex modifier.
    /// If armor is null, uses unarmored base 10 + full Dex.
    /// </summary>
    public static int CalculateAC(ArmorItem equippedArmor, int dexModifier, int shieldBonus = 0)
    {
        ArmorDef armorDef = RulesLookups.GetArmorDefOrNull(equippedArmor);

        // Unarmored fallback
        if (armorDef == null)
            return 10 + dexModifier + Mathf.Max(0, shieldBonus);

        int dexToAC = GetDexContribution(armorDef, dexModifier);
        return armorDef.BaseAC + dexToAC + Mathf.Max(0, shieldBonus);
    }

    private static int GetDexContribution(ArmorDef armorDef, int dexModifier)
    {
        switch (armorDef.DexRule)
        {
            case DexContribution.None:
                return 0;

            case DexContribution.Capped:
                // Cap positive DEX; allow negative DEX to still penalize AC.
                int cap = Mathf.Max(0, armorDef.DexCap);
                return dexModifier > 0 ? Mathf.Min(dexModifier, cap) : dexModifier;

            case DexContribution.Full:
            default:
                return dexModifier;
        }
    }
}
