using System;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

// TODO:
// - Don't allow selecting level if not finished in single player.
// - Should Ly's Punch Challenge be included?
// - Show screenshot for each level.
public class TimeAttackMenuPage : MenuPage
{
    public TimeAttackMenuPage(ModernMenuAll menu) : base(menu)
    {
        // Maps are same except for waterski levels being removed on GBA
        if (Rom.Platform == Platform.GBA)
        {
            Maps =
            [
                [
                    MapId.WoodLight_M1,
                    MapId.FairyGlade_M1,
                    MapId.BossMachine,
                    MapId.SanctuaryOfBigTree_M1,
                ],
                [
                    MapId.MissileRace1,
                    MapId.EchoingCaves_M1,
                    MapId.CavesOfBadDreams_M1,
                    MapId.BossBadDreams,
                    MapId.MenhirHills_M1,
                ],
                [
                    MapId.SanctuaryOfStoneAndFire_M1,
                    MapId.BeneathTheSanctuary_M1,
                    MapId.ThePrecipice_M1,
                    MapId.BossRockAndLava,
                    MapId.TheCanopy_M1,
                    MapId.SanctuaryOfRockAndLava_M1,
                ],
                [
                    MapId.TombOfTheAncients_M1,
                    MapId.BossScaleMan,
                    MapId.IronMountains_M1,
                    MapId.MissileRace2,
                    MapId.PirateShip_M1,
                    MapId.BossFinal_M1,
                ],
                [
                    MapId.Bonus1,
                    MapId.Bonus2,
                    MapId.Bonus3,
                    MapId.Bonus4,
                    MapId._1000Lums,
                ]
            ];
        }
        else if (Rom.Platform == Platform.NGage)
        {
            Maps = 
            [
                [
                    MapId.WoodLight_M1,
                    MapId.FairyGlade_M1,
                    MapId.MarshAwakening1,
                    MapId.BossMachine,
                    MapId.SanctuaryOfBigTree_M1,
                ],
                [
                    MapId.MissileRace1,
                    MapId.EchoingCaves_M1,
                    MapId.CavesOfBadDreams_M1,
                    MapId.BossBadDreams,
                    MapId.MenhirHills_M1,
                    MapId.MarshAwakening2,
                ],
                [
                    MapId.SanctuaryOfStoneAndFire_M1,
                    MapId.BeneathTheSanctuary_M1,
                    MapId.ThePrecipice_M1,
                    MapId.BossRockAndLava,
                    MapId.TheCanopy_M1,
                    MapId.SanctuaryOfRockAndLava_M1,
                ],
                [
                    MapId.TombOfTheAncients_M1,
                    MapId.BossScaleMan,
                    MapId.IronMountains_M1,
                    MapId.MissileRace2,
                    MapId.PirateShip_M1,
                    MapId.BossFinal_M1,
                ],
                [
                    MapId.Bonus1,
                    MapId.Bonus2,
                    MapId.Bonus3,
                    MapId.Bonus4,
                    MapId._1000Lums,
                ]
            ];
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
    }

    private const int LevelOptionsBaseIndex = 7;
    private const int LevelOptionsCount = 2;
    private const float GhostSelectionTextScale = 2 / 3f;
    private const float GhostSelectionArrowScale = 1 / 2f;

    public override bool UsesCursor => true;
    public override int BackgroundPalette => 2;
    public override int LineHeight => 12;

    public MapId[][] Maps { get; }
    public TimeAttackLevelMenuOption[][] WorldOptions { get; set; }
    public int SelectedWorld { get; set; }
    public MapId SelectedMap => Maps[SelectedWorld][SelectedOption];

    public bool HasSelectedLevel { get; set; }
    public int SelectedLevelOption { get; set; }

    public GhostOption[] GhostOptions { get; set; }
    public int SelectedGhostOption { get; set; }

    public AnimatedObject WorldNameCanvas { get; set; }
    public SpriteTextObject WorldName { get; set; }
    public MenuHorizontalArrows WorldNameArrows { get; set; }

    public SpriteTextureObject Cloth { get; set; }

    public TimeObject[] TargetTimes { get; set; }
    public TimeObject RecordTime { get; set; }
    
    public SpriteFontTextObject GhostText { get; set; }
    public SpriteFontTextObject GhostSelectionName { get; set; }
    public SpriteFontTextObject GhostSelectionTime { get; set; }
    public MenuHorizontalArrows GhostSelectionArrows { get; set; }
    public SpriteFontTextObject PlayText { get; set; }

    private void SetSelectedWorld(int selectedTab, bool playSound = true)
    {
        if (selectedTab > WorldOptions.Length - 1)
            selectedTab = 0;
        else if (selectedTab < 0)
            selectedTab = WorldOptions.Length - 1;

        SelectedWorld = selectedTab;

        ClearOptions();
        foreach (TimeAttackLevelMenuOption menuOption in WorldOptions[selectedTab])
            AddOption(menuOption);

        SetSelectedOption(0, playSound: false, forceUpdate: true);

        if (playSound)
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);

        string worldName = SelectedWorld == 4 
            ? "Bonus" 
            : Localization.GetText(TextBankId.LevelNames, 31 + SelectedWorld)[0];
        WorldName.Text = worldName;
        WorldName.ScreenPos = WorldNameCanvas.ScreenPos + new Vector2(-WorldName.GetStringWidth() / 2f, -15);
    }

