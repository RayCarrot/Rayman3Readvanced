using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public class FrameSideScrollerGCN : FrameSideScroller
{
    public FrameSideScrollerGCN(GameCubeMapInfo mapInfo, GameCubeMap map, int gcnMapId) : base(default)
    {
        MapInfo = mapInfo;
        Map = map;
        GcnMapId = gcnMapId;
    }

    public GameCubeMap Map { get; }

    public MapId PreviousMapId { get; set; }
    public Power PreviousPowers { get; set; }

    public int GcnMapId { get; }
    public GameCubeMapInfo MapInfo { get; }

    public void RestoreMapAndPowers()
    {
        GameInfo.MapId = PreviousMapId;
        GameInfo.Powers = PreviousPowers;
    }

    public void FUN_0808a9f4()
    {
        if (GcnMapId == 3)
        {
            // TODO: Implement
        }
    }

    public override void Init()
    {
        GameInfo.InitLevel(LevelType.GameCube);

        PreviousMapId = GameInfo.MapId;
        GameInfo.MapId = MapId.GameCube_Bonus1 + GcnMapId;

        PreviousPowers = GameInfo.Powers;
        GameInfo.Powers = Power.All;

        GameInfo.YellowLumsCount = MapInfo.LumsCount;
        GameInfo.CagesCount = MapInfo.CagesCount;

        LevelMusicManager.Init();

        if (MapInfo.StartMusicSoundEvent != Rayman3SoundEvent.None)
            SoundEventsManager.ProcessEvent(MapInfo.StartMusicSoundEvent);

        CircleTransitionScreenEffect = new CircleTransitionScreenEffect();

        TransitionsFX = new TransitionsFX(true);
        Scene = new Scene2D(Map, x => new CameraSideScroller(x), 3, 1);

        // Add user info (default hud)
        UserInfo = new UserInfoSideScroller(Scene, MapInfo.HasBlueLum);
        UserInfo.ProcessMessage(this, Message.UserInfo_GameCubeLevel);

        // Create pause dialog, but don't add yet
        PauseDialog = new PauseDialog(Scene);

        Scene.AddDialog(UserInfo, false, false);
        Scene.Init();
        Scene.Playfield.Step();

        InitNewCircleTransition(true);

        Scene.AnimationPlayer.Execute();

        CanPause = true;
        Fog = null;
        LyTimer = null;
        CurrentStepAction = Step_Normal;

        switch (GcnMapId)
        {
            case 0:
                // Add fog
                Fog = new FogDialog(Scene);
                Scene.AddDialog(Fog, false, false);
                break;
            
            case 1:
                // TODO: Add config option for scrolling on N-Gage
                if (Rom.Platform == Platform.GBA)
                {
                    TgxTileLayer cloudsLayer = ((TgxPlayfield2D)Scene.Playfield).TileLayers[0];
                    TextureScreenRenderer renderer = (TextureScreenRenderer)cloudsLayer.Screen.Renderer;
                    cloudsLayer.Screen.Renderer = new LevelCloudsRenderer(renderer.Texture, [32, 120, 227])
                    {
                        PaletteTexture = renderer.PaletteTexture
                    };
                }
                break;
            
            case 2:
                // TODO: Implement
                break;
            
            case 4:
                // TODO: Implement
                break;
            
            case 3 or 6:
                // TODO: Implement
                break;
        }
    }

    public override void Step()
    {
        base.Step();

        // TODO: Level-specific code
    }
}