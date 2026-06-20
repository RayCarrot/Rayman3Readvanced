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

        Engine.Multiplayer.Init();

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
        bool connected = Engine.Multiplayer.Step();

        if (connected && !EndOfFrame)
        {
            if (Engine.Multiplayer.HasReadJoyPads())
            {
                GameTime.Resume();
                base.Step();
                Engine.Multiplayer.FrameProcessed();
            }
            else
            {
                GameTime.Pause();
            }
        }
        else
        {
            Engine.Sem.StopAllSongs();

            InitialMenuPage menuPage = EndOfFrame
                ? InitialMenuPage.Multiplayer
                : InitialMenuPage.MultiplayerLostConnection;
            if (Engine.Settings.Active.Tweaks.UseModernMainMenu)
                Engine.FrameMngr.SetNextFrame(new ModernMenuAll(menuPage));
            else
                Engine.FrameMngr.SetNextFrame(new MenuAll(menuPage));
        }
    }
}