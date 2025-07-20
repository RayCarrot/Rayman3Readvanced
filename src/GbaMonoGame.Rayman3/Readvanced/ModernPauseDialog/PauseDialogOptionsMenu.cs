using System;
using System.Diagnostics;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

// NOTE: A lot of this code is copied from the OptionsMenuPage
public class PauseDialogOptionsMenu
{
    public PauseDialogOptionsMenu()
    {
        RenderContext = new FixedResolutionRenderContext(Resolution.Modern, verticalAlignment: VerticalAlignment.Top);
    }

    private const float TransitionHeight = 220;
    private const float TransitionSpeed = 5;
    private const float TabHeadersTransitionStart = 120;
    private const float TabHeadersTransitionHeight = 45;
    private const float TabHeadersTransitionSpeed = 2;

    private const float CanvasBaseY = 0;
    private const float CursorBaseY = 67;
    private const float TabBarBaseY = -37;
    private const float InfoTextBoxBaseY = 112;
    private const float InfoTextLinesBaseY = 109;
    private const float ScrollBarBaseY = 40;

    private const float LineHeight = 12;

    private const float InfoTextScale = 3 / 10f;
    private const int InfoTextMaxLines = 4;
    private const float InfoTextMaxWidth = 260;
    private const float ArrowScale = 1 / 2f;

    public RenderContext RenderContext { get; }

    public GameOptions.GameOptionsGroup[] Tabs { get; set; }
    public int SelectedTab { get; set; }
    public bool IsEditingOption { get; set; }
    public bool ShowInfoText { get; set; }

    public float? CursorStartY { get; set; }
    public float? CursorDestY { get; set; }

    public float? TabsCursorStartX { get; set; }
    public float? TabsCursorDestX { get; set; }

    public OptionsMenuOption[] Options { get; set; }
    public int SelectedOption { get; set; }
    public int MaxOptions => ShowInfoText ? 4 : 8;
    public int ScrollOffset { get; set; }
    public bool HasScrollableContent => Options.Length > MaxOptions;
    public int MaxScrollOffset => Math.Max(Options.Length - MaxOptions, 0);

    public SpriteTextureObject Canvas { get; set; }
    public AnimatedObject Cursor { get; set; }

    public MenuTabBar TabBar { get; set; }

    public SpriteTextureObject InfoTextBox { get; set; }
    public SpriteTextObject InfoText { get; set; }
    public MenuHorizontalArrows HorizontalArrows { get; set; }

    public MenuScrollBar ScrollBar { get; set; }

    public float OffsetY { get; set; }
    public float CursorOffsetY { get; set; }
    public float TabHeadersOffsetY { get; set; }
    public PauseDialogDrawStep DrawStep { get; set; }

    private Vector2 GetOptionPosition(int index) => new(75, 54 + LineHeight * index - OffsetY);

    private void SetSelectedTab(int selectedTab, bool playSound = true)
    {
        if (selectedTab > Tabs.Length - 1)
            selectedTab = 0;
        else if (selectedTab < 0)
            selectedTab = Tabs.Length - 1;

        SelectedTab = selectedTab;
        TabBar.SetSelectedTab(selectedTab);

        SelectedOption = 0;
        ScrollOffset = 0;
        Options = Tabs[selectedTab].Options;
        for (int i = 0; i < Options.Length; i++)
        {
            OptionsMenuOption option = Options[i];
            if (!option.IsInitialized)
            {
                option.Init(0, RenderContext, i);
                option.IsInitialized = true;
            }

            option.ChangeIsSelected(false);
        }

        // Reset all options when switching to the tab (handling presets last!)
        foreach (OptionsMenuOption option in Options.OrderBy(x => x is PresetSelectionOptionsMenuOption ? 1 : 0))
            option.Reset(Options);

        SetSelectedOption(0, false);

        if (playSound)
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
    }

    private void SetCursorMovement(float startY, float endY)
    {
        CursorStartY = startY;
        CursorDestY = endY;
    }

