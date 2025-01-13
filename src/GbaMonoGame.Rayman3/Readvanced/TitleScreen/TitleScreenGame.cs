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

        SetOptions(
        [
            "CONTINUE",
            "START",
            "OPTIONS"
        ]);
        SelectedIndex = -1;
    }

    private int _selectedIndex;

    public Platform Platform { get; }
    public Cursor Cursor { get; }
    public Vector2 Position { get; }
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

    private void SetOptions(string[] options)
    {
        OptionTexts = new SpriteFontTextObject[options.Length];
        for (int i = 0; i < options.Length; i++)
        {
            OptionTexts[i] = new SpriteFontTextObject
            {
                BgPriority = 0,
                ObjPriority = 0,
                ScreenPos = Position + new Vector2(0, 16) * i,
                Text = options[i],
                Font = ReadvancedFonts.MenuYellow,
            };
        }

        if (SelectedIndex != -1)
            SelectedIndex = 0;
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < OptionTexts.Length; i++)
            OptionTexts[i].Font = SelectedIndex == i ? ReadvancedFonts.MenuWhite : ReadvancedFonts.MenuYellow;
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
        }
    }

    public void Draw(AnimationPlayer animationPlayer)
    {
        foreach (SpriteFontTextObject optionText in OptionTexts)
            animationPlayer.Play(optionText);
    }
}