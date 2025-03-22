using System;
using System.Diagnostics;
using BinarySerializer;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.Rayman3.Readvanced;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;
using Action = System.Action;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace GbaMonoGame.Rayman3;

public class WorldMap : Frame, IHasScene, IHasPlayfield
{
    #region Constructor

    public WorldMap(MapId mapId)
    {
        GameInfo.SetNextMapId(mapId);

        CurrentMovement = WorldMapMovement.None;
        WorldId = GameInfo.WorldId;
        SelectedWorldType = WorldType.None;
        ScrollX = Rom.Platform switch
        {
            Platform.GBA => 56,
            Platform.NGage => 0,
            _ => throw new UnsupportedPlatformException()
        };

        GameInfo.IsInWorldMap = true;
    }

    #endregion

    #region Constant Fields

    // NOTE: The game uses 16, but it only updates every 8 frames. We instead update every frame.
    private const int VolcanoGlowMaxValue = 16 * 8;

    // NOTE: The game uses 8, but it only updates every 4 frames. We instead update every frame.
    private const int LightningMaxValue = 8 * 4;

    #endregion

    #region Public Properties

    public Scene2D Scene { get; set; }
    public TransitionsFX TransitionsFX { get; set; }
    public UserInfoWorldMap UserInfo { get; set; }
    public Dialog PauseDialog { get; set; }
    public FadeControl SavedFadeControl { get; set; }

    public AnimatedObject Rayman { get; set; }
    public AnimatedObject[] WorldPaths { get; set; }
    public AnimatedObject GameCubeSparkles { get; set; }

    public Action CurrentStepAction { get; set; }
    public Action CurrentExStepAction { get; set; }

    public float ScrollX { get; set; }
    public byte NGageScrollCooldown { get; set; }
    public ushort Timer { get; set; }

    public SpriteTextObject FullWorldName { get; set; }
    public int WorldNameAlpha { get; set; }
    public int EnterWorldStep { get; set; }

    public SquareTransitionScreenEffect GameCubeTransitionScreenEffect { get; set; }
    public int EnterGameCubeMenuStep { get; set; }

    public CircleWipeTransitionScreenEffect CircleWipeTransitionScreenEffect { get; set; }
    public int CircleWipeTransitionValue { get; set; }
    public TransitionMode CircleWipeTransitionMode { get; set; }

    public byte SpikyBagSinValue { get; set; }
    public bool SpikyBagScrollDirection { get; set; }
    
    public int LightningCountdown { get; set; }
    public byte LightningValue { get; set; }
    public bool LightningAlternation { get; set; }
    public bool ShouldSetLightningAlpha { get; set; }
    public PaletteTexture[] LightningSkyPaletteTextures { get; set; }

    public int VolcanoGlowValue { get; set; }
    public PaletteTexture[] VolcanoPaletteTextures { get; set; }

    public WorldMapMovement CurrentMovement { get; set; }
    public WorldType SelectedWorldType { get; set; }
    public WorldId WorldId { get; set; }
    public byte CheatValue { get; set; }

    public Vector2 BaseObjPos => Rom.Platform switch
    {
        Platform.GBA => new Vector2(246, 84),
        Platform.NGage => new Vector2(175, 114),
        _ => throw new UnsupportedPlatformException()
    };

    #endregion

    #region Interface Properties

    Scene2D IHasScene.Scene => Scene;
    TgxPlayfield IHasPlayfield.Playfield => Scene.Playfield;

    #endregion

    #region Private Methods

    private void InitSpikyBag()
    {
        GfxScreen spikyBagScreen = Gfx.GetScreen(3);

        // Since it scrolls with the camera we want to use the normal render context instead of a scaled parallax background one
        spikyBagScreen.RenderOptions.RenderContext = Scene.RenderContext;

        float offsetX = Rom.Platform switch
        {
            Platform.GBA => -16,
            Platform.NGage => -72,
            _ => throw new UnsupportedPlatformException()
        };

        // In the original game it keeps the wrap, but creates a window to hide the screen on the left side of the worldmap
        if (Rom.Platform == Platform.GBA)
        {
            spikyBagScreen.Wrap = false;
            offsetX += spikyBagScreen.Renderer.GetSize(spikyBagScreen).X;
        }

        spikyBagScreen.Offset = spikyBagScreen.Offset with { X = ScrollX - offsetX };

        SpikyBagSinValue = 0;
    }

    private void StepSpikyBag()
    {
        GfxScreen spikyBagScreen = Gfx.GetScreen(3);
        float offsetX = Rom.Platform switch
        {
            Platform.GBA => -16,
            Platform.NGage => 165,
            _ => throw new UnsupportedPlatformException()
        };

        // In the original game it keeps the wrap, but creates a window to hide the screen on the left side of the worldmap
        if (Rom.Platform == Platform.GBA)
            offsetX += spikyBagScreen.Renderer.GetSize(spikyBagScreen).X;

        spikyBagScreen.Offset = spikyBagScreen.Offset with { X = ScrollX - (4 * MathHelpers.Sin256(SpikyBagSinValue) + offsetX) };

        if (!SpikyBagScrollDirection)
        {
            SpikyBagSinValue++;
            if (SpikyBagSinValue == 63)
                SpikyBagScrollDirection = true;
        }
        else
        {
            SpikyBagSinValue--;
            if (SpikyBagSinValue == 192)
                SpikyBagScrollDirection = false;
        }
    }

    private void InitLightning()
    {
        // Create a palette texture for each modified palette frame (+4 because of N-Gage doing one additional one for some reason)
        LightningSkyPaletteTextures = new PaletteTexture[LightningMaxValue + 4 + 1];

        // Get the original colors
        GfxTileKitManager tileKitManager = Scene.Playfield.GfxTileKitManager;
        Color[] originalColors = tileKitManager.SelectedPalette.Colors;
        int palLength = originalColors.Length;

        // Create an array for the new colors
        Color[] colors = new Color[palLength];
        Array.Copy(originalColors, colors, palLength);

        // Create a palette texture for each 
        for (int value = 0; value < LightningSkyPaletteTextures.Length; value++)
        {
            // Lerp the colors in sub-palettes 1 and 2
            for (int subPaletteIndex = 0; subPaletteIndex < 32; subPaletteIndex++)
            {
                if (subPaletteIndex > 4 && 
                    subPaletteIndex != 6 &&
                    subPaletteIndex != 7 && 
                    subPaletteIndex is not 8 and not 11 && 
                    subPaletteIndex != 13 &&
                    subPaletteIndex != 14)
                {
                    int fullPalIndex = 16 * 1 + subPaletteIndex;
                    Vector3 originalColorVector = originalColors[fullPalIndex].ToVector3();

                    Vector3 newColorVector = originalColorVector + (Vector3.One - originalColorVector) * (value / (LightningMaxValue * 2f));

                    colors[fullPalIndex] = new Color(newColorVector);
                }
            }

            LightningSkyPaletteTextures[value] = new PaletteTexture(
                Texture: Engine.TextureCache.GetOrCreateObject(
                    pointer: tileKitManager.SelectedPalette.CachePointer,
                    id: value,
                    data: colors,
                    createObjFunc: static c => new PaletteTexture2D(c)),
                PaletteIndex: 0);
        }

        LightningCountdown = Random.GetNumber(120) + 120;
        LightningValue = 8;
        LightningAlternation = false;
        ShouldSetLightningAlpha = true;
        
        TgxPlayfield2D playfield = (TgxPlayfield2D)Scene.Playfield;
        TgxTileLayer lightningLayer = playfield.TileLayers[1];
        lightningLayer.Screen.IsEnabled = false;
    }

