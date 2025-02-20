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
        LastActionId = -1;
        field8_0xe = -1;
        ItemsIdList = new List<int>(GetItemsCount(gameType, mapId));
        Timer = 0;
    }

    public List<LumPosition> LumsTable { get; }

    public int LastActionId { get; set; }
    public int field8_0xe { get; set; } // TODO: Name. Why is it never set?
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
        if (randItemIndex >= LastActionId)
            randItemIndex++;

        ItemsMulti obj = scene.GetGameObject<ItemsMulti>(ItemsIdList[randItemIndex]);

        if (obj.IsInvisibleItem() && Timer != 0 && GameTime.ElapsedFrames - Timer <= 600)
        {
            List<int> validItems = new();

            for (int i = 0; i < ItemsIdList.Count; i++)
            {
                if (i != randItemIndex &&
                    i != LastActionId &&
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
        LastActionId = randItemIndex;
    }

    public int GetRandomActionId()
    {
        if (Timer != 0 && GameTime.ElapsedFrames - Timer <= 600)
        {
            int newActionId = Random.GetNumber(2);
            if (newActionId == field8_0xe)
                newActionId = (newActionId + 1) % 2;

            Debug.Assert(newActionId != 2, "Invisible item should not be spawned");

            LastActionId = newActionId;
            return newActionId;
        }
        else
        {
            int newActionId = Random.GetNumber(2);
            if (newActionId >= field8_0xe)
                newActionId++;
            LastActionId = newActionId;
            return newActionId;
        }
    }
}