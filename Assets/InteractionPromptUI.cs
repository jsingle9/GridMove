using UnityEngine;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.2f;

    private InteractionSystem interactionSystem;
    private bool isVisible = false;

    void Start()
    {
        interactionSystem = FindFirstObjectByType<InteractionSystem>();
        canvasGroup.alpha = 0;
    }

    void Update()
    {
        if(interactionSystem.HasInteractableInRange())
        {
            ShowPrompt(interactionSystem.GetCurrentInteractionPrompt());
        }
        else
        {
            HidePrompt();
        }
    }

    void ShowPrompt(string promptMessage)
    {
        if(isVisible) return;

        isVisible = true;
        promptText.text = $"Press F: {promptMessage}";
        StartCoroutine(FadeTo(1f));
    }

    void HidePrompt()
    {
        if(!isVisible) return;

        isVisible = false;
        StartCoroutine(FadeTo(0f));
    }

    System.Collections.IEnumerator FadeTo(float targetAlpha)
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while(elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}
