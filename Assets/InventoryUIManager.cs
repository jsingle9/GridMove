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

        if (Inventory.Instance.GetEquippedWeapon() != null)
        {
            displayText += $"\nEquipped: {Inventory.Instance.GetEquippedWeapon().WeaponName}";
        }

        inventoryDisplay.text = displayText;
    }
}
