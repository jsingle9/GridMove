using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    private CharacterSheet playerCharacter;

    public ApplicationState CurrentApplicationState { get; private set; }
    public GameState CurrentGameState { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        CurrentApplicationState = ApplicationState.MainMenu;
        CurrentGameState = GameState.FreeExplore;
    }

    public void SetPlayerCharacter(CharacterSheet sheet)
    {
        playerCharacter = sheet;
        Debug.Log($"Player character set to: {sheet.CharacterName} ({sheet.ClassId})");
    }

    public CharacterSheet GetPlayerCharacter()
    {
        if (playerCharacter == null)
        {
            Debug.LogWarning("No player character set! Returning Fighter default.");
            playerCharacter = CharacterFactory.CreateFighter_Example();
        }
        return playerCharacter;
    }

    public void SetApplicationState(ApplicationState newState)
    {
        CurrentApplicationState = newState;
        Debug.Log($"Application state changed to: {newState}");
    }

    public void SetGameState(GameState newState)
    {
        CurrentGameState = newState;
        Debug.Log($"Game state changed to: {newState}");
    }

    public void EnterCombat()
    {
        SetApplicationState(ApplicationState.InGame);
        SetGameState(GameState.Combat);
        Debug.Log("Entered combat");
    }

    public void ExitCombat()
    {
        SetGameState(GameState.FreeExplore);
        Debug.Log("Exited combat");
    }
}
