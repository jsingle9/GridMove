using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string testEnvironmentScene = "SampleScene";
    [SerializeField] private string verticalSliceScene = "VSlice";

    [SerializeField] private GameObject classSelectionPanel;

    void Start()
    {
        Debug.Log("MainMenuController.Start() Fired");

        // Make sure class selection panel is hidden on start
        if (classSelectionPanel != null)
            classSelectionPanel.SetActive(false);
    }

    public void LoadTestEnvironment()
    {
        if (string.IsNullOrWhiteSpace(testEnvironmentScene))
        {
            Debug.LogError("MainMenuController: testEnvironmentScene is empty.");
            return;
        }

        SceneManager.LoadScene(testEnvironmentScene);
    }

    public void LoadVerticalSlice()
    {
        // Show class selection instead of loading directly
        if (classSelectionPanel != null)
        {
            classSelectionPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Class selection panel not assigned!");
            // Fallback: load with default Fighter
            LoadVerticalSliceWithClass("fighter");
        }
    }

    public void LoadVerticalSliceWithClass(string classId)
    {
        if (string.IsNullOrWhiteSpace(verticalSliceScene))
        {
            Debug.LogError("MainMenuController: verticalSliceScene is empty.");
            return;
        }

        // Create character and store in GameStateManager
        CharacterSheet playerSheet = CharacterFactory.CreateCharacterByClass(classId);
        GameStateManager.Instance?.SetPlayerCharacter(playerSheet);
        GameStateManager.Instance?.SetApplicationState(ApplicationState.Loading);

        // Hide the panel and load scene
        if (classSelectionPanel != null)
            classSelectionPanel.SetActive(false);

        SceneManager.LoadScene(verticalSliceScene);
    }

    public void QuitGame()
    {
        Debug.Log("Quit requested.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
