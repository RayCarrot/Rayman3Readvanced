using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public class TimeAttackLevelMenuOption : TextMenuOption
{
    public TimeAttackLevelMenuOption(MapId mapId, float scale = 1f) : base(GetLevelName(mapId), scale) { }

    public SpriteFontTextObject TimeText { get; set; }

    private static string GetLevelName(MapId map)
    {
        int textId = GameInfo.Levels[(int)map].NameTextId;
        string name = Localization.GetText(TextBankId.LevelNames, textId)[0];
        return name.ToUpperInvariant().Replace('’', '\''); // TODO: Improve (the ’ glyph is not in the font!)
    }

    public override void Init(int bgPriority, RenderContext renderContext, int index)
    {
        base.Init(bgPriority, renderContext, index);

        TimeText = new SpriteFontTextObject
        {
            BgPriority = bgPriority,
            ObjPriority = 0,
            RenderContext = renderContext,
            // TODO: This is just dummy data for now
            Text = $"{ Random.GetNumber(4):00}:{Random.GetNumber(60):00}:{Random.GetNumber(60):00}",
            AffineMatrix = new AffineMatrix(0, new Vector2(Scale), false, false),
            Font = ReadvancedFonts.MenuYellow,
        };
    }

    public override void SetPosition(Vector2 position)
    {
        base.SetPosition(position);
        TimeText.ScreenPos = TextObject.ScreenPos + new Vector2(220, 0);
    }

    public override void ChangeIsSelected(bool isSelected)
    {
        base.ChangeIsSelected(isSelected);
        TimeText.Font = isSelected ? ReadvancedFonts.MenuWhite : ReadvancedFonts.MenuYellow;
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        base.Draw(animationPlayer);
        animationPlayer.Play(TimeText);
    }
}