using GbaMonoGame.AnimEngine;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

public class TextMenuOption : MenuOption
{
    public TextMenuOption(string text)
    {
        Text = text;
    }

    public string Text { get; }
    public SpriteFontTextObject TextObject { get; set; }

    public override void Init(ModernMenuAll menu, RenderContext renderContext, Vector2 position, int index)
    {
        TextObject = new SpriteFontTextObject()
        {
            BgPriority = 3,
            ObjPriority = 0,
            ScreenPos = position + new Vector2(0, 13),
            RenderContext = renderContext,
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