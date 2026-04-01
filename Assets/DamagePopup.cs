using UnityEngine;
using TMPro;
using System.Collections;

public class DamagePopup : MonoBehaviour
{
    private TextMeshProUGUI damageText;
    private CanvasGroup canvasGroup;
    private float duration = 1.5f;
    private float riseSpeed = 2f;
    private Vector3 startPos;

    void Awake()
    {
        damageText = GetComponent<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show(int damageAmount, Vector3 worldPosition)
    {
        // Set damage text
        damageText.text = damageAmount.ToString();

        // Convert world position to screen position
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);

        // Set RectTransform position
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.position = screenPos;

        startPos = rectTransform.position;

        // Start animation
        StartCoroutine(AnimatePopup());
    }

    private IEnumerator AnimatePopup()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            // Rise up
            RectTransform rectTransform = GetComponent<RectTransform>();
            Vector3 newPos = startPos + Vector3.up * (riseSpeed * progress * 50f);
            rectTransform.position = newPos;

            // Fade out
            canvasGroup.alpha = 1f - progress;

            yield return null;
        }

        // Return to pool
        DamagePopupManager.Instance.ReturnToPool(this);
    }
}
