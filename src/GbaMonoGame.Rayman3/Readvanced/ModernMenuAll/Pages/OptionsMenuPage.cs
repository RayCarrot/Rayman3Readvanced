using System;
using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

public class OptionsMenuPage : MenuPage
{
    public OptionsMenuPage(ModernMenuAll menu) : base(menu) { }

    private const float TabHeaderWidth = 60;
    private const float TabHeaderTextScale = 1 / 2f;
    private const float TabsCursorMoveTime = 12;
    private const float InfoTextScale = 3 / 10f;
    private const int InfoTextMaxLines = 4;
    private const float InfoTextMaxWidth = 260;
    private const float ArrowScale = 1 / 2f;

    public override bool UsesCursor => true;
    public override int BackgroundPalette => 1;
    public override int LineHeight => 12;
    public override int MaxOptions => 4;
    public override bool HasScrollBar => true;

    public GameOptions.GameOptionsGroup[] Tabs { get; set; }
    public int SelectedTab { get; set; }
    public bool IsEditingOption { get; set; }

    public float? TabsCursorStartX { get; set; }
    public float? TabsCursorDestX { get; set; }

    public AnimatedObject TabsCursor { get; set; }
    public SpriteTextureObject TabHeaders { get; set; }
    public SpriteFontTextObject[] TabHeaderTexts { get; set; }

    public SpriteTextureObject InfoTextBox { get; set; }
    public SpriteTextObject[] InfoTextLines { get; set; }
    public AnimatedObject ArrowLeft { get; set; }
    public AnimatedObject ArrowRight { get; set; }

    private void SetSelectedTab(int selectedTab, bool playSound = true)
    {
        if (selectedTab > Tabs.Length - 1)
            selectedTab = 0;
        else if (selectedTab < 0)
            selectedTab = Tabs.Length - 1;

        SelectedTab = selectedTab;

        SetTabsCursorMovement(TabsCursor.ScreenPos.X, selectedTab * TabHeaderWidth + 90);

        ClearOptions();
        foreach (OptionsMenuOption menuOption in Tabs[selectedTab].Options)
            AddOption(menuOption);

        SetSelectedOption(0, playSound: false, forceUpdate: true);

        if (playSound)
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
    }

    private void SetTabsCursorMovement(float startX, float endX)
    {
        TabsCursorStartX = startX;
        TabsCursorDestX = endX;
    }

    private void ManageTabsCursor()
    {
        if (TabsCursorStartX == null || TabsCursorDestX == null)
            return;

        float startX = TabsCursorStartX.Value;
        float destX = TabsCursorDestX.Value;

        // Move with a speed based on the distance
        float dist = destX - startX;
        float speed = dist / TabsCursorMoveTime;

        // Move
        if ((destX < startX && TabsCursor.ScreenPos.X > destX) ||
            (destX > startX && TabsCursor.ScreenPos.X < destX))
        {
            TabsCursor.ScreenPos += new Vector2(speed, 0);
        }
        // Finished moving
        else
        {
            TabsCursor.ScreenPos = TabsCursor.ScreenPos with { X = destX };
            TabsCursorStartX = null;
            TabsCursorDestX = null;
        }
    }

    protected override bool SetSelectedOption(int selectedOption, bool playSound = true, bool forceUpdate = false)
    {
        if (!base.SetSelectedOption(selectedOption, playSound, forceUpdate))
            return false;
        
        // Get the selected option
        OptionsMenuOption option = (OptionsMenuOption)Options[SelectedOption];

        // Set the info text
        byte[][] textLines = FontManager.GetWrappedStringLines(FontSize.Font32, option.InfoText, InfoTextMaxWidth * (1 / InfoTextScale));
        Debug.Assert(textLines.Length <= InfoTextMaxLines, "Info text has too many lines");
        for (int i = 0; i < InfoTextLines.Length; i++)
            InfoTextLines[i].Text = i < textLines.Length ? FontManager.GetTextString(textLines[i]) : String.Empty;

        return true;
    }

