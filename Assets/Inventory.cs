using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    private List<InventoryItem> items = new List<InventoryItem>();
    private Weapon equippedMeleeWeapon;
    private Weapon equippedRangedWeapon;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddWeapon(Weapon weapon)
    {
        items.Add(new InventoryItem(weapon));
        Debug.Log($"Added {weapon.WeaponName} to inventory");
        InventoryUIManager.Instance.UpdateUI();
    }

    public void AddPotion(HealingPotion potion)
    {
        items.Add(new InventoryItem(potion));
        Debug.Log($"Added Healing Potion to inventory");
        InventoryUIManager.Instance.UpdateUI();
    }

    public List<InventoryItem> GetItems()
    {
        return items;
    }

    public void RemoveItem(InventoryItem item)
    {
        items.Remove(item);
        InventoryUIManager.Instance.UpdateUI();
    }

    public Weapon GetEquippedWeapon()
    {
        // Return the currently active equipped weapon based on context
        // This will be set by the input handler based on ability selection
        return equippedMeleeWeapon ?? equippedRangedWeapon;
    }

    public Weapon GetEquippedMeleeWeapon()
    {
        return equippedMeleeWeapon;
    }

    public Weapon GetEquippedRangedWeapon()
    {
        return equippedRangedWeapon;
    }

    public void EquipWeapon(Weapon weapon)
    {
        if (weapon == null) return;

        if (weapon.WeaponType == WeaponType.Melee)
        {
            equippedMeleeWeapon = weapon;
            Debug.Log($"Equipped melee weapon: {weapon.WeaponName}");
        }
        else if (weapon.WeaponType == WeaponType.Ranged)
        {
            equippedRangedWeapon = weapon;
            Debug.Log($"Equipped ranged weapon: {weapon.WeaponName}");
        }

        InventoryUIManager.Instance.UpdateUI();
    }
}
