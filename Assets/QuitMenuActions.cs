using UnityEngine;

public class QuitMenuActions : MonoBehaviour
{
    [SerializeField] private int activeSlot = 1;
    [SerializeField] private BoxMover player;

    private BoxMover ResolvePlayer()
    {
        if (player == null)
            player = FindFirstObjectByType<BoxMover>();
        return player;
    }

    public void SaveGame()
    {
        Debug.Log("SaveGame() fired");
        var p = ResolvePlayer();
        if (p == null)
        {
            Debug.LogError("Save failed: no BoxMover found.");
            return;
        }

        SaveGame save = SaveLoadService.Exists(activeSlot) ? SaveLoadService.Load(activeSlot) : new SaveGame();
        if (save == null) save = new SaveGame();

        if (save.Party == null) save.Party = new System.Collections.Generic.List<CharacterSheet>();
        save.Party.Clear();
        save.Party.Add(p.Sheet);
        save.ActivePartyIndex = 0;
        save.LastUpdatedUnix = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        SaveLoadService.Save(activeSlot, save);
        Debug.Log($"Saved slot {activeSlot}");
    }

    public void LoadGame()
    {
        var p = ResolvePlayer();
        if (p == null)
        {
            Debug.LogError("Load failed: no BoxMover found.");
            return;
        }

        SaveGame save = SaveLoadService.Load(activeSlot);
        if (save == null || save.Party == null || save.Party.Count == 0)
        {
            Debug.LogWarning($"Load failed: slot {activeSlot} empty or invalid.");
            return;
        }

        int i = Mathf.Clamp(save.ActivePartyIndex, 0, save.Party.Count - 1);
        p.SetCharacterSheet(save.Party[i]);

        Debug.Log($"Loaded slot {activeSlot}");
    }

    public void SaveAndQuit()
    {
        SaveGame();
        QuitGame();
    }

    public void QuitWithoutSaving()
    {
        QuitGame();
    }

    private void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}
