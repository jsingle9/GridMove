using UnityEngine;

public static class CharacterFactory
{
    // Optional runtime-set reference (assign once from a bootstrap MonoBehaviour)
    public static ClassTemplateDatabase ClassDb { get; set; }

    public static CharacterSheet CreateCharacterByClass(string classId)
    {
        // 1) Try new data-driven path
        if (ClassDb != null)
        {
            var template = ClassDb.Get(classId);
            if (template != null)
            {
                var fromTemplate = CreateFromTemplate(template);
                if (fromTemplate != null) return fromTemplate;
            }
        }

        // 2) Guaranteed fallback to old behavior
        return classId switch
        {
            "fighter" => CreateFighter_Example(),
            "mystic"  => CreateMystic_Example(),
            "mage"    => CreateMage_Example(),
            _         => CreateFighter_Example()
        };
    }

    public static CharacterSheet CreateFromTemplate(ClassTemplate t)
    {
        // your new SO-driven method
        // (same as we outlined previously)
        return null; // replace with implementation
    }

    // Keep these unchanged for now:
    public static CharacterSheet CreateFighter_Example() { /* existing */ return null; }
    public static CharacterSheet CreateMystic_Example()  { /* existing */ return null; }
    public static CharacterSheet CreateMage_Example()    { /* existing */ return null; }
}