    private void SetSelectedLevelOption(int selectedOption, bool playSound = true)
    {
        if (selectedOption > LevelOptionsCount - 1)
            selectedOption = 0;
        else if (selectedOption < 0)
            selectedOption = LevelOptionsCount - 1;

        SelectedLevelOption = selectedOption;
        Menu.SetCursorTarget(LevelOptionsBaseIndex + SelectedLevelOption);

        if (SelectedLevelOption == 0)
        {
            GhostText.Font = ReadvancedFonts.MenuWhite;
            GhostSelectionName.Font = ReadvancedFonts.MenuWhite;
            GhostSelectionTime.Font = ReadvancedFonts.MenuWhite;
            GhostSelectionArrows.Resume();

            PlayText.Font = ReadvancedFonts.MenuYellow;
        }
        else if (SelectedLevelOption == 1)
        {
            GhostText.Font = ReadvancedFonts.MenuYellow;
            GhostSelectionName.Font = ReadvancedFonts.MenuYellow;
            GhostSelectionTime.Font = ReadvancedFonts.MenuYellow;
            GhostSelectionArrows.Pause();

            PlayText.Font = ReadvancedFonts.MenuWhite;
        }

        if (playSound)
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
    }

    private void LoadGhostOptions()
    {
        foreach (GhostOption ghostOption in GhostOptions)
            ghostOption.Load(SelectedMap);
    }

    // TODO: Maybe remember last option instead?
    private void SetDefaultSelectedGhostOption()
    {
        TimeAttackTime? recordTime = TimeAttackInfo.GetRecordTime(SelectedMap);

        GhostOption defaultGhostOption;
        if (recordTime == null)
        {
            defaultGhostOption = GhostOptions.FirstOrDefault(x => x.IsAvailable && x.Type == TimeAttackGhostType.Guide);
        }
        else
        {
            defaultGhostOption = GhostOptions.
                Where(x => x.IsAvailable && x.Type != TimeAttackGhostType.None).
                OrderBy(x => x.Time.Time).
                FirstOrDefault(x => x.Time.Time <= recordTime.Value.Time);
        }

        SetSelectedGhostOption(Array.IndexOf(GhostOptions, defaultGhostOption), playSound: false);
    }

    private void SetSelectedGhostOption(int selectedOption, bool playSound = true)
    {
        bool add = selectedOption > SelectedGhostOption;

        if (selectedOption > GhostOptions.Length - 1)
            selectedOption = 0;
        else if (selectedOption < 0)
            selectedOption = GhostOptions.Length - 1;

        while (!GhostOptions[selectedOption].IsAvailable)
        {
            if (add)
                selectedOption++;
            else
                selectedOption--;

            if (selectedOption > GhostOptions.Length - 1)
                selectedOption = 0;
            else if (selectedOption < 0)
                selectedOption = GhostOptions.Length - 1;
        }

        SelectedGhostOption = selectedOption;
        GhostOption ghostOption = GhostOptions[SelectedGhostOption];

        SetGhostSelectionText(ghostOption.Name, ghostOption.Type == TimeAttackGhostType.None ? null : ghostOption.Time.ToTimeString());

        if (playSound)
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
    }

