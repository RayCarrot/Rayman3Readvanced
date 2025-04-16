using System;
using System.Diagnostics;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework;
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
    private const float TabHeadersBaseY = -37;
    private const float TabHeaderTextsBaseY = 30;
    private const float TabsCursorBaseY = 12;
    private const float InfoTextBoxBaseY = 112;
    private const float InfoTextLinesBaseY = 109;
    private const float ScrollBarBaseY = 40;
    private const float ScrollBarThumbBaseY = 56;

    private const int MaxOptions = 4;
    private const float ScrollBarLength = 32;

    private const float LineHeight = 12;

    private const float TabHeaderWidth = 60;
    private const float TabHeaderTextScale = 1 / 2f;
    private const float TabsCursorMoveTime = 12;
    private const float InfoTextScale = 3 / 10f;
    private const int InfoTextMaxLines = 4;
    private const float InfoTextMaxWidth = 260;
    private const float ArrowScale = 1 / 2f;

    public RenderContext RenderContext { get; }

    public GameOptions.GameOptionsGroup[] Tabs { get; set; }
    public int SelectedTab { get; set; }
    public bool IsEditingOption { get; set; }

    public float? CursorStartY { get; set; }
    public float? CursorDestY { get; set; }

    public float? TabsCursorStartX { get; set; }
    public float? TabsCursorDestX { get; set; }

    public OptionsMenuOption[] Options { get; set; }
    public int SelectedOption { get; set; }
    public int ScrollOffset { get; set; }
    public bool HasScrollableContent => Options.Length > MaxOptions;
    public int MaxScrollOffset => Options.Length - MaxOptions;

    public SpriteTextureObject Canvas { get; set; }
    public AnimatedObject Cursor { get; set; }

    public AnimatedObject TabsCursor { get; set; }
    public SpriteTextureObject TabHeaders { get; set; }
    public SpriteFontTextObject[] TabHeaderTexts { get; set; }

    public SpriteTextureObject InfoTextBox { get; set; }
    public SpriteTextObject[] InfoTextLines { get; set; }
    public AnimatedObject ArrowLeft { get; set; }
    public AnimatedObject ArrowRight { get; set; }

    public SpriteTextureObject ScrollBar { get; set; }
    public SpriteTextureObject ScrollBarThumb { get; set; }

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

        SetTabsCursorMovement(TabsCursor.ScreenPos.X, selectedTab * TabHeaderWidth + 90);

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
        byte[][] textLines = FontManager.GetWrappedStringLines(FontSize.Font32, option.InfoText, InfoTextMaxWidth * (1 / InfoTextScale));
        Debug.Assert(textLines.Length <= InfoTextMaxLines, "Info text has too many lines");
        for (int i = 0; i < InfoTextLines.Length; i++)
            InfoTextLines[i].Text = i < textLines.Length ? FontManager.GetTextString(textLines[i]) : String.Empty;
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
        AnimatedObjectResource propsAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuPropAnimations);
        Texture2D canvasTexture = Engine.FixContentManager.Load<Texture2D>(Assets.OptionsDialogBoardTexture);
        AnimatedObjectResource startEraseAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuStartEraseAnimations);
        Texture2D tabHeadersTexture = Engine.FixContentManager.Load<Texture2D>(Assets.OptionsMenuTabsTexture);
        Texture2D infoTextBoxTexture = Engine.FixContentManager.Load<Texture2D>(Assets.MenuTextBoxTexture);
        AnimatedObjectResource multiplayerTypeFrameAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerTypeFrameAnimations);
        Texture2D scrollBarTexture = Engine.FixContentManager.Load<Texture2D>(Assets.ScrollBarTexture);
        Texture2D scrollBarThumbTexture = Engine.FixContentManager.Load<Texture2D>(Assets.ScrollBarThumbTexture);

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

        TabsCursor = new AnimatedObject(startEraseAnimations, startEraseAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(90, TabsCursorBaseY),
            CurrentAnimation = 40,
            RenderContext = RenderContext,
        };

        TabHeaders = new SpriteTextureObject
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(63, TabHeadersBaseY),
            Texture = tabHeadersTexture,
            RenderContext = RenderContext,
        };

        TabHeaderTexts = new SpriteFontTextObject[Tabs.Length];
        for (int i = 0; i < Tabs.Length; i++)
        {
            float width = ReadvancedFonts.MenuYellow.GetWidth(Tabs[i].Name) * TabHeaderTextScale;
            TabHeaderTexts[i] = new SpriteFontTextObject()
            {
                BgPriority = 0,
                ObjPriority = 0,
                ScreenPos = new Vector2(89 + i * TabHeaderWidth - width / 2, TabHeaderTextsBaseY),
                RenderContext = RenderContext,
                AffineMatrix = new AffineMatrix(0, new Vector2(TabHeaderTextScale), false, false),
                Text = Tabs[i].Name,
                Font = ReadvancedFonts.MenuYellow,
            };
        }

        InfoTextBox = new SpriteTextureObject
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(70, InfoTextBoxBaseY),
            RenderContext = RenderContext,
            Texture = infoTextBoxTexture,
        };

        InfoTextLines = new SpriteTextObject[InfoTextMaxLines];
        for (int i = 0; i < InfoTextLines.Length; i++)
        {
            float height = FontManager.GetFontHeight(FontSize.Font32) * InfoTextScale;

            InfoTextLines[i] = new SpriteTextObject
            {
                BgPriority = 0,
                ObjPriority = 0,
                YPriority = 0,
                ScreenPos = new Vector2(75, InfoTextLinesBaseY + height * i),
                RenderContext = RenderContext,
                AffineMatrix = new AffineMatrix(0, new Vector2(InfoTextScale), false, false),
                Color = TextColor.TextBox,
                FontSize = FontSize.Font32,
            };
        }
        // A bit hacky, but create a new render context for the arrows in order to scale them. We could do it through the
        // affine matrix, but that will misalign the animation sprites.
        RenderContext arrowRenderContext = new FixedResolutionRenderContext(RenderContext.Resolution * (1 / ArrowScale), verticalAlignment: VerticalAlignment.Top);
        ArrowLeft = new AnimatedObject(multiplayerTypeFrameAnimations, multiplayerTypeFrameAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 0,
            ObjPriority = 0,
            CurrentAnimation = 1,
            RenderContext = arrowRenderContext,
        };
        ArrowRight = new AnimatedObject(multiplayerTypeFrameAnimations, multiplayerTypeFrameAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 0,
            ObjPriority = 0,
            CurrentAnimation = 0,
            RenderContext = arrowRenderContext,
        };

        ScrollBar = new SpriteTextureObject
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(352, ScrollBarBaseY),
            Texture = scrollBarTexture,
            RenderContext = RenderContext,
        };

        ScrollBarThumb = new SpriteTextureObject
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(357, ScrollBarThumbBaseY),
            Texture = scrollBarThumbTexture,
            RenderContext = RenderContext,
        };

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
            return;

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

                OptionsMenuOption option = Options[SelectedOption];
                option.Reset();

                CursorClick();

                // Start arrow animations on frame 4 since it looks nicer
                ArrowLeft.CurrentFrame = 4;
                ArrowRight.CurrentFrame = 4;
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
            if (JoyPad.IsButtonJustPressed(GbaInput.A))
            {
                IsEditingOption = false;

                OptionsMenuOption option = Options[SelectedOption];
                option.Apply();

                CursorClick();
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.B))
            {
                IsEditingOption = false;

                OptionsMenuOption option = Options[SelectedOption];
                option.Reset();

                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);
            }
            else
            {
                OptionsMenuOption option = Options[SelectedOption];
                option.Step();
            }
        }

        // End click animation
        if (Cursor.CurrentAnimation == 16 && Cursor.EndOfAnimation)
            Cursor.CurrentAnimation = 0;

        ManageCursor();
        ManageTabsCursor();
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

            TabHeaders.ScreenPos = TabHeaders.ScreenPos with { Y = TabHeadersBaseY - TabHeadersOffsetY };
            
            foreach (SpriteFontTextObject tabHeaderText in TabHeaderTexts)
                tabHeaderText.ScreenPos = tabHeaderText.ScreenPos with { Y = TabHeaderTextsBaseY - TabHeadersOffsetY };
            
            TabsCursor.ScreenPos = TabsCursor.ScreenPos with { Y = TabsCursorBaseY - TabHeadersOffsetY };
            InfoTextBox.ScreenPos = InfoTextBox.ScreenPos with { Y = InfoTextBoxBaseY - OffsetY };
            
            float height = FontManager.GetFontHeight(FontSize.Font32) * InfoTextScale;
            for (int i = 0; i < InfoTextLines.Length; i++)
                InfoTextLines[i].ScreenPos = InfoTextLines[i].ScreenPos with { Y = InfoTextLinesBaseY + height * i - OffsetY };

            ScrollBar.ScreenPos = ScrollBar.ScreenPos with { Y = ScrollBarBaseY - OffsetY };

            float scrollY = MathHelper.Lerp(0, ScrollBarLength, ScrollOffset / (float)MaxScrollOffset);
            ScrollBarThumb.ScreenPos = ScrollBarThumb.ScreenPos with { Y = ScrollBarThumbBaseY + scrollY - OffsetY };

            // Draw
            animationPlayer.Play(Canvas);
            animationPlayer.Play(Cursor);
            foreach (OptionsMenuOption option in Options.Skip(ScrollOffset).Take(MaxOptions))
                option.Draw(animationPlayer);

            animationPlayer.Play(TabHeaders);

            foreach (SpriteFontTextObject tabHeaderText in TabHeaderTexts)
                animationPlayer.Play(tabHeaderText);

            animationPlayer.Play(TabsCursor);

            animationPlayer.Play(InfoTextBox);
            foreach (SpriteTextObject infoTextLine in InfoTextLines)
                animationPlayer.Play(infoTextLine);

            if (IsEditingOption)
            {
                OptionsMenuOption option = Options[SelectedOption];

                // Set the arrow positions
                ArrowLeft.ScreenPos = option.ArrowLeftPosition * (1 / ArrowScale);
                ArrowRight.ScreenPos = option.ArrowRightPosition * (1 / ArrowScale);

                animationPlayer.Play(ArrowLeft);
                animationPlayer.Play(ArrowRight);
            }

            animationPlayer.Play(ScrollBar);
            if (HasScrollableContent)
                animationPlayer.Play(ScrollBarThumb);
        }
    }
}