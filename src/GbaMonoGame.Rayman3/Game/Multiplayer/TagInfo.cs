using System.Collections.Generic;
using System.Linq;

namespace GbaMonoGame.Rayman3;

public class TagInfo
{
    public TagInfo(MultiplayerGameType gameType, int mapId)
    {
        LumsTable = new List<LumPosition>(GetLumsCount(gameType, mapId));
        field7_0xd = -1;
        field8_0xe = -1;
        ItemsIdList = new List<int>(GetItemsCount(gameType, mapId));
        Timer = 0;
    }

    public List<LumPosition> LumsTable { get; }

    // TODO: Name these. Why is field8 never set?
    public int field7_0xd { get; set; }
    public int field8_0xe { get; set; }
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
        if (randItemIndex >= field7_0xd)
            randItemIndex++;

        ItemsMulti obj = scene.GetGameObject<ItemsMulti>(ItemsIdList[randItemIndex]);

        if (obj.IsInvisibleItem() && Timer != 0 && GameTime.ElapsedFrames - Timer <= 600)
        {
            List<int> validItems = new();

            for (int i = 0; i < ItemsIdList.Count; i++)
            {
                if (i != randItemIndex &&
                    i != field7_0xd &&
                    !scene.GetGameObject<ItemsMulti>(ItemsIdList[i]).IsInvisibleItem())
                {
                    validItems.Add(i);
                }
            }

            randItemIndex = validItems[Random.GetNumber(validItems.Count)];
            obj = scene.GetGameObject<ItemsMulti>(randItemIndex);
        }

        obj.Spawn();
        field7_0xd = randItemIndex;
    }

    public int GetRandomActionId()
    {
        if (Timer != 0 && GameTime.ElapsedFrames - Timer <= 600)
        {
            int rand = Random.GetNumber(2);
            if (rand == field8_0xe)
                rand = (rand + 1) % 2;
            field7_0xd = rand;
            return rand;
        }
        else
        {
            int rand = Random.GetNumber(2);
            if (rand >= field8_0xe)
                rand++;
            field7_0xd = rand;
            return rand;
        }
    }
}