    private void ManageCursor()
    {
        // Move with a constant speed of 4
        const float speed = 4;

        if (CursorStartY != null && CursorDestY != null)
        {
            float startY = CursorStartY.Value;
            float destY = CursorDestY.Value;

            // Move up
            if (destY < startY && CursorOffsetY > destY)
            {
                CursorOffsetY -= speed;
            }
            // Move down
            else if (destY > startY && CursorOffsetY < destY)
            {
                CursorOffsetY += speed;
            }
            // Finished moving
            else
            {
                CursorOffsetY = destY;
                CursorStartY = null;
                CursorDestY = null;
            }
        }
    }

    private void CursorClick()
    {
        Cursor.CurrentAnimation = 16;
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
    }

    private void SetSelectedOption(int selectedOption, bool playSound = true)
    {
        int prevSelectedOption = SelectedOption;

        int newSelectedOption = selectedOption;

        int newScrollOffset = ScrollOffset;
        if (newSelectedOption > prevSelectedOption)
        {
            if (newSelectedOption >= ScrollOffset + MaxOptions)
                newScrollOffset++;
        }
        else if (newSelectedOption < prevSelectedOption)
        {
            if (newSelectedOption < ScrollOffset)
                newScrollOffset--;
        }

        if (newSelectedOption > Options.Length - 1)
        {
            newSelectedOption = 0;
            newScrollOffset = 0;
        }
        else if (newSelectedOption < 0)
        {
            newSelectedOption = Options.Length - 1;
            newScrollOffset = MaxScrollOffset;
        }
        SetCursorMovement(CursorOffsetY, (newSelectedOption - newScrollOffset) * LineHeight);

        SelectedOption = newSelectedOption;
        Options[prevSelectedOption].ChangeIsSelected(false);
        Options[newSelectedOption].ChangeIsSelected(true);

        if (newScrollOffset != ScrollOffset)
            ScrollOffset = newScrollOffset;

        if (playSound)
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);

