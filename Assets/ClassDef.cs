using System;

[Serializable]
public class ClassDef
{
    public string ClassId;          // "fighter"
    public int HitDie = 10;         // d10 for fighter
    public int Level1BaseHP = 10;   // usually equal to HitDie
}