    private void StepLightning()
    {
        TgxPlayfield2D playfield = (TgxPlayfield2D)Scene.Playfield;
        TgxTileLayer lightningSkyLayer = playfield.TileLayers[0];
        GfxScreen lightningSkyScreen = lightningSkyLayer.Screen;
        TgxTileLayer lightningLayer = playfield.TileLayers[1];
        TileMapScreenRenderer lightningRenderer = (TileMapScreenRenderer)lightningLayer.Screen.Renderer;

        // NOTE: In the original game the clip, which is done using a window, is set to mask away 392-420 or 420-464.
        //       This however masks half tiles which would be really annoying to implement, since we'd need to clip
        //       individual tiles then, and also seems to be a miscalculation by the game since it produces graphical
        //       artifact of a few pixels that are left on both sides.
        int lightningClip = Rom.Platform switch
        {
            Platform.GBA => 416,
            Platform.NGage => 352,
            _ => throw new ArgumentOutOfRangeException()
        };

        Vector2 size = lightningRenderer.GetSize(lightningLayer.Screen);

        // TODO: Allow this effect on N-Gage
        if (Rom.Platform == Platform.GBA)
        {
            // NOTE: The game only does this if ScrollX > 152, but we can ignore that

            // Show the right flash
            if (LightningAlternation)
            {
                lightningRenderer.TilesClip = new Rectangle(
                    x: lightningClip / Tile.Size, 
                    y: 0, 
                    width: ((int)size.X - lightningClip) / Tile.Size, 
                    height: (int)size.Y / Tile.Size);
            }
            // Show the left flash
            else
            {
                lightningRenderer.TilesClip = new Rectangle(
                    x: 0, 
                    y: 0, 
                    width: lightningClip / Tile.Size, 
                    height: (int)size.Y / Tile.Size);
            }
        }

        if (ShouldSetLightningAlpha)
        {
            lightningLayer.Screen.RenderOptions.BlendMode = BlendMode.AlphaBlend;
            lightningLayer.Screen.GbaAlpha = 8;

            ShouldSetLightningAlpha = false;
        }

        if (LightningCountdown < 1)
        {
            // NOTE: The game only updates this every 4 frames
            const int factor = 4;

            lightningLayer.Screen.IsEnabled = LightningValue < 10 * factor;

            int value = LightningValue;

            int max = Rom.Platform switch
            {
                Platform.GBA => LightningMaxValue,
                Platform.NGage => LightningMaxValue + 1 * factor, // Why does it do this?
                _ => throw new ArgumentOutOfRangeException()
            };
            if (LightningValue > max)
            {
                value = LightningMaxValue * 2 - LightningValue;

                if (LightningValue == 12 * factor && LightningCountdown != -1 && Random.GetNumber(100) > 50)
                {
                    LightningValue = LightningMaxValue;
                    value = LightningMaxValue;
                    LightningCountdown = -1;

                    lightningLayer.Screen.IsEnabled = true;

                    LightningAlternation = Random.GetNumber(100) >= 50;
                }
            }

            lightningSkyScreen.RenderOptions.PaletteTexture = LightningSkyPaletteTextures[value];

            LightningValue++;

            if (LightningValue > LightningMaxValue * 2)
            {
                LightningCountdown = Random.GetNumber(120) + 120;
                LightningValue = LightningMaxValue;
                LightningAlternation = Random.GetNumber(100) >= 50;
            }
        }
        else
        {
            LightningCountdown--;
        }
    }

    private void InitVolcanoGlow()
    {
        // Create a palette texture for each modified palette frame
        VolcanoPaletteTextures = new PaletteTexture[VolcanoGlowMaxValue + 1];

        // Get the original colors
        GfxTileKitManager tileKitManager = Scene.Playfield.GfxTileKitManager;
        Color[] originalColors = tileKitManager.SelectedPalette.Colors;
        int palLength = originalColors.Length;

        // Create the target colors for sub-palette 4
        Color[] targetColors =
        [
            new RGB555Color(0x3c0c).ToColor(),
            new RGB555Color(0x32).ToColor(),
            new RGB555Color(0x72).ToColor(),
            new RGB555Color(0x115).ToColor(),
            new RGB555Color(0x94).ToColor(),
            new RGB555Color(0x2193).ToColor(),
            new RGB555Color(0xd0).ToColor(),
            new RGB555Color(0x175).ToColor(),
            new RGB555Color(0x172).ToColor(),
            new RGB555Color(0xe).ToColor(),
            new RGB555Color(0x117).ToColor(),
            new RGB555Color(0x1d6).ToColor(),
            new RGB555Color(0x219).ToColor(),
            new RGB555Color(0x178).ToColor(),
            new RGB555Color(0x51).ToColor(),
            new RGB555Color(0x53).ToColor(),
        ];

        // Create an array for the new colors
        Color[] colors = new Color[palLength];
        Array.Copy(originalColors, colors, palLength);

        // Create a palette texture for each 
        for (int value = 0; value < VolcanoPaletteTextures.Length; value++)
        {
            // Lerp the colors in sub-palette 4
            for (int subPaletteIndex = 0; subPaletteIndex < 16; subPaletteIndex++)
            {
                int fullPalIndex = 16 * 4 + subPaletteIndex;
                colors[fullPalIndex] = Color.Lerp(originalColors[fullPalIndex], targetColors[subPaletteIndex], value / (float)VolcanoGlowMaxValue);
            }

            VolcanoPaletteTextures[value] = new PaletteTexture(
                Texture: Engine.TextureCache.GetOrCreateObject(
                    pointer: tileKitManager.SelectedPalette.CachePointer,
                    id: LightningSkyPaletteTextures.Length + value,
                    data: colors,
                    createObjFunc: static c => new PaletteTexture2D(c)),
                PaletteIndex: 0);
        }

        VolcanoGlowValue = 0;
    }

