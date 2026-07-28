using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BossVictoryPanelUI : MonoBehaviour
{
    public static BossVictoryPanelUI Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI goldSummaryText;
    [SerializeField] private TextMeshProUGUI percentText;
    [SerializeField] private TextMeshProUGUI starsText;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void Start()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
    }

    public void ShowVictory(int remainingGold, int totalGold)
    {
        if (panelRoot == null) return;

        float pct = (totalGold <= 0) ? 0f : (remainingGold / (float)totalGold);
        int stars = CalculateStars(pct);

        if (titleText != null)
            titleText.text = "Boss Defeated!";

        if (goldSummaryText != null)
            goldSummaryText.text = $"Gold Preserved: {remainingGold}/{totalGold}";

        if (percentText != null)
            percentText.text = $"Preservation: {Mathf.RoundToInt(pct * 100f)}%";

        if (starsText != null)
            starsText.text = BuildStarsString(stars);

        panelRoot.SetActive(true);
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private int CalculateStars(float pct)
    {
        // Tune these thresholds however you want
        if (pct >= 0.80f) return 3;
        if (pct >= 0.50f) return 2;
        return 1;
    }

    private string BuildStarsString(int stars)
    {
        // Example: "★★☆"
        string s = "";
        for (int i = 0; i < 3; i++)
            s += (i < stars) ? "★" : "☆";
        return s;
    }

    private void OnContinueClicked()
    {
        Hide();
        // Optional: trigger next scene / checkpoint / dialogue here
        // SceneManager.LoadScene(...);
    }
}
