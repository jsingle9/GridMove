using UnityEngine;

[CreateAssetMenu(
    fileName = "SpellDefinition",
    menuName = "RPG/Spells/Spell Definition"
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
    [Tooltip("Damage dice (e.g., '2d6', '3d6'). For upcast spells, this is the base; extras scale per level slot.")]
    public string damageDice;
    [Tooltip("Which ability score to save against (STR, DEX, CON, INT, WIS, CHA)")]
    public SavingThrowUtility.AbilityScore saveAbility = SavingThrowUtility.AbilityScore.DEX;

    [Tooltip("Optional: extra dice per spell slot level above base (e.g., Fireball adds 1d6 per slot above 3).")]
    public string upcastDamageBonus;  // e.g., "1d6" means +1d6 per slot level above base level
    public string damageType;

    [Tooltip("What happens on a failed save (e.g., 'Lose Reaction', 'Prone', etc)")]
    public string failureEffect = "Lose Reaction";

    [Tooltip("DC for the saving throw")]
    [Min(8)]
    public int saveDC = 12;  // Default DC


    [Header("Presentation")]
    public Sprite icon;
}
