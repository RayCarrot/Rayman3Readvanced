using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public class TextMenuOption : MenuOption
{
    public TextMenuOption(string text, float scale = 1)
    {
        Text = text;
        Scale = scale;
    }

    public string Text { get; }
    public float Scale { get; }
    public SpriteFontTextObject TextObject { get; set; }

    public override void Init(int bgPriority, RenderContext renderContext, int index)
    {
        TextObject = new SpriteFontTextObject()
        {
            BgPriority = bgPriority,
            ObjPriority = 0,
            RenderContext = renderContext,
            AffineMatrix = new AffineMatrix(0, new Vector2(Scale), false, false),
            Text = Text,
            Font = ReadvancedFonts.MenuYellow,
        };
    }

    public override void SetPosition(Vector2 position)
    {
        TextObject.ScreenPos = position + new Vector2(0, 13 * Scale);
    }

    public override void ChangeIsSelected(bool isSelected)
    {
        TextObject.Font = isSelected ? ReadvancedFonts.MenuWhite : ReadvancedFonts.MenuYellow;
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        animationPlayer.Play(TextObject);
    }
}