using System;
using System.IO;
using UnityEngine;

public static class SaveLoadService
{
    private const string SaveFilePrefix = "save_slot_";
    private const string SaveFileSuffix = ".json";

    public static string GetSavePath(int slot)
    {
        string file = $"{SaveFilePrefix}{slot}{SaveFileSuffix}";
        return Path.Combine(Application.persistentDataPath, file);
    }

    public static void Save(int slot, SaveGame save)
    {
        if (save == null) throw new ArgumentNullException(nameof(save));
        if (slot < 1) throw new ArgumentOutOfRangeException(nameof(slot), "Slot must be >= 1");

        save.LastUpdatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        string json = JsonUtility.ToJson(save, true);
        string path = GetSavePath(slot);
        File.WriteAllText(path, json);

        Debug.Log($"[SaveLoadService] Saved slot {slot} -> {path}");
    }

    public static bool Exists(int slot)
    {
        return File.Exists(GetSavePath(slot));
    }

    public static SaveGame Load(int slot)
    {
        if (slot < 1) throw new ArgumentOutOfRangeException(nameof(slot), "Slot must be >= 1");

        string path = GetSavePath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveLoadService] No save file in slot {slot}");
            return null;
        }

        string json = File.ReadAllText(path);
        SaveGame save = JsonUtility.FromJson<SaveGame>(json);

        if (save == null)
        {
            Debug.LogError($"[SaveLoadService] Failed to parse save slot {slot}");
            return null;
        }

        // Version migration hook
        if (save.SaveVersion < 1)
        {
            // future migration logic
            save.SaveVersion = 1;
        }

        return save;
    }

    public static void Delete(int slot)
    {
        string path = GetSavePath(slot);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[SaveLoadService] Deleted slot {slot}");
        }
    }
}
