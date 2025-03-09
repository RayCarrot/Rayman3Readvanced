using GbaMonoGame.AnimEngine;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

public class OptionsMenuOption : MenuOption
{
    public OptionsMenuOption(string text, string infoText)
    {
        Text = text;
        InfoText = infoText;
    }

    private const float TextScale = 2 / 3f;
    private const float ValueTextScale = 1 / 3f;
    private const float ValueTextXPosition = 200;
    private const float ValueTextPadding = 5;

    public string Text { get; }
    public string InfoText { get; }

    public SpriteFontTextObject TextObject { get; set; }
    public SpriteFontTextObject ValueTextObject { get; set; }
    
    public Vector2 ArrowLeftPosition { get; set; }
    public Vector2 ArrowRightPosition { get; set; }

    public override void Init(ModernMenuAll menu, RenderContext renderContext, Vector2 position, int index)
    {
        TextObject = new SpriteFontTextObject()
        {
            BgPriority = 3,
            ObjPriority = 0,
            ScreenPos = position + new Vector2(0, 13 * TextScale),
            RenderContext = renderContext,
            AffineMatrix = new AffineMatrix(0, new Vector2(TextScale), false, false),
            Text = Text,
            Font = ReadvancedFonts.MenuYellow,
        };

        ValueTextObject = new SpriteFontTextObject()
        {
            BgPriority = 3,
            ObjPriority = 0,
            ScreenPos = position + new Vector2(ValueTextXPosition, 13 * ValueTextScale),
            RenderContext = renderContext,
            AffineMatrix = new AffineMatrix(0, new Vector2(ValueTextScale), false, false),
            Text = index % 2 == 0 ? "ON" : "OFF", // TODO: Implement
            Font = ReadvancedFonts.MenuYellow,
        };

        float valueTextWidth = ValueTextObject.Font.GetWidth(ValueTextObject.Text) * ValueTextScale;
        ArrowLeftPosition = position + new Vector2(ValueTextXPosition - ValueTextPadding, 6);
        ArrowRightPosition = position + new Vector2(ValueTextXPosition + valueTextWidth + ValueTextPadding, 7);
    }

    public override void ChangeIsSelected(bool isSelected)
    {
        TextObject.Font = isSelected ? ReadvancedFonts.MenuWhite : ReadvancedFonts.MenuYellow;
        ValueTextObject.Font = isSelected ? ReadvancedFonts.MenuWhite : ReadvancedFonts.MenuYellow;
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        animationPlayer.Play(TextObject);
        animationPlayer.Play(ValueTextObject);
    }
}