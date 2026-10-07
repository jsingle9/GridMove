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

    void Awake()
    {
        Debug.Log($"[SpellSelectionPanel] Awake called! GameObject: {gameObject.name}, Scene: {gameObject.scene.name}");
        Debug.Log($"[SpellSelectionPanel] This component type: {this.GetType().Name}");
        Debug.Log($"[SpellSelectionPanel] gameObject active: {gameObject.activeSelf}, activeInHierarchy: {gameObject.activeInHierarchy}");

        // Try to find itself
        SpellSelectionPanel found = Object.FindFirstObjectByType<SpellSelectionPanel>();
        Debug.Log($"[SpellSelectionPanel] Can find itself immediately after Awake: {(found != null ? "YES" : "NO")}");
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
        Debug.Log($"[SpellSelectionPanel] PopulateSpells called! Player has {player.GetSpellCount()} spells");
        Debug.Log($"[SpellSelectionPanel] spellButtonContainer is: {(spellButtonContainer != null ? spellButtonContainer.name : "NULL")}");

        // Clear old buttons
        foreach (Transform child in spellButtonContainer)
            Destroy(child.gameObject);
        spellButtons.Clear();

        // Loop through the spells list directly
        List<SpellAbility> playerSpells = player.GetSpells();
        for (int i = 0; i < playerSpells.Count; i++)
        {
            SpellAbility spellAbility = playerSpells[i];

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

        if (spellButtons.Count == 0)
            Debug.LogWarning($"[SpellSelectionPanel] No spells found for {player.Name}");
    }

    private void OnSpellSelected(SpellAbility spell, int slotIndex)
    {
        Debug.Log($"[SpellSelectionPanel] Selected spell: {spell.AbilityName} at slot {slotIndex}");

        // Select the spell in AbilityUI
        AbilityUI.Instance.SelectAbility(slotIndex);

        // Trigger targeting mode for the selected spell
        if (spell.targetingMode != TargetingMode.Self && spell.Range > 0f)
        {
            AbilityUI.Instance.BeginTargetingForSelectedAbility("SPELL_MENU");
        }
        else
        {
            // Self-cast spell, execute immediately
            TargetData selfTarget = new TargetData
            {
                primaryTarget = player,
                user = player
            };
            selfTarget.unitsInArea.Add(player);

            AbilityResult result = spell.TryUse(player, selfTarget);
            if (!result.Success)
                Debug.Log($"Spell failed: {result.FailureReason}");
        }

        // Close panel
        Close();
    }

    void OnEnable()
    {
        Debug.Log("[SpellSelectionPanel] OnEnable called!");
    }

    public void Close()
    {
        gameObject.SetActive(false);
        Debug.Log("[SpellSelectionPanel] Closed");
    }
}
