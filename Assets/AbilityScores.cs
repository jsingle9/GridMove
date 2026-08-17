using System;

[Serializable]
public class AbilityScores
{
    public int STR = 10;
    public int DEX = 10;
    public int CON = 10;
    public int INT = 10;
    public int WIS = 10;
    public int CHA = 10;

    public int ModSTR => (STR - 10) / 2;
    public int ModDEX => (DEX - 10) / 2;
    public int ModCON => (CON - 10) / 2;
    public int ModINT => (INT - 10) / 2;
    public int ModWIS => (WIS - 10) / 2;
    public int ModCHA => (CHA - 10) / 2;

    // ---- TEMP COMPAT ALIASES ----
    public int StrMod => ModSTR;
    public int DexMod => ModDEX;
    public int ConMod => ModCON;
    public int IntMod => ModINT;
    public int WisMod => ModWIS;
    public int ChaMod => ModCHA;
}
