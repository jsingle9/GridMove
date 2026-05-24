using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string testEnvironmentScene = "SampleScene";
    [SerializeField] private string verticalSliceScene = "VSlice";

    void Start(){
      Debug.Log("MainMenuController.Start() Fired");
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
        if (string.IsNullOrWhiteSpace(verticalSliceScene))
        {
            Debug.LogError("MainMenuController: verticalSliceScene is empty.");
            return;
        }

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
