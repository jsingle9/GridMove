using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string testEnvironmentScene = "SampleScene";
    [SerializeField] private string verticalSliceScene = "VSlice";

    [Header("UI")]
    [SerializeField] private GameObject classSelectionPanel;

    [Header("Class Data")]
    [SerializeField] private ClassTemplateDatabase classDb;

    private void Awake()
    {
        // Initialize the factory before Start() or any button click.
        if (classDb != null)
        {
            CharacterFactory.ClassDb = classDb;

            Debug.Log(
                $"MainMenuController: ClassDb initialized. " +
                $"Database assigned = {CharacterFactory.ClassDb != null}"
            );
        }
        else
        {
            Debug.LogError(
                "MainMenuController: ClassTemplateDatabase is not assigned. " +
                "Assign it in the Inspector."
            );
        }
    }

    private void Start()
    {
        Debug.Log("MainMenuController.Start() Fired");

        // Keep the class selection panel hidden until requested.
        if (classSelectionPanel != null)
            classSelectionPanel.SetActive(false);
    }

    public void LoadTestEnvironment()
    {
        if (string.IsNullOrWhiteSpace(testEnvironmentScene))
        {
            Debug.LogError(
                "MainMenuController: testEnvironmentScene is empty."
            );
            return;
        }

        SceneManager.LoadScene(testEnvironmentScene);
    }

    public void LoadVerticalSlice()
    {
        // Show class selection instead of loading directly.
        if (classSelectionPanel != null)
        {
            classSelectionPanel.SetActive(true);
        }
        else
        {
            Debug.LogError(
                "MainMenuController: class selection panel is not assigned. " +
                "Loading with default Fighter."
            );

            LoadVerticalSliceWithClass("fighter");
        }
    }

    public void LoadVerticalSliceWithClass(string classId)
    {
        if (string.IsNullOrWhiteSpace(verticalSliceScene))
        {
            Debug.LogError(
                "MainMenuController: verticalSliceScene is empty."
            );
            return;
        }

        if (string.IsNullOrWhiteSpace(classId))
        {
            Debug.LogWarning(
                "MainMenuController: No class ID supplied. " +
                "Defaulting to fighter."
            );

            classId = "fighter";
        }

        classId = classId.Trim().ToLowerInvariant();

        // Save the selected class for any gameplay systems that need it.
        PlayerPrefs.SetString("SelectedClassId", classId);
        PlayerPrefs.Save();

        Debug.Log(
            $"MainMenuController: Loading class '{classId}'."
        );

        if (CharacterFactory.ClassDb == null)
        {
            Debug.LogError(
                "MainMenuController: CharacterFactory.ClassDb is null. " +
                "The class database is not assigned."
            );
        }

        // Create the selected character before changing scenes.
        CharacterSheet playerSheet =
            CharacterFactory.CreateCharacterByClass(classId);

        if (playerSheet == null)
        {
            Debug.LogError(
                $"MainMenuController: CharacterFactory returned null " +
                $"for class '{classId}'."
            );
            return;
        }

        Debug.Log(
            $"MainMenuController: Created character. " +
            $"Requested class = '{classId}', " +
            $"Created class = '{playerSheet.ClassId}'."
        );

        if (GameStateManager.Instance == null)
        {
            Debug.LogError(
                "MainMenuController: GameStateManager.Instance is null."
            );
            return;
        }

        GameStateManager.Instance.SetPlayerCharacter(playerSheet);
        GameStateManager.Instance.SetApplicationState(
            ApplicationState.Loading
        );

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
