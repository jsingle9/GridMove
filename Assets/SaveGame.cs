using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[Serializable]
public class SaveGame
{
    public int SaveVersion;
    public string SaveId;
    public long CreatedUnix;
    public long LastUpdatedUnix;
    public int TotalPlaySeconds;

    public int ActivePartyIndex;
    public List<CharacterSheet> Party;

    public WorldState World;
    public StoryState Story;

    public SaveGame()
    {
        SaveVersion = 1;
        SaveId = Guid.NewGuid().ToString();
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        CreatedUnix = now;
        LastUpdatedUnix = now;
        TotalPlaySeconds = 0;

        ActivePartyIndex = 0;
        Party = new List<CharacterSheet>();

        World = new WorldState();
        Story = new StoryState();
    }

    public void CaptureRuntime(BoxMover player)
    {
        LastUpdatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        World.CurrentScene = SceneManager.GetActiveScene().name;
        if (player != null)
        {
            var p = player.transform.position;
            World.PlayerWorldPosition = new SerializableVector3(p.x, p.y, p.z);
        }
    }
}

[Serializable]
public class WorldState
{
    public string CurrentScene; // set at save time
    public SerializableVector3 PlayerWorldPosition;

    public List<string> DefeatedBossIds = new();
    public List<string> OpenedDoorIds = new();
    public List<string> LootedChestIds = new();

    public WorldState()
    {
        CurrentScene = "";
        PlayerWorldPosition = new SerializableVector3(0, 0, 0);
    }
}

[Serializable]
public class StoryState
{
    public string CurrentQuestId = "prologue_enter_dungeon";
    public List<string> CompletedQuestIds = new();
    public List<string> StoryFlags = new();
}
