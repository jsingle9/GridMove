using UnityEngine;
using UnityEngine.SceneManagement;

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

        // capture world/scene at save time
        save.CaptureRuntime(p);

        if (save.Party == null) save.Party = new System.Collections.Generic.List<CharacterSheet>();
        save.Party.Clear();

        if (p.Sheet == null)
        {
            Debug.LogError("Save failed: player sheet is null.");
            return;
        }

        // Deep copy sheet so we don't keep a live reference
        string sheetJson = JsonUtility.ToJson(p.Sheet);
        CharacterSheet sheetCopy = JsonUtility.FromJson<CharacterSheet>(sheetJson);

        // CRITICAL: runtime -> save
        sheetCopy.CurrentHP = p.CurrentHP;

        save.Party.Add(sheetCopy);
        save.ActivePartyIndex = 0;
        save.LastUpdatedUnix = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        SaveLoadService.Save(activeSlot, save);
        Debug.Log($"Saved slot {activeSlot} | scene={save.World.CurrentScene} | runtimeHP={p.CurrentHP} | savedHP={sheetCopy.CurrentHP}");
    }

    public void LoadGame()
    {
        Debug.Log($"LoadGame() fired. activeSlot={activeSlot}");
        Debug.Log($"Load path: {SaveLoadService.GetSavePath(activeSlot)}");

        var p = ResolvePlayer();
        Debug.Log($"ResolvePlayer() -> {(p == null ? "NULL" : p.name)}");
        if (p == null)
        {
            Debug.LogError("Load failed: no BoxMover found.");
            return;
        }

        bool exists = SaveLoadService.Exists(activeSlot);
        Debug.Log($"Slot exists? {exists}");
        if (!exists)
        {
            Debug.LogWarning($"Load failed: slot {activeSlot} does not exist.");
            return;
        }

        SaveGame save = SaveLoadService.Load(activeSlot);
        Debug.Log($"Load returned null? {save == null}");
        if (save == null)
        {
            Debug.LogWarning("Load failed: save is null.");
            return;
        }

        Debug.Log($"Loaded scene in file: {save.World?.CurrentScene}");
        Debug.Log($"Party count: {(save.Party == null ? -1 : save.Party.Count)}");
        if (save.Party == null || save.Party.Count == 0)
        {
            Debug.LogWarning($"Load failed: slot {activeSlot} empty or invalid.");
            return;
        }

        int i = Mathf.Clamp(save.ActivePartyIndex, 0, save.Party.Count - 1);
        Debug.Log($"ActivePartyIndex(raw)={save.ActivePartyIndex}, clamped={i}");

        CharacterSheet loaded = save.Party[i];
        Debug.Log($"LOAD: applying {loaded.CharacterName} HP={loaded.CurrentHP}");
        Debug.Log($"Before apply runtimeHP={p.CurrentHP}, sheetHP={p.Sheet?.CurrentHP}");

        p.SetCharacterSheet(loaded);

        Debug.Log($"After apply runtimeHP={p.CurrentHP}, sheetHP={p.Sheet?.CurrentHP}");
        Debug.Log($"Loaded slot {activeSlot}");

        FindFirstObjectByType<InventoryUIManager>()?.UpdateUI();
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

    public void LoadAfterDeath()
    {
        Debug.Log("LoadAfterDeath called - reloading VSlice scene");
        SceneManager.LoadScene("VSlice");
    }
}
