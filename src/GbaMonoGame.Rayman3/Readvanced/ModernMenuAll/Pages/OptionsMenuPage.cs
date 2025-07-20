using System.Diagnostics;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

public class OptionsMenuPage : MenuPage
{
    public OptionsMenuPage(ModernMenuAll menu) : base(menu) { }

    private const float InfoTextScale = 3 / 10f;
    private const int InfoTextMaxLines = 4;
    private const float InfoTextMaxWidth = 260;
    private const float ArrowScale = 1 / 2f;

    public override bool UsesCursor => true;
    public override int BackgroundPalette => 1;
    public override int LineHeight => 12;
    public override int MaxOptions => ShowInfoText ? 4 : 8;
    public override bool HasScrollBar => true;
    public override MenuScrollBarSize ScrollBarSize => ShowInfoText ? MenuScrollBarSize.Small : MenuScrollBarSize.Big;

    public GameOptions.GameOptionsGroup[] Tabs { get; set; }
    public OptionsMenuOption[] OptionsMenuOptions => Tabs[SelectedTab].Options;
    public int SelectedTab { get; set; }
    public bool IsEditingOption { get; set; }
    public bool ShowInfoText { get; set; }

    public MenuTabBar TabBar { get; set; }

    public SpriteTextureObject InfoTextBox { get; set; }
    public SpriteTextObject InfoText { get; set; }
    public MenuHorizontalArrows HorizontalArrows { get; set; }

    private void SetSelectedTab(int selectedTab, bool playSound = true)
    {
        if (selectedTab > Tabs.Length - 1)
            selectedTab = 0;
        else if (selectedTab < 0)
            selectedTab = Tabs.Length - 1;

        SelectedTab = selectedTab;
        TabBar.SetSelectedTab(selectedTab);

        ClearOptions();
        foreach (OptionsMenuOption menuOption in Tabs[selectedTab].Options)
            AddOption(menuOption);

        // Reset all options when switching to the tab (handling presets last!)
        foreach (OptionsMenuOption option in OptionsMenuOptions.OrderBy(x => x is PresetSelectionOptionsMenuOption ? 1 : 0))
            option.Reset(OptionsMenuOptions);

        SetSelectedOption(0, playSound: false, forceUpdate: true);

        if (playSound)
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
    }

    protected override bool SetSelectedOption(int selectedOption, bool playSound = true, bool forceUpdate = false)
    {
        if (!base.SetSelectedOption(selectedOption, playSound, forceUpdate))
            return false;
        
        // Get the selected option
        OptionsMenuOption option = (OptionsMenuOption)Options[SelectedOption];

        // Set the info text
        if (option.InfoText != null)
        {
            ShowInfoText = true;

            string wrappedInfoText = FontManager.WrapText(InfoText.FontSize, option.InfoText, InfoTextMaxWidth * (1 / InfoTextScale));
            Debug.Assert(wrappedInfoText.Count(x => x == '\n') + 1 <= InfoTextMaxLines, "Info text has too many lines");
            InfoText.Text = wrappedInfoText;
        }
        else
        {
            ShowInfoText = false;
        }

        return true;
    }

    protected override void Init()
    {
        // Add game option tabs
        Tabs = GameOptions.Create();

        // Create animations
        Texture2D infoTextBoxTexture = Engine.FixContentManager.Load<Texture2D>(Assets.MenuTextBoxTexture);

        TabBar = new MenuTabBar(RenderContext, new Vector2(63, -37), 1, Tabs.Select(x => x.Name).ToArray());

        InfoTextBox = new SpriteTextureObject
        {
            BgPriority = 3,
            ObjPriority = 0,
            ScreenPos = new Vector2(70, 112),
            RenderContext = RenderContext,
            Texture = infoTextBoxTexture,
        };

        InfoText = new SpriteTextObject
        {
            BgPriority = 3,
            ObjPriority = 0,
            YPriority = 0,
            ScreenPos = new Vector2(75, 109),
            RenderContext = RenderContext,
            AffineMatrix = new AffineMatrix(0, new Vector2(InfoTextScale)),
            Color = TextColor.TextBox,
            FontSize = FontSize.Font32,
        };

        HorizontalArrows = new MenuHorizontalArrows(RenderContext, 1, ArrowScale);

        // Reset values
        IsEditingOption = false;

        // Set the initial tab
        SetSelectedTab(0, false);
    }

    protected override void Step_TransitionIn()
    {
        TabBar.Position = TabBar.Position with { Y = (-37 - 80) + TransitionValue / 2f };
        TabBar.Step();
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

                // Reset option before editing in case it has changed (like the window might have been resized)
                OptionsMenuOption option = (OptionsMenuOption)Options[SelectedOption];
                option.Reset(OptionsMenuOptions);

                CursorClick(null);
                HorizontalArrows.Start();
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.B))
            {
                // Go back to the game mode menu
                Menu.ChangePage(new GameModeMenuPage(Menu), NewPageMode.Back);
            }
        }
        // Editing
        else
        {
            OptionsMenuOption option = (OptionsMenuOption)Options[SelectedOption];
            OptionsMenuOption.EditStepResult result = option.EditStep(OptionsMenuOptions);

            if (result == OptionsMenuOption.EditStepResult.Apply)
            {
                // Reset preset options
                foreach (OptionsMenuOption o in OptionsMenuOptions)
                {
                    if (o != option && o is PresetSelectionOptionsMenuOption)
                        o.Reset(OptionsMenuOptions);
                }

                IsEditingOption = false;
                CursorClick(null);
            }
            else if (result == OptionsMenuOption.EditStepResult.Cancel)
            {
                IsEditingOption = false;
                option.Reset(OptionsMenuOptions);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);
            }
        }

        TabBar.Step();
    }

    protected override void Step_TransitionOut()
    {
        TabBar.Position = TabBar.Position with { Y = -37 - TransitionValue / 2f };
        TabBar.Step();
    }

    protected override void UnInit()
    {
        // Save
        Engine.SaveConfig();
    }

    protected override void Draw(AnimationPlayer animationPlayer)
    {
        DrawOptions(animationPlayer);
        TabBar.Draw(animationPlayer);

        if (ShowInfoText)
        {
            animationPlayer.Play(InfoTextBox);
            animationPlayer.Play(InfoText);
        }

        if (IsEditingOption)
        {
            OptionsMenuOption option = (OptionsMenuOption)Options[SelectedOption];

            if (option.ShowArrows)
            {
                HorizontalArrows.Position = option.ArrowsPosition;
                HorizontalArrows.Width = option.ArrowsWidth;
                HorizontalArrows.Draw(animationPlayer);
            }
        }
    }
}