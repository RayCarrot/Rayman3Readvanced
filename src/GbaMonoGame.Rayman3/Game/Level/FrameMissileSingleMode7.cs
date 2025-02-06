namespace GbaMonoGame.Rayman3;

public class FrameMissileSingleMode7 : FrameMode7
{
    public FrameMissileSingleMode7(MapId mapId, ushort[] lapTimes) : base(mapId)
    {
        LapTimes = lapTimes;
    }

    public ushort[] LapTimes { get; }
    public RaceManager RaceManager { get; set; }
    public bool[] CollectedLums { get; set; }

    // TODO: Name
    public byte field_0x3a { get; set; }
    public byte field_0x3b { get; set; }

    public override void Init()
    {
        base.Init();

        GameInfo.LevelType = LevelType.Race;
        field_0x3b = 0xff;
        field_0x3a = 0;

        // TODO: Hook into VCOUNTER_MATCH for fog effects

        UserInfo = new UserInfoSingleMode7(Scene);

        RaceManager = new RaceManager(LapTimes);

        int lumsCount = GameInfo.GetTotalYelloLumsInLevel();
        CollectedLums = new bool[lumsCount];

        Scene.AddDialog(UserInfo, false, false);

        // TODO: Set VSYNC callback for fade effects
    }

    public override void Step()
    {
        base.Step();

        if (EndOfFrame)
            GameInfo.LoadLevel(GameInfo.GetNextLevelId());

        if (!IsPaused())
            RaceManager.Step();
    }
}