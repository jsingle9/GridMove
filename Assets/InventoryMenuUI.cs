using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InventoryMenuUI : MonoBehaviour
{
    public static InventoryMenuUI Instance { get; private set; }

    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TextMeshProUGUI menuDisplay;
    [SerializeField] private Color selectedItemColor = Color.yellow;
    [SerializeField] private Color normalItemColor = Color.white;

    private List<InventoryItem> displayItems = new List<InventoryItem>();
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
        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    public void OpenMenu()
    {
        menuOpen = true;
        currentSelectedIndex = 0;
        displayItems = new List<InventoryItem>(Inventory.Instance.GetItems());

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

    public InventoryItem GetSelectedItem()
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

        for (int i = 0; i < displayItems.Count; i++)
        {
            string itemName = displayItems[i].GetDisplayName();

            if (displayItems[i].isWeapon)
            {
                Weapon w = displayItems[i].weapon;
                itemName += $" ({w.WeaponType}, +{w.DamageBonus})";
            }

            if (i == currentSelectedIndex)
            {
                displayText += $"> {itemName} <\n";
            }
            else
            {
                displayText += $"  {itemName}\n";
            }
        }

        displayText += "\n[E] Equip | [C] Consume | [I] Close";

        menuDisplay.text = displayText;
    }
}
