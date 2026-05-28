using System;
using System.Diagnostics;

namespace GbaMonoGame.Rayman3;

public class MultiplayerInfo
{
    public MultiplayerGameType GameType { get; set; }
    public CaptureTheFlagMode CaptureTheFlagMode { get; set; }
    public int? MapId { get; set; }
    public uint InitialGameTime { get; set; }
    public TagInfo TagInfo { get; set; }

    public void Init()
    {
        GameType = default;
        MapId = null;
        InitialGameTime = 0;
        TagInfo = null;
    }

    public void SetGameType(MultiplayerGameType gameType)
    {
        if (MapId == null)
            throw new Exception("Can't set game type before setting map");

        GameType = gameType;

        if (GameType is MultiplayerGameType.RayTag or MultiplayerGameType.CatAndMouse or MultiplayerGameType.CaptureTheFlag)
        {
            Debug.Assert(TagInfo == null, "Tag info has already been set");
            TagInfo = new TagInfo(GameType, MapId.Value);
        }
    }
}