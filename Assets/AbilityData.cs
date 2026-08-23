using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Ability")]
public class AbilityData : ScriptableObject
{
    public string abilityName;
    public string damageDice;     // e.g. "1d8"
    public int attackBonus;
    public bool usesAttackRoll = true;
}
