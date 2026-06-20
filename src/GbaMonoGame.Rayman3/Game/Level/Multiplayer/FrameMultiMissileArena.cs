using GbaMonoGame.Rayman3.Readvanced;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3;

public class FrameMultiMissileArena : FrameMissileMultiMode7
{
    public FrameMultiMissileArena() : base(MapId.GbaMulti_MissileArena, 3) { }

    public override void Init()
    {
        Gfx.ClearColor = Color.Black;
        base.Init();

        Engine.Multiplayer.Init();

        AddWalls(new Point(2, 4), new Point(3, 3));

        // The map data is 128x128, but the actual map is only 100x100, so we need to override the dimensions to avoid you seeing outside the map!
        ExtendMap(
        [
            new(3), new(4), new(5),
            new(2), new(8), new(1),
            new(7), new(9), new(6)
        ], 3, 3, overrideMapWidth: 100, overrideMapHeight: 100);
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