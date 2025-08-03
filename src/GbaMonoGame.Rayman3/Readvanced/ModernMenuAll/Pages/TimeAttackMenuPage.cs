using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

// TODO:
// - Don't allow selecting level if not finished in single player.
// - Should the waterski levels be included? Should Ly's Punch Challenge be included?
// - Show screenshot for each level.
public class TimeAttackMenuPage : MenuPage
{
    public TimeAttackMenuPage(ModernMenuAll menu) : base(menu) { }

    private const int LevelOptionsBaseIndex = 7;
    private const float GhostSelectionTextScale = 2 / 3f;

    private static MapId[][] Maps { get; } =
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

    public override bool UsesCursor => true;
    public override int BackgroundPalette => 2;
    public override int LineHeight => 12;

    public TimeAttackLevelMenuOption[][] WorldOptions { get; set; }
    public int SelectedWorld { get; set; }
    public MapId SelectedMap => Maps[SelectedWorld][SelectedOption];

    public bool HasSelectedLevel { get; set; }

    public AnimatedObject WorldNameCanvas { get; set; }
    public SpriteTextObject WorldName { get; set; }
    public MenuHorizontalArrows WorldNameArrows { get; set; }

    public SpriteTextureObject Cloth { get; set; }

    public TimeObject[] TargetTimes { get; set; }
    public TimeObject RecordTime { get; set; }

    public SpriteFontTextObject GhostSelectionText { get; set; }
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

        GhostSelectionText = new SpriteFontTextObject
        {
            BgPriority = 3,
            ObjPriority = 0,
            ScreenPos = GetOptionPosition(LevelOptionsBaseIndex) + new Vector2(0, 13 * GhostSelectionTextScale),
            RenderContext = RenderContext,
            Text = "GHOST: NONE",
            AffineMatrix = new AffineMatrix(0, new Vector2(GhostSelectionTextScale), false, false),
            Font = ReadvancedFonts.MenuYellow,
        };

        PlayText = new SpriteFontTextObject
        {
            BgPriority = 3,
            ObjPriority = 0,
            ScreenPos = GetOptionPosition(LevelOptionsBaseIndex + 1) + new Vector2(0, 13),
            RenderContext = RenderContext,
            Text = "PLAY",
            Font = ReadvancedFonts.MenuWhite,
        };

        // Set the initial world
        SetSelectedWorld(0, false);

        HasSelectedLevel = false;
    }

    protected override void Step_Active()
    {
        if (!HasSelectedLevel)
        {
            if (JoyPad.IsButtonJustPressed(GbaInput.Up))
            {
                SetSelectedOption(SelectedOption - 1);
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.Down))
            {
                SetSelectedOption(SelectedOption + 1);
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.Left) || JoyPad.IsButtonJustPressed(GbaInput.L))
            {
                SetSelectedWorld(SelectedWorld - 1);
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.Right) || JoyPad.IsButtonJustPressed(GbaInput.R))
            {
                SetSelectedWorld(SelectedWorld + 1);
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.A))
            {
                CursorClick(() =>
                {
                    HasSelectedLevel = true;
                    Menu.SetCursorTarget(LevelOptionsBaseIndex + 1);
                    WorldNameArrows.Pause();
                });
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.B))
            {
                Menu.ChangePage(new BonusMenuPage(Menu), NewPageMode.Back);
            }
        }
        else
        {
            // TODO: Implement setting ghost

            if (JoyPad.IsButtonJustPressed(GbaInput.A))
            {
                CursorClick(() =>
                {
                    SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.None, 1);
                    FadeOut(2, () =>
                    {
                        SoundEventsManager.StopAllSongs();
                        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
                        Gfx.Fade = 1;

                        TimeAttackInfo.Init();
                        TimeAttackInfo.LoadLevel(SelectedMap);
                    });
                });
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.B))
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
            animationPlayer.Play(GhostSelectionText);
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
}