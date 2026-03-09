using UnityEngine;

public abstract class Item : ScriptableObject
{
    public string itemName;
    public string description;
    public bool consumable = true;

    public abstract void Use(ICombatant user, ICombatant target);
}
