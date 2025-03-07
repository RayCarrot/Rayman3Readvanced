using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3;

public class BonusMenuPage : MenuPage
{
    public BonusMenuPage(ModernMenuAll menu) : base(menu) { }

    public override bool UsesCursor => true;
    public override int BackgroundPalette => 2;
    public override int LineHeight => 18;

    protected override void Init()
    {
        // TODO: Add proper values and icons (icons are currently temporary)
        // Add menu options
        AddOption(new BonusTextMenuOption("ACHIEVEMENTS", 
        [
            new BonusTextMenuOption.Collection("SaveSlotTime", "10/55"),
        ]));
        AddOption(new BonusTextMenuOption("CHALLENGES",
        [
            new BonusTextMenuOption.Collection("SaveSlotTime", "3/10"),
        ]));
        AddOption(new BonusTextMenuOption("TIME ATTACK",
        [
            new BonusTextMenuOption.Collection("SaveSlotTime", "40/45"),
            new BonusTextMenuOption.Collection("SaveSlotTime", "25/45"),
            new BonusTextMenuOption.Collection("SaveSlotTime", "2/45"),
        ]));
        AddOption(new TextMenuOption("LEVEL EDITOR"));
        AddOption(new TextMenuOption("SAVE TRANSFER"));
        AddOption(new TextMenuOption("ORIGINAL MENU"));
    }

    protected override void Step_Active()
    {
        if (JoyPad.IsButtonJustPressed(GbaInput.Up))
        {
            ChangeSelectedOption(-1);
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.Down))
        {
            ChangeSelectedOption(1);
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.B))
        {
            Menu.ChangePage(new GameModeMenuPage(Menu), NewPageMode.Back);
        }
    }

    protected override void Draw(AnimationPlayer animationPlayer)
    {
        DrawOptions(animationPlayer);
    }
}