using UnityEngine;

[CreateAssetMenu(
    fileName = "SpellDefinition",
    menuName = "Game/Spells/Spell Definition"
)]
public class SpellDefinition : ScriptableObject
{
    [Header("Identity")]
    public string spellId;
    public string displayName;

    [TextArea]
    public string description;

    [Header("Spell Slot Cost")]
    [Tooltip("0 = cantrip; 1+ = consumes one spell slot of this level.")]
    [Range(0, 9)]
    public int spellLevel;

    [Header("Targeting")]
    public TargetingMode targetingMode;
    public float range;
    public int radius;

    [Header("Effect")]
    public SpellEffectType effectType;
    public string damageDice;
    public string damageType;

    [Header("Presentation")]
    public Sprite icon;
}
