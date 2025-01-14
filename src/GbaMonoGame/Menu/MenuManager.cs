using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace GbaMonoGame;

// TODO: Handle wrapping when multiple columns (see display page on N-Gage resolution)
// TODO: Tooltip when having button selected - appears at bottom of screen as text
// TODO: Implement scrolling if too tall
public class MenuManager
{
    #region Events

    public event EventHandler Closed;

    #endregion

    #region Private Properties

    private const int LineHeight = 40;
    private const float TransitionTextStep = 1 / 8f;

    private MenuRenderContext RenderContext { get; } = new();
    private List<Sprite> Sprites { get; } = new();
    private Vector2 Margin { get; } = new(250, 80);
    private Color DisabledColor { get; } = new(0.4f, 0.4f, 0.4f);
    private Color Color { get; } = Color.White;
    private Color DisabledHighlightColor { get; } = new(66, 54, 14);
    private Color HighlightColor { get; } = new(227, 175, 11);

    private Menu CurrentMenu { get; set; }
    private MenuState NextMenuState { get; set; }
    private bool NewMenu { get; set; }

    private Box FullRenderBox { get; set; }
    private Box[] ColumnRenderBoxes { get; set; }
    private Vector2 Position { get; set; }
    private int ColumnsCount => ColumnRenderBoxes.Length;
    private int CurrentColumnIndex { get; set; }
    public float NextLineY { get; set; }
    private int SelectableElementsCount { get; set; }
    private int CurrentSelectionIndex { get; set; }
    private HorizontalAlignment DefaultHorizontalAlignment { get; set; }

    private float TransitionValue { get; set; }

    private byte TransitionTextOutDelay { get; set; }
    private float TransitionTextValue { get; set; } = 1;

    private Stack<MenuState> MenuStack { get; } = new();

    #endregion

    #region Public Properties

    public bool IsTransitioningIn { get; private set; }
    public bool IsTransitioningOut { get; private set; }

    public bool IsTransitioningTextOut { get; private set; }
    public bool IsTransitioningTextIn { get; private set; }
    public bool IsTransitioningText => IsTransitioningTextOut || IsTransitioningTextIn;

    #endregion

    #region Private Methods

    private void OnClosed()
    {
        Closed?.Invoke(this, EventArgs.Empty);
    }

    private void DrawText(string text, ref Vector2 position, HorizontalAlignment horizontalAlignment, FontSize fontSize, Color color, bool animate) => 
        DrawText(FontManager.GetTextBytes(text), ref position, horizontalAlignment, fontSize, color, animate);

    private void DrawText(byte[] text, ref Vector2 position, HorizontalAlignment horizontalAlignment, FontSize fontSize, Color color, bool animate)
    {
        if (horizontalAlignment == HorizontalAlignment.Center)
        {
            int width = FontManager.GetStringWidth(fontSize, text);
            position -= new Vector2(width / 2f, 0);
        }
        else if (horizontalAlignment == HorizontalAlignment.Right)
        {
            int width = FontManager.GetStringWidth(fontSize, text);
            position -= new Vector2(width, 0);
        }

        AffineMatrix? matrix = animate ? new AffineMatrix(0, new Vector2(1, TransitionTextValue)) : null;
        foreach (byte b in text)
        {
            Sprite sprite = FontManager.GetCharacterSprite(b, fontSize, ref position, 0, matrix, null, color, RenderContext);
            Sprites.Add(sprite);
        }
    }

