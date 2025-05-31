using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public class ChallengeLy : FrameSideScroller
{
    public ChallengeLy(MapId mapId) : base(mapId) { }

    public int Timer { get; set; }

    public override void Init()
    {
        Timer = 0;
        base.Init();

        if (GameInfo.MapId != MapId.ChallengeLyGCN)
            Scene.AddDialog(new TextBoxDialog(Scene), false, false);

        GameInfo.RemainingTime = GameInfo.MapId != MapId.ChallengeLyGCN ? 4200 : 3900;
        UserInfo.HideBars();
    }

    public override void Step()
    {
        base.Step();

        if (Timer <= 120)
            Timer++;

        // Wait 1 second before starting the timer
        if (Timer == 60)
            IsTimed = true;

        // Kill Rayman if time has run out
        if (GameInfo.RemainingTime == 0)
            Scene.MainActor.ProcessMessage(this, Message.Actor_Explode);
    }
}