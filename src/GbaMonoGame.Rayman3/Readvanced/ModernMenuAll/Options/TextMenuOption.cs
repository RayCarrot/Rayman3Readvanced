using GbaMonoGame.AnimEngine;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

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

    public override void Init(int bgPriority, RenderContext renderContext, Vector2 position, int index)
    {
        TextObject = new SpriteFontTextObject()
        {
            BgPriority = bgPriority,
            ObjPriority = 0,
            ScreenPos = position + new Vector2(0, 13 * Scale),
            RenderContext = renderContext,
            AffineMatrix = new AffineMatrix(0, new Vector2(Scale), false, false),
            Text = Text,
            Font = ReadvancedFonts.MenuYellow,
        };
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