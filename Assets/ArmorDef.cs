using System;

[Serializable]
public class ArmorDef
{
    public string ArmorId;          // "chain_mail"
    public int BaseAC = 10;         // leather 11, chain mail 16, etc.
    public DexContribution DexRule = DexContribution.Full;
    public int DexCap = 99;         // used when DexRule == Capped
    public bool IsHeavy = false;
}
