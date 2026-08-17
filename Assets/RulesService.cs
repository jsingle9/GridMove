using System;
using UnityEngine;

public static class RulesService
{
    /// <summary>
    /// 5e-ish HP:
    /// Level 1 = class base HP + CON mod
    /// Higher levels = prior max + (fixed gain or roll) + CON mod
    /// For demo: fixed gain = (HitDie/2 + 1), e.g. d10 -> 6.
    /// </summary>
    public static int CalculateMaxHP(CharacterSheet c, ClassDef classDef)
    {
        if (c == null || classDef == null) return 1;

        int conMod = c.Scores.ModCON;
        int level = Mathf.Max(1, c.Level);

        int level1 = classDef.Level1BaseHP + conMod;
        int perLevelFixed = (classDef.HitDie / 2) + 1; // d10 => 6

        int hp = level1 + Mathf.Max(0, level - 1) * (perLevelFixed + conMod);
        return Mathf.Max(1, hp);
    }

    /// <summary>
    /// Default AC pipeline:
    /// base 10 (or armor base)
    /// + DEX contribution based on armor type
    /// + shield (+2)
    /// + feature bonuses (e.g., fighter defense style +1 while armored)
    /// </summary>
    public static int CalculateAC(CharacterSheet c, ArmorDef equippedArmorOrNull)
    {
        if (c == null) return 10;

        int dexMod = c.Scores.ModDEX;

        int baseAC;
        int dexToAdd;

        if (equippedArmorOrNull == null)
        {
            // Unarmored default
            baseAC = 10;
            dexToAdd = dexMod;
        }
        else
        {
            baseAC = equippedArmorOrNull.BaseAC;
            dexToAdd = equippedArmorOrNull.DexRule switch
            {
                DexContribution.None => 0,
                DexContribution.Full => dexMod,
                DexContribution.Capped => Mathf.Min(dexMod, equippedArmorOrNull.DexCap),
                _ => dexMod
            };
        }

        int shieldBonus = c.HasShieldEquipped ? 2 : 0;
        int featureBonus = CalculateFeatureACBonus(c, equippedArmorOrNull != null);

        return baseAC + dexToAdd + shieldBonus + featureBonus;
    }

    public static int CalculateSpeed(CharacterSheet c)
    {
        if (c == null) return 6;

        int speed = c.BaseSpeed;

        // future: apply conditions/features here
        // e.g., if (c.FeatureIds.Contains("mobile")) speed += 2;

        return Mathf.Max(0, speed);
    }

    private static int CalculateFeatureACBonus(CharacterSheet c, bool isArmored)
    {
        int bonus = 0;

        // Fighter fighting style: Defense (+1 AC while wearing armor)
        if (isArmored && c.FeatureIds.Contains("fighting_style_defense"))
            bonus += 1;

        return bonus;
    }
}