    private void SetGhostSelectionText(string name, string time)
    {
        Vector2 pos = GhostSelectionArrows.Position + new Vector2(0, 2);

        if (time != null)
            pos -= new Vector2(0, GhostSelectionName.Font.LineHeight / 2f) * GhostSelectionTextScale;

        float nameWidth = GhostSelectionName.Font.GetWidth(name) * GhostSelectionTextScale;
        float timeWidth = time == null ? 0 : GhostSelectionTime.Font.GetWidth(time) * GhostSelectionTextScale;
        float maxWidth = MathF.Max(nameWidth, timeWidth);

        GhostSelectionName.Text = name;
        GhostSelectionName.ScreenPos = pos + new Vector2((maxWidth - nameWidth) / 2, 0);

        GhostSelectionTime.Text = time;
        GhostSelectionTime.ScreenPos = pos + new Vector2((maxWidth - timeWidth) / 2, GhostSelectionTime.Font.LineHeight * GhostSelectionTextScale);

        GhostSelectionArrows.Width = maxWidth;
    }

    protected override bool SetSelectedOption(int selectedOption, bool playSound = true, bool forceUpdate = false)
    {
        if (base.SetSelectedOption(selectedOption, playSound, forceUpdate))
        {
            MapId mapId = SelectedMap;

            TimeAttackTime? recordTime = TimeAttackInfo.GetRecordTime(mapId);

            TimeAttackTime[] targetTimes = TimeAttackInfo.GetTargetTimes(mapId);
            for (int i = 0; i < TargetTimes.Length; i++)
            {
                TimeAttackTime? targetTime = i < targetTimes.Length ? targetTimes[i] : null;
                TargetTimes[i].SetTime(targetTime, recordTime?.Time <= targetTime?.Time);
            }

            RecordTime.SetTime(recordTime, true);

            return true;
        }
        else
        {
            return false;
        }
    }

    protected override void Init()
    {
        MapId selectedMap = Maps[0][0];
        if (TimeAttackInfo.IsActive)
        {
            if (TimeAttackInfo.LevelId != null)
                selectedMap = TimeAttackInfo.LevelId.Value;
            TimeAttackInfo.UnInit();
        }

        WorldOptions = new TimeAttackLevelMenuOption[Maps.Length][];
        for (int tabIndex = 0; tabIndex < Maps.Length; tabIndex++)
        {
            MapId[] maps = Maps[tabIndex];
            WorldOptions[tabIndex] = new TimeAttackLevelMenuOption[maps.Length];
            for (int mapIndex = 0; mapIndex < maps.Length; mapIndex++)
                WorldOptions[tabIndex][mapIndex] = new TimeAttackLevelMenuOption(maps[mapIndex], 2 / 3f);
        }

        AnimatedObjectResource worldNameAnimations = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.WorldDashboardAnimations);
        WorldNameCanvas = new AnimatedObject(worldNameAnimations, false)
        {
            IsFramed = true,
            BgPriority = 3,
            ObjPriority = 0,
            ScreenPos = new Vector2(192 + 20, 50),
            CurrentAnimation = 0,
            RenderContext = RenderContext,
        };

        WorldName = new SpriteTextObject()
        {
            BgPriority = 3,
            ObjPriority = 0,
            Color = TextColor.WorldName,
            RenderContext = RenderContext,
        };

        WorldNameArrows = new MenuHorizontalArrows(RenderContext, 3, 1)
        {
            Position = WorldNameCanvas.ScreenPos + new Vector2(-84, -8),
            Width = 168
        };

        Cloth = new SpriteTextureObject
        {
            BgPriority = 3,
            ObjPriority = 0,
            ScreenPos = new Vector2(195, 52),
            RenderContext = RenderContext,
            Texture = Engine.FrameContentManager.Load<Texture2D>(Assets.ClothTexture)
        };

        TargetTimes =
        [
            new TimeObject(RenderContext, Cloth.ScreenPos + new Vector2(9, 8 + 18 * 0), new Vector2(20, 0)),
            new TimeObject(RenderContext, Cloth.ScreenPos + new Vector2(9, 8 + 18 * 1), new Vector2(20, 0)),
            new TimeObject(RenderContext, Cloth.ScreenPos + new Vector2(9, 8 + 18 * 2), new Vector2(20, 0)),
        ];

