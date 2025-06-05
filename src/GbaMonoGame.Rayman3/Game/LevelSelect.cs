using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.TgxEngine;
using Action = System.Action;

namespace GbaMonoGame.Rayman3;

// Original name: Cheat
public class LevelSelect : Frame
{
    #region Constructor

    public LevelSelect()
    {
        SelectedWorldIndex = 0;
        SelectedLevelIndex = 0;
        SelectedMapId = MapId.WoodLight_M1;
    }

    #endregion

    #region Constant Fields

    private const float RowHeight = 16;

    #endregion

    #region Public Properties

    public Action CurrentStepAction { get; set; }

    public AnimationPlayer AnimationPlayer { get; set; }

    public SpriteTextObject Header { get; set; }
    public SpriteTextObject[] Rows { get; set; }
    public SpriteTextObject Cursor { get; set; }

    public int SelectedRow { get; set; }
    public int SelectedSaveSlotIndex { get; set; }
    public int SelectedLanguageIndex { get; set; }
    public int SelectedWorldIndex { get; set; }
    public int SelectedLevelIndex { get; set; }
    public MapId SelectedMapId { get; set; }
    public bool UnlockLums { get; set; }
    public bool UnlockCages { get; set; }

    public string[] WorldNames { get; } =
    [
        "World: World 1",
        "World: World 2",
        "World: World 3",
        "World: World 4",
        "World: Extras",
        "World: Story",
    ];

    public string[] World1LevelNames { get; } =
    [
        "WoodLight #1",            
        "WoodLight #2",
        "FairyGlade #1",
        "FairyGlade #2",
        "MarshAwakening #1",
        "Boss Machine",
        "Sanctuary of Big Tree #1",
        "Sanctuary of Big Tree #2",
    ];

    public string[] World2LevelNames { get; } =
    [
        "Missile sur pattes #1",
        "Echoing Caves #1",
        "Echoing Caves #2",
        "Caves of Bad Dreams #1",
        "Caves of Bad Dreams #2",
        "Boss Bad Dreams",
        "Menhir Hill #1",
        "Menhir Hill #2",
        "MarshAwakening #2",
    ];

    public string[] World3LevelNames { get; } =
    [
        "Sanctuary of Stone and Fire #1",
        "Sanctuary of Stone and Fire #2",
        "Sanctuary of Stone and Fire #3",
        "Beneath The Sanctuary #1",
        "Beneath The Sanctuary #2",
        "The Precipice #1",
        "The Precipice #2",
        "Boss Rock And Lava",
        "The Canopy #1",
        "The Canopy #2",
        "Sanctuary of Rock and Lava #1",
        "Sanctuary of Rock and Lava #2",
        "Sanctuary of Rock and Lava #3",
    ];

    public string[] World4LevelNames { get; } =
    [
        "Tomb of the Ancients #1",
        "Tomb of the Ancients #2",
        "Boss ScaleMan",
        "Iron Mountains #1",
        "Iron Mountains #2",
        "Missile sur pattes #2",
        "Pirate Ship #1",
        "Pirate Ship #2",
        "Boss Final",
        "Boss Final #2",
    ];

    public string[] WorldExtrasLevelNames { get; } =
    [
        "Bonus #1",
        "Bonus #2",
        "Bonus #3",
        "Bonus #4",
        "1000e lums",
        "Challenge Ly #1",
        "Challenge Ly #2",
        "Challenge Ly GCN",
        "Ly: Double Poing",
        "Ly: Lum Violet",
        "Ly: Wall Grab",
        "Ly: Brise Sol",
        "Ly: Lum Bleu",
        "Ly: Point Charge",
        "World #1",
        "World #2",
        "World #3",
        "World #4",
        "WorldMap",
    ];

    public string[] WorldStoryLevelNames { get; } =
    [
        "Act #1",
        "Act #2",
        "Act #3",
        "Act #4",
        "Act #5",
        "Act #6",
    ];

    #endregion

    #region Private Methods

