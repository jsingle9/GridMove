using System;
using System.Collections.Generic;
using UnityEngine;

public class GameSaveCoordinator : MonoBehaviour
{
    public static GameSaveCoordinator Instance { get; private set; }

    [SerializeField] private BoxMover player;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private BoxMover ResolvePlayer()
    {
        if (player == null){
            player = FindFirstObjectByType<BoxMover>(FindObjectsInactive.Include);
            Debug.Log($"ResolvePlayer: Found player = {(player != null ? player.name : "NULL")}");
        }
        return player;
    }

    public bool SaveToSlot(int slot)
    {
        Debug.Log("SaveToSlot() fired");
        var p = ResolvePlayer();
        if (p == null)
        {
            Debug.LogError("Save failed: no BoxMover found.");
            return false;
        }

        SaveGame save = SaveLoadService.Exists(slot) ? SaveLoadService.Load(slot) : new SaveGame();
        if (save == null) save = new SaveGame();

        if (save.Party == null)
            save.Party = new List<CharacterSheet>();

        save.Party.Clear();
        save.Party.Add(p.Sheet);
        save.ActivePartyIndex = 0;
        save.LastUpdatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        SaveLoadService.Save(slot, save);
        Debug.Log($"Saved slot {slot}");
        return true;
    }

    public bool LoadFromSlot(int slot)
    {
        var p = ResolvePlayer();
        if (p == null)
        {
            Debug.LogError("Load failed: no BoxMover found.");
            return false;
        }

        SaveGame save = SaveLoadService.Load(slot);
        if (save == null || save.Party == null || save.Party.Count == 0)
        {
            Debug.LogWarning($"Load failed: slot {slot} empty or invalid.");
            return false;
        }

        int i = Mathf.Clamp(save.ActivePartyIndex, 0, save.Party.Count - 1);
        p.SetCharacterSheet(save.Party[i]); // uses your existing API
        Debug.Log($"Loaded slot {slot}");
        return true;
    }
}