    private void DrawWrappedText(string text, ref Vector2 position, HorizontalAlignment horizontalAlignment, FontSize fontSize, Color color)
    {
        Box renderBox = ColumnRenderBoxes[CurrentColumnIndex];

        float posX = position.X;

        float availableWidth = horizontalAlignment switch
        {
            HorizontalAlignment.Left => renderBox.MaxX - position.X,
            HorizontalAlignment.Right => position.X - renderBox.MinX,
            HorizontalAlignment.Center => (renderBox.MaxX - position.X < position.X - renderBox.MinX ? renderBox.MaxX - position.X : position.X - renderBox.MinX) * 2,
            _ => throw new ArgumentOutOfRangeException(nameof(horizontalAlignment), horizontalAlignment, null)
        };

        byte[][] lines = FontManager.GetWrappedStringLines(fontSize, text, availableWidth);
        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            DrawText(lines[lineIndex], ref position, horizontalAlignment, fontSize, color, true);

            if (lineIndex < lines.Length - 1)
                position = new Vector2(posX, position.Y + FontManager.GetFontHeight(fontSize));
        }
    }

    private Vector2 GetPosition()
    {
        return DefaultHorizontalAlignment switch
        {
            HorizontalAlignment.Left => new Vector2(ColumnRenderBoxes[CurrentColumnIndex].MinX, Position.Y),
            HorizontalAlignment.Center => new Vector2(ColumnRenderBoxes[CurrentColumnIndex].Center.X, Position.Y),
            HorizontalAlignment.Right => new Vector2(ColumnRenderBoxes[CurrentColumnIndex].MaxX, Position.Y),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void NextLine(Vector2 pos, int lineHeight = LineHeight)
    {
        CurrentColumnIndex++;

        NextLineY = Math.Max(NextLineY, pos.Y + lineHeight);

        if (CurrentColumnIndex >= ColumnsCount)
        {
            CurrentColumnIndex = 0;
            Position = new Vector2(ColumnRenderBoxes[CurrentColumnIndex].MinX, NextLineY);
            NextLineY = 0;
        }
        else
        {
            Position = new Vector2(ColumnRenderBoxes[CurrentColumnIndex].MinX, Position.Y);
        }
    }

    #endregion

    #region Public Methods

    public void Update()
    {
        FullRenderBox = new Box(Margin.X, Margin.Y, RenderContext.Resolution.X - Margin.X, RenderContext.Resolution.Y - Margin.Y);
        Position = new Vector2(FullRenderBox.MinX, FullRenderBox.MinY);
        CurrentColumnIndex = 0;

        Sprites.Clear();
        SelectableElementsCount = 0;

        if (IsTransitioningTextOut)
        {
            TransitionTextValue -= TransitionTextStep;

            if (TransitionTextValue <= 0)
            {
                TransitionTextValue = 0;
                IsTransitioningTextOut = false;
                CurrentMenu.OnExit();
                CurrentMenu = NextMenuState?.Menu;

                if (CurrentMenu != null)
                {
                    IsTransitioningTextIn = NextMenuState != null;
                    TransitionTextOutDelay = 2;
                    NewMenu = true;
                }
            }
        }
        else if (IsTransitioningTextIn)
        {
            if (TransitionTextOutDelay == 0)
            {
                TransitionTextValue += TransitionTextStep;

                if (TransitionTextValue >= 1)
                {
                    TransitionTextValue = 1;
                    IsTransitioningTextOut = false;
                    IsTransitioningTextIn = false;
                }
            }
            else
            {
                TransitionTextOutDelay--;
            }
        }

        if (NewMenu)
        {
            CurrentSelectionIndex = NextMenuState?.SelectedIndex ?? 0;
            NewMenu = false;
        }

        CurrentMenu?.Update(this);

        if (!IsTransitioningText)
        {
            // Run standard controls
            if (InputManager.IsButtonJustPressed(Input.Menu_Down))
            {
                if (CurrentSelectionIndex >= SelectableElementsCount - 1)
                    CurrentSelectionIndex = 0;
                else
                    CurrentSelectionIndex++;
            }
            else if (InputManager.IsButtonJustPressed(Input.Menu_Up))
            {
                if (CurrentSelectionIndex <= 0)
                    CurrentSelectionIndex = SelectableElementsCount - 1;
                else
                    CurrentSelectionIndex--;
            }
            else if (InputManager.IsButtonJustPressed(Input.Menu_Back))
            {
                GoBack();
            }
        }

        // Draw engine version in the corner
        Vector2 versionPos = new(RenderContext.Resolution.X - 12, RenderContext.Resolution.Y - 37);
        DrawText(
            text: $"Version {Engine.Version.ToString(3)}",
            position: ref versionPos,
            horizontalAlignment: HorizontalAlignment.Right,
            fontSize: FontSize.Font32,
            color: Color,
            animate: false);
    }

    public void GoBack()
    {
        if (IsTransitioningIn || IsTransitioningOut || IsTransitioningText)
            return;

        if (MenuStack.Count == 0)
        {
            Close();
        }
        else
        {
            NextMenuState = MenuStack.Pop();
            IsTransitioningTextOut = true;
            IsTransitioningTextIn = true;
            TransitionTextValue = 1;
        }
    }

    public void ChangeMenu(Menu newMenu)
    {
        if (IsTransitioningIn || IsTransitioningOut || IsTransitioningText)
            return;

        NextMenuState = new MenuState(newMenu, 0);
        IsTransitioningTextOut = true;
        IsTransitioningTextIn = true;
        TransitionTextValue = 1;
        MenuStack.Push(new MenuState(CurrentMenu, CurrentSelectionIndex));
    }

    public void SetColumns(params float[] columns)
    {
        float total = columns.Sum();

        // Replace with widths
        for (int i = 0; i < columns.Length; i++)
        {
            columns[i] = FullRenderBox.Width * (columns[i] / total);
        }

        ColumnRenderBoxes = new Box[columns.Length];
        float xPos = FullRenderBox.MinX;
        for (int i = 0; i < columns.Length; i++)
        {
            float width = columns[i];
            ColumnRenderBoxes[i] = new Box(xPos, FullRenderBox.MinY, xPos + width, FullRenderBox.MaxY);
            xPos += width;
        }
        CurrentColumnIndex = 0;
    }

    public void SetHorizontalAlignment(HorizontalAlignment horizontalAlignment)
    {
        DefaultHorizontalAlignment = horizontalAlignment;
    }

    public void Text(string text)
    {
        Vector2 pos = GetPosition();

        DrawWrappedText(
            text: text,
            position: ref pos,
            horizontalAlignment: DefaultHorizontalAlignment,
            fontSize: FontSize.Font32,
            color: Color);

        NextLine(pos);
    }

    public void Spacing()
    {
        NextLine(GetPosition());
    }

    public void SmallSpacing()
    {
        NextLine(GetPosition(), LineHeight / 4);
    }

    public bool Button(string text, bool isEnabled = true)
    {
        Vector2 pos = GetPosition();
        int index = isEnabled ? SelectableElementsCount : -1;
        Color color = Color;

        if (!isEnabled)
            color = DisabledColor;
        else if (CurrentSelectionIndex == index)
            color = HighlightColor;

        DrawWrappedText(
            text: text,
            position: ref pos,
            horizontalAlignment: DefaultHorizontalAlignment,
            fontSize: FontSize.Font32,
            color: color);

        if (isEnabled)
            SelectableElementsCount++;
        NextLine(pos);

        return CurrentSelectionIndex == index && !IsTransitioningText && InputManager.IsButtonJustPressed(Input.Menu_Confirm);
    }

    public int Selection(string[] options, int selectedOption)
    {
        Vector2 pos = GetPosition();
        int index = SelectableElementsCount;
        string text = options[selectedOption];

        if (CurrentSelectionIndex == index)
        {
            if (InputManager.IsButtonJustPressed(Input.Menu_Left))
            {
                selectedOption--;
                if (selectedOption < 0)
                    selectedOption = 0;
            }
            else if (InputManager.IsButtonJustPressed(Input.Menu_Right))
            {
                selectedOption++;
                if (selectedOption > options.Length - 1)
                    selectedOption = options.Length - 1;
            }
        }

        if (CurrentSelectionIndex == index)
        {
            DrawWrappedText(
                text: "< ",
                position: ref pos,
                horizontalAlignment: DefaultHorizontalAlignment,
                fontSize: FontSize.Font32,
                color: selectedOption > 0 ? HighlightColor : DisabledHighlightColor);
        }

        DrawWrappedText(
            text: text,
            position: ref pos,
            horizontalAlignment: DefaultHorizontalAlignment,
            fontSize: FontSize.Font32,
            color: CurrentSelectionIndex == index ? HighlightColor : Color);

        if (CurrentSelectionIndex == index)
        {
            DrawWrappedText(
                text: " >",
                position: ref pos,
                horizontalAlignment: DefaultHorizontalAlignment,
                fontSize: FontSize.Font32,
                color: selectedOption < options.Length - 1 ? HighlightColor : DisabledHighlightColor);
        }

        SelectableElementsCount++;
        NextLine(pos);

        return selectedOption;
    }

    public bool IsElementSelected() => CurrentSelectionIndex == SelectableElementsCount - 1;

    public void Open(Menu menu)
    {
        if (IsTransitioningIn || IsTransitioningOut || IsTransitioningText)
            return;

        CurrentMenu = menu;
        NewMenu = true;

        IsTransitioningTextOut = false;
        IsTransitioningTextIn = true;
        TransitionTextOutDelay = 2;
        TransitionTextValue = 0;
        IsTransitioningOut = false;
        IsTransitioningIn = true;
        TransitionValue = 0;
        MenuStack.Clear();
    }

    public void Close()
    {
        if (IsTransitioningIn || IsTransitioningOut || IsTransitioningText)
            return;

        NextMenuState = null;
        IsTransitioningTextOut = true;
        IsTransitioningTextIn = false;
        TransitionTextValue = 1;
        IsTransitioningOut = true;
        IsTransitioningIn = false;
        TransitionValue = 1;
    }

    public void Draw(GfxRenderer renderer)
    {
        if (IsTransitioningIn)
        {
            TransitionValue += 1 / 10f;

            if (TransitionValue >= 1)
            {
                TransitionValue = 1;
                IsTransitioningIn = false;
            }
        }
        else if (IsTransitioningOut)
        {
            TransitionValue -= 1 / 10f;

            if (TransitionValue <= 0)
            {
                TransitionValue = 0;
                IsTransitioningOut = false;
                OnClosed();
                return;
            }
        }

        // Fade out the game
        renderer.BeginRender(new RenderOptions(Engine.GameRenderContext));
        renderer.DrawFilledRectangle(Vector2.Zero, Engine.GameRenderContext.Resolution, Color.Black * MathHelper.Lerp(0.0f, 0.7f, TransitionValue));

        // Draw the sprites
        foreach (Sprite sprite in Sprites)
            sprite.Draw(renderer, Color.White);
    }

    #endregion

    #region Data Types

    public enum HorizontalAlignment
    {
        Left,
        Center,
        Right,
    }

    private class MenuRenderContext : RenderContext
    {
        // Scale by 5 to fit more text on screen
        protected override Vector2 GetResolution()
        {
            float ratio = Engine.GameViewPort.GameResolution.X / Engine.GameViewPort.GameResolution.Y;

            Vector2 res;
            if (ratio > 1)
                res = new Vector2(ratio * Engine.GameViewPort.RequestedGameResolution.Y, Engine.GameViewPort.RequestedGameResolution.Y);
            else
                res = new Vector2(Engine.GameViewPort.RequestedGameResolution.X, 1 / ratio * Engine.GameViewPort.RequestedGameResolution.X);

            return res * 5f;
        }
    }

    private class MenuState
    {
        public MenuState(Menu menu, int selectedIndex)
        {
            Menu = menu;
            SelectedIndex = selectedIndex;
        }

        public Menu Menu { get; }
        public int SelectedIndex { get; }
    }

    #endregion
}