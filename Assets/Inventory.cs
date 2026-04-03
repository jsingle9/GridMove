using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    private List<InventoryItem> items = new List<InventoryItem>();
    private Weapon equippedWeapon;

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
        return equippedWeapon;
    }

    public void EquipWeapon(Weapon weapon)
    {
        equippedWeapon = weapon;
        Debug.Log($"Equipped {weapon.WeaponName}");
        InventoryUIManager.Instance.UpdateUI();
    }
}
