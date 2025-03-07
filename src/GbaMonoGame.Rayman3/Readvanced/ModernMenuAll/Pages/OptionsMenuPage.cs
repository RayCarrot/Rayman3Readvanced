using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Rayman3.Readvanced;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3;

public class OptionsMenuPage : MenuPage
{
    public OptionsMenuPage(ModernMenuAll menu) : base(menu) { }

    public override bool UsesCursor => true;
    public override int BackgroundPalette => 1;
    public override int LineHeight => 12;

    public Tab[] Tabs { get; set; }
    public int PrevSelectedTab { get; set; }
    public int SelectedTab { get; set; }

    public AnimatedObject TabsCursor { get; set; }
    public SpriteTextureObject TabHeaders { get; set; }
    public SpriteFontTextObject[] TabHeaderTexts { get; set; }

    private void SetSelectedTab(int selectedTab, bool playSound = true)
    {
        PrevSelectedTab = SelectedTab;

        if (selectedTab > Tabs.Length - 1)
            selectedTab = 0;
        else if (selectedTab < 0)
            selectedTab = Tabs.Length - 1;

        SelectedTab = selectedTab;
        
        ClearOptions();
        foreach (MenuOption menuOption in Tabs[selectedTab].MenuOptions)
            AddOption(menuOption);

        SetSelectedOption(0, false);

        if (playSound)
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
    }

    private void MoveTabsCursor()
    {
        if (SelectedTab != PrevSelectedTab)
        {
            int targetXPos = SelectedTab * 60 + 90;

            if (SelectedTab < PrevSelectedTab)
            {
                if (TabsCursor.ScreenPos.X > targetXPos)
                {
                    TabsCursor.ScreenPos -= new Vector2(4, 0);
                }
                else
                {
                    TabsCursor.ScreenPos = TabsCursor.ScreenPos with { X = targetXPos };
                    PrevSelectedTab = SelectedTab;
                }
            }
            else
            {
                if (TabsCursor.ScreenPos.X < targetXPos)
                {
                    TabsCursor.ScreenPos += new Vector2(4, 0);
                }
                else
                {
                    TabsCursor.ScreenPos = TabsCursor.ScreenPos with { X = targetXPos };
                    PrevSelectedTab = SelectedTab;
                }
            }
        }
    }

    protected override void Init()
    {
        // TODO: Finish setting up the tabs
        // Add tabs
        Tabs =
        [
            new Tab("DISPLAY",
            [
                new TextMenuOption("DISPLAY MODE", 2/3f),
                new TextMenuOption("FULLSCREEN RESOLUTION", 2 / 3f),
                new TextMenuOption("WINDOW RESOLUTION", 2 / 3f),
                new TextMenuOption("LOCK WINDOW ASPECT RATIO", 2 / 3f),
            ]),
            new Tab("SOUND", 
            [ 
                new TextMenuOption("MUSIC VOLUME", 2/3f),
                new TextMenuOption("SOUND FX VOLUME", 2/3f),
            ]),
            new Tab("GAME",
            [
                new TextMenuOption("TEMP OPTION", 2/3f),
            ]),
            new Tab("CONTROLS",
            [
                new TextMenuOption("TEMP OPTION", 2/3f),
            ]),
            new Tab("DEBUG",
            [
                new TextMenuOption("TEMP OPTION", 2/3f),
            ]),
        ];

        // Create animations
        AnimatedObjectResource startEraseAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuStartEraseAnimations);
        Texture2D tabFrameTexture = Engine.FrameContentManager.Load<Texture2D>("OptionsMenuTabs");

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
        const float headerTextSize = 0.5f;
        for (int i = 0; i < Tabs.Length; i++)
        {
            float width = ReadvancedFonts.MenuYellow.GetWidth(Tabs[i].Name) * headerTextSize;
            TabHeaderTexts[i] = new SpriteFontTextObject()
            {
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(89 + i * 60 - width / 2, 34),
                RenderContext = RenderContext,
                AffineMatrix = new AffineMatrix(0, new Vector2(headerTextSize), false, false),
                Text = Tabs[i].Name,
                Font = ReadvancedFonts.MenuYellow,
            };
        }

        // Set the initial tab
        SetSelectedTab(0, false);
    }

    protected override void Step_TransitionIn()
    {
        TabsCursor.ScreenPos = TabsCursor.ScreenPos with { Y = (12 - 80) + TransitionValue / 2f };
        TabHeaders.ScreenPos = TabHeaders.ScreenPos with { Y = (-37 - 80) + TransitionValue / 2f };
        
        foreach (SpriteFontTextObject tabHeader in TabHeaderTexts)
            tabHeader.ScreenPos = tabHeader.ScreenPos with { Y = (34 - 80) + TransitionValue / 2f };
    }

    protected override void Step_Active()
    {
        if (JoyPad.IsButtonJustPressed(GbaInput.Up))
        {
            SetSelectedOption(SelectedOption - 1);
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.Down))
        {
            SetSelectedOption(SelectedOption + 1);
        }
        else if ((JoyPad.IsButtonJustPressed(GbaInput.Left) || JoyPad.IsButtonJustPressed(GbaInput.L)) && Menu.Cursor.CurrentAnimation != 16)
        {
            SetSelectedTab(SelectedTab - 1);
        }
        else if ((JoyPad.IsButtonJustPressed(GbaInput.Right) || JoyPad.IsButtonJustPressed(GbaInput.R)) && Menu.Cursor.CurrentAnimation != 16)
        {
            SetSelectedTab(SelectedTab + 1);
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.B))
        {
            Menu.ChangePage(new GameModeMenuPage(Menu), NewPageMode.Back);
        }

        MoveTabsCursor();
    }

    protected override void Step_TransitionOut()
    {
        TabsCursor.ScreenPos = TabsCursor.ScreenPos with { Y = 12 - TransitionValue / 2f };
        TabHeaders.ScreenPos = TabHeaders.ScreenPos with { Y = -37 - TransitionValue / 2f };
        
        foreach (SpriteFontTextObject tabHeader in TabHeaderTexts)
            tabHeader.ScreenPos = tabHeader.ScreenPos with { Y = 34 - TransitionValue / 2f };
    }

    protected override void Draw(AnimationPlayer animationPlayer)
    {
        DrawOptions(animationPlayer);

        animationPlayer.Play(TabHeaders);

        foreach (SpriteFontTextObject tabHeaderText in TabHeaderTexts)
            animationPlayer.Play(tabHeaderText);

        animationPlayer.Play(TabsCursor);
    }

    public class Tab
    {
        public Tab(string name, MenuOption[] menuOptions)
        {
            Name = name;
            MenuOptions = menuOptions;
        }

        public string Name { get; }
        public MenuOption[] MenuOptions { get; }
    }
}