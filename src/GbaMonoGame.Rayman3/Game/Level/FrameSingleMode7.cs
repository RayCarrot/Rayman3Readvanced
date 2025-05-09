using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework.Graphics;

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

    public GfxScreen FogScreen { get; set; }
    public Mode7RedFogScreenRenderer FogScreenRenderer { get; set; }
    public GfxScreen WallsScreen { get; set; }
    public Mode7WallsScreenRenderer WallsScreenRenderer { get; set; }
    public int ColorAdd { get; set; }
    public int ColorAddDelta { get; set; }

    private void InitFog()
    {
        // NOTE: The game handles the fog by updating the backdrop color based on the following scanlines and then blends it with the screen

        FogScreenRenderer = new Mode7RedFogScreenRenderer(
        [
            new Mode7RedFogScreenRenderer.FogLine(0x4, 0x00), // NOTE: Defined as scanline 0x14 in-game, but we want it to start from the beginning
            new Mode7RedFogScreenRenderer.FogLine(0x5, 0x1C),
            new Mode7RedFogScreenRenderer.FogLine(0x6, 0x22),
            new Mode7RedFogScreenRenderer.FogLine(0x7, 0x24),
            new Mode7RedFogScreenRenderer.FogLine(0x8, 0x28),
            new Mode7RedFogScreenRenderer.FogLine(0x9, 0x2C),
            new Mode7RedFogScreenRenderer.FogLine(0xA, 0x30),
            new Mode7RedFogScreenRenderer.FogLine(0xB, 0x34),
            new Mode7RedFogScreenRenderer.FogLine(0xC, 0x38),
            new Mode7RedFogScreenRenderer.FogLine(0xD, 0x3A),
            new Mode7RedFogScreenRenderer.FogLine(0xE, 0x3C),
            new Mode7RedFogScreenRenderer.FogLine(0xF, 0x3E),
            new Mode7RedFogScreenRenderer.FogLine(0xC, 0x42),
            new Mode7RedFogScreenRenderer.FogLine(0x9, 0x44),
            new Mode7RedFogScreenRenderer.FogLine(0x6, 0x46),
            new Mode7RedFogScreenRenderer.FogLine(0x4, 0x48),
        ]);

        FogScreen = new GfxScreen(5)
        {
            Priority = 0,
            Wrap = false,
            Is8Bit = null,
            Offset = Vector2.Zero,
            GbaAlpha = 12,
            IsEnabled = true,
            Renderer = FogScreenRenderer,
            RenderOptions = { BlendMode = BlendMode.Additive, RenderContext = Scene.RenderContext }
        };

        Gfx.AddScreen(FogScreen);
    }

    private void StepFog()
    {
        // NOTE: The game updates the fog color in a VSYNC callback

        if (!TransitionsFX.IsFadingIn && !TransitionsFX.IsFadingOut && !IsPaused())
        {
            FogScreen.IsEnabled = true;

            if (ColorAdd == 4)
                ColorAddDelta = -1;
            else if (ColorAdd == -4)
                ColorAddDelta = 1;

            if ((GameTime.ElapsedFrames & 3) == 0)
            {
                ColorAdd += ColorAddDelta;
                FogScreenRenderer.ColorAdd = ColorAdd;
            }
        }
        else
        {
            ColorAddDelta = 1;
            ColorAdd = 0;

            FogScreen.IsEnabled = false;
        }
    }

    private void InitWalls()
    {
        TgxPlayfieldMode7 playfield = (TgxPlayfieldMode7)Scene.Playfield;
        TgxRotscaleLayerMode7 layer = playfield.RotScaleLayers[0];

        MapTile[] wallTiles = new MapTile[3 * 3];
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                int absX = 1 + x; // TODO: magic coordinate (1,22)
                int absY = 22 + y;

                int index = absX + absY * layer.Width;
                MapTile tile = layer.TileMap[index];

                wallTiles[x + y * 3] = tile;
            }
        }

        Texture2D texture = new TiledTexture2D(
            width: 3, // TODO: Magic number
            height: 3, // TODO: Magic number
            tileSet: playfield.GfxTileKitManager.TileSet,
            tileMap: wallTiles,
            baseTileIndex: 512, // TODO: Magic number
            palette: playfield.GfxTileKitManager.SelectedPalette,
            is8Bit: true,
            ignoreZero: true);

        WallsScreenRenderer = new Mode7WallsScreenRenderer(layer.TileMap, texture, (TgxPlayfieldMode7)Scene.Playfield);

        WallsScreen = new GfxScreen(6)
        {
          Priority = 0,
          Wrap = false,
          Is8Bit = null,
          Offset = Vector2.Zero,
          GbaAlpha = 16, // TODO: Magic number
          IsEnabled = true,
          Renderer = WallsScreenRenderer,
          RenderOptions = { BlendMode = BlendMode.None, RenderContext = Scene.RenderContext }
        };

        Gfx.AddScreen(WallsScreen);
    }

    public void KillLum(int lumId)
    {
        CollectedLums[lumId] = true;

        int lumsBarValue = UserInfo.LumsBar.CollectedLumsDigitValue1 * 10 + UserInfo.LumsBar.CollectedLumsDigitValue2;
        if (lumsBarValue == GameInfo.GetLumsCountForCurrentMap())
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
                GameInfo.KillLum(i);
        }
    }

    public override void Init()
    {
        base.Init();

        GameInfo.LevelType = LevelType.Race;
        ColorAddDelta = -1;
        ColorAdd = 0;

        UserInfo = new UserInfoSingleMode7(Scene);

        RaceManager = new RaceManager(Scene, UserInfo, LapTimes);

        int lumsCount = GameInfo.GetLumsCountForCurrentMap();
        CollectedLums = new bool[lumsCount];

        Scene.AddDialog(UserInfo, false, false);

        InitFog();
        InitWalls();
    }

    public override void Step()
    {
        base.Step();

        if (EndOfFrame)
            GameInfo.LoadLevel(GameInfo.GetNextLevelId());

        if (!IsPaused())
            RaceManager.Step();

        StepFog();
    }
}