using UnityEngine;

public class InventoryInputHandler : MonoBehaviour
{
    void Update()
    {
        // I key to toggle inventory menu
        if (UnityEngine.InputSystem.Keyboard.current.iKey.wasPressedThisFrame)
        {
            ToggleInventoryMenu();
        }

        // Only process inventory actions if menu is open
        if (InventoryMenuUI.Instance.IsMenuOpen())
        {
            // Arrow keys to navigate
            if (UnityEngine.InputSystem.Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                InventoryMenuUI.Instance.SelectPrevious();
            }

            if (UnityEngine.InputSystem.Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                InventoryMenuUI.Instance.SelectNext();
            }

            // E key to equip weapon
            if (UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
            {
                HandleWeaponEquip();
            }

            // C key to consume potion
            if (UnityEngine.InputSystem.Keyboard.current.cKey.wasPressedThisFrame)
            {
                HandlePotionConsumption();
            }
        }
    }

    void ToggleInventoryMenu()
    {
        if (InventoryMenuUI.Instance.IsMenuOpen())
        {
            InventoryMenuUI.Instance.CloseMenu();
        }
        else
        {
            InventoryMenuUI.Instance.OpenMenu();
        }
    }

    void HandleWeaponEquip()
    {
        InventoryItem selectedItem = InventoryMenuUI.Instance.GetSelectedItem();

        if (selectedItem == null)
        {
            Debug.Log("No item selected");
            return;
        }

        if (!selectedItem.isWeapon)
        {
            Debug.Log("Selected item is not a weapon");
            return;
        }

        Inventory.Instance.EquipWeapon(selectedItem.weapon);
        Debug.Log($"Equipped {selectedItem.weapon.WeaponName}");
        InventoryUIManager.Instance.UpdateUI(); // Update stats display
    }

    void HandlePotionConsumption()
    {
        InventoryItem selectedItem = InventoryMenuUI.Instance.GetSelectedItem();

        if (selectedItem == null)
        {
            Debug.Log("No item selected");
            return;
        }

        if (selectedItem.isWeapon)
        {
            Debug.Log("Selected item is not consumable");
            return;
        }

        BoxMover player = FindFirstObjectByType<BoxMover>();

        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }

        selectedItem.potion.Use(player, player);
        Inventory.Instance.RemoveItem(selectedItem);
        Debug.Log("Consumed Healing Potion");
        InventoryMenuUI.Instance.OpenMenu(); // Refresh menu after consumption
        InventoryUIManager.Instance.UpdateUI(); // Update stats display
    }
}
