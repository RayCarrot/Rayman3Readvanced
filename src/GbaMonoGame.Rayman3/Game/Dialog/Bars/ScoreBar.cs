using System;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3;

public class ScoreBar : Bar
{
    public ScoreBar(Scene2D scene) : base(scene)
    {
        DrawStep = BarDrawStep.Hide;
        OffsetX = 142;
    }

    public int OffsetX { get; set; }
    public AnimatedObject[] PlayerRanks { get; set; }
    public AnimatedObject[] PlayerIcons { get; set; }
    public AnimatedObject Crown1 { get; set; }
    public AnimatedObject Crown2 { get; set; }

    public override void Load()
    {
        AnimatedObjectResource playerRanksResource = Storage.LoadResource<AnimatedObjectResource>(GameResource.MultiplayerRankAnimations);
        AnimatedObjectResource playerIconsResource = Storage.LoadResource<AnimatedObjectResource>(GameResource.MultiplayerPlayerIconAnimations);

        const int rowHeight = 24;

        PlayerRanks = new AnimatedObject[RSMultiplayer.MaxPlayersCount];
        for (int i = 0; i < PlayerRanks.Length; i++)
        {
            PlayerRanks[i] = new AnimatedObject(playerRanksResource, false)
            {
                BgPriority = 0,
                ObjPriority = 1,
                ScreenPos = Engine.Settings.Platform switch
                {
                    Platform.GBA => new Vector2(92, 60 + i * rowHeight),
                    Platform.NGage => new Vector2(63, 75 + i * rowHeight),
                    _ => throw new UnsupportedPlatformException()
                },
                CurrentAnimation = i,
                HorizontalAnchor = HorizontalAnchorMode.Scale,
                RenderContext = Scene.HudRenderContext,
            };
        }

        PlayerIcons = new AnimatedObject[RSMultiplayer.MaxPlayersCount];
        for (int i = 0; i < PlayerIcons.Length; i++)
        {
            PlayerIcons[i] = new AnimatedObject(playerIconsResource, false)
            {
                BgPriority = 0,
                ObjPriority = 1,
                ScreenPos = Engine.Settings.Platform switch
                {
                    Platform.GBA => new Vector2(122, 44 + i * rowHeight),
                    Platform.NGage => new Vector2(93, 59 + i * rowHeight),
                    _ => throw new UnsupportedPlatformException()
                },
                CurrentAnimation = i,
                HorizontalAnchor = HorizontalAnchorMode.Scale,
                RenderContext = Scene.HudRenderContext,
            };
        }

        Crown1 = new AnimatedObject(playerRanksResource, false)
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = Engine.Settings.Platform switch
            {
                Platform.GBA => new Vector2(152, 60),
                Platform.NGage => new Vector2(103, 62),
                _ => throw new UnsupportedPlatformException()
            },
            CurrentAnimation = 4,
            HorizontalAnchor = HorizontalAnchorMode.Scale,
            RenderContext = Scene.HudRenderContext,
        };

