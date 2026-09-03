public static class SaveStatUtility
{
    // Temporary: until save proficiencies + full sheet wiring are in.
    public static int GetEstimatedStrMod(ICombatant target)
    {
        // If target is BoxMover, we can estimate STR mod from current attack profile if needed.
        // For now return a baseline to prove save pipeline works.
        return 1;
    }
}