        RecordTime = new TimeObject(RenderContext, Cloth.ScreenPos + new Vector2(9 + 75, 8 + 18 * 1), new Vector2(17, 0));

        GhostOptions =
        [
            new GhostOption(TimeAttackGhostType.None, "NONE"),
            new GhostOption(TimeAttackGhostType.Record, "RECORD"),
            new GhostOption(TimeAttackGhostType.Guide, "GUIDE"), // NOTE: This name will be overwritten later
            new GhostOption(TimeAttackGhostType.Developer, "DEV"),
        ];

        GhostText = new SpriteFontTextObject
        {
            BgPriority = 3,
            ObjPriority = 0,
            ScreenPos = GetOptionPosition(LevelOptionsBaseIndex) + new Vector2(0, 13 * GhostSelectionTextScale) - new Vector2(0, 3),
            RenderContext = RenderContext,
            Text = "GHOST",
            AffineMatrix = new AffineMatrix(0, new Vector2(GhostSelectionTextScale), false, false),
            Font = ReadvancedFonts.MenuYellow,
        };

        GhostSelectionName = new SpriteFontTextObject
        {
            BgPriority = 3,
            ObjPriority = 0,
            RenderContext = RenderContext,
            AffineMatrix = new AffineMatrix(0, new Vector2(GhostSelectionTextScale), false, false),
            Font = ReadvancedFonts.MenuYellow,
        };

        GhostSelectionTime = new SpriteFontTextObject
        {
            BgPriority = 3,
            ObjPriority = 0,
            RenderContext = RenderContext,
            AffineMatrix = new AffineMatrix(0, new Vector2(GhostSelectionTextScale), false, false),
            Font = ReadvancedFonts.MenuYellow,
        };

        GhostSelectionArrows = new MenuHorizontalArrows(RenderContext, 3, GhostSelectionArrowScale)
        {
            Position = GhostText.ScreenPos + new Vector2(GhostText.Font.GetWidth(GhostText.Text), 0) * GhostSelectionTextScale + new Vector2(20, -2),
        };

        PlayText = new SpriteFontTextObject
        {
            BgPriority = 3,
            ObjPriority = 0,
            ScreenPos = GetOptionPosition(LevelOptionsBaseIndex + 1) + new Vector2(0, 13),
            RenderContext = RenderContext,
            Text = "PLAY",
            Font = ReadvancedFonts.MenuYellow,
        };

        // Set the selected world and option
        int selectedWorld = Array.FindIndex(Maps, x => Array.IndexOf(x, selectedMap) >= 0);
        SetSelectedWorld(selectedWorld, false);
        SetSelectedOption(Array.IndexOf(Maps[selectedWorld], selectedMap), playSound: false, forceUpdate: true);

