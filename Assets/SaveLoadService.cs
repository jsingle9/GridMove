using System.IO;
using UnityEngine;

public static class SaveLoadService
{
    private static Vector3 lastDeathPosition = Vector3.zero;

    public static string GetSavePath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"slot{slot}.json");
    }

    public static bool Exists(int slot)
    {
        return File.Exists(GetSavePath(slot));
    }

    public static SaveGame Load(int slot)
    {
        string path = GetSavePath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Load failed: no file at {path}");
            return null;
        }

        string json = File.ReadAllText(path);
        SaveGame save = JsonUtility.FromJson<SaveGame>(json);
        Debug.Log($"LOAD path={path}");
        return save;
    }

    public static void Save(int slot, SaveGame save)
    {
        if (save == null)
        {
            Debug.LogError("Save failed: SaveGame is null");
            return;
        }

        save.LastUpdatedUnix = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        string json = JsonUtility.ToJson(save, true);
        string path = GetSavePath(slot);
        File.WriteAllText(path, json);

        Debug.Log($"SAVE path={path}");
        Debug.Log($"SAVE scene={save.World?.CurrentScene}");
    }

    // Optional helper if you want runtime->save in one call
    public static SaveGame BuildFromRuntime(BoxMover player, SaveGame existing = null)
    {
        SaveGame save = existing ?? new SaveGame();
        save.CaptureRuntime(player);

        if (save.Party == null) save.Party = new System.Collections.Generic.List<CharacterSheet>();
        save.Party.Clear();

        if (player != null && player.Sheet != null)
        {
            // No Clone required: serialize/deserialize deep copy
            string sheetJson = JsonUtility.ToJson(player.Sheet);
            CharacterSheet sheetCopy = JsonUtility.FromJson<CharacterSheet>(sheetJson);

            sheetCopy.CurrentHP = player.CurrentHP; // runtime -> save
            save.Party.Add(sheetCopy);
            save.ActivePartyIndex = 0;
        }

        return save;
    }

    public static void SetLastDeathPosition(Vector3 position)
    {
        lastDeathPosition = position;
        Debug.Log($"Death position saved: {position}");
    }

    public static Vector3 GetLastDeathPosition()
    {
        return lastDeathPosition;
    }
}
