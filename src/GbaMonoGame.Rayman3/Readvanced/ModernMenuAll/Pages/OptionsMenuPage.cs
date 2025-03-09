using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Rayman3.Readvanced;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3;

public class OptionsMenuPage : MenuPage
{
    public OptionsMenuPage(ModernMenuAll menu) : base(menu) { }

    private const float TabHeaderTextScale = 1 / 2f;
    private const float InfoTextScale = 1 / 4f;
    private const float ArrowScale = 1 / 2f;

    public override bool UsesCursor => true;
    public override int BackgroundPalette => 1;
    public override int LineHeight => 12;

    public Tab[] Tabs { get; set; }
    public int PrevSelectedTab { get; set; }
    public float PrevCursorXPosition { get; set; }
    public int SelectedTab { get; set; }
    public bool IsEditingOption { get; set; }

    public AnimatedObject TabsCursor { get; set; }
    public SpriteTextureObject TabHeaders { get; set; }
    public SpriteFontTextObject[] TabHeaderTexts { get; set; }

    public SpriteTextObject InfoTextObject { get; set; }
    public AnimatedObject ArrowLeft { get; set; }
    public AnimatedObject ArrowRight { get; set; }

    private int GetTabsCursorTargetX(int tabIndex)
    {
        return tabIndex * 60 + 90;
    }

    private void SetSelectedTab(int selectedTab, bool playSound = true)
    {
        PrevSelectedTab = SelectedTab;
        PrevCursorXPosition = TabsCursor.ScreenPos.X;

        if (selectedTab > Tabs.Length - 1)
            selectedTab = 0;
        else if (selectedTab < 0)
            selectedTab = Tabs.Length - 1;

        SelectedTab = selectedTab;
        
        ClearOptions();
        foreach (OptionsMenuOption menuOption in Tabs[selectedTab].MenuOptions)
            AddOption(menuOption);

        SetSelectedOption(0, false);

        if (playSound)
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
    }

    private void ManageTabsCursor()
    {
        if (SelectedTab != PrevSelectedTab)
        {
            float prevTargetX = PrevCursorXPosition;
            float newTargetX = GetTabsCursorTargetX(SelectedTab);
            float dist = Math.Abs(newTargetX - prevTargetX);
            float speed = dist / 15f;

            if (newTargetX < prevTargetX)
            {
                if (TabsCursor.ScreenPos.X > newTargetX)
                {
                    TabsCursor.ScreenPos -= new Vector2(speed, 0);
                }
                else
                {
                    TabsCursor.ScreenPos = TabsCursor.ScreenPos with { X = newTargetX };
                    PrevSelectedTab = SelectedTab;
                }
            }
            else
            {
                if (TabsCursor.ScreenPos.X < newTargetX)
                {
                    TabsCursor.ScreenPos += new Vector2(speed, 0);
                }
                else
                {
                    TabsCursor.ScreenPos = TabsCursor.ScreenPos with { X = newTargetX };
                    PrevSelectedTab = SelectedTab;
                }
            }
        }
    }

    protected override bool SetSelectedOption(int selectedOption, bool playSound = true)
    {
        if (!base.SetSelectedOption(selectedOption, playSound))
            return false;
        
        // Get the selected option
        OptionsMenuOption option = (OptionsMenuOption)Options[SelectedOption];

        // Set the info text
        InfoTextObject.Text = option.InfoText;

        // Set the arrow positions
        ArrowLeft.ScreenPos = option.ArrowLeftPosition * (1 / ArrowScale);
        ArrowRight.ScreenPos = option.ArrowRightPosition * (1 / ArrowScale);

        return true;
    }

