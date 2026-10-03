using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Individual spell button in the spell selection panel.
/// Displays spell name, level, and triggers selection callback.
/// </summary>
public class SpellButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI spellNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image spellIcon;

    private SpellAbility spell;
    private int slotIndex;
    private Action<SpellAbility, int> onSelected;

    public void Setup(SpellAbility spellAbility, int slot, Action<SpellAbility, int> callback)
    {
        spell = spellAbility;
        slotIndex = slot;
        onSelected = callback;

        if (spellNameText != null)
            spellNameText.text = spell.AbilityName;

        if (costText != null)
        {
            // Display spell level or cost info
            int level = spell.GetSpellLevel();
            if (level == 0)
                costText.text = "Cantrip";
            else
                costText.text = $"Level {level}";
        }

        if (button != null)
            button.onClick.AddListener(OnButtonClicked);

        Debug.Log($"[SpellButton] Setup: {spell.AbilityName} at slot {slot}");
    }

    private void OnButtonClicked()
    {
        onSelected?.Invoke(spell, slotIndex);
    }
}
