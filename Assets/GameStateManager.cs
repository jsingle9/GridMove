using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    public GameState CurrentState { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        CurrentState = GameState.FreeExplore;
    }

    public void EnterCombat()
    {
        Debug.Log("Entered Combat State");
        CurrentState = GameState.Combat;
    }

    public void EnterFreeExplore()
    {
        CurrentState = GameState.FreeExplore;
        Debug.Log("Entered Free Explore State");
    }

    public void ExitCombat()
    {
        Debug.Log("Returning to Free Explore");
        CurrentState = GameState.FreeExplore;
    }

    public void EnterBusy()
    {
        Debug.Log("Entered Busy State");
        CurrentState = GameState.Busy;
    }

    public void ExitBusy()
    {
        Debug.Log("Exiting Busy State");
        CurrentState = GameState.FreeExplore;
    }

    public void EnterDialog()
    {
        Debug.Log("Entering Dialog");
        CurrentState = GameState.Dialog;
    }

    public void ExitDialog()
    {
        Debug.Log("Exiting Busy State");
        CurrentState = GameState.FreeExplore;
    }
}
