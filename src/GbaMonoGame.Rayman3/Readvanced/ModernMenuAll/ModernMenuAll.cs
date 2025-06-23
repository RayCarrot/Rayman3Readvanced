using System;
using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

public class ModernMenuAll : Frame, IHasPlayfield
{
    #region Constructor

    public ModernMenuAll(InitialMenuPage initialPage)
    {
        InitialPage = initialPage;
    }

    #endregion

    #region Properties

    private const float CursorBaseY = 67;

    TgxPlayfield IHasPlayfield.Playfield => Playfield;

    public AnimationPlayer AnimationPlayer { get; set; }
    public TgxPlayfield2D Playfield { get; set; }

    public AnimatedObject Wheel1 { get; set; }
    public AnimatedObject Wheel2 { get; set; }
    public AnimatedObject Wheel3 { get; set; }
    public AnimatedObject Wheel4 { get; set; }
    public AnimatedObject Wheel5 { get; set; }
    public AnimatedObject Cursor { get; set; }
    public AnimatedObject Stem { get; set; }
    public AnimatedObject Steam { get; set; }

    public MenuScrollBar ScrollBar { get; set; }

    public InitialMenuPage InitialPage { get; }
    public MenuPage CurrentPage { get; set; }
    public MenuPage NextPage { get; set; }

    public float? CursorStartY { get; set; }
    public float? CursorDestY { get; set; }
    public StemMode StemMode { get; set; }

    public int WheelRotation { get; set; }
    public int SteamTimer { get; set; }

    public bool IsLoadingMultiplayerMap { get; set; }
    public bool HasLoadedGameInfo { get; set; }
    public Slot[] Slots { get; } = new Slot[GameInfo.ModernSaveSlotsCount];
    public bool FinishedLyChallenge1 { get; set; }
    public bool FinishedLyChallenge2 { get; set; }
    public bool HasAllCages { get; set; }

    #endregion

    #region Methods

    public void LoadGameInfo()
    {
        if (HasLoadedGameInfo)
            return;

        GameInfo.Init();
        HasLoadedGameInfo = true;

        for (int i = 0; i < Slots.Length; i++)
        {
            if (SaveGameManager.SlotExists(i))
            {
                // Load the slot
                GameInfo.Load(i);

                // Get the info from the slot
                Slots[i] = new Slot(GameInfo.GetTotalDeadLums(), GameInfo.GetTotalDeadCages(), GameInfo.PersistentInfo.Lives);

                if (Rom.Platform == Platform.GBA)
                {
                    if (GameInfo.PersistentInfo.FinishedLyChallenge1)
                        FinishedLyChallenge1 = true;

                    if (GameInfo.PersistentInfo.FinishedLyChallenge2)
                        FinishedLyChallenge2 = true;

                    if (Slots[i]?.CagesCount == 50)
                        HasAllCages = true;
                }
            }
            else
            {
                Slots[i] = null;
            }
        }
    }