    private void InitSelectSaveSlot()
    {
        Header.Text = "Select a savegame slot:";
        Rows[0].Text = "Slot #1";
        SelectedSaveSlotIndex = 0;
        SelectedRow = 0;
        CurrentStepAction = Step_SelectSaveSlot;
    }

    private void InitSelectLanguage()
    {
        Header.Text = "Select a language:";
        Rows[0].Text = "English";
        SelectedLanguageIndex = 0;
        CurrentStepAction = Step_SelectLanguage;
    }

    private void InitSelectStartingLevel()
    {
        SelectedWorldIndex = 0;
        SelectedLevelIndex = 0;
        UnlockLums = false;
        UnlockCages = false;
        SelectedRow = 0;
        Header.Text = "Select the starting level:";
        Rows[0].Text = "World :";
        Rows[1].Text = "Level :";
        Rows[2].Text = "Unlock Lums: true";
        Rows[3].Text = "Unlock Cages: true";
        SetMapText();
        CurrentStepAction = Step_SelectStartingLevel;
    }

    private void SetMapText()
    {
        Rows[0].Text = WorldNames[SelectedWorldIndex];

        string[] levelNames = SelectedWorldIndex switch
        {
            0 => World1LevelNames,
            1 => World2LevelNames,
            2 => World3LevelNames,
            3 => World4LevelNames,
            4 => WorldExtrasLevelNames,
            5 => WorldStoryLevelNames,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (SelectedLevelIndex == -1)
            SelectedLevelIndex = levelNames.Length - 1;
        else if (SelectedLevelIndex > levelNames.Length - 1)
            SelectedLevelIndex = 0;

        SelectedMapId = SelectedLevelIndex + SelectedWorldIndex switch
        {
            0 => MapId.WoodLight_M1,
            1 => MapId.MissileRace1,
            2 => MapId.SanctuaryOfStoneAndFire_M1,
            3 => MapId.TombOfTheAncients_M1,
            4 => MapId.Bonus1,
            5 => default,
            _ => throw new ArgumentOutOfRangeException()
        };

        Rows[1].Text = $"Level: {levelNames[SelectedLevelIndex]}";
        Rows[2].Text = UnlockLums 
            ? "Unlock Lums: true"
            : "Unlock Lums: false";
        Rows[3].Text = UnlockCages 
            ? "Unlock Cages: true"
            : "Unlock Cages: false";
    }

    private void LoadLevel()
    {
        GameInfo.Init();

        if (SelectedWorldIndex < 4)
        {
            if (SelectedMapId != MapId.WoodLight_M1)
            {
                GameInfo.MapId = SelectedMapId - 1;
                GameInfo.UpdateLastCompletedLevel();
                GameInfo.PersistentInfo.LastPlayedLevel = (byte)SelectedMapId;
            }

            if (SelectedMapId > MapId.WoodLight_M2)
            {
                GameInfo.EnablePower(Power.DoubleFist);
                GameInfo.PersistentInfo.PlayedMurfyWorldHelp = true;
            }

            if (SelectedMapId > MapId.BossMachine)
            {
                GameInfo.EnablePower(Power.Grab);
            }

            if (SelectedMapId > MapId.EchoingCaves_M2)
            {
                GameInfo.EnablePower(Power.WallJump);
            }

            if (SelectedMapId > MapId.BossRockAndLava)
            {
                GameInfo.EnablePower(Power.BodyShot);
            }

            if (SelectedMapId > MapId.BossScaleMan)
            {
                GameInfo.EnablePower(Power.SuperFist);
            }

            if (SelectedMapId > MapId.PirateShip_M2)
            {
                GameInfo.PersistentInfo.UnlockedFinalBoss = true;
            }

            if (SelectedMapId > MapId.SanctuaryOfBigTree_M2)
            {
                GameInfo.PersistentInfo.UnlockedWorld2 = true;
                GameInfo.PersistentInfo.PlayedWorld2Unlock = true;
            }

            if (SelectedMapId > MapId.MarshAwakening2)
            {
                GameInfo.PersistentInfo.UnlockedWorld3 = true;
                GameInfo.PersistentInfo.PlayedWorld3Unlock = true;
            }

            if (SelectedMapId > MapId.SanctuaryOfRockAndLava_M3)
            {
                GameInfo.PersistentInfo.UnlockedWorld4 = true;
                GameInfo.PersistentInfo.PlayedWorld4Unlock = true;
                GameInfo.PersistentInfo.PlayedAct4 = true;
            }

            if (UnlockLums && SelectedMapId != MapId.WoodLight_M1)
            {
                LevelInfo levelInfo = GameInfo.Levels[(int)SelectedMapId];

                for (int i = 0; i < levelInfo.GlobalLumsIndex; i++)
                    GameInfo.SetLumStatus(i, true);

                if (GameInfo.World1LumsCompleted())
                    GameInfo.PersistentInfo.UnlockedBonus1 = true;

                if (GameInfo.World2LumsCompleted())
                    GameInfo.PersistentInfo.UnlockedBonus2 = true;

                if (GameInfo.World3LumsCompleted())
                    GameInfo.PersistentInfo.UnlockedBonus3 = true;

                if (GameInfo.World4LumsCompleted())
                    GameInfo.PersistentInfo.UnlockedBonus4 = true;
            }

            if (UnlockCages && SelectedMapId != MapId.WoodLight_M1)
            {
                LevelInfo levelInfo = GameInfo.Levels[(int)SelectedMapId];

                for (int i = 0; i < levelInfo.GlobalCagesIndex; i++)
                    GameInfo.SetCageStatus(i, true);
            }

            GameInfo.CurrentSlot = SelectedSaveSlotIndex;
            GameInfo.Save(SelectedSaveSlotIndex);
        }
        else
        {
            GameInfo.MapId = MapId.BossFinal_M2;
            GameInfo.UpdateLastCompletedLevel();

            // New power levels have to have the previous map id set before loading
            GameInfo.MapId = SelectedMapId switch
            {
                MapId.Power1 => MapId.WoodLight_M2,
                MapId.Power2 => MapId.BossMachine,
                MapId.Power3 => MapId.EchoingCaves_M2,
                MapId.Power4 => MapId.BossRockAndLava,
                MapId.Power5 => MapId.SanctuaryOfStoneAndFire_M3,
                MapId.Power6 => MapId.BossScaleMan,
                _ => GameInfo.MapId
            };
        }

        if (SelectedWorldIndex == 5)
        {
            FrameManager.SetNextFrame((int)SelectedMapId switch
            {
                0 => new Act1(),
                1 => new Act2(),
                2 => new Act3(),
                3 => new Act4(),
                4 => new Act5(),
                5 => new Act6(),
                _ => throw new ArgumentOutOfRangeException()
            });
        }
        else
        {
            FrameManager.SetNextFrame(LevelFactory.Create(SelectedMapId));
        }
    }

    #endregion

    #region Public Methods

    public override void Init()
    {
        TransitionsFX.Init(true);
        TransitionsFX.FadeInInit(2);
        AnimationPlayer = new AnimationPlayer(false, null);
        Gfx.ClearColor = ColorHelpers.FromRGB555(0x5555);

        Header = new SpriteTextObject()
        {
            Text = "Select a savegame slot:",
            Color = TextColor.LevelSelect,
            ScreenPos = new Vector2(60, 20),
            RenderContext = Engine.GameRenderContext,
        };

        Rows = new SpriteTextObject[4];
        for (int i = 0; i < Rows.Length; i++)
        {
            Rows[i] = new SpriteTextObject()
            {
                Text = "WoodLight #1",
                Color = TextColor.LevelSelect,
                ScreenPos = new Vector2(30, 60 + RowHeight * i),
                RenderContext = Engine.GameRenderContext,
            };
        }

        Cursor = new SpriteTextObject()
        {
            Text = "->",
            // NOTE: The color is supposed to be red, but the color gets set after the text is drawn, so it's never used
            Color = TextColor.LevelSelect,
            ScreenPos = new Vector2(10, 60),
            RenderContext = Engine.GameRenderContext,
        };

        InitSelectSaveSlot();
    }

    public override void Step()
    {
        CurrentStepAction();
        Cursor.ScreenPos = new Vector2(10, 60 + SelectedRow * RowHeight);

        if ((GameTime.ElapsedFrames & 0x10) != 0)
            AnimationPlayer.Play(Cursor);

        TransitionsFX.StepAll();
        AnimationPlayer.Execute();
    }

    #endregion

    #region Steps

    public void Step_SelectSaveSlot()
    {
        if (JoyPad.IsButtonJustPressed(GbaInput.Left))
        {
            SelectedSaveSlotIndex--;
            
            if (SelectedSaveSlotIndex < 0)
                SelectedSaveSlotIndex = GameInfo.ModernSaveSlotsCount - 1;
            
            Rows[0].Text = $"Slot #{SelectedSaveSlotIndex + 1}";
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.Right))
        {
            SelectedSaveSlotIndex++;
            
            if (SelectedSaveSlotIndex > GameInfo.ModernSaveSlotsCount - 1)
                SelectedSaveSlotIndex = 0;

            Rows[0].Text = $"Slot #{SelectedSaveSlotIndex + 1}";
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.A))
        {
            InitSelectLanguage();
        }

