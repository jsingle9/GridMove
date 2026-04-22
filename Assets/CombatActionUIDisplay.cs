using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CombatActionUIDisplay : MonoBehaviour
{
    [SerializeField] private Transform actionListContainer;
    [SerializeField] private GameObject actionButtonPrefab;
    [SerializeField] private CanvasGroup canvasGroup;

    private List<TextMeshProUGUI> actionTexts = new List<TextMeshProUGUI>();

    void OnEnable()
    {
        Debug.Log($"CombatActionUIDisplay OnEnable - canvasGroup: {canvasGroup}, actionListContainer: {actionListContainer}, actionButtonPrefab: {actionButtonPrefab}");
        Debug.Log($"OnEnable - Instance {GetInstanceID()}, canvasGroup: {canvasGroup}, actionListContainer: {actionListContainer}, actionButtonPrefab: {actionButtonPrefab}");
    }

    void Start()
    {
        Debug.Log($"Start() called on instance {GetInstanceID()}");
        Debug.Log($"Start() - canvasGroup is {(canvasGroup == null ? "NULL" : "ASSIGNED")}");
        Debug.Log($"Start() - actionListContainer is {(actionListContainer == null ? "NULL" : "ASSIGNED")}");
        Debug.Log($"Start() - actionButtonPrefab is {(actionButtonPrefab == null ? "NULL" : "ASSIGNED")}");

        if (canvasGroup == null || actionListContainer == null || actionButtonPrefab == null)
        {
            Debug.LogError("Missing required component assignments!");
            Debug.LogError($"GameObject: {gameObject.name}");
            Debug.LogError($"Instance ID: {GetInstanceID()}");
            return;
        }

        InitializeActionButtons();
        HideActionUI();
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
        return (slotIndex + 1).ToString();
    }

    public void ShowActionUI()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void HideActionUI()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
