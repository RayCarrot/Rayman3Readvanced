using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3;

public class ModernMenuAll : Frame, IHasPlayfield
{
    #region Properties

    private const float CursorBaseY = 67;

    TgxPlayfield IHasPlayfield.Playfield => Playfield;

    public AnimationPlayer AnimationPlayer { get; set; }
    public TgxPlayfield2D Playfield { get; set; }
    public TransitionsFX TransitionsFX { get; set; }

    public AnimatedObject Wheel1 { get; set; }
    public AnimatedObject Wheel2 { get; set; }
    public AnimatedObject Wheel3 { get; set; }
    public AnimatedObject Wheel4 { get; set; }
    public AnimatedObject Wheel5 { get; set; }
    public AnimatedObject Cursor { get; set; }
    public AnimatedObject Stem { get; set; }
    public AnimatedObject Steam { get; set; }

    public MenuPage CurrentPage { get; set; }
    public MenuPage NextPage { get; set; }

    public int PrevSelectedOption { get; set; }
    public int SelectedOption { get; set; }
    public StemMode StemMode { get; set; }

    public int WheelRotation { get; set; }
    public int SteamTimer { get; set; }

    public bool IsLoadingMultiplayerMap { get; set; }
    public bool HasLoadedGameInfo { get; set; }
    public Slot[] Slots { get; } = new Slot[3];
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

        for (int i = 0; i < 3; i++)
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
                {
                    Cursor.CurrentAnimation = 16;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                }

                SelectOption(0, false);
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
            int lineHeight = CurrentPage.LineHeight;

            if (SelectedOption != PrevSelectedOption)
            {
                if (SelectedOption < PrevSelectedOption)
                {
                    float yPos = SelectedOption * lineHeight + CursorBaseY;

                    if (yPos < Cursor.ScreenPos.Y)
                    {
                        Cursor.ScreenPos -= new Vector2(0, 4);
                    }
                    else
                    {
                        Cursor.ScreenPos = Cursor.ScreenPos with { Y = yPos };
                        PrevSelectedOption = SelectedOption;
                    }
                }
                else
                {
                    float yPos = SelectedOption * lineHeight + CursorBaseY;

                    if (yPos > Cursor.ScreenPos.Y)
                    {
                        Cursor.ScreenPos += new Vector2(0, 4);
                    }
                    else
                    {
                        Cursor.ScreenPos = Cursor.ScreenPos with { Y = yPos };
                        PrevSelectedOption = SelectedOption;
                    }
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
        if (StemMode is StemMode.Active or StemMode.Inactive)
        {
            PrevSelectedOption = SelectedOption;
            SelectedOption = 0;
        }

        StemMode = StemMode.MoveOut;

        Stem.CurrentAnimation = 1;

        if (Cursor.ScreenPos.Y <= CursorBaseY && Cursor.CurrentAnimation != 16)
            Stem.CurrentAnimation = 15;
    }

    public void SelectOption(int selectedOption, bool playSound)
    {
        if (StemMode is StemMode.Active or StemMode.Inactive)
        {
            PrevSelectedOption = SelectedOption;
            SelectedOption = selectedOption;

            if (playSound)
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
        }
    }

    public override void Init()
    {
        RenderContext renderContext = new FixedResolutionRenderContext(new Vector2(384, 216));

        LoadGameInfo();

        AnimationPlayer = new AnimationPlayer(false, null);

        AnimatedObjectResource propsAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuPropAnimations);
        AnimatedObjectResource steamAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuSteamAnimations);

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

        WheelRotation = 0;

        // Load the playfield
        PlayfieldResource menuPlayField = Rom.LoadResource<PlayfieldResource>(GameResource.MenuPlayfield);
        Playfield = TgxPlayfield.Load<TgxPlayfield2D>(menuPlayField);
        Playfield.RenderContext.SetFixedResolution(renderContext.Resolution);

        // Tile the background twice so that it fills the width
        GfxScreen bgScreen = Playfield.TileLayers[0].Screen;
        bgScreen.Renderer = new MultiScreenRenderer(
        [
            // Move to the right by 32, and add a copy on the left (the background width is 168)
            new(bgScreen.Renderer, new Vector2(-168 + 32, 0)),
            new(bgScreen.Renderer, new Vector2(32, 0))
        ], renderContext.Resolution);

        // Replace the curtain with a new widescreen texture
        GfxScreen curtainScreen = Playfield.TileLayers[1].Screen;
        Texture2D curtainTexture = Engine.FrameContentManager.Load<Texture2D>("MenuCurtain");
        curtainScreen.Renderer = new TextureScreenRenderer(curtainTexture);
        curtainScreen.RenderOptions.PaletteTexture = null;
        curtainScreen.RenderOptions.RenderContext = renderContext;
        curtainScreen.Wrap = false;

        // We also need to update the curtain cluster. Normally the size should match the screen size, but we want to
        // allow it to be scrolled up, so we simulate padded space by increasing the cluster height
        Playfield.Camera.GetCluster(1).Size = new Vector2(curtainTexture.Width, curtainTexture.Height + renderContext.Resolution.Y);

        // Replace the wooden frame with a new widescreen texture
        GfxScreen woodenFrameScreen = Playfield.TileLayers[2].Screen;
        Texture2D woodenFrameTexture = Engine.FrameContentManager.Load<Texture2D>("MenuWoodenFrame");
        woodenFrameScreen.Renderer = new TextureScreenRenderer(woodenFrameTexture);
        woodenFrameScreen.RenderOptions.PaletteTexture = null;
        woodenFrameScreen.RenderOptions.RenderContext = renderContext;

        Gfx.ClearColor = Color.Black;

        // Set the default camera positions
        Playfield.Camera.GetMainCluster().Position = Vector2.Zero;
        Playfield.Camera.GetCluster(1).Position = new Vector2(0, 160);
        Playfield.Camera.GetCluster(2).Position = Vector2.Zero;

        Playfield.Step();

        // TODO: Allow the initial page to be changed like how the original menu does it
        Playfield.TileLayers[3].Screen.IsEnabled = false;
        ChangePage(new GameModeMenuPage(this), NewPageMode.Initial);

        // Play the music
        if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__raytheme) &&
            !SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__sadslide))
        {
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__raytheme);
            SoundEngineInterface.SetNbVoices(10);
        }

        RSMultiplayer.UnInit();
        RSMultiplayer.Init();

        // TODO: Implement
        //if (Rom.Platform == Platform.GBA)
        //    MultiplayerInititialGameTime = GameTime.ElapsedFrames;

        MultiplayerInfo.Init();
        MultiplayerManager.Init();

        GameTime.Resume();

        TransitionsFX = new TransitionsFX(false);
        TransitionsFX.FadeInInit(1 / 16f);

        SteamTimer = 0;
    }

    public override void UnInit()
    {
        SoundEngineInterface.SetNbVoices(7);

        if (!IsLoadingMultiplayerMap)
        {
            RSMultiplayer.UnInit();
            GameTime.Resume();
        }

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