    private void StepVolcanoGlow()
    {
        // NOTE: The game only updates this every 8 frames

        TgxPlayfield2D playfield = (TgxPlayfield2D)Scene.Playfield;
        TgxTileLayer volcanoLayer = playfield.TileLayers[2];

        int index = VolcanoGlowValue <= VolcanoGlowMaxValue
            ? VolcanoGlowValue
            : VolcanoGlowMaxValue * 2 - VolcanoGlowValue;

        volcanoLayer.Screen.RenderOptions.PaletteTexture = VolcanoPaletteTextures[index];

        VolcanoGlowValue++;

        if (VolcanoGlowValue > VolcanoGlowMaxValue * 2)
            VolcanoGlowValue = 0;
    }

    private bool ProcessCheatInput(GbaInput input)
    {
        GbaInput[] cheatInputs =
        [
            GbaInput.A,
            GbaInput.B,
            GbaInput.Right,
            GbaInput.Left,
            GbaInput.Up,
            GbaInput.Down,
            GbaInput.R,
            GbaInput.L
        ];

        if (JoyPad.IsButtonJustPressed(input))
        {
            CheatValue++;
            return true;
        }
        else
        {
            foreach (GbaInput cheatInput in cheatInputs)
            {
                if (cheatInput != input && JoyPad.IsButtonJustPressed(cheatInput))
                {
                    CheatValue = 0;
                    break;
                }
            }

            return false;
        }
    }

    private void ManageCheats()
    {
        // Make sure select is held down
        if (JoyPad.IsButtonReleased(GbaInput.Select))
        {
            CheatValue = 0;
            return;
        }

        // TODO: There is currently no feedback to the player if a cheat has been entered. In the original game
        //       you notice it since the game freezes for a second when saving. Perhaps we should play some sound
        //       effect here?
        switch (CheatValue)
        {
            // Start
            case 0:
                if (JoyPad.IsButtonJustPressed(GbaInput.R))
                    CheatValue = 1;

                if (JoyPad.IsButtonJustPressed(GbaInput.B))
                    CheatValue = 32;

                if (JoyPad.IsButtonJustPressed(GbaInput.Up))
                    CheatValue = 64;

                if (JoyPad.IsButtonJustPressed(GbaInput.Down))
                    CheatValue = 100;
                break;

            // All powers (R + A + A + A)
            case 1:
                ProcessCheatInput(GbaInput.A);
                break;
            
            case 2:
                ProcessCheatInput(GbaInput.A);
                break;

            case 3:
                if (ProcessCheatInput(GbaInput.A))
                {
                    GameInfo.EnableCheat(Scene, Cheat.AllPowers);
                    
                    CheatValue = 0;
                    
                    GameInfo.Save(GameInfo.CurrentSlot);
                }
                break;

            // 99 lives (B + B + B + A + A + A)
            case 32:
                ProcessCheatInput(GbaInput.B);
                break;

            case 33:
                ProcessCheatInput(GbaInput.B);
                break;

            case 34:
                ProcessCheatInput(GbaInput.A);
                break;

            case 35:
                ProcessCheatInput(GbaInput.A);
                break;

            case 36:
                if (ProcessCheatInput(GbaInput.A))
                {
                    GameInfo.EnableCheat(Scene, Cheat.InfiniteLives);
                    
                    CheatValue = 0;
                }
                break;

            // Unlock all levels (Up + Left + Down + Right + B + A + L + R)
            case 64:
                ProcessCheatInput(GbaInput.Left);
                break;

            case 65:
                ProcessCheatInput(GbaInput.Down);
                break;

            case 66:
                ProcessCheatInput(GbaInput.Right);
                break;

            case 67:
                ProcessCheatInput(GbaInput.B);
                break;

            case 68:
                ProcessCheatInput(GbaInput.A);
                break;

            case 69:
                ProcessCheatInput(GbaInput.L);
                break;

            case 70:
                if (ProcessCheatInput(GbaInput.R))
                {
                    GameInfo.EnableCheat(Scene, Cheat.AllPowers);
                    UnlockAllLevels();
                    
                    CheatValue = 0;
                 
                    GameInfo.Save(GameInfo.CurrentSlot);
                }
                break;

            // 100% (Down + Up + Down + Up + A + Left + B + Right)
            case 100:
                ProcessCheatInput(GbaInput.Up);
                break;

            case 101:
                ProcessCheatInput(GbaInput.Down);
                break;

            case 102:
                ProcessCheatInput(GbaInput.Up);
                break;

            case 103:
                ProcessCheatInput(GbaInput.A);
                break;

            case 104:
                ProcessCheatInput(GbaInput.Left);
                break;

            case 105:
                ProcessCheatInput(GbaInput.B);
                break;

            case 106:
                if (ProcessCheatInput(GbaInput.Right))
                {
                    GameInfo.PersistentInfo.FinishedLyChallenge1 = true;
                    GameInfo.PersistentInfo.FinishedLyChallenge2 = true;

                    if (Rom.Platform == Platform.GBA)
                        GameInfo.PersistentInfo.FinishedLyChallengeGCN = true;
                    
                    GameInfo.PersistentInfo.PlayedAct4 = true;
                    GameInfo.PersistentInfo.PlayedMurfyWorldHelp = true;
                    GameInfo.PersistentInfo.UnlockedFinalBoss = true;

                    if (Rom.Platform == Platform.GBA)
                        GameInfo.PersistentInfo.CompletedGCNBonusLevels = 10;

                    GameInfo.KillAllCages();
                    GameInfo.KillAllLums();

                    GameInfo.EnableCheat(Scene, Cheat.InfiniteLives);
                    GameInfo.EnableCheat(Scene, Cheat.AllPowers);
                    UnlockAllLevels();
                    
                    CheatValue = 0;

                    UserInfo.Lums1000Bar.Set();
                    UserInfo.Cages50Bar.Set();

                    GameInfo.Save(GameInfo.CurrentSlot);
                }
                break;
        }
    }