        if (Engine.Settings.Platform == Platform.NGage)
        {
            Crown2 = new AnimatedObject(playerRanksResource, false)
            {
                BgPriority = 0,
                ObjPriority = 0,
                ScreenPos = new Vector2(123, 62),
                CurrentAnimation = 4,
                HorizontalAnchor = HorizontalAnchorMode.Scale,
                RenderContext = Scene.HudRenderContext,
            };
        }
    }

    public override void Set() { }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        throw new InvalidOperationException($"Use {nameof(DrawScore)} when drawing the timer bar");
    }

    public void DrawScore(AnimationPlayer animationPlayer, int[] playerRanks)
    {
        switch (DrawStep)
        {
            case BarDrawStep.Hide:
                OffsetX = 142;
                break;

            case BarDrawStep.MoveIn:
                if (OffsetX < 1)
                    DrawStep = BarDrawStep.Wait;
                else
                    OffsetX -= 2;
                break;

            case BarDrawStep.MoveOut:
                if (OffsetX < 142)
                {
                    OffsetX += 2;
                }
                else
                {
                    OffsetX = 142;
                    DrawStep = BarDrawStep.Hide;
                }
                break;
        }

        if (DrawStep != BarDrawStep.Hide)
        {
            float crownBaseX = Engine.Settings.Platform switch
            {
                Platform.GBA => 152,
                Platform.NGage => 103,
                _ => throw new UnsupportedPlatformException()
            };
            float playerIconsBaseX = Engine.Settings.Platform switch
            {
                Platform.GBA => 122,
                Platform.NGage => 93,
                _ => throw new UnsupportedPlatformException()
            };
            float playerRanksBaseX = Engine.Settings.Platform switch
            {
                Platform.GBA => 92,
                Platform.NGage => 63,
                _ => throw new UnsupportedPlatformException()
            };

            Crown1.ScreenPos = Crown1.ScreenPos with { X = crownBaseX - OffsetX };
            
            PlayerIcons[0].ScreenPos = PlayerIcons[0].ScreenPos with { X = playerIconsBaseX - OffsetX };
            PlayerIcons[2].ScreenPos = PlayerIcons[2].ScreenPos with { X = playerIconsBaseX - OffsetX };
            PlayerRanks[0].ScreenPos = PlayerRanks[0].ScreenPos with { X = playerRanksBaseX - OffsetX };
            PlayerRanks[2].ScreenPos = PlayerRanks[2].ScreenPos with { X = playerRanksBaseX - OffsetX };

            PlayerIcons[1].ScreenPos = PlayerIcons[1].ScreenPos with { X = playerIconsBaseX + OffsetX };
            PlayerIcons[3].ScreenPos = PlayerIcons[3].ScreenPos with { X = playerIconsBaseX + OffsetX };
            PlayerRanks[1].ScreenPos = PlayerRanks[1].ScreenPos with { X = playerRanksBaseX + OffsetX };
            PlayerRanks[3].ScreenPos = PlayerRanks[3].ScreenPos with { X = playerRanksBaseX + OffsetX };

            for (int i = 0; i < PlayerIcons.Length; i++)
                PlayerIcons[i].CurrentAnimation = playerRanks[i];

            if (Engine.Settings.Platform == Platform.GBA)
                animationPlayer.PlayFront(Crown1);

            for (int i = 0; i < MultiplayerManager.PlayersCount; i++)
            {
                animationPlayer.PlayFront(PlayerIcons[i]);
                animationPlayer.PlayFront(PlayerRanks[i]);
            }

            if (Engine.Settings.Platform == Platform.NGage)
                animationPlayer.PlayFront(Crown1);
        }
    }

    public void DrawTeamsScore(AnimationPlayer animationPlayer, int[] playerRanks)
    {
        switch (DrawStep)
        {
            case BarDrawStep.Hide:
                OffsetX = 142;
                break;

            case BarDrawStep.MoveIn:
                if (OffsetX < 1)
                    DrawStep = BarDrawStep.Wait;
                else
                    OffsetX -= 2;
                break;

            case BarDrawStep.MoveOut:
                if (OffsetX < 142)
                {
                    OffsetX += 2;
                }
                else
                {
                    OffsetX = 142;
                    DrawStep = BarDrawStep.Hide;
                }
                break;
        }

        if (DrawStep != BarDrawStep.Hide)
        {
            for (int i = 0; i < PlayerIcons.Length; i += 2)
            {
                PlayerIcons[i + 0].ScreenPos = PlayerIcons[i + 0].ScreenPos with { Y = i / 2f * 24 + 59 };
                PlayerIcons[i + 1].ScreenPos = PlayerIcons[i + 1].ScreenPos with { Y = i / 2f * 24 + 59 };
            }

            Crown1.ScreenPos = Crown1.ScreenPos with { X = 103 - OffsetX };
            Crown2.ScreenPos = Crown2.ScreenPos with { X = 123 - OffsetX };

            PlayerIcons[0].ScreenPos = PlayerIcons[0].ScreenPos with { X = 93 - OffsetX };
            PlayerIcons[1].ScreenPos = PlayerIcons[1].ScreenPos with { X = 113 - OffsetX };
            PlayerIcons[2].ScreenPos = PlayerIcons[2].ScreenPos with { X = 93 + OffsetX };
            PlayerIcons[3].ScreenPos = PlayerIcons[3].ScreenPos with { X = 113 + OffsetX };
            
            PlayerRanks[0].ScreenPos = PlayerRanks[0].ScreenPos with { X = 63 - OffsetX };
            PlayerRanks[1].ScreenPos = PlayerRanks[1].ScreenPos with { X = 63 + OffsetX };
            
            for (int i = 0; i < PlayerIcons.Length; i++)
                PlayerIcons[i].CurrentAnimation = playerRanks[i];

            animationPlayer.PlayFront(PlayerRanks[0]);
            animationPlayer.PlayFront(PlayerRanks[1]);

            animationPlayer.PlayFront(PlayerIcons[0]);
            animationPlayer.PlayFront(PlayerIcons[1]);
            animationPlayer.PlayFront(PlayerIcons[2]);
            animationPlayer.PlayFront(PlayerIcons[3]);

            animationPlayer.PlayFront(Crown1);
            animationPlayer.PlayFront(Crown2);
        }
    }
}