using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public class UserInfoMultiMode7 : Dialog
{
    #region Constructor

    public UserInfoMultiMode7(Scene2D scene) : base(scene)
    {
        IsCountdownActive = false;
        IsGameOver = false;
        PreviousHitPoints = 0;
        HitPointsChanged = false;
        MainActor = (MissileMode7)Scene.MainActor;
        ScoreBar = new ScoreBar(Scene);
    }

    #endregion

    #region Properties

    public ScoreBar ScoreBar { get; set; }
    public MissileMode7 MainActor { get; set; }

    public AnimatedObject[] PlayerMapIcons { get; set; }
    public AnimatedObject Rank { get; set; }
    public AnimatedObject HitPoints { get; set; }
    public AnimatedObject Countdown { get; set; }
    public AnimatedObject Map { get; set; }
    public AnimatedObject GameOverSign { get; set; }
    public AnimatedObject PlayerIcon { get; set; }
    public AnimatedObject Laps { get; set; }
    public AnimatedObject[] LapDigits { get; set; }
    public SpriteTextObject WrongWayText { get; set; }
    public SpriteTextObject UnusedText { get; set; } // Unused

    public int PreviousHitPoints { get; set; }
    public bool HitPointsChanged { get; set; }
    public int MapOffset { get; set; }
    public int CountdownValue { get; set; }
    public bool IsCountdownActive { get; set; }
    public bool IsGameOver { get; set; }

    #endregion

    #region Methods

    private void DrawHitPoints(AnimationPlayer animationPlayer)
    {
        if (MainActor.HitPoints == 1 && (GameTime.ElapsedFrames & 0x3f) == 0x3f)
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MinHP);

        if (PreviousHitPoints == MainActor.HitPoints)
        {
            if (HitPointsChanged && HitPoints.EndOfAnimation)
            {
                HitPoints.CurrentAnimation += 5;
                HitPoints.DeactivateChannel(0);
                HitPointsChanged = false;
            }
        }
        else
        {
            PreviousHitPoints = MainActor.HitPoints;
            HitPoints.CurrentAnimation = MainActor.HitPoints + 10;
                HitPoints.DeactivateChannel(0);

            if (MainActor.HitPoints != 0)
                HitPointsChanged = true;
        }

        animationPlayer.PlayFront(HitPoints);
    }

    private void DrawTime()
    {
        // Empty in the final game
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        return false;
    }

    public void SetCountdownValue(int value)
    {
        CountdownValue = value;
        IsCountdownActive = true;
        Countdown.CurrentAnimation = value;
        Countdown.IsFramed = true;
    }

    public override void Load()
    {
        // NOTE: Game has it set up so Load can be called multiple times. Dynamic objects don't get recreated after the first time, but instead
        //       reloaded into VRAM. We don't need to do that though due to how the graphics system works here, so just always create everything.

        AnimatedObjectResource countdownResource = Rom.LoadResource<AnimatedObjectResource>(GameResource.CountdownAnimations);
        AnimatedObjectResource gameOverSignResource = Rom.LoadResource<AnimatedObjectResource>(GameResource.MultiplayerGameOverSignAnimations);
        AnimatedObjectResource mapsResource = Rom.LoadResource<AnimatedObjectResource>(GameResource.MissileMapAnimations);
        AnimatedObjectResource hudResource = Rom.LoadResource<AnimatedObjectResource>(GameResource.HudAnimations);
        AnimatedObjectResource lapsResource = Rom.LoadResource<AnimatedObjectResource>(GameResource.LapAndTimerAnimations);
        AnimatedObjectResource playerIconsResource = Rom.LoadResource<AnimatedObjectResource>(GameResource.MultiplayerPlayerIconAnimations);
        AnimatedObjectResource ranksResource = Rom.LoadResource<AnimatedObjectResource>(GameResource.MultiplayerRankAnimations);

        Countdown = new AnimatedObject(countdownResource, false)
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(0, 90),
            HorizontalAnchor = HorizontalAnchorMode.Center,
            VerticalAnchor = VerticalAnchorMode.Scale,
            RenderContext = Scene.HudRenderContext,
        };

        if (IsCountdownActive)
            Countdown.CurrentAnimation = CountdownValue;

        GameOverSign = new AnimatedObject(gameOverSignResource, gameOverSignResource.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 0,
            ObjPriority = 0,
            CurrentAnimation = 0,
            ScreenPos = new Vector2(10, 180),
            HorizontalAnchor = HorizontalAnchorMode.Center,
            VerticalAnchor = VerticalAnchorMode.Scale,
            RenderContext = Scene.HudRenderContext,
        };

        MapOffset = GameInfo.MapId == MapId.GbaMulti_MissileArena ? 12 : 0;

        Map = new AnimatedObject(mapsResource, false)
        {
            BgPriority = 0,
            ObjPriority = 2,
            CurrentAnimation = GameInfo.MapId == MapId.GbaMulti_MissileArena ? 1 : 0,
            ScreenPos = new Vector2(-64 + MapOffset, -64 + MapOffset),
            HorizontalAnchor = HorizontalAnchorMode.Right,
            VerticalAnchor = VerticalAnchorMode.Bottom,
            RenderOptions = { BlendMode = BlendMode.AlphaBlend },
            RenderContext = Scene.HudRenderContext,
        };

        if (GameInfo.MapId == MapId.GbaMulti_MissileArena)
            MapOffset = 15;

        HitPoints = new AnimatedObject(hudResource, false)
        {
            IsFramed = true,
            CurrentAnimation = 15,
            ScreenPos = new Vector2(-4, 0),
            BgPriority = 0,
            ObjPriority = 0,
            RenderContext = Scene.HudRenderContext,
        };
        HitPoints.DeactivateChannel(0);

        Laps = new AnimatedObject(lapsResource, false)
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(-60, 25),
            CurrentAnimation = 10,
            HorizontalAnchor = HorizontalAnchorMode.Right,
            RenderContext = Scene.HudRenderContext,
        };

        LapDigits = new AnimatedObject[2];
        for (int i = 0; i < LapDigits.Length; i++)
        {
            LapDigits[i] = new AnimatedObject(lapsResource, false)
            {
                BgPriority = 0,
                ObjPriority = 0,
                ScreenPos = new Vector2(-30 + i * 16, 23),
                CurrentAnimation = 0,
                HorizontalAnchor = HorizontalAnchorMode.Right,
                RenderContext = Scene.HudRenderContext,
            };
        }

        PlayerIcon = new AnimatedObject(playerIconsResource, playerIconsResource.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(30, 4),
            CurrentAnimation = 0,
            RenderContext = Scene.HudRenderContext,
        };

        ScoreBar.Load();
        ScoreBar.SetToStayVisible();

        Rank = new AnimatedObject(ranksResource, false)
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(10, -15),
            CurrentAnimation = 0,
            VerticalAnchor = VerticalAnchorMode.Bottom,
            RenderContext = Scene.HudRenderContext,
        };

        PlayerMapIcons = new AnimatedObject[RSMultiplayer.MaxPlayersCount];
        for (int i = 0; i < PlayerMapIcons.Length; i++)
        {
            PlayerMapIcons[i] = new AnimatedObject(playerIconsResource, playerIconsResource.IsDynamic)
            {
                BgPriority = 0,
                ObjPriority = 1,
                CurrentAnimation = 8 + i,
                HorizontalAnchor = HorizontalAnchorMode.Right,
                VerticalAnchor = VerticalAnchorMode.Bottom,
                RenderContext = Scene.HudRenderContext,
            };
        }

        WrongWayText = new SpriteTextObject()
        {
            Color = TextColor.RaceWrongWayText,
            FontSize = FontSize.Font16,
            Text = Localization.GetText(TextBankId.Connectivity, 16)[0],
            ScreenPos = new Vector2(0, 70),
            HorizontalAnchor = HorizontalAnchorMode.Center,
            VerticalAnchor = VerticalAnchorMode.Scale,
            RenderContext = Scene.HudRenderContext,
        };
        WrongWayText.ScreenPos = WrongWayText.ScreenPos with { X = -WrongWayText.GetStringWidth() / 2f };

        // Unknown color. Might have been 0x77de.
        UnusedText = new SpriteTextObject()
        {
            FontSize = FontSize.Font16,
            Text = "Winner P1!",
            ScreenPos = new Vector2(90, 70),
            HorizontalAnchor = HorizontalAnchorMode.Scale,
            VerticalAnchor = VerticalAnchorMode.Scale,
            RenderContext = Scene.HudRenderContext,
        };
    }

    public override void Init() { }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        MultiRaceManager raceManager = ((FrameMissileMultiMode7)Frame.Current).RaceManager;

        if (GameInfo.MapId != MapId.GbaMulti_MissileArena)
        {
            if (MainActor.CollectedBlueLums == 3 && (MainActor.BoostTimer & 0x10) != 0)
                Laps.CurrentAnimation = 10;
            else
                Laps.CurrentAnimation = 10 + MainActor.CollectedBlueLums;

            LapDigits[0].CurrentAnimation = raceManager.PlayersCurrentLap[MainActor.InstanceId];
            LapDigits[1].CurrentAnimation = raceManager.LapsCount;

            animationPlayer.PlayFront(Laps);
            animationPlayer.PlayFront(LapDigits[0]);
            animationPlayer.PlayFront(LapDigits[1]);
        }

        Rank.CurrentAnimation = Array.IndexOf(raceManager.PlayerRanks, MainActor.InstanceId);

        float mapScale = GameInfo.MapId == MapId.GbaMulti_MissileArena ? 16 : 32;
        for (int id = 0; id < MultiplayerManager.PlayersCount; id++)
        {
            PlayerMapIcons[id].ScreenPos = new Vector2(
                x: -67 + MapOffset + ((Scene.GetGameObject(id).Position.X / mapScale) % 64),
                y: -62 + MapOffset + ((Scene.GetGameObject(id).Position.Y / mapScale) % 64));

            animationPlayer.Play(PlayerMapIcons[id]);
        }

        if (GameInfo.MapId != MapId.GbaMulti_MissileArena)
            animationPlayer.PlayFront(Rank);

        if (!raceManager.DrivingTheRightWay &&
            (GameTime.ElapsedFrames & 0x20) != 0 &&
            raceManager.IsRacing &&
            GameInfo.MapId != MapId.GbaMulti_MissileArena &&
            !((FrameMode7)Frame.Current).IsPaused())
        {
            animationPlayer.PlayFront(WrongWayText);

            if ((GameTime.ElapsedFrames & 0x4f) == 0x4f &&
                (GameTime.ElapsedFrames & 0x20) != 0 &&
                !SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__OnoEquil_Mix03))
            {
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoEquil_Mix03);
            }
        }

        if (IsCountdownActive && !((FrameMode7)Frame.Current).IsPaused())
            animationPlayer.PlayFront(Countdown);

        if (IsGameOver)
        {
            if (ScoreBar.DrawStep == BarDrawStep.Hide)
                ScoreBar.DrawStep = BarDrawStep.MoveIn;

            ScoreBar.DrawScore(animationPlayer, raceManager.PlayerRanks);
        }

        if (Scene.Camera.LinkedObject.HitPoints == 0)
        {
            if (!IsGameOver)
            {
                if (GameOverSign.ScreenPos.Y > 50)
                    GameOverSign.ScreenPos -= new Vector2(0, 2);

                animationPlayer.PlayFront(GameOverSign);
            }

            PlayerIcon.CurrentAnimation = Scene.Camera.LinkedObject.InstanceId + 4;
        }
        else
        {
            PlayerIcon.CurrentAnimation = Scene.Camera.LinkedObject.InstanceId;
        }

        animationPlayer.PlayFront(PlayerIcon);
        animationPlayer.Play(Map);

        DrawHitPoints(animationPlayer);
        DrawTime();
    }

    #endregion
}