    private void UnlockAllLevels()
    {
        MapId mapId = GameInfo.MapId;
        GameInfo.MapId = MapId.BossFinal_M2;
        GameInfo.UpdateLastCompletedLevel();
        GameInfo.MapId = mapId;

        GameInfo.PersistentInfo.UnlockedWorld2 = true;
        GameInfo.PersistentInfo.PlayedWorld2Unlock = true;
        GameInfo.PersistentInfo.UnlockedWorld3 = true;
        GameInfo.PersistentInfo.PlayedWorld3Unlock = true;
        GameInfo.PersistentInfo.UnlockedWorld4 = true;
        GameInfo.PersistentInfo.PlayedWorld4Unlock = true;
        GameInfo.PersistentInfo.PlayedAct4 = true;

        WorldPaths[0].CurrentAnimation = 3;
    }

    private void WorldNameInit()
    {
        string worldNameText = Localization.GetText(TextBankId.LevelNames, WorldId switch
        {
            WorldId.World1 => 31,
            WorldId.World2 => 32,
            WorldId.World3 => 33,
            WorldId.World4 => 34,
            _ => throw new ArgumentOutOfRangeException(nameof(WorldId), WorldId, "Invalid world id"),
        })[0];

        FullWorldName.Text = worldNameText;
        FullWorldName.ScreenPos = new Vector2(-FullWorldName.GetStringWidth() / 2f, 64);

        WorldNameAlpha = 0;
        EnterWorldStep = 0;

        if (Rom.Platform == Platform.GBA)
            LightningCountdown = 0;

        FullWorldName.GbaAlpha = WorldNameAlpha;

        // NOTE: The original game hides the background and sprites by setting all palette colors to fully black. Since
        //       we can't do that here the easiest seems to be to just remove all screens, dialogs and pending sprites.
        Gfx.ClearColor = Color.Black;
        Gfx.ClearScreens();
        while (Scene.Dialogs.Count > 0)
            Scene.RemoveLastDialog();
        Scene.AnimationPlayer.Clear();

        CurrentExStepAction = StepEx_EnterWorld;
    }

    private void SelectGameCube()
    {
        // NOTE: The game disables the spiky bag layer here due to it creating a new window for the
        //       transition. We however don't want to do that because it might be visible if in widescreen.

        GameCubeTransitionScreenEffect = new SquareTransitionScreenEffect()
        {
            Square = new Box(Vector2.Zero, Scene.RenderContext.Resolution),
            RenderOptions = { RenderContext = Scene.RenderContext },
        };
        Gfx.SetScreenEffect(GameCubeTransitionScreenEffect);

        EnterGameCubeMenuStep = 0;
        Timer = 0;
        CurrentMovement = WorldMapMovement.None;
        WorldId = WorldId.Special;
        CurrentExStepAction = StepEx_EnterGameCubeMenu;
    }

    #endregion

    #region Public Methods

    public override void Init()
    {
        switch (WorldId)
        {
            case WorldId.World1:
                ScrollX = 0;
                break;

            case WorldId.World2:
                ScrollX = Rom.Platform switch
                {
                    Platform.GBA => 128,
                    Platform.NGage => 72,
                    _ => throw new UnsupportedPlatformException()
                };
                break;
            
            case WorldId.World3:
            case WorldId.World4:
                ScrollX = Rom.Platform switch
                {
                    Platform.GBA => MathHelpers.FromFixedPoint(0xdf3544), // ???
                    Platform.NGage => 176,
                    _ => throw new UnsupportedPlatformException()
                };
                break;

            default:
                throw new Exception("Invalid world id");
        }

        CircleWipeTransitionValue = 0xFF;
        CircleWipeTransitionMode = TransitionMode.In;
        
        // Add the circle wipe transition as a screen effect. On the GBA this is done using a window.
        CircleWipeTransitionScreenEffect = new CircleWipeTransitionScreenEffect
        {
            Value = 256,
            RenderOptions = { RenderContext = Engine.GameRenderContext },
        };
        Gfx.SetScreenEffect(CircleWipeTransitionScreenEffect);

        TransitionsFX = new TransitionsFX(true);
        GameInfo.InitLevel(LevelType.Normal);
        LevelMusicManager.Init();
        
        Scene = new Scene2D((int)GameInfo.MapId, x => new CameraWorldMap(x), 3, 1);

        // For some reason this playfield has 8 pixels of blank space on the bottom, so we have to limit the vertical resolution
        Vector2 maxRes = new(((TgxPlayfield2D)Scene.Playfield).Size.X, Rom.OriginalResolution.Y);
        ((TgxPlayfield2D)Scene.Playfield).RenderContext.MaxResolution = maxRes;

        // Create pause dialog, but don't add yet
        PauseDialog = Engine.Config.UseModernPauseDialog ? new ModernPauseDialog(Scene, false) : new PauseDialog(Scene);

        Scene.Init();
        Scene.Playfield.Step();

        Scene.AnimationPlayer.Execute();

        if (!SoundEventsManager.IsSongPlaying(GameInfo.GetLevelMusicSoundEvent()))
            GameInfo.PlayLevelMusic();

        UserInfo = new UserInfoWorldMap(Scene, GameInfo.GetLevelHasBlueLum());
        Scene.AddDialog(UserInfo, false, false);

        AnimatedObjectResource raymanResource = Rom.LoadResource<AnimatedObjectResource>(GameResource.RaymanWorldMapAnimations);
        Rayman = new AnimatedObject(raymanResource, raymanResource.IsDynamic)
        {
            IsFramed = true,
            CurrentAnimation = 15,
            BgPriority = 1,
            ObjPriority = 0,
            ScreenPos = BaseObjPos - new Vector2(ScrollX, 0),
            RenderContext = Scene.RenderContext,
        };

        AnimatedObjectResource worldPathsResource = Rom.LoadResource<AnimatedObjectResource>(GameResource.WorldMapPathAnimations);
        WorldPaths = new AnimatedObject[3];
        for (int i = 0; i < WorldPaths.Length; i++)
        {
            WorldPaths[i] = new AnimatedObject(worldPathsResource, worldPathsResource.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 32,
                RenderContext = Scene.RenderContext,
            };
        }

        WorldPaths[0].CurrentAnimation = GameInfo.PersistentInfo.UnlockedWorld2 ? 3 : 6;
        WorldPaths[0].ScreenPos = BaseObjPos - new Vector2(ScrollX, 0);

        WorldPaths[1].CurrentAnimation = 4;
        WorldPaths[1].ScreenPos = BaseObjPos - new Vector2(ScrollX, 0);

        WorldPaths[2].CurrentAnimation = 5;
        WorldPaths[2].ScreenPos = BaseObjPos - new Vector2(ScrollX, 0);

        if (Rom.Platform == Platform.GBA)
        {
            GameCubeSparkles = new AnimatedObject(worldPathsResource, worldPathsResource.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 32,
                CurrentAnimation = 7,
                ScreenPos = BaseObjPos - new Vector2(ScrollX, 0),
                RenderContext = Scene.RenderContext,
            };
        }

        FullWorldName = new SpriteTextObject()
        {
            Color = TextColor.FullWorldName,
            FontSize = FontSize.Font16,
            ScreenPos = new Vector2(120, 60),
            HorizontalAnchor = HorizontalAnchorMode.Center,
            Text = "",
            RenderContext = Scene.HudRenderContext,
            RenderOptions = { BlendMode = BlendMode.AlphaBlend },
        };

        Rayman.CurrentAnimation = WorldId switch
        {
            WorldId.World1 => 15,
            WorldId.World2 => 16,
            WorldId.World3 => 17,
            WorldId.World4 => 18,
            _ => throw new Exception("Invalid world id")
        };

        if (GameInfo.PersistentInfo.UnlockedWorld2 && !GameInfo.PersistentInfo.PlayedWorld2Unlock)
        {
            Debug.Assert(WorldId == WorldId.World1, "World #2 cannot be unlocked here");
            CurrentExStepAction = StepEx_UnlockWorld2;
        }
        else if (GameInfo.PersistentInfo.UnlockedWorld3 && !GameInfo.PersistentInfo.PlayedWorld3Unlock)
        {
            Debug.Assert(WorldId == WorldId.World2, "World #3 cannot be unlocked here");
            CurrentExStepAction = StepEx_UnlockWorld3;
        }
        else if (GameInfo.PersistentInfo.UnlockedWorld4 && !GameInfo.PersistentInfo.PlayedWorld4Unlock)
        {
            Debug.Assert(WorldId == WorldId.World3, "World #4 cannot be unlocked here");
            CurrentExStepAction = StepEx_UnlockWorld4;
        }
        else
        {
            CurrentExStepAction = StepEx_Play;
        }

        Timer = 0;

        InitSpikyBag();
        InitLightning();
        InitVolcanoGlow();

        UserInfo.SetWorldId(WorldId);
        UserInfo.ShowWorldBar();

        CurrentStepAction = Step_Normal;

        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Spirale_Mix01);