    protected override void Init()
    {
        // Add game option tabs
        Tabs = GameOptions.Create();

        // Create animations
        AnimatedObjectResource startEraseAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuStartEraseAnimations);
        Texture2D tabHeadersTexture = Engine.FixContentManager.Load<Texture2D>("OptionsMenuTabs");
        Texture2D infoTextBoxTexture = Engine.FixContentManager.Load<Texture2D>("MenuTextBox");
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
            Texture = tabHeadersTexture,
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
                ScreenPos = new Vector2(89 + i * TabHeaderWidth - width / 2, 30),
                RenderContext = RenderContext,
                AffineMatrix = new AffineMatrix(0, new Vector2(TabHeaderTextScale), false, false),
                Text = Tabs[i].Name,
                Font = ReadvancedFonts.MenuYellow,
            };
        }

        InfoTextBox = new SpriteTextureObject
        {
            BgPriority = 3,
            ObjPriority = 0,
            ScreenPos = new Vector2(70, 112),
            RenderContext = RenderContext,
            Texture = infoTextBoxTexture,
        };

        InfoTextLines = new SpriteTextObject[InfoTextMaxLines];
        for (int i = 0; i < InfoTextLines.Length; i++)
        {
            float height = FontManager.GetFontHeight(FontSize.Font32) * InfoTextScale;

            InfoTextLines[i] = new SpriteTextObject
            {
                BgPriority = 3,
                ObjPriority = 0,
                YPriority = 0,
                ScreenPos = new Vector2(75, 109 + height * i),
                RenderContext = RenderContext,
                AffineMatrix = new AffineMatrix(0, new Vector2(InfoTextScale), false, false),
                Color = TextColor.TextBox,
                FontSize = FontSize.Font32,
            };
        }

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
        TabsCursorStartX = null;
        TabsCursorDestX = null;

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

                OptionsMenuOption option = (OptionsMenuOption)Options[SelectedOption];
                option.Reset();

                CursorClick(null);

                // Start arrow animations on frame 4 since it looks nicer
                ArrowLeft.CurrentFrame = 4;
                ArrowRight.CurrentFrame = 4;
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
            if (JoyPad.IsButtonJustPressed(GbaInput.A))
            {
                IsEditingOption = false;

                OptionsMenuOption option = (OptionsMenuOption)Options[SelectedOption];
                option.Apply();

                CursorClick(null);
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.B))
            {
                IsEditingOption = false;

                OptionsMenuOption option = (OptionsMenuOption)Options[SelectedOption];
                option.Reset();

                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);
            }
            else
            {
                OptionsMenuOption option = (OptionsMenuOption)Options[SelectedOption];
                option.Step();
            }
        }

        ManageTabsCursor();
    }

    protected override void Step_TransitionOut()
    {
        TabsCursor.ScreenPos = TabsCursor.ScreenPos with { Y = 12 - TransitionValue / 2f };
        TabHeaders.ScreenPos = TabHeaders.ScreenPos with { Y = -37 - TransitionValue / 2f };
        
        foreach (SpriteFontTextObject tabHeader in TabHeaderTexts)
            tabHeader.ScreenPos = tabHeader.ScreenPos with { Y = 30 - TransitionValue / 2f };
    }

    protected override void UnInit()
    {
        // Save
        Engine.SaveConfig();
    }

    protected override void Draw(AnimationPlayer animationPlayer)
    {
        DrawOptions(animationPlayer);

        animationPlayer.Play(TabHeaders);

        foreach (SpriteFontTextObject tabHeaderText in TabHeaderTexts)
            animationPlayer.Play(tabHeaderText);

        animationPlayer.Play(TabsCursor);

        animationPlayer.Play(InfoTextBox);
        foreach (SpriteTextObject infoTextLine in InfoTextLines)
            animationPlayer.Play(infoTextLine);

        if (IsEditingOption)
        {
            OptionsMenuOption option = (OptionsMenuOption)Options[SelectedOption];

            // Set the arrow positions
            ArrowLeft.ScreenPos = option.ArrowLeftPosition * (1 / ArrowScale);
            ArrowRight.ScreenPos = option.ArrowRightPosition * (1 / ArrowScale);

            animationPlayer.Play(ArrowLeft);
            animationPlayer.Play(ArrowRight);
        }
    }
}