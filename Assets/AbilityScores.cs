using UnityEngine;

[System.Serializable]
public class AbilityScores
{
    public int STR = 10;
    public int DEX = 10;
    public int CON = 10;
    public int INT = 10;
    public int WIS = 10;
    public int CHA = 10;

    public static int ToModifier(int score)
    {
        return Mathf.FloorToInt((score - 10) / 2f);
    }

    public int StrMod => ToModifier(STR);
    public int DexMod => ToModifier(DEX);
    public int ConMod => ToModifier(CON);
    public int IntMod => ToModifier(INT);
    public int WisMod => ToModifier(WIS);
    public int ChaMod => ToModifier(CHA);
}
