using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3;

public class FrameSingleMode7 : FrameMode7
{
    public FrameSingleMode7(MapId mapId, ushort[] lapTimes) : base(mapId)
    {
        LapTimes = lapTimes;
    }

    public new UserInfoSingleMode7 UserInfo
    {
        get => (UserInfoSingleMode7)base.UserInfo;
        set => base.UserInfo = value;
    }

    public ushort[] LapTimes { get; }
    public RaceManager RaceManager { get; set; }
    public bool[] CollectedLums { get; set; }

    // TODO: Name
    public byte field_0x3a { get; set; }
    public byte field_0x3b { get; set; }

    public void KillLum(int lumId)
    {
        CollectedLums[lumId] = true;

        int lumsBarValue = UserInfo.LumsBar.CollectedLumsDigitValue1 * 10 + UserInfo.LumsBar.CollectedLumsDigitValue2;
        if (lumsBarValue == GameInfo.GetTotalYelloLumsInLevel())
        {
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumTotal_Mix02);
            LevelMusicManager.OverrideLevelMusic(Rayman3SoundEvent.Play__win2);
        }
    }

    public void SaveLums()
    {
        for (int i = 0; i < CollectedLums.Length; i++)
        {
            if (CollectedLums[i])
                GameInfo.SetYellowLumAsCollected(i);
        }
    }

    public override void Init()
    {
        base.Init();

        GameInfo.LevelType = LevelType.Race;
        field_0x3b = 0xff;
        field_0x3a = 0;

        // TODO: Hook into VCOUNTER_MATCH for fog effects

        UserInfo = new UserInfoSingleMode7(Scene);

        RaceManager = new RaceManager(Scene, UserInfo, LapTimes);

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