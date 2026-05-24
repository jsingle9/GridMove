using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBootstrapper : MonoBehaviour
{
    [SerializeField] private string firstSceneToLoad = "MainMenu";

    void Start()
    {
        if (string.IsNullOrWhiteSpace(firstSceneToLoad))
        {
            Debug.LogError("SceneBootstrapper: firstSceneToLoad is empty.");
            return;
        }

        SceneManager.LoadScene(firstSceneToLoad);
    }
}
