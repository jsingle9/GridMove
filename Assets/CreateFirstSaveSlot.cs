using UnityEngine;

public class CreateFirstSaveSlot : MonoBehaviour
{
    [ContextMenu("Create Save Slot 1 (Fighter Example)")]
    public void CreateSaveSlot1()
    {
        var save = new SaveGame();
        save.World.CurrentScene = "Dungeon01";
        save.World.PlayerWorldPosition = new SerializableVector3(0, 0, 0);

        var fighter = CharacterFactory.CreateFighter_Example();
        save.Party.Add(fighter);
        save.ActivePartyIndex = 0;

        SaveLoadService.Save(1, save);

        Debug.Log($"Created slot 1 save at: {SaveLoadService.GetSavePath(1)}");
    }

    [ContextMenu("Load Save Slot 1")]
    public void LoadSaveSlot1()
    {
        var save = SaveLoadService.Load(1);
        if (save == null)
        {
            Debug.Log("No save found in slot 1.");
            return;
        }

        Debug.Log($"Loaded slot 1. Party count: {save.Party.Count}, Scene: {save.World.CurrentScene}");
        if (save.Party.Count > 0)
        {
            var c = save.Party[save.ActivePartyIndex];
            Debug.Log($"Active: {c.CharacterName} L{c.Level} STR {c.Scores.STR} CON {c.Scores.CON}");
        }
    }
}
