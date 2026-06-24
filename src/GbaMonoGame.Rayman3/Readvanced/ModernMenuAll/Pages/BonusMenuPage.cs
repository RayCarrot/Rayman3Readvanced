using System;
using System.IO;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Rayman3.J2me;
using Game = GbaMonoGame.Rayman3.J2me.Game;

namespace GbaMonoGame.Rayman3.Readvanced;

public class BonusMenuPage : MenuPage
{
    public BonusMenuPage(ModernMenuAll menu) : base(menu) { }

    public override bool UsesCursor => true;
    public override int BackgroundPalette => 2;
    public override int LineHeight => 18;

    private void GetJ2meProgress(out int lums, out int cages)
    {
        try
        {
            // Read the record flag
            bool recordFlag = false;
            using (RecordStore recordFlagRecordStore = RecordStore.openRecordStore(Game.RECORD_FLAG_SAVE_NAME, true))
            {
                byte[] data = recordFlagRecordStore.getRecord(1);
                if (data != null)
                    recordFlag = data[9] != 0;
            }

            // No progress exist, so return empty data
            if (!recordFlag)
            {
                lums = 0;
                cages = 0;
                return;
            }

            // Read the progress
            using (RecordStore saveRecordStore = RecordStore.openRecordStore(Game.GAME_SAVE_NAME, true))
            {
                byte[] data = saveRecordStore.getRecord(10);
                if (data != null)
                {
                    lums = data[0];
                    cages = data[2];
                }
                else
                {
                    lums = 0;
                    cages = 0;
                }
            }
        }
        catch (Exception ex)
        {
            lums = 0;
            cages = 0;

            Engine.Messages.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when reading the save.",
                header: "Error reading game save");
        }
    }

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

        // Get J2ME progress
        GetJ2meProgress(out int j2meLums, out int j2meCages);

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
                new BonusActionMenuOption.Collection(lumIcon, new Vector2(1, 1), $"{j2meLums}/{Game.TOTAL_LUMS}"),
                new BonusActionMenuOption.Collection(cageIcon, new Vector2(0, -1), $"{j2meCages}/{Game.TOTAL_CAGES}"),
            ],
            action: () =>
            {
                CursorClick(() =>
                {
                    Rayman3J2meVersion gameVersion = Rayman3J2meVersion.Rayman3_1_0_3_SonyEricssonS700_240x320;
                    J2meRom.GetGamePaths(gameVersion, out string gameDirectory, out string gameFileName, out _);

                    string romFilePath = Path.Combine(gameDirectory, gameFileName);

                    if (File.Exists(romFilePath))
                    {
                        FadeOut(2, () =>
                        {
                            Engine.FrameMngr.SetNextFrame(new GameMidlet());
                        });
                    }
                    else
                    {
                        string selectedFilePath = Engine.FileDialog.OpenFile("Select the Sony Ericsson S700 - 240x320 (1.0.3) ROM file", 
                            new FileDialogManager.FileFilter("jar", "Java Archive files"));

                        if (selectedFilePath != null)
                        {
                            bool isValid;
                            try
                            {
                                if (J2meRom.ValidateRom(selectedFilePath, gameVersion))
                                {
                                    isValid = true;
                                }
                                else
                                {
                                    isValid = false;
                                    Engine.Messages.EnqueueMessage(
                                        text: "The game ROM is not valid. Make sure it's the Sony Ericsson S700 - 240x320 (1.0.3) version.",
                                        header: "Invalid game ROM");
                                }
                            }
                            catch (Exception ex)
                            {
                                isValid = false;
                                Engine.Messages.EnqueueExceptionMessage(
                                    ex: ex,
                                    text: "An error occurred when validating the selected game ROM.",
                                    header: "Error validating game ROM");
                            }

                            if (isValid)
                            {
                                try
                                {
                                    // Copy the file
                                    Directory.CreateDirectory(gameDirectory);
                                    File.Copy(selectedFilePath, romFilePath);

                                    // Load the game
                                    FadeOut(2, () =>
                                    {
                                        Engine.FrameMngr.SetNextFrame(new GameMidlet());
                                    });
                                }
                                catch (Exception ex)
                                {
                                    Engine.Messages.EnqueueExceptionMessage(
                                        ex: ex,
                                        text: "An error occurred when copying the selected game ROM.",
                                        header: "Error copying game ROM");
                                }
                            }
                        }
                    }
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