        // Get the selected option
        OptionsMenuOption option = Options[SelectedOption];

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
    }

    public void MoveIn()
    {
        switch (Frame.Current)
        {
            case FrameSideScroller frameSideScroller:
                // Move out the life, lums and cages bars
                frameSideScroller.UserInfo.MoveOutBars();

                // The Lums1000Bar can't normally transition out, so we force it to here
                if (frameSideScroller.UserInfo.Lums1000Bar != null)
                    frameSideScroller.UserInfo.Lums1000Bar.EnableTransitions = true;
                break;
            
            case FrameSingleMode7 frameSingleMode7:
                frameSingleMode7.UserInfo.MoveOutBars();
                break;
            
            case FrameWaterSkiMode7 frameWaterSkiMode7:
                frameWaterSkiMode7.UserInfo.MoveOutBars();
                break;
            
            case FrameWorldSideScroller frameWorldSideScroller:
                frameWorldSideScroller.UserInfo.LifeBar.MoveOut();

                frameWorldSideScroller.UserInfo.Lums1000Bar.MoveOut();
                frameWorldSideScroller.UserInfo.Lums1000Bar.EnableTransitions = true;

                frameWorldSideScroller.UserInfo.Cages50Bar.MoveOut();
                frameWorldSideScroller.UserInfo.Cages50Bar.EnableTransitions = true;

                frameWorldSideScroller.UserInfo.MoveOutCurtains();
                break;
            
            case WorldMap worldMap:
                worldMap.UserInfo.LifeBar.MoveOut();

                worldMap.UserInfo.Lums1000Bar.MoveOut();
                worldMap.UserInfo.Lums1000Bar.EnableTransitions = true;

                worldMap.UserInfo.Cages50Bar.MoveOut();
                worldMap.UserInfo.Cages50Bar.EnableTransitions = true;
                break;
            
            default:
                throw new InvalidOperationException("Invalid frame");
        }

        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);

        OffsetY = TransitionHeight;
        TabHeadersOffsetY = TabHeadersTransitionHeight;
        DrawStep = PauseDialogDrawStep.MoveIn;
    }

    public void MoveOut()
    {
        switch (Frame.Current)
        {
            case FrameSideScroller frameSideScroller:
                // Only move in the life, lums and cages bars if we're not in a Ly challenge level
                if (GameInfo.MapId is not (MapId.ChallengeLy1 or MapId.ChallengeLy2 or MapId.ChallengeLyGCN)) 
                    frameSideScroller.UserInfo.MoveInBars();
                break;

            case FrameSingleMode7 frameSingleMode7:
                frameSingleMode7.UserInfo.MoveInBars();
                break;

            case FrameWaterSkiMode7 frameWaterSkiMode7:
                frameWaterSkiMode7.UserInfo.MoveInBars();
                break;

            case FrameWorldSideScroller frameWorldSideScroller:
                frameWorldSideScroller.UserInfo.LifeBar.SetToStayVisible();
                frameWorldSideScroller.UserInfo.LifeBar.MoveIn();

                frameWorldSideScroller.UserInfo.Lums1000Bar.SetToStayVisible();
                frameWorldSideScroller.UserInfo.Lums1000Bar.MoveIn();

                frameWorldSideScroller.UserInfo.Cages50Bar.SetToStayVisible();
                frameWorldSideScroller.UserInfo.Cages50Bar.MoveIn();

                frameWorldSideScroller.UserInfo.MoveInCurtains();
                break;

            case WorldMap worldMap:
                worldMap.UserInfo.LifeBar.SetToStayVisible();
                worldMap.UserInfo.LifeBar.MoveIn();

                worldMap.UserInfo.Lums1000Bar.SetToStayVisible();
                worldMap.UserInfo.Lums1000Bar.MoveIn();

                worldMap.UserInfo.Cages50Bar.SetToStayVisible();
                worldMap.UserInfo.Cages50Bar.MoveIn();
                break;

            default:
                throw new InvalidOperationException("Invalid frame");
        }

        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
        
        OffsetY = 0;
        TabHeadersOffsetY = 0;
        DrawStep = PauseDialogDrawStep.MoveOut;
    }

    public void Load()
    {
        // Add game option tabs
        Tabs = GameOptions.Create();

        // Create animations
        AnimatedObjectResource propsAnimations = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.MenuPropAnimations);
        Texture2D canvasTexture = Engine.FixContentManager.Load<Texture2D>(Assets.OptionsDialogBoardTexture);
        Texture2D infoTextBoxTexture = Engine.FixContentManager.Load<Texture2D>(Assets.MenuTextBoxTexture);

        Canvas = new SpriteTextureObject
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(0, CanvasBaseY),
            RenderContext = RenderContext,
            Texture = canvasTexture,
        };

        Cursor = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(33, CursorBaseY),
            CurrentAnimation = 0,
            RenderContext = RenderContext,
        };

        TabBar = new MenuTabBar(RenderContext, new Vector2(63, TabBarBaseY - TabHeadersOffsetY), 0, Tabs.Select(x => x.Name).ToArray());

        InfoTextBox = new SpriteTextureObject
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(70, InfoTextBoxBaseY),
            RenderContext = RenderContext,
            Texture = infoTextBoxTexture,
        };

        InfoText = new SpriteTextObject
        {
            BgPriority = 0,
            ObjPriority = 0,
            YPriority = 0,
            ScreenPos = new Vector2(75, InfoTextLinesBaseY),
            RenderContext = RenderContext,
            AffineMatrix = new AffineMatrix(0, new Vector2(InfoTextScale)),
            Color = TextColor.TextBox,
            FontSize = FontSize.Font32,
        };

        HorizontalArrows = new MenuHorizontalArrows(RenderContext, 0, ArrowScale, VerticalAlignment.Top);

        ScrollBar = new MenuScrollBar(RenderContext, new Vector2(352, ScrollBarBaseY), 0);

        // Reset values
        IsEditingOption = false;
        TabsCursorStartX = null;
        TabsCursorDestX = null;

        // Set the initial tab
        SetSelectedTab(0, false);
    }

    public void Step()
    {
        if (DrawStep != PauseDialogDrawStep.Wait)
        {
            TabBar.Step();
            return;
        }

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
                OptionsMenuOption option = Options[SelectedOption];
                option.Reset(Options);

                CursorClick();
                HorizontalArrows.Start();
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.B))
            {
                // Go back
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);
                MoveOut();
            }
        }
        // Editing
        else
        {
            OptionsMenuOption option = Options[SelectedOption];
            OptionsMenuOption.EditStepResult result = option.EditStep(Options);

            if (result == OptionsMenuOption.EditStepResult.Apply)
            {
                // Reset preset options
                foreach (OptionsMenuOption o in Options)
                {
                    if (o != option && o is PresetSelectionOptionsMenuOption)
                        o.Reset(Options);
                }

                IsEditingOption = false;
                CursorClick();
            }
            else if (result == OptionsMenuOption.EditStepResult.Cancel)
            {
                IsEditingOption = false;
                option.Reset(Options);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);
            }
        }

        // End click animation
        if (Cursor.CurrentAnimation == 16 && Cursor.EndOfAnimation)
            Cursor.CurrentAnimation = 0;

        ManageCursor();
        TabBar.Step();
    }

    public void Draw(AnimationPlayer animationPlayer)
    {
        switch (DrawStep)
        {
            case PauseDialogDrawStep.Hide:
                OffsetY = TransitionHeight;
                break;

            case PauseDialogDrawStep.MoveIn:
                if (OffsetY > 0)
                    OffsetY -= TransitionSpeed;
                else
                    OffsetY = 0;

                if (OffsetY < TabHeadersTransitionStart)
                {
                    if (TabHeadersOffsetY > 0) 
                        TabHeadersOffsetY -= TabHeadersTransitionSpeed;
                    else
                        TabHeadersOffsetY = 0;
                }

                if (OffsetY <= 0 && TabHeadersOffsetY <= 0)
                    DrawStep = PauseDialogDrawStep.Wait;
                break;

            case PauseDialogDrawStep.MoveOut:
                if (OffsetY < TransitionHeight)
                    OffsetY += TransitionSpeed;
                else
                    OffsetY = TransitionHeight;

                if (TabHeadersOffsetY < TabHeadersTransitionHeight)
                    TabHeadersOffsetY += TabHeadersTransitionSpeed;
                else
                    TabHeadersOffsetY = TabHeadersTransitionHeight;

                if (Frame.Current is not FrameWorldSideScroller frameWorldSideScroller ||
                    frameWorldSideScroller.UserInfo.HasFinishedMovingInCurtains())
                {
                    if (OffsetY >= TransitionHeight && TabHeadersOffsetY >= TabHeadersTransitionHeight)
                        DrawStep = PauseDialogDrawStep.Hide;
                }
                break;
        }

        if (DrawStep != PauseDialogDrawStep.Hide)
        {
            // Transition
            Canvas.ScreenPos = Canvas.ScreenPos with { Y = CanvasBaseY - OffsetY };
            Cursor.ScreenPos = Cursor.ScreenPos with { Y = CursorBaseY + CursorOffsetY - OffsetY };

            int index = 0;
            foreach (OptionsMenuOption option in Options.Skip(ScrollOffset).Take(MaxOptions))
            {
                option.SetPosition(GetOptionPosition(index));
                index++;
            }

            TabBar.Position = TabBar.Position with { Y = TabBarBaseY - TabHeadersOffsetY };

            InfoTextBox.ScreenPos = InfoTextBox.ScreenPos with { Y = InfoTextBoxBaseY - OffsetY };
            InfoText.ScreenPos = InfoText.ScreenPos with { Y = InfoTextLinesBaseY - OffsetY };

            ScrollBar.Position = ScrollBar.Position with { Y = ScrollBarBaseY - OffsetY };

            // Draw
            animationPlayer.Play(Canvas);
            animationPlayer.Play(Cursor);
            foreach (OptionsMenuOption option in Options.Skip(ScrollOffset).Take(MaxOptions))
                option.Draw(animationPlayer);

            TabBar.Draw(animationPlayer);

            if (ShowInfoText)
            {
                animationPlayer.Play(InfoTextBox);
                animationPlayer.Play(InfoText);
            }

            if (IsEditingOption)
            {
                OptionsMenuOption option = Options[SelectedOption];

                if (option.ShowArrows)
                {
                    HorizontalArrows.Position = option.ArrowsPosition;
                    HorizontalArrows.Width = option.ArrowsWidth;
                    HorizontalArrows.Draw(animationPlayer);
                }
            }

            if (HasScrollableContent)
            {
                ScrollBar.ScrollOffset = ScrollOffset;
                ScrollBar.MaxScrollOffset = MaxScrollOffset;
            }
            else
            {
                ScrollBar.ScrollOffset = 0;
                ScrollBar.MaxScrollOffset = 0;
            }

            ScrollBar.Size = ShowInfoText ? MenuScrollBarSize.Small : MenuScrollBarSize.Big;
            ScrollBar.Draw(animationPlayer);
        }
    }
}