using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Rayman3.J2me;

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

        // Create icons for J2ME collections
        AnimatedObjectResource propsAnimations = Rom.Loader.ReadResource<AnimatedObjectResource>(Rayman3DefinedResource.MenuPropAnimations);
        AnimatedObject lumIcon = new(propsAnimations, propsAnimations.IsDynamic)
        {
            IsFramed = true,
            CurrentAnimation = 13,
        };
        AnimatedObject cageIcon = new(propsAnimations, propsAnimations.IsDynamic)
        {
            IsFramed = true,
            CurrentAnimation = 11,
        };

        // Hide the "x" part of the animations
        lumIcon.DeactivateChannel(0);
        cageIcon.DeactivateChannel(0);

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
        AddOption(new BonusActionMenuOption(
            text: "MOBILE (J2ME)", 
            collections:
            [
                // TODO: Implement showing progress
                new BonusActionMenuOption.Collection(lumIcon, new Vector2(1, 1), "0/175"),
                new BonusActionMenuOption.Collection(cageIcon, new Vector2(0, -1), "0/12"),
            ],
            action: () =>
            {
                CursorClick(() =>
                {
                    FadeOut(2, () =>
                    {
                        Engine.FrameMngr.SetNextFrame(new GameMidlet());
                    });
                });
            }));
        AddOption(new ActionMenuOption("ORIGINAL MENU", () =>
        {
            CursorClick(() =>
            {
                FadeOut(2, () =>
                {
                    Menu.StopMusicOnExit = false;
                    Engine.FrameMngr.SetNextFrame(new MenuAll(InitialMenuPage.GameMode));
                });
            });
        }));
    }

    protected override void Step_Active()
    {
        if (Engine.JoyPad.IsButtonJustPressed(Rayman3Input.MenuUp))
        {
            SetSelectedOption(SelectedOption - 1);
        }
        else if (Engine.JoyPad.IsButtonJustPressed(Rayman3Input.MenuDown))
        {
            SetSelectedOption(SelectedOption + 1);
        }
        else if (Engine.JoyPad.IsButtonJustPressed(Rayman3Input.MenuConfirm))
        {
            if (Options[SelectedOption] is ActionMenuOption action)
                action.Invoke();
        }
        else if (Engine.JoyPad.IsButtonJustPressed(Rayman3Input.MenuBack))
        {
            Menu.ChangePage(new GameModeMenuPage(Menu), NewPageMode.Back);
        }
    }

    protected override void Draw(AnimationPlayer animationPlayer)
    {
        DrawOptions(animationPlayer);
    }
}