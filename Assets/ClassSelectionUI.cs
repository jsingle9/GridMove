using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple class selection UI for the main menu.
/// Allows player to pick Fighter, Mystic, or Mage before loading the game.
/// </summary>
public class ClassSelectionUI : MonoBehaviour
{
    [SerializeField] private Button fighterButton;
    [SerializeField] private Button mysticButton;
    [SerializeField] private Button mageButton;
    [SerializeField] private Button backButton;

    [SerializeField] private Text classNameDisplay;
    [SerializeField] private Text classDescriptionDisplay;

    [SerializeField] private MainMenuController mainMenuController;

    private string selectedClassId = "";

    private void Start()
    {
        // Hook up buttons
        if (fighterButton != null)
            fighterButton.onClick.AddListener(() => SelectAndStartClass("fighter"));

        if (mysticButton != null)
            mysticButton.onClick.AddListener(() => SelectAndStartClass("mystic"));

        if (mageButton != null)
            mageButton.onClick.AddListener(() => SelectAndStartClass("mage"));

        if (backButton != null)
            backButton.onClick.AddListener(GoBack);
    }

    private void SelectAndStartClass(string classId)
    {
        selectedClassId = classId;
        UpdateDisplay();

        // Auto-start after brief delay to show selection
        Invoke(nameof(StartGameWithSelectedClass), 0.3f);
    }

    private void UpdateDisplay()
    {
        switch (selectedClassId)
        {
            case "fighter":
                classNameDisplay.text = "Fighter";
                classDescriptionDisplay.text =
                    "Master of weapons and armor.\n\n" +
                    "Features:\n" +
                    "• Second Wind (L1)\n" +
                    "• Action Surge (L2)\n" +
                    "• Fighting Style (L3)\n\n" +
                    "Hit Die: d10";
                break;

            case "mystic":
                classNameDisplay.text = "Mystic";
                classDescriptionDisplay.text =
                    "Wisdom-based healer and psychic warrior.\n\n" +
                    "Features:\n" +
                    "• Psionic Focus (L1)\n" +
                    "• Psionic Strike (L2)\n" +
                    "• Psionic Aura (L3)\n\n" +
                    "Spell Slots: 1st & 2nd level";
                break;

            case "mage":
                classNameDisplay.text = "Mage";
                classDescriptionDisplay.text =
                    "Intelligence-based spellcaster.\n\n" +
                    "Features:\n" +
                    "• Cantrip (L1)\n" +
                    "• Spell Selection (L2)\n" +
                    "• Enhanced Spellcasting (L3)\n\n" +
                    "Spell Slots: Full progression";
                break;

            default:
                classNameDisplay.text = "Select a Class";
                classDescriptionDisplay.text = "Click a class button to view details.";
                break;
        }
    }

    private void StartGameWithSelectedClass()
    {
        if (string.IsNullOrEmpty(selectedClassId))
        {
            Debug.LogWarning("No class selected!");
            return;
        }

        if (mainMenuController != null)
        {
            mainMenuController.LoadVerticalSliceWithClass(selectedClassId);
        }
        else
        {
            Debug.LogError("MainMenuController not assigned to ClassSelectionUI!");
        }
    }

    private void GoBack()
    {
        selectedClassId = "";
        UpdateDisplay();
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (fighterButton != null)
            fighterButton.onClick.RemoveAllListeners();
        if (mysticButton != null)
            mysticButton.onClick.RemoveAllListeners();
        if (mageButton != null)
            mageButton.onClick.RemoveAllListeners();
        if (backButton != null)
            backButton.onClick.RemoveAllListeners();
    }
}
