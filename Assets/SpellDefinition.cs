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
    public SpellAreaShape areaShape = SpellAreaShape.Single;
    [Min(0)]
    public int coneLength = 0;
    [Range(1f, 179f)]
    public float coneAngleDegrees = 60f;
    [Tooltip("Optional minimum range in tiles before cone can begin.")]
    [Min(0)]
    public int coneMinRange = 0;

    [Header("Effect")]
    public SpellEffectType effectType;
    public string damageDice;
    public string damageType;

    [Header("Presentation")]
    public Sprite icon;
}
