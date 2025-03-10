﻿using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using Action = System.Action;

namespace GbaMonoGame.Rayman3.Readvanced;

public class TitleScreenOptionsList
{
    public TitleScreenOptionsList(RenderContext renderContext, Cursor cursor, Vector2 position)
    {
        RenderContext = renderContext;
        Cursor = cursor;
        Position = position;

        _selectedIndex = -1;
    }

    private int _selectedIndex;

    public RenderContext RenderContext { get; }
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

    private void SetText(int index)
    {
        Option option = Options[index];
        OptionTexts[index].Text = option.Text;
    }

    public void SetOptions(Option[] options)
    {
        Options = options.Where(x => x.IsAvailable).ToArray();

        // Center for first option
        float width = ReadvancedFonts.MenuYellow.GetWidth(Options[0].Text);
        Vector2 basePos = Position - new Vector2(width / 2, 0);

        OptionTexts = new SpriteFontTextObject[Options.Length];
        for (int i = 0; i < Options.Length; i++)
        {
            OptionTexts[i] = new SpriteFontTextObject
            {
                BgPriority = 0,
                ObjPriority = 0,
                ScreenPos = basePos + new Vector2(0, 16) * i,
                Font = ReadvancedFonts.MenuYellow,
                RenderContext = RenderContext,
            };

            SetText(i);
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
                Options[SelectedIndex].Action();
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
        public Option(string text, Action action)
        {
            Text = text;
            IsAvailable = true;
            Action = action;
        }

        public Option(string text, bool isAvailable, Action action)
        {
            Text = text;
            IsAvailable = isAvailable;
            Action = action;
        }

        public string Text { get; }
        public bool IsAvailable { get; }
        public Action Action { get; }
    }
}