using UnityEngine;

/// <summary>
/// Tracks spell slots for a spellcasting class (Mage, Mystic, etc).
/// Slots are organized by level (0 = cantrips, 1-9 = spell levels).
/// </summary>
[System.Serializable]
public class SpellSlots
{
    [SerializeField] private int[] slotsPerLevel = new int[10];    // Max slots available at each level
    [SerializeField] private int[] usedSlots = new int[10];        // Slots already spent

    public SpellSlots()
    {
        // Initialize arrays
        slotsPerLevel = new int[10];
        usedSlots = new int[10];
    }

    /// <summary>
    /// Set the maximum available slots for a given spell level.
    /// </summary>
    public void SetSlots(int spellLevel, int count)
    {
        if (spellLevel < 0 || spellLevel > 9)
        {
            Debug.LogWarning($"Invalid spell level: {spellLevel}");
            return;
        }

        slotsPerLevel[spellLevel] = Mathf.Max(0, count);
    }

    /// <summary>
    /// Get the number of available (unspent) slots at a given level.
    /// </summary>
    public int GetAvailableSlots(int spellLevel)
    {
        if (spellLevel < 0 || spellLevel > 9)
            return 0;

        return slotsPerLevel[spellLevel] - usedSlots[spellLevel];
    }

    /// <summary>
    /// Try to spend a spell slot at the given level.
    /// Returns true if successful, false if no slots available.
    /// </summary>
    public bool TrySpendSlot(int spellLevel)
    {
        if (spellLevel < 0 || spellLevel > 9)
        {
            Debug.LogWarning($"Invalid spell level: {spellLevel}");
            return false;
        }

        if (GetAvailableSlots(spellLevel) > 0)
        {
            usedSlots[spellLevel]++;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Restore all spent slots (typically called on rest).
    /// </summary>
    public void RestoreAllSlots()
    {
        for (int i = 0; i < usedSlots.Length; i++)
            usedSlots[i] = 0;
    }

    /// <summary>
    /// Get total used slots at a given level.
    /// </summary>
    public int GetUsedSlots(int spellLevel)
    {
        if (spellLevel < 0 || spellLevel > 9)
            return 0;

        return usedSlots[spellLevel];
    }

    /// <summary>
    /// Get total max slots at a given level.
    /// </summary>
    public int GetMaxSlots(int spellLevel)
    {
        if (spellLevel < 0 || spellLevel > 9)
            return 0;

        return slotsPerLevel[spellLevel];
    }
}
