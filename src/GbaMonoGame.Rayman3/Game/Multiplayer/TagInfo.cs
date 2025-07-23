using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GbaMonoGame.Rayman3;

public class TagInfo
{
    public TagInfo(MultiplayerGameType gameType, int mapId)
    {
        LumsTable = new List<LumPosition>(GetLumsCount(gameType, mapId));
        LastSpawnedItemId = -1;
        LastActionId = -1;
        ItemsIdList = new List<int>(GetItemsCount(gameType, mapId));
        Timer = 0;
    }

    public List<LumPosition> LumsTable { get; }

    public int LastSpawnedItemId { get; set; }
    public int LastActionId { get; set; }
    public List<int> ItemsIdList { get; }

    private uint Timer { get; set; }

    // The game uses hard-coded tables for these. Since the N-Gage version has more maps it's easier to instead do it like this.
    private int GetLumsCount(MultiplayerGameType gameType, int mapId)
    {
        if (gameType == MultiplayerGameType.CatAndMouse && mapId == 1)
            return 11;
        else
            return 0;
    }
    private int GetItemsCount(MultiplayerGameType gameType, int mapId)
    {
        if (gameType == MultiplayerGameType.RayTag && mapId is 0)
            return 5;
        else if (gameType == MultiplayerGameType.RayTag && mapId is 1)
            return 5;
        else if (gameType == MultiplayerGameType.CatAndMouse && mapId is 0)
            return 4;
        else if (gameType == MultiplayerGameType.CatAndMouse && mapId is 1)
            return 5;
        else
            return 0;
    }

    public void SaveLumPosition(int instanceId, ActorResource actor)
    {
        LumsTable.Add(new LumPosition(instanceId, new Vector2(actor.Pos.X, actor.Pos.Y)));
    }

    public Vector2 GetLumPosition(int instanceId)
    {
        return LumsTable.FirstOrDefault(x => x.InstanceId == instanceId)?.Position ?? Vector2.Zero;
    }

    public void RegisterItem(int instanceId)
    {
        ItemsIdList.Add(instanceId);
    }

    public void SpawnNewItem(Scene2D scene, bool resetTimer)
    {
        if (ItemsIdList.Count == 0)
            return;

        if (resetTimer)
            Timer = GameTime.ElapsedFrames;

        int randItemIndex = Random.GetNumber(ItemsIdList.Count - 1);
        if (randItemIndex >= LastSpawnedItemId)
            randItemIndex++;

        ItemsMulti obj = scene.GetGameObject<ItemsMulti>(ItemsIdList[randItemIndex]);

        if (obj.IsInvisibleItem() && Timer != 0 && GameTime.ElapsedFrames - Timer <= 600)
        {
            List<int> validItems = new();

            for (int i = 0; i < ItemsIdList.Count; i++)
            {
                if (i != randItemIndex &&
                    i != LastSpawnedItemId &&
                    !scene.GetGameObject<ItemsMulti>(ItemsIdList[i]).IsInvisibleItem())
                {
                    validItems.Add(i);
                }
            }

            if (validItems.Count == 0)
                throw new Exception("No random item could be found");

            randItemIndex = validItems[Random.GetNumber(validItems.Count)];
            obj = scene.GetGameObject<ItemsMulti>(randItemIndex);
        }

        obj.Spawn();
        LastSpawnedItemId = randItemIndex;
    }

    public ItemsMulti.Action GetRandomActionId()
    {
        // Get a random value, 0 or 1
        int newActionId = Random.GetNumber(2);

        // Don't allow invisible item to spawn yet...
        if (Timer != 0 && GameTime.ElapsedFrames - Timer <= 600)
        {
            // If it's the same as the last action then increment to the next one
            if (newActionId == LastActionId)
            {
                newActionId++;

                // Maintain the range of 0-1
                newActionId %= 2;
            }

            Debug.Assert((ItemsMulti.Action)newActionId != ItemsMulti.Action.Invisibility, "Invisible item should not be spawned");
        }
        // Allow invisible item to spawn
        else
        {
            // If the same or greater than the last action then increment to the next one
            if (newActionId >= LastActionId)
                newActionId++;
        }

        // NOTE: The original game assigns the wrong variable here, causing the randomization not to fully work!
        if (Engine.ActiveConfig.Tweaks.FixBugs)
            LastActionId = newActionId;
        else
            LastSpawnedItemId = newActionId;

        return (ItemsMulti.Action)newActionId;
    }
}