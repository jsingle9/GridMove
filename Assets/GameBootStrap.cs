using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private int slot = 1;
    [SerializeField] private BoxMover player;

    void Start()
    {
        if (player == null)
            player = FindFirstObjectByType<BoxMover>();

        if (player == null)
        {
            Debug.LogError("GameBootstrap: BoxMover not found.");
            return;
        }

        SaveGame save = null;

        if (SaveLoadService.Exists(slot))
        {
            save = SaveLoadService.Load(slot);
        }
        else
        {
            save = CreateInitialSave();
            SaveLoadService.Save(slot, save);
            Debug.Log($"Created initial save in slot {slot}.");
        }

        if (save == null || save.Party == null || save.Party.Count == 0)
        {
            Debug.LogError("GameBootstrap: Save invalid or empty party.");
            return;
        }

        int i = Mathf.Clamp(save.ActivePartyIndex, 0, save.Party.Count - 1);
        player.SetCharacterSheet(save.Party[i]);

        Debug.Log($"Loaded persistent character: {save.Party[i].CharacterName}");
    }

    private SaveGame CreateInitialSave()
    {
        var save = new SaveGame();
        save.World.CurrentScene = "Dungeon01";
        save.World.PlayerWorldPosition = new SerializableVector3(0, 0, 0);

        var fighter = CharacterFactory.CreateFighter_Example(); // your STR 17 / CON 15 build
        save.Party.Add(fighter);
        save.ActivePartyIndex = 0;
        return save;
    }
}