    protected override void Init()
    {
        // TODO: Finish setting up the options
        // Add tabs
        Tabs =
        [
            new Tab("DISPLAY",
            [
                new OptionsMenuOption(
                    text: "DISPLAY MODE", 
                    infoText: "Sets the display mode for the game. In borderless fullscreen mode the resolution\n" +
                              "can not be changed as it will always use the screen resolution."),
                new OptionsMenuOption(
                    text: "FULLSCREEN RESOLUTION", 
                    infoText: "The resolution to use when in fullscreen mode."),
                new OptionsMenuOption(
                    text: "WINDOW RESOLUTION", 
                    infoText: "The resolution factor, based on the internal resolution, to use when in windowed mode."),
                new OptionsMenuOption(
                    text: "LOCK WINDOW ASPECT RATIO",  
                    infoText: "Determines if the window, in windowed mode, should automatically resize to fit the\n" +
                              "game's internal resolution's aspect ratio."),
            ]),
            new Tab("SOUND", 
            [ 
                new OptionsMenuOption("MUSIC VOLUME", "This is some info text"),
                new OptionsMenuOption("SOUND FX VOLUME", "This is some info text"),
            ]),
            new Tab("GAME",
            [
                new OptionsMenuOption("LANGUAGE", "This is some info text"),
                new OptionsMenuOption("INTERNAL RESOLUTION", "This is some info text"),
            ]),
            new Tab("CONTROLS",
            [
                new OptionsMenuOption("TEMP OPTION", "This is some info text"),
            ]),
            new Tab("DEBUG",
            [
                new OptionsMenuOption("DEBUG MODE", "This is some info text"),
                new OptionsMenuOption("SERIALIZER LOG", "This is some info text"),
            ]),
        ];

        // Create animations
        AnimatedObjectResource startEraseAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuStartEraseAnimations);
        Texture2D tabFrameTexture = Engine.FrameContentManager.Load<Texture2D>("OptionsMenuTabs");
        AnimatedObjectResource multiplayerTypeFrameAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerTypeFrameAnimations);

        TabsCursor = new AnimatedObject(startEraseAnimations, startEraseAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 0,
            ScreenPos = new Vector2(90, 12),
            CurrentAnimation = 40,
            RenderContext = RenderContext,
        };

        TabHeaders = new SpriteTextureObject
        {
            BgPriority = 1,
            ObjPriority = 0,
            ScreenPos = new Vector2(63, -37),
            Texture = tabFrameTexture,
            RenderContext = RenderContext,
        };

        TabHeaderTexts = new SpriteFontTextObject[Tabs.Length];
        for (int i = 0; i < Tabs.Length; i++)
        {
            float width = ReadvancedFonts.MenuYellow.GetWidth(Tabs[i].Name) * TabHeaderTextScale;
            TabHeaderTexts[i] = new SpriteFontTextObject()
            {
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(89 + i * 60 - width / 2, 30),
                RenderContext = RenderContext,
                AffineMatrix = new AffineMatrix(0, new Vector2(TabHeaderTextScale), false, false),
                Text = Tabs[i].Name,
                Font = ReadvancedFonts.MenuYellow,
            };
        }

        InfoTextObject = new SpriteTextObject
        {
            BgPriority = 3,
            ObjPriority = 0,
            YPriority = 0,
            ScreenPos = new Vector2(75, 110),
            RenderContext = RenderContext,
            AffineMatrix = new AffineMatrix(0, new Vector2(InfoTextScale), false, false),
            Color = TextColor.Menu,
            FontSize = FontSize.Font32,
        };

        // A bit hacky, but create a new render context for the arrows in order to scale them. We could do it through the
        // affine matrix, but that will misalign the animation sprites.
        RenderContext arrowRenderContext = new FixedResolutionRenderContext(RenderContext.Resolution * (1 / ArrowScale));
        ArrowLeft = new AnimatedObject(multiplayerTypeFrameAnimations, multiplayerTypeFrameAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 0,
            CurrentAnimation = 1,
            RenderContext = arrowRenderContext,
        };
        ArrowRight = new AnimatedObject(multiplayerTypeFrameAnimations, multiplayerTypeFrameAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 0,
            CurrentAnimation = 0,
            RenderContext = arrowRenderContext,
        };

        // Reset values
        IsEditingOption = false;
        PrevSelectedTab = 0;

        // Set the initial tab
        SetSelectedTab(0, false);
    }

    protected override void Step_TransitionIn()
    {
        TabsCursor.ScreenPos = TabsCursor.ScreenPos with { Y = (12 - 80) + TransitionValue / 2f };
        TabHeaders.ScreenPos = TabHeaders.ScreenPos with { Y = (-37 - 80) + TransitionValue / 2f };
        
        foreach (SpriteFontTextObject tabHeader in TabHeaderTexts)
            tabHeader.ScreenPos = tabHeader.ScreenPos with { Y = (30 - 80) + TransitionValue / 2f };
    }

    protected override void Step_Active()
    {
        // Not editing
        if (!IsEditingOption)
        {
            if (JoyPad.IsButtonJustPressed(GbaInput.Up))
            {
                SetSelectedOption(SelectedOption - 1);
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.Down))
            {
                SetSelectedOption(SelectedOption + 1);
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.Left) || JoyPad.IsButtonJustPressed(GbaInput.L))
            {
                SetSelectedTab(SelectedTab - 1);
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.Right) || JoyPad.IsButtonJustPressed(GbaInput.R))
            {
                SetSelectedTab(SelectedTab + 1);
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.A))
            {
                IsEditingOption = true;

                Menu.Cursor.CurrentAnimation = 16;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);

                // Start arrow animations on frame 4
                ArrowLeft.CurrentFrame = 4;
                ArrowRight.CurrentFrame = 4;
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.B))
            {
                Menu.ChangePage(new GameModeMenuPage(Menu), NewPageMode.Back);
            }
        }
        // Editing
        else
        {
            if (JoyPad.IsButtonJustPressed(GbaInput.A))
            {
                IsEditingOption = false;

                Menu.Cursor.CurrentAnimation = 16;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.B))
            {
                IsEditingOption = false;
            }
        }

        ManageTabsCursor();

        // End the selected animation
        if (Menu.Cursor.CurrentAnimation == 16 && Menu.Cursor.EndOfAnimation)
            Menu.Cursor.CurrentAnimation = 0;
    }

    protected override void Step_TransitionOut()
    {
        TabsCursor.ScreenPos = TabsCursor.ScreenPos with { Y = 12 - TransitionValue / 2f };
        TabHeaders.ScreenPos = TabHeaders.ScreenPos with { Y = -37 - TransitionValue / 2f };
        
        foreach (SpriteFontTextObject tabHeader in TabHeaderTexts)
            tabHeader.ScreenPos = tabHeader.ScreenPos with { Y = 30 - TransitionValue / 2f };
    }

    protected override void Draw(AnimationPlayer animationPlayer)
    {
        DrawOptions(animationPlayer);

        animationPlayer.Play(TabHeaders);

        foreach (SpriteFontTextObject tabHeaderText in TabHeaderTexts)
            animationPlayer.Play(tabHeaderText);

        animationPlayer.Play(TabsCursor);

        animationPlayer.Play(InfoTextObject);

        if (IsEditingOption)
        {
            animationPlayer.Play(ArrowLeft);
            animationPlayer.Play(ArrowRight);
        }
    }

    public class Tab
    {
        public Tab(string name, OptionsMenuOption[] menuOptions)
        {
            Name = name;
            MenuOptions = menuOptions;
        }

        public string Name { get; }
        public OptionsMenuOption[] MenuOptions { get; }
    }
}