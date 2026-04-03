using UnityEngine;

public class InventoryInputHandler : MonoBehaviour
{
    void Update()
    {
        // C key to consume potion
        if (UnityEngine.InputSystem.Keyboard.current.cKey.wasPressedThisFrame)
        {
            HandlePotionConsumption();
        }

        // E key to equip weapon
        if (UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
        {
            HandleWeaponEquip();
        }
    }

    void HandlePotionConsumption()
    {
        var items = Inventory.Instance.GetItems();

        foreach (var item in items)
        {
            if (!item.isWeapon)
            {
                item.potion.Use(null, null); // You may need to adjust this based on your Use() method
                Inventory.Instance.RemoveItem(item);
                Debug.Log("Consumed Healing Potion");
                break;
            }
        }
    }

    void HandleWeaponEquip()
    {
        var items = Inventory.Instance.GetItems();

        foreach (var item in items)
        {
            if (item.isWeapon)
            {
                Inventory.Instance.EquipWeapon(item.weapon);
                Debug.Log($"Equipped {item.weapon.WeaponName}");
                break;
            }
        }
    }
}
