using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public class TimeAttackMenuPage : MenuPage
{
    public TimeAttackMenuPage(ModernMenuAll menu) : base(menu) { }

    private const int LevelOptionsBaseIndex = 7;
    private const float GhostSelectionTextScale = 2 / 3f;

    private static MapId[][] Maps =>
    [
        [
            MapId.WoodLight_M1,
            MapId.FairyGlade_M1,
            //MapId.MarshAwakening1,
            MapId.BossMachine,
            MapId.SanctuaryOfBigTree_M1,
        ],
        [
            MapId.MissileRace1,
            MapId.EchoingCaves_M1,
            MapId.CavesOfBadDreams_M1,
            MapId.BossBadDreams,
            MapId.MenhirHills_M1,
            //MapId.MarshAwakening2,
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
            //MapId.ChallengeLy1,
            //MapId.ChallengeLy2,
            //MapId.ChallengeLyGCN,
        ]
    ];

    public override bool UsesCursor => true;
    public override int BackgroundPalette => 2;
    public override int LineHeight => 12;

    public TimeAttackLevelMenuOption[][] WorldOptions { get; set; }
    public int SelectedWorld { get; set; }

    public bool HasSelectedLevel { get; set; }

    public AnimatedObject WorldNameCanvas { get; set; }
    public SpriteTextObject WorldName { get; set; }
    public MenuHorizontalArrows WorldNameArrows { get; set; }
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

        GhostSelectionText = new SpriteFontTextObject
        {
            BgPriority = 3,
            ObjPriority = 0,
            ScreenPos = GetOptionPosition(LevelOptionsBaseIndex) + new Vector2(0, 13 * GhostSelectionTextScale),
            RenderContext = RenderContext,
            Text = "NO GHOST",
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
                });
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.B))
            {
                Menu.ChangePage(new BonusMenuPage(Menu), NewPageMode.Back);
            }
        }
        else
        {
            if (JoyPad.IsButtonJustPressed(GbaInput.A))
            {

            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.B))
            {
                HasSelectedLevel = false;
                SetSelectedOption(SelectedOption, playSound: false, forceUpdate: true);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);
            }
        }
    }

    protected override void Draw(AnimationPlayer animationPlayer)
    {
        DrawOptions(animationPlayer);
        animationPlayer.Play(WorldNameCanvas);
        animationPlayer.Play(WorldName);

        WorldNameArrows.Draw(animationPlayer);

        if (HasSelectedLevel)
        {
            animationPlayer.Play(GhostSelectionText);
            animationPlayer.Play(PlayText);
        }
    }
}