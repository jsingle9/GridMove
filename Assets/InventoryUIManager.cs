using UnityEngine;
using TMPro;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI statsDisplay;
    private BoxMover player;  // ← Declare here at class level

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

    void Start()
    {
        player = FindFirstObjectByType<BoxMover>();
        UpdateUI(); // Initial update
    }

    void Update()
    {
        // Update stats every frame so HP changes are reflected immediately
        UpdateUI();
    }


    public void UpdateUI()
    {
        if (statsDisplay == null || player == null)
            return;

        string displayText = $"HP: {player.CurrentHP}\n";
        displayText += $"AC: {player.ArmorClass}\n";

        WeaponItem weapon = player.EquippedWeapon;
        ArmorItem armor = player.GetEquippedItem(EquipmentSlot.Torso) as ArmorItem;
        ShieldItem shield = player.GetEquippedItem(EquipmentSlot.Shield) as ShieldItem;

        if (weapon != null)
            displayText += $"Weapon: {weapon.itemName} ({weapon.weaponType})\n";

        if (armor != null)
            displayText += $"Armor: {armor.itemName}\n";

        if (shield != null)
            displayText += $"Shield: {shield.itemName}\n";

        statsDisplay.text = displayText;
    }
}
