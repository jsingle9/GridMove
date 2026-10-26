using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuRoot;
    [SerializeField] private GameObject confirmReturnRoot;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private AbilityUI abilityUI;

    private bool isPaused = false;

    void Start()
    {
        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);

        if (confirmReturnRoot != null)
            confirmReturnRoot.SetActive(false);

        ResumeGameTime();
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        if (!Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        // If player is targeting an ability, cancel targeting instead of opening menu
        if (IsAbilityTargetingActive())
        {
            CancelAbilityTargeting();
            return;
        }

        if (confirmReturnRoot != null && confirmReturnRoot.activeSelf)
        {
            CloseReturnConfirmation();
            return;
        }

        if (isPaused)
        {
            ResumeGame();
            return;
        }

        OpenPauseMenu();
    }

    private bool IsAbilityTargetingActive()
    {
        if (abilityUI == null)
            return false;

        return abilityUI.CurrentPhase == PlayerTurnPhase.WaitingForTarget;
    }

    private void CancelAbilityTargeting()
    {
        GridController grid;
        grid = FindFirstObjectByType<GridController>();
        abilityUI.CancelAbility();
        abilityUI.CurrentPhase = PlayerTurnPhase.WaitingForAction;
        if (grid != null)
            grid.ClearAllHighlights();
    }

    public void OpenPauseMenu()
    {
        isPaused = true;

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(true);

        if (confirmReturnRoot != null)
            confirmReturnRoot.SetActive(false);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);

        if (confirmReturnRoot != null)
            confirmReturnRoot.SetActive(false);

        ResumeGameTime();
    }

    public void OpenReturnConfirmation()
    {
        if (confirmReturnRoot != null)
            confirmReturnRoot.SetActive(true);
    }

    public void CloseReturnConfirmation()
    {
        if (confirmReturnRoot != null)
            confirmReturnRoot.SetActive(false);
    }

    public void ConfirmReturnToMainMenu()
    {
        ResumeGameTime();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        ResumeGameTime();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ResumeGameTime()
    {
        Time.timeScale = 1f;
        isPaused = false;
    }
}
