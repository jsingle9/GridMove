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
                HandleItemEquip();
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

    void HandleItemEquip()
    {
        Item selectedItem = InventoryMenuUI.Instance.GetSelectedItem();

        if (selectedItem == null)
        {
            Debug.Log("No item selected");
            return;
        }

        BoxMover player = FindFirstObjectByType<BoxMover>();
        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }

        if (!selectedItem.CanEquip)
        {
            Debug.Log("Selected item is not equippable");
            return;
        }

        if (!player.TryEquip(selectedItem))
        {
            Debug.Log("Equip failed");
            return;
        }

        Debug.Log($"Equipped {selectedItem.itemName}");
        InventoryUIManager.Instance.UpdateUI();
        InventoryMenuUI.Instance.OpenMenu();
    }

    void HandlePotionConsumption()
    {
        Item selectedItem = InventoryMenuUI.Instance.GetSelectedItem();

        if (selectedItem == null)
        {
            Debug.Log("No item selected");
            return;
        }

        BoxMover player = FindFirstObjectByType<BoxMover>();

        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }

        if (selectedItem.CanEquip)
        {
            Debug.Log("Selected item is equipment, not a consumable");
            return;
        }

        selectedItem.Use(player, player);

        if (selectedItem.consumable)
            player.RemoveItem(selectedItem);

        Debug.Log($"Used {selectedItem.itemName}");
        InventoryMenuUI.Instance.OpenMenu();
        InventoryUIManager.Instance.UpdateUI();
    }
}
