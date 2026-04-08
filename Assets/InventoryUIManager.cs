using UnityEngine;
using TMPro;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI statsDisplay;

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
        if (statsDisplay == null)
            return;

        BoxMover player = FindFirstObjectByType<BoxMover>();
        if (player == null)
            return;

        string displayText = $"HP: {player.CurrentHP}\n";
        displayText += $"AC: {player.ArmorClass}\n";

        Weapon melee = Inventory.Instance.GetEquippedMeleeWeapon();
        Weapon ranged = Inventory.Instance.GetEquippedRangedWeapon();

        if (melee != null)
            displayText += $"Melee: {melee.WeaponName}\n";

        if (ranged != null)
            displayText += $"Ranged: {ranged.WeaponName}";

        statsDisplay.text = displayText;
    }
}
