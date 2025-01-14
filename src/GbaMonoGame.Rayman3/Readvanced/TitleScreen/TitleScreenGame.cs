using System;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public class TitleScreenGame
{
    public TitleScreenGame(Platform platform, Cursor cursor, Vector2 position)
    {
        Platform = platform;
        Cursor = cursor;
        Position = position;

        _selectedIndex = -1;
    }

    private int _selectedIndex;

    public Platform Platform { get; }
    public Cursor Cursor { get; }
    public Vector2 Position { get; }

    public Option[] Options { get; set; }
    public SpriteFontTextObject[] OptionTexts { get; set; }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            _selectedIndex = value;
            UpdateSelection();

            if (value != -1)
                Cursor.SetTargetPosition(OptionTexts[value].ScreenPos);
        }
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < OptionTexts.Length; i++)
            OptionTexts[i].Font = SelectedIndex == i ? ReadvancedFonts.MenuWhite : ReadvancedFonts.MenuYellow;
    }

    public void SetOptions(Option[] options)
    {
        Options = options;

        // Center for first option
        float width = ReadvancedFonts.MenuYellow.GetWidth(options[0].Text);
        Vector2 basePos = Position - new Vector2(width / 2, 0);

        OptionTexts = new SpriteFontTextObject[options.Length];
        for (int i = 0; i < options.Length; i++)
        {
            OptionTexts[i] = new SpriteFontTextObject
            {
                BgPriority = 0,
                ObjPriority = 0,
                ScreenPos = basePos + new Vector2(0, 16) * i,
                Text = options[i].Text,
                Font = ReadvancedFonts.MenuYellow,
            };
        }

        if (SelectedIndex != -1)
            SelectedIndex = 0;
    }

    public void Step()
    {
        if (SelectedIndex != -1)
        {
            if (JoyPad.IsButtonJustPressed(GbaInput.Down))
            {
                if (SelectedIndex == OptionTexts.Length - 1)
                    SelectedIndex = 0;
                else
                    SelectedIndex++;
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.Up))
            {
                if (SelectedIndex == 0)
                    SelectedIndex = OptionTexts.Length - 1;
                else
                    SelectedIndex--;
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.A))
            {
                Options[SelectedIndex].Action(this);
            }
        }
    }

    public void Draw(AnimationPlayer animationPlayer)
    {
        foreach (SpriteFontTextObject optionText in OptionTexts)
            animationPlayer.Play(optionText);
    }

    public class Option
    {
        public Option(string text, Action<TitleScreenGame> action)
        {
            Text = text;
            Action = action;
        }

        public string Text { get; }
        public Action<TitleScreenGame> Action { get; }
    }
}