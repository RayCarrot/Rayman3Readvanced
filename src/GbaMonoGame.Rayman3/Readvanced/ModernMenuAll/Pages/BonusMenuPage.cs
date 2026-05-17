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
        // Get achievements
        Rayman3.Achievements.GetTotalEarnedAchievements(out int earnedAchievements, out int totalAchievements);

        // Get time attack medals
        Rayman3.TimeAttack.GetTotalEarnedMedals(
            out int earnedBronze, out int earnedSilver, out int earnedGold, 
            out int totalBronze, out int totalSilver, out int totalGold);

        // Add menu options
        AddOption(new BonusActionMenuOption(
            text: "ACHIEVEMENTS", 
            collections:
            [
                new BonusActionMenuOption.Collection(Assets.Achievements.AchievementsIcon, $"{earnedAchievements}/{totalAchievements}"),
            ], 
            action: () =>
            {
                Menu.ChangePage(new AchievementsMenuPage(Menu), NewPageMode.Next);
            }));
        AddOption(new BonusActionMenuOption(
            text: "TIME ATTACK",
            collections:
            [
                new BonusActionMenuOption.Collection(Assets.TimeAttack.BronzeStarSmall, $"{earnedBronze}/{totalBronze}"),
                new BonusActionMenuOption.Collection(Assets.TimeAttack.SilverStarSmall, $"{earnedSilver}/{totalSilver}"),
                new BonusActionMenuOption.Collection(Assets.TimeAttack.GoldStarSmall, $"{earnedGold}/{totalGold}"),
            ], 
            action: () =>
            {
                Menu.ChangePage(new TimeAttackMenuPage(Menu), NewPageMode.Next);
            }));
        AddOption(new ActionMenuOption("ORIGINAL MENU", () =>
        {
            CursorClick(() =>
            {
                FadeOut(2, () =>
                {
                    Engine.FrameMngr.SetNextFrame(new MenuAll(InitialMenuPage.GameMode));
                });
            });
        }));
    }

    protected override void Step_Active()
    {
        if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuUp))
        {
            SetSelectedOption(SelectedOption - 1);
        }
        else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuDown))
        {
            SetSelectedOption(SelectedOption + 1);
        }
        else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuConfirm))
        {
            if (Options[SelectedOption] is ActionMenuOption action)
                action.Invoke();
        }
        else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuBack))
        {
            Menu.ChangePage(new GameModeMenuPage(Menu), NewPageMode.Back);
        }
    }

    protected override void Draw(AnimationPlayer animationPlayer)
    {
        DrawOptions(animationPlayer);
    }
}