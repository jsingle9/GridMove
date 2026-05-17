using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapSceneLoader : MonoBehaviour
{
    [SerializeField] private string firstSceneName = "VerticalSlice";

    private static bool hasLoadedInitialScene = false;

    private void Start()
    {
        if (hasLoadedInitialScene)
            return;

        hasLoadedInitialScene = true;
        SceneManager.LoadScene(firstSceneName);
    }
}