        HasSelectedLevel = false;
    }

    protected override void Step_Active()
    {
        if (!HasSelectedLevel)
        {
            if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuUp))
            {
                SetSelectedOption(SelectedOption - 1);
            }
            else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuDown))
            {
                SetSelectedOption(SelectedOption + 1);
            }
            else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuLeftExt))
            {
                SetSelectedWorld(SelectedWorld - 1);
            }
            else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuRightExt))
            {
                SetSelectedWorld(SelectedWorld + 1);
            }
            else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuConfirm))
            {
                CursorClick(() =>
                {
                    HasSelectedLevel = true;
                    SetSelectedLevelOption(1, playSound: false);
                    LoadGhostOptions();
                    SetDefaultSelectedGhostOption();
                    WorldNameArrows.Pause();
                });
            }
            else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuBack))
            {
                Menu.ChangePage(new BonusMenuPage(Menu), NewPageMode.Back);
            }
        }
        else
        {
            if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuUp))
            {
                SetSelectedLevelOption(SelectedLevelOption - 1);
            }
            else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuDown))
            {
                SetSelectedLevelOption(SelectedLevelOption + 1);
            }
            else if (SelectedLevelOption == 0 && JoyPad.IsButtonJustPressed(Rayman3Input.MenuLeft))
            {
                SetSelectedGhostOption(SelectedGhostOption - 1);
            }
            else if (SelectedLevelOption == 0 && JoyPad.IsButtonJustPressed(Rayman3Input.MenuRight))
            {
                SetSelectedGhostOption(SelectedGhostOption + 1);
            }
            else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuConfirm))
            {
                if (SelectedLevelOption == 0)
                {
                    SetSelectedLevelOption(1);
                }
                else if (SelectedLevelOption == 1)
                {
                    CursorClick(() =>
                    {
                        SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.None, 1);
                        FadeOut(2, () =>
                        {
                            SoundEventsManager.StopAllSongs();
                            Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
                            Gfx.Fade = AlphaCoefficient.Max;

                            TimeAttackInfo.Init();
                            TimeAttackInfo.LoadLevel(SelectedMap, GhostOptions[SelectedGhostOption].Type);
                        });
                    });
                }
            }
            else if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuBack))
            {
                HasSelectedLevel = false;
                SetSelectedOption(SelectedOption, playSound: false, forceUpdate: true);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);
                WorldNameArrows.Resume();
            }
        }
    }

    protected override void Draw(AnimationPlayer animationPlayer)
    {
        DrawOptions(animationPlayer);
        animationPlayer.Play(WorldNameCanvas);
        animationPlayer.Play(WorldName);
        WorldNameArrows.Draw(animationPlayer);

        animationPlayer.Play(Cloth);

        foreach (TimeObject targetTime in TargetTimes)
            targetTime.Draw(animationPlayer);

        RecordTime.Draw(animationPlayer);

        if (HasSelectedLevel)
        {
            animationPlayer.Play(GhostText);
            animationPlayer.Play(GhostSelectionName);
            animationPlayer.Play(GhostSelectionTime);
            GhostSelectionArrows.Draw(animationPlayer);
            animationPlayer.Play(PlayText);
        }
    }

    public class TimeObject
    {
        public TimeObject(RenderContext renderContext, Vector2 iconPosition, Vector2 textOffset)
        {
            Icon = new SpriteTextureObject
            {
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = iconPosition,
                RenderContext = renderContext,
            };

            TimeText = new SpriteTextObject
            {
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = iconPosition + textOffset,
                RenderContext = renderContext,
                Color = TextColor.TextBox,
            };
        }

        public SpriteTextureObject Icon { get; }
        public SpriteTextObject TimeText { get; }
        public bool ShouldDraw { get; set; }

        public void SetTime(TimeAttackTime? time, bool filledIn)
        {
            ShouldDraw = time.HasValue;

            if (time != null)
            {
                Icon.Texture = time.Value.LoadIcon(filledIn);
                TimeText.Text = time.Value.ToTimeString();
            }
        }

        public void Draw(AnimationPlayer animationPlayer)
        {
            if (ShouldDraw)
            {
                animationPlayer.Play(Icon);
                animationPlayer.Play(TimeText);
            }
        }
    }

    public class GhostOption
    {
        public GhostOption(TimeAttackGhostType type, string defaultName)
        {
            Type = type;
            DefaultName = defaultName;
        }

        public TimeAttackGhostType Type { get; }
        public string DefaultName { get; }

        public TimeAttackTime Time { get; set; }
        public bool IsAvailable { get; set; }
        public string Name { get; set; }

        public void Load(MapId mapId)
        {
            Name = DefaultName;

            if (Type == TimeAttackGhostType.None)
            {
                Time = default;
                IsAvailable = true;
                return;
            }

            switch (Type)
            {
                case TimeAttackGhostType.Record:
                    TimeAttackTime? recordTime = TimeAttackInfo.GetRecordTime(mapId);
                    if (recordTime != null)
                    {
                        Time = recordTime.Value;
                        IsAvailable = true;
                    }
                    else
                    {
                        Time = default;
                        IsAvailable = false;
                    }
                    break;

                // TODO: Implement
                case TimeAttackGhostType.Guide:
                    Name = "GLOBOX";
                    Time = new TimeAttackTime(TimeAttackTimeType.Record, 44 * 60 + 03);
                    IsAvailable = true;
                    break;

                // TODO: Implement
                case TimeAttackGhostType.Developer:
                    Time = new TimeAttackTime(TimeAttackTimeType.Record, 39 * 60 + 30);
                    IsAvailable = true;
                    break;
            }
        }
    }
}