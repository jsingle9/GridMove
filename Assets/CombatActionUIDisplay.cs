using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CombatActionUIDisplay : MonoBehaviour
{
    [SerializeField] private Transform actionListContainer; // Parent for action buttons
    [SerializeField] private GameObject actionButtonPrefab; // UI prefab for each action
    [SerializeField] private CanvasGroup canvasGroup; // For fading in/out

    private List<TextMeshProUGUI> actionTexts = new List<TextMeshProUGUI>();

    void Start()
    {
        // Create UI elements for each action slot (1-4)
        InitializeActionButtons();
    }

    void InitializeActionButtons()
    {
        for (int i = 0; i < 4; i++)
        {
            GameObject buttonObj = Instantiate(actionButtonPrefab, actionListContainer);
            TextMeshProUGUI textComponent = buttonObj.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                actionTexts.Add(textComponent);
            }
        }
    }

    public void UpdateActionDisplay(BoxMover player)
    {
        for (int i = 0; i < 4; i++)
        {
            Ability ability = player.GetAbility(i);

            if (ability != null)
            {
                string keyDisplay = GetKeyDisplay(i);
                string actionText = $"{keyDisplay}: {ability.AbilityName}";

                if (actionTexts[i] != null)
                {
                    actionTexts[i].text = actionText;
                    actionTexts[i].gameObject.SetActive(true);
                }
            }
            else
            {
                if (actionTexts[i] != null)
                {
                    actionTexts[i].gameObject.SetActive(false);
                }
            }
        }
    }

    private string GetKeyDisplay(int slotIndex)
    {
        return (slotIndex + 1).ToString(); // Returns "1", "2", "3", "4"
    }

    public void ShowActionUI()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void HideActionUI()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