    public void ChangePage(MenuPage page, NewPageMode mode)
    {
        if (mode == NewPageMode.Initial)
        {
            CurrentPage = page;
            page.State = MenuPage.MenuPageState.Init;
        }
        else
        {
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);

            if (CurrentPage.UsesCursor)
            {
                if (mode == NewPageMode.Next)
                    CursorClick();

                TransitionOutCursorAndStem();
            }

            CurrentPage.State = MenuPage.MenuPageState.TransitionOut;
            NextPage = page;
        }
    }

    public void SetBackgroundPalette(int index)
    {
        GfxTileKitManager tileKitManager = Playfield.GfxTileKitManager;
        GfxScreen screen = Playfield.TileLayers[0].Screen;

        screen.RenderOptions.PaletteTexture = new PaletteTexture(
            Texture: Engine.TextureCache.GetOrCreateObject(
                pointer: tileKitManager.SelectedPalette.CachePointer,
                id: index + 1, // +1 since 0 is the default
                data: index,
                createObjFunc: static i => new PaletteTexture2D(MenuAll.GetBackgroundPalette(i))),
            PaletteIndex: 0);
    }

    public void CursorClick()
    {
        Cursor.CurrentAnimation = 16;
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
    }

    public void InvalidCursorClick()
    {
        Cursor.CurrentAnimation = 16;
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);
    }

    public bool HasFinishedCursorClick()
    {
        return Cursor.CurrentAnimation == 16 && Cursor.EndOfAnimation;
    }

    public void SetCursorToIdle()
    {
        Cursor.CurrentAnimation = 0;
    }

    public void ResetStem()
    {
        StemMode = StemMode.MoveIn;
        Stem.CurrentAnimation = 12;
    }

    public void ManageCursorAndStem()
    {
        if (StemMode == StemMode.MoveOut)
        {
            if (Cursor.CurrentAnimation == 16)
            {
                Debug.Assert(Stem.CurrentAnimation == 1, "The stem has the wrong animation");

                if (Cursor.EndOfAnimation)
                {
                    Cursor.CurrentAnimation = 0;

                    if (Cursor.ScreenPos.Y <= CursorBaseY)
                    {
                        Stem.CurrentAnimation = 15;
                    }
                }
            }
            else if (Cursor.ScreenPos.Y > CursorBaseY)
            {
                Cursor.ScreenPos -= new Vector2(0, 4);

                if (Cursor.ScreenPos.Y <= CursorBaseY)
                {
                    Cursor.ScreenPos = Cursor.ScreenPos with { Y = CursorBaseY };
                    Stem.CurrentAnimation = 15;
                }
            }
            else if (Stem.CurrentAnimation == 15 && Stem.EndOfAnimation)
            {
                Stem.CurrentAnimation = 14;
                StemMode = StemMode.Inactive;
            }
        }
        else if (StemMode == StemMode.MoveIn)
        {
            if (Stem.CurrentAnimation == 12 && Stem.EndOfAnimation)
            {
                Stem.CurrentAnimation = 17;
            }
            else if (Stem.CurrentAnimation == 17 && Stem.EndOfAnimation)
            {
                Stem.CurrentAnimation = 1;
                StemMode = StemMode.Active;
            }
        }
        else if (StemMode == StemMode.Active)
        {
            // Move with a constant speed of 4
            const float speed = 4;

            if (CursorStartY != null && CursorDestY != null)
            {
                float startY = CursorStartY.Value;
                float destY = CursorDestY.Value;

                // Move up
                if (destY < startY && Cursor.ScreenPos.Y > destY)
                {
                    Cursor.ScreenPos -= new Vector2(0, speed);
                }
                // Move down
                else if (destY > startY && Cursor.ScreenPos.Y < destY)
                {
                    Cursor.ScreenPos += new Vector2(0, speed);
                }
                // Finished moving
                else
                {
                    Cursor.ScreenPos = Cursor.ScreenPos with { Y = destY };
                    CursorStartY = null;
                    CursorDestY = null;
                }
            }
        }

        AnimationPlayer.Play(Stem);

        // The cursor is usually included in the stem animation, except for animation 1
        if (Stem.CurrentAnimation == 1)
            AnimationPlayer.Play(Cursor);
    }

    public void TransitionOutCursorAndStem()
    {
        SetCursorTarget(0);

        StemMode = StemMode.MoveOut;

        Stem.CurrentAnimation = 1;

        if (Cursor.ScreenPos.Y <= CursorBaseY && Cursor.CurrentAnimation != 16)
            Stem.CurrentAnimation = 15;
    }

    public bool SetCursorTarget(int selectedIndex)
    {
        if (StemMode is StemMode.Active or StemMode.Inactive)
        {
            CursorStartY = Cursor.ScreenPos.Y;
            CursorDestY = CursorBaseY + selectedIndex * CurrentPage.LineHeight;
            return true;
        }
        else
        {
            return false;
        }
    }

    public override void Init()
    {
        RenderContext renderContext = new FixedResolutionRenderContext(Resolution.Modern);

        LoadGameInfo();

        AnimationPlayer = new AnimationPlayer(false, null);

        AnimatedObjectResource propsAnimations = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.MenuPropAnimations);
        AnimatedObjectResource steamAnimations = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.MenuSteamAnimations);

        Wheel1 = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 32,
            ScreenPos = new Vector2(7, 166),
            CurrentAnimation = 2,
            AffineMatrix = AffineMatrix.Identity,
            RenderContext = renderContext,
        };

        Wheel2 = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 32,
            ScreenPos = new Vector2(136, 166),
            CurrentAnimation = 3,
            AffineMatrix = AffineMatrix.Identity,
            RenderContext = renderContext,
        };

        Wheel3 = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 33,
            ScreenPos = new Vector2(172, 166),
            CurrentAnimation = 4,
            AffineMatrix = AffineMatrix.Identity,
            RenderContext = renderContext,
        };

        Wheel4 = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 32,
            ScreenPos = new Vector2(66, 200),
            CurrentAnimation = 3,
            AffineMatrix = AffineMatrix.Identity,
            RenderContext = renderContext,
        };

        Wheel5 = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 33,
            ScreenPos = new Vector2(320, 176),
            CurrentAnimation = 4,
            AffineMatrix = AffineMatrix.Identity,
            RenderContext = renderContext,
        };

        Cursor = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 0,
            ScreenPos = new Vector2(33, CursorBaseY),
            CurrentAnimation = 0,
            RenderContext = renderContext,
        };

        Stem = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 0,
            ScreenPos = new Vector2(47, CursorBaseY + 93),
            CurrentAnimation = 14,
            RenderContext = renderContext,
        };

        Steam = new AnimatedObject(steamAnimations, steamAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 0,
            ScreenPos = new Vector2(27, 20),
            CurrentAnimation = 0,
            RenderContext = renderContext,
        };

        ScrollBar = new MenuScrollBar(renderContext, new Vector2(352, 40), 3);

        WheelRotation = 0;

        // Load the playfield
        PlayfieldResource menuPlayField = Rom.LoadResource<PlayfieldResource>(Rayman3DefinedResource.MenuPlayfield);
        Playfield = TgxPlayfield.Load<TgxPlayfield2D>(menuPlayField);
        Playfield.RenderContext.SetFixedResolution(renderContext.Resolution);

        // Tile the background twice so that it fills the width
        GfxScreen bgScreen = Playfield.TileLayers[0].Screen;
        int bgWidth = Rom.Platform switch
        {
            Platform.GBA => 168,
            Platform.NGage => 120, // NOTE: Width is actually 144, but it wraps at 120
            _ => throw new UnsupportedPlatformException()
        };
        Vector2 bgOffset = Rom.Platform switch
        {
            Platform.GBA => new Vector2(32, 0),
            Platform.NGage => new Vector2(96, -24),
            _ => throw new UnsupportedPlatformException()
        };
        bgScreen.Renderer = new MultiScreenRenderer(
        [
            new(bgScreen.Renderer, bgOffset),
            new(bgScreen.Renderer, new Vector2(-bgWidth, 0) + bgOffset),
        ], renderContext.Resolution);

        // Replace the curtain with a new widescreen texture
        GfxScreen curtainScreen = Playfield.TileLayers[1].Screen;
        Texture2D curtainTexture = Engine.FrameContentManager.Load<Texture2D>(Assets.MenuCurtainTexture);
        curtainScreen.Renderer = new TextureScreenRenderer(curtainTexture);
        curtainScreen.RenderOptions.PaletteTexture = null;
        curtainScreen.RenderOptions.RenderContext = renderContext;
        curtainScreen.Wrap = false;

        // We also need to update the curtain cluster. Normally the size should match the screen size, but we want to
        // allow it to be scrolled up, so we simulate padded space by increasing the cluster height
        Playfield.Camera.GetCluster(1).Size = new Vector2(curtainTexture.Width, curtainTexture.Height + renderContext.Resolution.Y);

        // Replace the wooden frame with a new widescreen texture
        GfxScreen woodenFrameScreen = Playfield.TileLayers[2].Screen;
        Texture2D woodenFrameTexture = Engine.FrameContentManager.Load<Texture2D>(Assets.MenuWoodenFrameTexture);
        woodenFrameScreen.Renderer = new TextureScreenRenderer(woodenFrameTexture);
        woodenFrameScreen.RenderOptions.PaletteTexture = null;
        woodenFrameScreen.RenderOptions.RenderContext = renderContext;

        Gfx.ClearColor = Color.Black;

        // Set the default camera positions
        Playfield.Camera.GetMainCluster().Position = Vector2.Zero;
        Playfield.Camera.GetCluster(1).Position = new Vector2(0, 160);
        Playfield.Camera.GetCluster(2).Position = Vector2.Zero;

        Playfield.Step();

        GfxScreen languageCurtainScreen = Playfield.TileLayers[3].Screen;

        switch (InitialPage)
        {
            // TODO: Select language first time you start the game? Or just skip and have players do it in the new options page?
            case InitialMenuPage.Language:
            case InitialMenuPage.NGage_FirstPage when Rom.Platform == Platform.NGage:
                languageCurtainScreen.IsEnabled = false;
                ChangePage(new GameModeMenuPage(this), NewPageMode.Initial);
                break;

            case InitialMenuPage.GameMode:
                languageCurtainScreen.IsEnabled = false;
                ChangePage(new GameModeMenuPage(this), NewPageMode.Initial);
                break;

            case InitialMenuPage.Options:
                languageCurtainScreen.IsEnabled = false;
                ChangePage(new OptionsMenuPage(this), NewPageMode.Initial);
                break;

            // TODO: Implement multiplayer page
            case InitialMenuPage.Multiplayer:
                languageCurtainScreen.IsEnabled = false;
                ChangePage(new GameModeMenuPage(this), NewPageMode.Initial);
                break;

            // TODO: Implement multiplayer page
            case InitialMenuPage.MultiplayerLostConnection:
                languageCurtainScreen.IsEnabled = false;
                ChangePage(new GameModeMenuPage(this), NewPageMode.Initial);
                break;

            default:
                throw new Exception("Invalid start page for ModernMenuAll");
        }

        // Play the music
        if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__raytheme) &&
            !SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__sadslide))
        {
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__raytheme);
        }

        RSMultiplayer.UnInit();
        RSMultiplayer.Init();

        // TODO: Implement
        //if (Rom.Platform == Platform.GBA)
        //    MultiplayerInititialGameTime = GameTime.ElapsedFrames;

        MultiplayerInfo.Init();
        MultiplayerManager.Init();

        GameTime.Resume();

        TransitionsFX.Init(false);
        TransitionsFX.FadeInInit(1);

        SteamTimer = 0;
    }

    public override void UnInit()
    {
        if (!IsLoadingMultiplayerMap)
        {
            RSMultiplayer.UnInit();
            GameTime.Resume();
        }

        if (SoundEventsManager.IsLoaded)
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__raytheme);

        Playfield.UnInit();
    }

    public override void Step()
    {
        Playfield.Step();
        TransitionsFX.StepAll();
        AnimationPlayer.Execute();

        CurrentPage.Step();

        if (NextPage != null && CurrentPage.State == MenuPage.MenuPageState.Inactive)
        {
            CurrentPage = NextPage;
            NextPage.State = MenuPage.MenuPageState.Init;
        }

        WheelRotation += 4;

        if (WheelRotation >= 2048)
            WheelRotation = 0;

        Wheel1.AffineMatrix = new AffineMatrix(WheelRotation % 256, 1, 1);
        Wheel2.AffineMatrix = new AffineMatrix(255 - WheelRotation / 2f % 256, 1, 1);
        Wheel3.AffineMatrix = new AffineMatrix(WheelRotation / 4f % 256, 1, 1);
        Wheel4.AffineMatrix = new AffineMatrix(WheelRotation / 8f % 256, 1, 1);
        Wheel5.AffineMatrix = new AffineMatrix(255 - WheelRotation / 4f % 256, 1, 1);

        AnimationPlayer.Play(Wheel1);
        AnimationPlayer.Play(Wheel2);
        AnimationPlayer.Play(Wheel3);
        AnimationPlayer.Play(Wheel4);
        AnimationPlayer.Play(Wheel5);

        if (CurrentPage.HasScrollBar)
        {
            if (CurrentPage.HasScrollableContent)
            {
                ScrollBar.ScrollOffset = CurrentPage.ScrollOffset;
                ScrollBar.MaxScrollOffset = CurrentPage.MaxScrollOffset;
            }
            else
            {
                ScrollBar.ScrollOffset = 0;
                ScrollBar.MaxScrollOffset = 0;
            }

            ScrollBar.Size = CurrentPage.ScrollBarSize;
            ScrollBar.Draw(AnimationPlayer);
        }

        if (SteamTimer == 0)
        {
            if (!Steam.EndOfAnimation)
            {
                AnimationPlayer.Play(Steam);
            }
            else
            {
                SteamTimer = Random.GetNumber(180) + 60; // Value between 60 and 240
                Steam.CurrentAnimation = Random.GetNumber(200) < 100 ? 0 : 1;
            }
        }
        else
        {
            SteamTimer--;
        }
    }

    #endregion

    #region Data Types

    public record Slot(int LumsCount, int CagesCount, int LivesCount);

    #endregion
}