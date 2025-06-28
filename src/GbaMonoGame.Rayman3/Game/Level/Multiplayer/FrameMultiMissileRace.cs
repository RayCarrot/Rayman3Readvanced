using GbaMonoGame.Rayman3.Readvanced;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public class FrameMultiMissileRace : FrameMissileMultiMode7
{
    public FrameMultiMissileRace() : base(MapId.GbaMulti_MissileRace, 3) { }

    public override void Init()
    {
        Gfx.ClearColor = Color.Black;
        base.Init();

        MultiplayerManager.Init();

        AddWalls(new Point(1, 22), new Point(3, 3));

        ExtendMap(
        [
            new(2), new(3), new(4),
            new(6), new(1), new(7),
            new(8), new(5), new(9)
        ], 3, 3);
    }

    public override void Step()
    {
        bool connected = MultiplayerManager.Step();

        if (connected && !EndOfFrame)
        {
            if (MultiplayerManager.HasReadJoyPads())
            {
                GameTime.Resume();
                base.Step();
                MultiplayerManager.FrameProcessed();
            }
            else
            {
                GameTime.Pause();
            }
        }
        else
        {
            SoundEventsManager.StopAllSongs();

            InitialMenuPage menuPage = EndOfFrame
                ? InitialMenuPage.Multiplayer
                : InitialMenuPage.MultiplayerLostConnection;
            FrameManager.SetNextFrame(new ModernMenuAll(menuPage));
        }
    }
}