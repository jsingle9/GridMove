using UnityEngine;

public class FighterRules : IClassRules
{
    public CharacterClassType ClassType => CharacterClassType.Fighter;

    public void ApplyLevel1(CharacterSheet sheet)
    {
        // 5e Fighter level 1 baseline:
        // HP = 10 + CON mod
        sheet.MaxHP = Mathf.Max(1, 10 + sheet.Scores.ConMod);
        sheet.CurrentHP = sheet.MaxHP;

        // Temporary baseline AC (until armor system fully drives this)
        sheet.ArmorClass = Mathf.Max(1, 10 + sheet.Scores.DexMod);

        // Your grid movement default
        if (sheet.Speed <= 0) sheet.Speed = 6;
    }
}
