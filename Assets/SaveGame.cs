using System;
using System.Collections.Generic;

[Serializable]
public class SaveGame
{
    public int SaveVersion = 1;
    public string SaveId = Guid.NewGuid().ToString();
    public long CreatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public long LastUpdatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public int TotalPlaySeconds = 0;

    public int ActivePartyIndex = 0;
    public List<CharacterSheet> Party = new();

    public WorldState World = new();
    public StoryState Story = new();
}

[Serializable]
public class WorldState
{
    public string CurrentScene = "Dungeon01";
    public SerializableVector3 PlayerWorldPosition = new SerializableVector3(0, 0, 0);

    public List<string> DefeatedBossIds = new();
    public List<string> OpenedDoorIds = new();
    public List<string> LootedChestIds = new();
}

[Serializable]
public class StoryState
{
    public string CurrentQuestId = "prologue_enter_dungeon";
    public List<string> CompletedQuestIds = new();
    public List<string> StoryFlags = new(); // e.g. "met_old_knight"
}

[Serializable]
public struct SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(float x, float y, float z)
    {
        this.x = x; this.y = y; this.z = z;
    }
}
