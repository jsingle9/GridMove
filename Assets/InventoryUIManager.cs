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
        if (statsDisplay == null)
            return;

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
