using System;
using System.Collections.Generic;

[Serializable]
public class SpellSlotContainer
{
    public Dictionary<int, SpellSlotLevel> Slots = new()
    {
        { 1, new SpellSlotLevel { Max = 0, Current = 0 } },
        { 2, new SpellSlotLevel { Max = 0, Current = 0 } },
        { 3, new SpellSlotLevel { Max = 0, Current = 0 } },
        { 4, new SpellSlotLevel { Max = 0, Current = 0 } },
        { 5, new SpellSlotLevel { Max = 0, Current = 0 } },
        { 6, new SpellSlotLevel { Max = 0, Current = 0 } },
        { 7, new SpellSlotLevel { Max = 0, Current = 0 } },
        { 8, new SpellSlotLevel { Max = 0, Current = 0 } },
        { 9, new SpellSlotLevel { Max = 0, Current = 0 } },
    };

    public bool HasSlot(int spellLevel)
    {
        return Slots.ContainsKey(spellLevel) && Slots[spellLevel].Current > 0;
    }

    public bool TrySpendSlot(int spellLevel)
    {
        if (!HasSlot(spellLevel))
            return false;

        Slots[spellLevel].Current--;
        return true;
    }

    public void RestoreSlots()
    {
        foreach (var kvp in Slots)
            kvp.Value.Current = kvp.Value.Max;
    }
}
