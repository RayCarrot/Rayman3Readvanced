﻿using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

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
        AddOption(new BonusActionMenuOption(
            text: "ACHIEVEMENTS", 
            collections:
            [
                new BonusActionMenuOption.Collection(Assets.SaveSlotTimeTexture, "10/55"),
            ], 
            action: () =>
            {
                // TODO: Implement
            }));
        AddOption(new BonusActionMenuOption(
            text: "TIME ATTACK",
            collections:
            [
                new BonusActionMenuOption.Collection(Assets.BronzeStarSmallTexture, "25/28"),
                new BonusActionMenuOption.Collection(Assets.SilverStarSmallTexture, "15/28"),
                new BonusActionMenuOption.Collection(Assets.GoldStarSmallTexture, "2/28"),
            ], 
            action: () =>
            {
                Menu.ChangePage(new TimeAttackMenuPage(Menu), NewPageMode.Next);
            }));
        AddOption(new TextMenuOption("LEVEL EDITOR"));
        AddOption(new TextMenuOption("SAVE TRANSFER"));
        AddOption(new TextMenuOption("ORIGINAL MENU"));
    }

    protected override void Step_Active()
    {
        if (JoyPad.IsButtonJustPressed(GbaInput.Up))
        {
            SetSelectedOption(SelectedOption - 1);
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.Down))
        {
            SetSelectedOption(SelectedOption + 1);
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.A))
        {
            if (Options[SelectedOption] is ActionMenuOption action)
                action.Invoke();
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