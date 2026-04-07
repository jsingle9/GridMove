using UnityEngine;
using TMPro;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI inventoryDisplay;

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

    public void UpdateUI()
    {
        if (inventoryDisplay == null)
            return;

        string displayText = "Inventory:\n";

        foreach (var item in Inventory.Instance.GetItems())
        {
            displayText += $"- {item.GetDisplayName()}\n";
        }

        Weapon melee = Inventory.Instance.GetEquippedMeleeWeapon();
        Weapon ranged = Inventory.Instance.GetEquippedRangedWeapon();

        if (melee != null)
            displayText += $"\nEquipped (Melee): {melee.WeaponName}";

        if (ranged != null)
            displayText += $"\nEquipped (Ranged): {ranged.WeaponName}";

        inventoryDisplay.text = displayText;
    }
}
