using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// UI panel that displays available spells for the player to select from.
/// Opens when the "Cast Spell" ability is activated.
/// </summary>
public class SpellSelectionPanel : MonoBehaviour
{
    [SerializeField] private GameObject spellButtonPrefab;
    [SerializeField] private Transform spellButtonContainer;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI panelTitle;

    private BoxMover player;
    private List<SpellButton> spellButtons = new List<SpellButton>();

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        // Start hidden
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Opens the spell selection panel for a given caster.
    /// </summary>
    public void Open(BoxMover caster)
    {
        player = caster;
        gameObject.SetActive(true);

        if (panelTitle != null)
            panelTitle.text = $"{player.Name}'s Spells";

        PopulateSpells();

        Debug.Log($"[SpellSelectionPanel] Opened for {caster.Name}");
    }

    private void PopulateSpells()
    {
        // Clear old buttons
        foreach (Transform child in spellButtonContainer)
            Destroy(child.gameObject);
        spellButtons.Clear();

        // Create a button for each spell ability
        for (int i = 0; i < player.GetAbilityCount(); i++)
        {
            Ability ability = player.GetAbility(i);

            // Only show spell abilities (not CastSpellAbility itself)
            if (ability is SpellAbility spellAbility)
            {
                GameObject buttonObj = Instantiate(spellButtonPrefab, spellButtonContainer);
                SpellButton spellButton = buttonObj.GetComponent<SpellButton>();

                if (spellButton != null)
                {
                    spellButton.Setup(spellAbility, i, OnSpellSelected);
                    spellButtons.Add(spellButton);
                }
                else
                {
                    Debug.LogError("[SpellSelectionPanel] Spell button prefab missing SpellButton component!");
                    Destroy(buttonObj);
                }
            }
        }

        if (spellButtons.Count == 0)
            Debug.LogWarning($"[SpellSelectionPanel] No spells found for {player.Name}");
    }

    private void OnSpellSelected(SpellAbility spell, int slotIndex)
    {
        Debug.Log($"[SpellSelectionPanel] Selected spell: {spell.AbilityName} at slot {slotIndex}");

        // Select the spell in AbilityUI for targeting
        AbilityUI.Instance.SelectAbility(slotIndex);

        // Close panel
        Close();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        Debug.Log("[SpellSelectionPanel] Closed");
    }
}