        AnimationPlayer.Play(Header);
        AnimationPlayer.Play(Rows[0]);
    }

    public void Step_SelectLanguage()
    {
        Language[] languages = Localization.GetLanguages();

        if (JoyPad.IsButtonJustPressed(GbaInput.Left))
        {
            SelectedLanguageIndex--;

            if (SelectedLanguageIndex < 0)
                SelectedLanguageIndex = languages.Length - 1;

            Rows[0].Text = languages[SelectedLanguageIndex].LevelSelectName;
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.Right))
        {
            SelectedLanguageIndex++;

            if (SelectedLanguageIndex > languages.Length - 1)
                SelectedLanguageIndex = 0;

            Rows[0].Text = languages[SelectedLanguageIndex].LevelSelectName;
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.A))
        {
            InitSelectStartingLevel();
            Localization.SetLanguage(SelectedLanguageIndex);
        }

        AnimationPlayer.Play(Header);
        AnimationPlayer.Play(Rows[0]);
    }

    public void Step_SelectStartingLevel()
    {
        if (JoyPad.IsButtonJustPressed(GbaInput.Up))
        {
            SelectedRow--;

            if (SelectedRow == -1)
                SelectedRow = SelectedWorldIndex == 4 ? 1 : 3;
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.Down))
        {
            SelectedRow++;

            if ((SelectedWorldIndex == 4 && SelectedRow == 2) ||
                (SelectedWorldIndex != 4 && SelectedRow == 4))
                SelectedRow = 0;
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.Left))
        {
            switch (SelectedRow)
            {
                case 0:
                    SelectedWorldIndex--;

                    if (SelectedWorldIndex == -1)
                        SelectedWorldIndex = 5;

                    SelectedLevelIndex = 0;
                    break;

                case 1:
                    SelectedLevelIndex--;
                    break;

                case 2:
                    UnlockLums = !UnlockLums;
                    break;

                case 3:
                    UnlockCages = !UnlockCages;
                    break;
            }

            SetMapText();
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.Right))
        {
            switch (SelectedRow)
            {
                case 0:
                    SelectedWorldIndex++;

                    if (SelectedWorldIndex > 5)
                        SelectedWorldIndex = 0;

                    SelectedLevelIndex = 0;
                    break;

                case 1:
                    SelectedLevelIndex++;
                    break;

                case 2:
                    UnlockLums = !UnlockLums;
                    break;

                case 3:
                    UnlockCages = !UnlockCages;
                    break;
            }

            SetMapText();
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.A))
        {
            LoadLevel();
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.B))
        {
            InitSelectSaveSlot();
        }

        AnimationPlayer.Play(Header);
        AnimationPlayer.Play(Rows[0]);
        AnimationPlayer.Play(Rows[1]);

        if (SelectedWorldIndex != 4)
        {
            AnimationPlayer.Play(Rows[2]);
            AnimationPlayer.Play(Rows[3]);
        }
    }

    #endregion
}
