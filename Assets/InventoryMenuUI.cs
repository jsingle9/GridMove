using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InventoryMenuUI : MonoBehaviour
{
    public static InventoryMenuUI Instance { get; private set; }

    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TextMeshProUGUI menuDisplay;

    private List<Item> displayItems = new List<Item>();
    private BoxMover player;
    private int currentSelectedIndex = 0;
    private bool menuOpen = false;

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
        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    public void OpenMenu()
    {
        menuOpen = true;
        currentSelectedIndex = 0;

        if (player == null)
            player = FindFirstObjectByType<BoxMover>();

        displayItems = player != null
            ? new List<Item>(player.GetInventoryItems())
            : new List<Item>();

        if (menuPanel != null)
            menuPanel.SetActive(true);

        UpdateMenuDisplay();
    }

    public void CloseMenu()
    {
        menuOpen = false;
        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    public bool IsMenuOpen()
    {
        return menuOpen;
    }

    public void SelectNext()
    {
        if (displayItems.Count == 0) return;

        currentSelectedIndex = (currentSelectedIndex + 1) % displayItems.Count;
        UpdateMenuDisplay();
    }

    public void SelectPrevious()
    {
        if (displayItems.Count == 0) return;

        currentSelectedIndex--;
        if (currentSelectedIndex < 0)
            currentSelectedIndex = displayItems.Count - 1;

        UpdateMenuDisplay();
    }

    public Item GetSelectedItem()
    {
        if (displayItems.Count == 0 || currentSelectedIndex < 0 || currentSelectedIndex >= displayItems.Count)
            return null;

        return displayItems[currentSelectedIndex];
    }

    private void UpdateMenuDisplay()
    {
        if (menuDisplay == null)
            return;

        string displayText = "=== INVENTORY ===\n\n";

        if (displayItems.Count == 0)
        {
            displayText += "Empty";
        }
        else
        {
            for (int i = 0; i < displayItems.Count; i++)
            {
                string itemName = displayItems[i] != null ? displayItems[i].itemName : "(null)";

                if (displayItems[i] is WeaponItem w)
                    itemName += $" ({w.weaponType}, +{w.damageBonus})";
                else if (displayItems[i] is ArmorItem a)
                    itemName += $" (AC {a.baseAC})";
                else if (displayItems[i] is ShieldItem s)
                    itemName += $" (+{s.acBonus} AC)";
                else if (displayItems[i] is Potion potion)
                    itemName += $" (Potency: {potion.potency})";
                else if (displayItems[i] is Scroll scroll)
                    itemName += $" ({scroll.scrollType}, Power: {scroll.spellPower})";

                if (i == currentSelectedIndex)
                {
                    displayText += $"> {itemName} <\n";
                }
                else
                {
                    displayText += $"  {itemName}\n";
                }
            }
        }

        displayText += "\n[E] Equip | [C] Use/Consume | [I] Close";

        menuDisplay.text = displayText;
    }
}