        CheatValue = 0;

        if (Rom.Platform == Platform.NGage)
            NGageScrollCooldown = 0;
    }

    public override void UnInit()
    {
        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
        Gfx.Fade = 1;

        Scene.UnInit();
        Scene = null;

        Gfx.ClearScreenEffect();
        Gfx.ClearColor = Color.Black;
    }

    public override void Step()
    {
        // The game doesn't do this, but since we support widescreen it means the scroll value might be too big, for example
        // from the initial value or from changing resolution, which would desync it with the camera. So we do this to sync
        // the values and by setting the position in the camera we also make sure it doesn't go beyond the max value.
        TgxCamera cam = Scene.Playfield.Camera;
        cam.Position = cam.Position with { X = ScrollX };
        ScrollX = cam.Position.X;

        if (CurrentStepAction == Step_Normal)
            CurrentExStepAction();

        CurrentStepAction();
    }

    #endregion

    #region Steps

    public void StepEx_UnlockWorld2()
    {
        float xPos = BaseObjPos.X - ScrollX;
        Rayman.ScreenPos = Rayman.ScreenPos with { X = xPos };
        WorldPaths[0].ScreenPos = WorldPaths[0].ScreenPos with { X = xPos };

        if (Rom.Platform == Platform.GBA)
            GameCubeSparkles.ScreenPos = GameCubeSparkles.ScreenPos with { X = xPos };

        if (Timer <= 120)
        {
            if (Timer == 120)
            {
                WorldPaths[0].CurrentAnimation = 0;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PathFX_Mix01);
                Scene.AnimationPlayer.Play(WorldPaths[0]);
            }

            Timer++;
        }
        else
        {
            if (WorldPaths[0].EndOfAnimation)
            {
                WorldPaths[0].CurrentAnimation = 3;
                CurrentExStepAction = StepEx_Play;
                GameInfo.PersistentInfo.PlayedWorld2Unlock = true;
            }

            Scene.AnimationPlayer.Play(WorldPaths[0]);
        }

        Scene.AnimationPlayer.Play(Rayman);

        if (Rom.Platform == Platform.GBA)
            Scene.AnimationPlayer.Play(GameCubeSparkles);

        StepSpikyBag();
        StepLightning();
        StepVolcanoGlow();
    }

    public void StepEx_UnlockWorld3()
    {
        float xPos = BaseObjPos.X - ScrollX;
        Rayman.ScreenPos = Rayman.ScreenPos with { X = xPos };
        WorldPaths[0].ScreenPos = WorldPaths[0].ScreenPos with { X = xPos };
        WorldPaths[1].ScreenPos = WorldPaths[1].ScreenPos with { X = xPos };

        if (Rom.Platform == Platform.GBA)
            GameCubeSparkles.ScreenPos = GameCubeSparkles.ScreenPos with { X = xPos };

        if (Timer <= 120)
        {
            if (Timer == 120)
            {
                WorldPaths[1].CurrentAnimation = 1;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PathFX_Mix01);
                Scene.AnimationPlayer.Play(WorldPaths[1]);
            }

            Timer++;
        }
        else
        {
            if (WorldPaths[1].EndOfAnimation)
            {
                WorldPaths[1].CurrentAnimation = 4;
                CurrentExStepAction = StepEx_Play;
                GameInfo.PersistentInfo.PlayedWorld3Unlock = true;
            }

            Scene.AnimationPlayer.Play(WorldPaths[1]);
        }

        Scene.AnimationPlayer.Play(Rayman);
        Scene.AnimationPlayer.Play(WorldPaths[0]);

        if (Rom.Platform == Platform.GBA)
            Scene.AnimationPlayer.Play(GameCubeSparkles);

        StepSpikyBag();
        StepLightning();
        StepVolcanoGlow();
    }

    public void StepEx_UnlockWorld4()
    {
        float xPos = BaseObjPos.X - ScrollX;
        Rayman.ScreenPos = Rayman.ScreenPos with { X = xPos };
        WorldPaths[0].ScreenPos = WorldPaths[0].ScreenPos with { X = xPos };
        WorldPaths[1].ScreenPos = WorldPaths[1].ScreenPos with { X = xPos };
        WorldPaths[2].ScreenPos = WorldPaths[2].ScreenPos with { X = xPos };

        if (Rom.Platform == Platform.GBA)
            GameCubeSparkles.ScreenPos = GameCubeSparkles.ScreenPos with { X = xPos };

        if (Timer <= 120)
        {
            if (Timer == 120)
            {
                WorldPaths[2].CurrentAnimation = 2;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PathFX_Mix01);
                Scene.AnimationPlayer.Play(WorldPaths[2]);
            }

            Timer++;
        }
        else
        {
            if (WorldPaths[2].EndOfAnimation)
            {
                WorldPaths[2].CurrentAnimation = 5;
                CurrentExStepAction = StepEx_Play;
                GameInfo.PersistentInfo.PlayedWorld4Unlock = true;
            }

            Scene.AnimationPlayer.Play(WorldPaths[2]);
        }

        Scene.AnimationPlayer.Play(Rayman);
        Scene.AnimationPlayer.Play(WorldPaths[0]);
        Scene.AnimationPlayer.Play(WorldPaths[1]);

        if (Rom.Platform == Platform.GBA)
            Scene.AnimationPlayer.Play(GameCubeSparkles);

        StepSpikyBag();
        StepLightning();
        StepVolcanoGlow();
    }

    public void StepEx_Play()
    {
        if (SelectedWorldType == WorldType.None && CircleWipeTransitionMode == TransitionMode.None)
        {
            // Move forward
            if ((JoyPad.IsButtonJustPressed(GbaInput.Right) || 
                 (JoyPad.IsButtonJustPressed(GbaInput.Up) && WorldId is WorldId.World1 or WorldId.World3)) && 
                CurrentMovement == WorldMapMovement.None &&
                JoyPad.IsButtonReleased(GbaInput.Select))
            {
                switch (WorldId)
                {
                    case WorldId.World1:
                        if (GameInfo.PersistentInfo.UnlockedWorld2)
                        {
                            Rayman.CurrentAnimation = 0;
                            CurrentMovement = WorldMapMovement.World1To2;
                            UserInfo.HideWorldBar();
                        }
                        break;

                    case WorldId.World2:
                        if (GameInfo.PersistentInfo.UnlockedWorld3)
                        {
                            Rayman.CurrentAnimation = 2;
                            CurrentMovement = WorldMapMovement.World2To3;
                            UserInfo.HideWorldBar();
                        }
                        break;

                    case WorldId.World3:
                        if (GameInfo.PersistentInfo.UnlockedWorld4)
                        {
                            Rayman.CurrentAnimation = 6;
                            CurrentMovement = WorldMapMovement.World3To4;
                            UserInfo.HideWorldBar();
                        }
                        break;
                }
            }
            // Move back
            else if ((JoyPad.IsButtonJustPressed(GbaInput.Left) || 
                      (JoyPad.IsButtonJustPressed(GbaInput.Down) && WorldId == WorldId.World2) || 
                      (JoyPad.IsButtonJustPressed(GbaInput.Up) && WorldId == WorldId.World4)) && 
                     CurrentMovement == WorldMapMovement.None && 
                     JoyPad.IsButtonReleased(GbaInput.Select))
            {
                switch (WorldId)
                {
                    case WorldId.World1:
                        if (Rom.Platform == Platform.GBA)
                        {
                            Rayman.CurrentAnimation = 22;
                            CurrentMovement = WorldMapMovement.World1ToGameCube;
                            SelectedWorldType = WorldType.GameCube;
                            UserInfo.HideWorldBar();
                        }
                        break;

                    case WorldId.World2:
                        Rayman.CurrentAnimation = 13;
                        CurrentMovement = WorldMapMovement.World2To1;
                        UserInfo.HideWorldBar();
                        break;

                    case WorldId.World3:
                        Rayman.CurrentAnimation = 10;
                        CurrentMovement = WorldMapMovement.World3To2;
                        UserInfo.HideWorldBar();
                        break;

                    case WorldId.World4:
                        Rayman.CurrentAnimation = 8;
                        CurrentMovement = WorldMapMovement.World4To3;
                        UserInfo.HideWorldBar();
                        break;
                }
            }
            // Select world
            else if (JoyPad.IsButtonJustPressed(GbaInput.A) &&
                     CurrentMovement == WorldMapMovement.None &&
                     JoyPad.IsButtonReleased(GbaInput.Select))
            {
                SelectedWorldType = WorldType.World;
                CircleWipeTransitionMode = TransitionMode.Out;
                Gfx.SetScreenEffect(CircleWipeTransitionScreenEffect);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Spirale_Mix01);
            }
        }

        Vector2 camDelta = Vector2.Zero;

        switch (CurrentMovement)
        {
            case WorldMapMovement.World1To2:
                if (Rayman.CurrentAnimation == 0)
                {
                    if (Rayman.EndOfAnimation)
                        Rayman.CurrentAnimation = 1;
                }
                else
                {
                    if (Rayman.EndOfAnimation)
                    {
                        CurrentMovement = WorldMapMovement.None;
                        WorldId = WorldId.World2;
                        UserInfo.SetWorldId(WorldId);
                        UserInfo.ShowWorldBar();
                        Rayman.CurrentAnimation = 16;
                    }
                }

                if ((Rom.Platform == Platform.GBA && ScrollX < 128) ||
                    (Rom.Platform == Platform.NGage && ScrollX < 72))
                    camDelta = new Vector2(1, 0);
                break;

            case WorldMapMovement.World2To3:
                if (Rayman.CurrentAnimation == 2 && Rayman.EndOfAnimation)
                    Rayman.CurrentAnimation = 3;

                if (Rayman.CurrentAnimation == 3 && Rayman.EndOfAnimation)
                    Rayman.CurrentAnimation = 4;

                if (Rayman.CurrentAnimation == 4)
                {
                    if (Rayman.EndOfAnimation)
                        Rayman.CurrentAnimation = 5;
                }
                else
                {
                    if (Rayman.EndOfAnimation)
                    {
                        CurrentMovement = WorldMapMovement.None;
                        WorldId = WorldId.World3;
                        UserInfo.SetWorldId(WorldId);
                        UserInfo.ShowWorldBar();
                        Rayman.CurrentAnimation = 17;
                    }
                }

                if (Rayman.CurrentAnimation != 2)
                    camDelta = new Vector2(1, 0);
                break;

            case WorldMapMovement.World3To4:
                if (Rayman.CurrentAnimation == 6)
                {
                    if (Rayman.EndOfAnimation)
                        Rayman.CurrentAnimation = 7;
                }
                else
                {
                    if (Rayman.EndOfAnimation)
                    {
                        CurrentMovement = WorldMapMovement.None;
                        WorldId = WorldId.World4;
                        UserInfo.SetWorldId(WorldId);
                        UserInfo.ShowWorldBar();
                        Rayman.CurrentAnimation = 18;
                    }
                }
                break;

            case WorldMapMovement.World4To3:
                if (Rayman.CurrentAnimation == 8)
                {
                    if (Rayman.EndOfAnimation)
                        Rayman.CurrentAnimation = 9;
                }
                else
                {
                    if (Rayman.EndOfAnimation)
                    {
                        CurrentMovement = WorldMapMovement.None;
                        WorldId = WorldId.World3;
                        UserInfo.SetWorldId(WorldId);
                        UserInfo.ShowWorldBar();
                        Rayman.CurrentAnimation = 17;
                    }
                }
                break;

            case WorldMapMovement.World3To2:
                if (Rayman.CurrentAnimation == 10 && Rayman.EndOfAnimation)
                    Rayman.CurrentAnimation = 11;

                if (Rayman.CurrentAnimation == 11)
                {
                    if (Rayman.EndOfAnimation)
                        Rayman.CurrentAnimation = 12;
                }
                else
                {
                    if (Rayman.EndOfAnimation)
                    {
                        CurrentMovement = WorldMapMovement.None;
                        WorldId = WorldId.World2;
                        UserInfo.SetWorldId(WorldId);
                        UserInfo.ShowWorldBar();
                        Rayman.CurrentAnimation = 16;
                    }
                }

                if ((Rom.Platform == Platform.GBA && ScrollX > 128) ||
                    (Rom.Platform == Platform.NGage && ScrollX > 72))
                    camDelta = new Vector2(-1, 0);
                break;

            case WorldMapMovement.World2To1:
                if (Rayman.CurrentAnimation == 13 && Rayman.EndOfAnimation)
                    Rayman.CurrentAnimation = 14;

                if (Rayman.CurrentAnimation == 14)
                {
                    if (Rayman.EndOfAnimation)
                        Rayman.CurrentAnimation = 15;
                }
                else
                {
                    if (Rayman.EndOfAnimation)
                    {
                        CurrentMovement = WorldMapMovement.None;
                        WorldId = WorldId.World1;
                        UserInfo.SetWorldId(WorldId);
                        UserInfo.ShowWorldBar();
                        Rayman.CurrentAnimation = 15;
                    }
                }

                camDelta = new Vector2(-1, 0);
                break;

            case WorldMapMovement.World1ToGameCube:
                camDelta = new Vector2(-1, 0);
                break;
        }

        TgxCamera2D cam = ((TgxPlayfield2D)Scene.Playfield).Camera;
        TgxCluster mainCluster = cam.GetMainCluster();
        if ((camDelta.X > 0 && !mainCluster.IsOnLimit(Edge.Right)) || 
            (camDelta.X < 0 && !mainCluster.IsOnLimit(Edge.Left)))
        {
            if (Rom.Platform == Platform.NGage)
            {
                if (NGageScrollCooldown < 1)
                {
                    cam.Position += camDelta;
                    ScrollX += camDelta.X;
                    NGageScrollCooldown = 1;
                }
                else
                {
                    NGageScrollCooldown--;
                }
            }
            else
            {
                cam.Position += camDelta;
                ScrollX += camDelta.X;
            }
        }

        float xPos = BaseObjPos.X - ScrollX;

        Rayman.ScreenPos = Rayman.ScreenPos with { X = xPos };

        foreach (AnimatedObject worldPath in WorldPaths)
            worldPath.ScreenPos = worldPath.ScreenPos with { X = xPos };

        if (Rom.Platform == Platform.GBA)
            GameCubeSparkles.ScreenPos = GameCubeSparkles.ScreenPos with { X = xPos };

        if (SelectedWorldType != WorldType.GameCube)
            Scene.AnimationPlayer.Play(Rayman);

        Scene.AnimationPlayer.Play(WorldPaths[0]);

        if (GameInfo.PersistentInfo.UnlockedWorld3)
            Scene.AnimationPlayer.Play(WorldPaths[1]);

        if (GameInfo.PersistentInfo.UnlockedWorld4)
            Scene.AnimationPlayer.Play(WorldPaths[2]);
        
        if (Rom.Platform == Platform.GBA)
            Scene.AnimationPlayer.Play(GameCubeSparkles);

        StepSpikyBag();
        StepLightning();
        StepVolcanoGlow();

        if (SelectedWorldType == WorldType.GameCube)
        {
            if (Rom.Platform == Platform.GBA)
                SelectGameCube();
        }
        else if (SelectedWorldType != WorldType.None && CircleWipeTransitionMode == TransitionMode.FinishedOut)
        {
            WorldNameInit();
            Gfx.ClearScreenEffect();
        }

        ManageCheats();
    }

    public void StepEx_EnterWorld()
    {
        // NOTE: The game only updates this every 4 frames
        const int factor = 4;

        // Fade in text
        if (EnterWorldStep == 0)
        {
            WorldNameAlpha++;

            if (WorldNameAlpha == 16 * factor)
                EnterWorldStep = 1;
        }
        // Wait
        else if (EnterWorldStep == 1)
        {
            if (LightningCountdown < 11 * factor)
                LightningCountdown++;
            else
                EnterWorldStep = 2;
        }
        // Fade out text
        else if (EnterWorldStep == 2)
        {
            WorldNameAlpha--;

            if (WorldNameAlpha == 0)
                EnterWorldStep = 3;
        }
        // Finish
        else if (EnterWorldStep == 3)
        {
            if (WorldId == WorldId.World4 && !GameInfo.PersistentInfo.PlayedAct4)
            {
                FrameManager.SetNextFrame(new Act4());
                SoundEventsManager.StopAllSongs();
                GameInfo.PersistentInfo.PlayedAct4 = true;
                GameInfo.Save(GameInfo.CurrentSlot);
            }
            else
            {
                GameInfo.LoadLevel(MapId.World1 + (int)WorldId);
            }
        }

        FullWorldName.GbaAlpha = WorldNameAlpha / (float)factor;

        Scene.AnimationPlayer.PlayFront(FullWorldName);
    }

    public void StepEx_EnterGameCubeMenu()
    {
        // Wait
        if (EnterGameCubeMenuStep == 0)
        {
            if (Timer < 60)
            {
                Timer++;
            }
            else
            {
                EnterGameCubeMenuStep = 1;
                Timer = 0;
            }
        }
        // Zoom in
        else if (EnterGameCubeMenuStep == 1)
        {
            // Scale for widescreen
            Vector2 max = new(120, 8);
            Vector2 range = GameCubeTransitionScreenEffect.RenderContext.Resolution - max;
            Vector2 scale = range / (Rom.OriginalResolution - max);

            GameCubeTransitionScreenEffect.Square = new Box(
                minX: Timer * 8 / 120f,
                minY: Timer * 72 / 120f,
                maxX: GameCubeTransitionScreenEffect.RenderContext.Resolution.X - Timer * scale.X,
                maxY: GameCubeTransitionScreenEffect.RenderContext.Resolution.Y - Timer * 8 / 120f * scale.Y);

            if (Timer < 120)
            {
                Timer += 2;
            }
            else
            {
                EnterGameCubeMenuStep = 2;
                Timer = 0;
            }
        }
        // Square
        else if (EnterGameCubeMenuStep == 2)
        {
            GameCubeTransitionScreenEffect.Square = new Box(
                minX: 8,
                minY: 72 + Timer * 8 / 64f,
                maxX: 120 - Timer,
                maxY: 152 - Timer * 24 / 64f);

            if (Timer < 64)
            {
                Timer++;
            }
            else
            {
                EnterGameCubeMenuStep = 3;
                Timer = 0;
            }
        }
        // Wait
        else if (EnterGameCubeMenuStep == 3)
        {
            if (Rayman.EndOfAnimation)
            {
                EnterGameCubeMenuStep = 4;
                Timer = 0;
            }
        }
        // Zoom in
        else
        {
            GameCubeTransitionScreenEffect.Square = new Box(
                minX: MathF.Min(Timer + 8, 32),
                minY: MathF.Min(Timer + 80, 104),
                maxX: MathF.Max(56 - Timer, 32),
                maxY: MathF.Max(128 - Timer, 104));

            if (Timer >= 48)
                FrameManager.SetNextFrame(new GameCubeMenu());

            Timer++;
        }

        if (EnterGameCubeMenuStep <= 3)
            Scene.AnimationPlayer.Play(Rayman);

        Scene.AnimationPlayer.Play(WorldPaths[0]);

        if (GameInfo.PersistentInfo.UnlockedWorld3)
            Scene.AnimationPlayer.Play(WorldPaths[1]);

        if (GameInfo.PersistentInfo.UnlockedWorld4)
            Scene.AnimationPlayer.Play(WorldPaths[2]);

        Scene.AnimationPlayer.Play(GameCubeSparkles);
    }

    public void Step_Normal()
    {
        Scene.Step();

        if (CircleWipeTransitionMode != TransitionMode.None)
            CircleWipeTransitionScreenEffect.Value = CircleWipeTransitionValue;

        if (CircleWipeTransitionMode == TransitionMode.Out)
        {
            CircleWipeTransitionValue += 4;

            if (CircleWipeTransitionValue >= 255)
            {
                CircleWipeTransitionMode = TransitionMode.FinishedOut;
                CircleWipeTransitionValue = 0;
            }
        }
        else if (CircleWipeTransitionMode == TransitionMode.In)
        {
            CircleWipeTransitionValue -= 4;

            if (CircleWipeTransitionValue <= 0)
            {
                CircleWipeTransitionMode = TransitionMode.None;
                CircleWipeTransitionValue = 0;
                Gfx.ClearScreenEffect();
            }
        }

        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();
        LevelMusicManager.Step();

        if (JoyPad.IsButtonJustPressed(GbaInput.Start) && 
            CurrentExStepAction == StepEx_Play &&
            CircleWipeTransitionMode == TransitionMode.None)
        {
            CurrentStepAction = Step_Pause_Init;
            GameTime.Pause();
        }
    }

    public void Step_Pause_Init()
    {
        SavedFadeControl = Gfx.FadeControl;

        // Fade after drawing screen 0, thus only leaving the sprites 0 as not faded
        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease, FadeFlags.Screen0);
        Gfx.GbaFade = 6;

        Scene.ProcessDialogs();

        SoundEventsManager.FinishReplacingAllSongs();
        SoundEventsManager.PauseAllSongs();

        UserInfo.ProcessMessage(this, Message.UserInfo_Pause);

        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();
        CurrentStepAction = Step_Pause_AddDialog;
    }

    public void Step_Pause_AddDialog()
    {
        Scene.AddDialog(PauseDialog, true, false);
        Scene.Step();
        UserInfo.Draw(Scene.AnimationPlayer);
        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();
        CurrentStepAction = Step_Pause_Paused;
    }

    public void Step_Pause_Paused()
    {
        if (PauseDialog is PauseDialog { DrawStep: PauseDialogDrawStep.Hide } or ModernPauseDialog { DrawStep: PauseDialogDrawStep.Hide })
            CurrentStepAction = Step_Pause_UnInit;

        Scene.Step();

        // The original game doesn't have this check, but since we're still running the game loop
        // while in the simulated sleep mode we have to make sure to not draw the HUD then
        if (PauseDialog is not PauseDialog { IsInSleepMode: true })
            UserInfo.Draw(Scene.AnimationPlayer);

        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();
    }

    public void Step_Pause_UnInit()
    {
        Scene.RemoveLastDialog();
        Scene.RefreshDialogs();
        Scene.ProcessDialogs();

        // We probably don't need to do this, but in the original game it needs to reload things like
        // palette indexes since it might be allocated differently in VRAM after unpausing.
        foreach (GameObject gameObj in Scene.KnotManager.GameObjects)
            gameObj.ProcessMessage(this, Message.ReloadAnimation);

        // NOTE: The game calls Load on the animated objects here, but we don't need to do that

        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();
        CurrentStepAction = Step_Pause_Resume;
    }

    public void Step_Pause_Resume()
    {
        Gfx.FadeControl = SavedFadeControl;
        Gfx.Fade = 0;

        UserInfo.ProcessMessage(this, Message.UserInfo_Unpause);

        Scene.Step();

        SoundEventsManager.ResumeAllSongs();

        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();

        if (Rom.Platform == Platform.NGage)
            NGage_0x4 = false;

        CurrentStepAction = Step_Normal;
        GameTime.Resume();
    }

    #endregion

    #region Enums

    public enum WorldMapMovement
    {
        None = 0,
        World1To2 = 1,
        World2To3 = 2,
        World3To4 = 3,
        World4To3 = 4,
        World3To2 = 5,
        World2To1 = 6,
        World1ToGameCube = 7,
    }

    public enum TransitionMode
    {
        None = 0,
        Out = 1,
        In = 2,
        Mode3 = 3, // Unused
        Mode4 = 4, // Unused
        FinishedOut = 5,
    }

    public enum WorldType
    {
        None = 0,
        GameCube = 1,
        World = 2,
    }

    #endregion
}