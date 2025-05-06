using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3;

public class FlagBar : Bar
{
    public FlagBar(Scene2D scene) : base(scene)
    {
        Players = new FlagBarPlayer[RSMultiplayer.MaxPlayersCount];
        BlinkPlayerId = -1;
        Timer = 0;
        CaptureTheFlagMode = MultiplayerInfo.CaptureTheFlagMode;
        DrawStep = BarDrawStep.Wait;
    }

    public FlagBarPlayer[] Players { get; }
    public int BlinkPlayerId { get; set; }
    public ushort Timer { get; set; }
    public CaptureTheFlagMode CaptureTheFlagMode { get; set; }

    private void DrawTeams(AnimationPlayer animationPlayer)
    {
        FrameMultiCaptureTheFlag frame = (FrameMultiCaptureTheFlag)Frame.Current;

        if (frame.TargetFlagsCount == 0)
        {
            for (int i = 0; i < 2; i++)
            {
                int playerId = MultiplayerHelpers.HudIndexToMachineId(i);

                if (BlinkPlayerId != playerId || (Timer & 2) == 0)
                {
                    FlagBarPlayer player = Players[playerId];

                    if (frame.PlayerFlagCounts[playerId] > 9)
                    {
                        player.FlagsDigit1.CurrentAnimation = frame.PlayerFlagCounts[playerId] / 10;
                        animationPlayer.Play(player.FlagsDigit1);

                        if ((i & 1) == 0)
                            player.FlagsDigit2.ScreenPos = player.FlagsDigit2.ScreenPos with { X = player.FlagsDigit1.ScreenPos.X + 12 };
                    }

                    player.FlagsDigit2.CurrentAnimation = frame.PlayerFlagCounts[playerId] % 10;
                    animationPlayer.Play(player.FlagsDigit2);
                }
            }
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                if (BlinkPlayerId != i || (Timer & 2) == 0)
                {
                    FlagBarPlayer player = Players[i];

                    player.FlagsDigit2.CurrentAnimation = frame.PlayerFlagCounts[i];

                    animationPlayer.Play(player.Slash);
                    animationPlayer.Play(player.FlagsDigit2);
                    animationPlayer.Play(player.TargetFlagsDigit);
                }
            }
        }
    }

    public override void Load()
    {
        FrameMultiCaptureTheFlag frame = (FrameMultiCaptureTheFlag)Frame.Current;

        for (int i = 0; i < Players.Length; i++)
        {
            FlagBarPlayer player = new();

            AnimatedObjectResource resource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.HudAnimations);

            player.FlagsDigit2 = new AnimatedObject(resource, false)
            {
                IsFramed = true,
                CurrentAnimation = 0,
                BgPriority = 0,
                ObjPriority = 0,
                RenderContext = Scene.HudRenderContext,
            };

            if (frame.TargetFlagsCount == 0)
            {
                player.FlagsDigit1 = new AnimatedObject(resource, false)
                {
                    IsFramed = true,
                    CurrentAnimation = 0,
                    BgPriority = 0,
                    ObjPriority = 0,
                    RenderContext = Scene.HudRenderContext,
                };
            }
            else
            {
                player.Slash = new AnimatedObject(resource, false)
                {
                    IsFramed = true,
                    CurrentAnimation = 41,
                    BgPriority = 0,
                    ObjPriority = 0,
                    RenderContext = Scene.HudRenderContext,
                };
                player.TargetFlagsDigit = new AnimatedObject(resource, false)
                {
                    IsFramed = true,
                    CurrentAnimation = 0,
                    BgPriority = 0,
                    ObjPriority = 0,
                    RenderContext = Scene.HudRenderContext,
                };
            }

            Players[i] = player;
        }

        if (CaptureTheFlagMode == CaptureTheFlagMode.Teams)
        {
            for (int i = 0; i < 2; i++)
            {
                float xPos;
                HorizontalAnchorMode horizontalAnchor;
                if ((i & 1) == 0)
                {
                    xPos = 24;
                    horizontalAnchor = HorizontalAnchorMode.Left;
                }
                else
                {
                    xPos = frame.TargetFlagsCount == 0 ? -40 : -52;
                    horizontalAnchor = HorizontalAnchorMode.Right;
                }

                const float yPos = 22;
                const VerticalAnchorMode verticalAnchor = VerticalAnchorMode.Top;

                int teamId;
                if ((i == 0 && MultiplayerManager.MachineId < 2) ||
                    (i == 1 && MultiplayerManager.MachineId >= 2))
                {
                    teamId = 0;
                }
                else
                {
                    teamId = 1;
                }

                if (frame.TargetFlagsCount == 0)
                {
                    Players[teamId].FlagsDigit1.ScreenPos = new Vector2(xPos, yPos);
                    Players[teamId].FlagsDigit1.HorizontalAnchor = horizontalAnchor;
                    Players[teamId].FlagsDigit1.VerticalAnchor = verticalAnchor;

                    if ((i & 1) != 0)
                        xPos += 12;

                    Players[teamId].FlagsDigit2.ScreenPos = new Vector2(xPos, yPos);
                    Players[teamId].FlagsDigit2.HorizontalAnchor = horizontalAnchor;
                    Players[teamId].FlagsDigit2.VerticalAnchor = verticalAnchor;
                }
                else
                {
                    Players[teamId].FlagsDigit2.ScreenPos = new Vector2(xPos, yPos);
                    Players[teamId].FlagsDigit2.HorizontalAnchor = horizontalAnchor;
                    Players[teamId].FlagsDigit2.VerticalAnchor = verticalAnchor;

                    xPos += 12;
                    Players[teamId].Slash.ScreenPos = new Vector2(xPos, yPos);
                    Players[teamId].Slash.HorizontalAnchor = horizontalAnchor;
                    Players[teamId].Slash.VerticalAnchor = verticalAnchor;

                    xPos += 12;
                    Players[teamId].TargetFlagsDigit.ScreenPos = new Vector2(xPos, yPos);
                    Players[teamId].TargetFlagsDigit.HorizontalAnchor = horizontalAnchor;
                    Players[teamId].TargetFlagsDigit.VerticalAnchor = verticalAnchor;
                }

                DrawStep = BarDrawStep.Wait;
            }
        }
        else
        {
            for (int i = 0; i < RSMultiplayer.MaxPlayersCount; i++)
            {
                float xPos;
                HorizontalAnchorMode horizontalAnchor;
                if ((i & 1) == 0)
                {
                    xPos = 24;
                    horizontalAnchor = HorizontalAnchorMode.Left;
                }
                else
                {
                    xPos = frame.TargetFlagsCount == 0 ? -40 : -52;
                    horizontalAnchor = HorizontalAnchorMode.Right;
                }

                float yPos;
                VerticalAnchorMode verticalAnchor;
                if ((i & 2) == 0)
                {
                    yPos = 22;
                    verticalAnchor = VerticalAnchorMode.Top;
                }
                else
                {
                    yPos = -6;
                    verticalAnchor = VerticalAnchorMode.Bottom;
                }

                int playerId = MultiplayerHelpers.HudIndexToMachineId(i);

                if (frame.TargetFlagsCount == 0)
                {
                    Players[playerId].FlagsDigit1.ScreenPos = new Vector2(xPos, yPos);
                    Players[playerId].FlagsDigit1.HorizontalAnchor = horizontalAnchor;
                    Players[playerId].FlagsDigit1.VerticalAnchor = verticalAnchor;

                    if ((i & 1) != 0)
                        xPos += 12;

                    Players[playerId].FlagsDigit2.ScreenPos = new Vector2(xPos, yPos);
                    Players[playerId].FlagsDigit2.HorizontalAnchor = horizontalAnchor;
                    Players[playerId].FlagsDigit2.VerticalAnchor = verticalAnchor;
                }
                else
                {
                    Players[playerId].FlagsDigit2.ScreenPos = new Vector2(xPos, yPos);
                    Players[playerId].FlagsDigit2.HorizontalAnchor = horizontalAnchor;
                    Players[playerId].FlagsDigit2.VerticalAnchor = verticalAnchor;

                    xPos += 12;
                    Players[playerId].Slash.ScreenPos = new Vector2(xPos, yPos);
                    Players[playerId].Slash.HorizontalAnchor = horizontalAnchor;
                    Players[playerId].Slash.VerticalAnchor = verticalAnchor;

                    xPos += 12;
                    Players[playerId].TargetFlagsDigit.ScreenPos = new Vector2(xPos, yPos);
                    Players[playerId].TargetFlagsDigit.HorizontalAnchor = horizontalAnchor;
                    Players[playerId].TargetFlagsDigit.VerticalAnchor = verticalAnchor;
                }

                DrawStep = BarDrawStep.Wait;
            }
        }

        BlinkPlayerId = -1;
    }

    public override void Set()
    {
        FrameMultiCaptureTheFlag frame = (FrameMultiCaptureTheFlag)Frame.Current;

        // Set the target flags count digits
        for (int i = 0; i < RSMultiplayer.MaxPlayersCount; i++)
        {
            if (frame.TargetFlagsCount != 0)
            {
                int playerId = MultiplayerHelpers.HudIndexToMachineId(i);
                Players[playerId].TargetFlagsDigit.CurrentAnimation = frame.TargetFlagsCount;
            }
        }
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        if (Mode is BarMode.StayHidden or BarMode.Disabled)
            return;

        Timer++;

        if (CaptureTheFlagMode == CaptureTheFlagMode.Teams)
        {
            DrawTeams(animationPlayer);
        }
        else
        {
            if (DrawStep == BarDrawStep.Wait)
            {
                FrameMultiCaptureTheFlag frame = (FrameMultiCaptureTheFlag)Frame.Current;

                if (frame.TargetFlagsCount == 0)
                {
                    for (int i = 0; i < MultiplayerManager.PlayersCount; i++)
                    {
                        int playerId = MultiplayerHelpers.HudIndexToMachineId(i);

                        if (BlinkPlayerId != playerId || (Timer & 2) == 0)
                        {
                            FlagBarPlayer player = Players[playerId];

                            if (frame.PlayerFlagCounts[playerId] > 9 && frame.TargetFlagsCount == 0)
                            {
                                player.FlagsDigit1.CurrentAnimation = frame.PlayerFlagCounts[playerId] / 10;
                                animationPlayer.Play(player.FlagsDigit1);
                                
                                if ((i & 1) == 0)
                                    player.FlagsDigit2.ScreenPos = player.FlagsDigit2.ScreenPos with { X = player.FlagsDigit1.ScreenPos.X + 12 };
                            }

                            player.FlagsDigit2.CurrentAnimation = frame.PlayerFlagCounts[playerId] % 10;
                            animationPlayer.Play(player.FlagsDigit2);
                            
                            if (frame.TargetFlagsCount != 0)
                            {
                                animationPlayer.Play(player.Slash);
                                animationPlayer.Play(player.TargetFlagsDigit);
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < MultiplayerManager.PlayersCount; i++)
                    {
                        int playerId = MultiplayerHelpers.HudIndexToMachineId(i);

                        if (BlinkPlayerId != playerId || (Timer & 2) == 0)
                        {
                            FlagBarPlayer player = Players[playerId];

                            player.FlagsDigit2.CurrentAnimation = frame.PlayerFlagCounts[playerId];

                            animationPlayer.Play(player.Slash);
                            animationPlayer.Play(player.FlagsDigit2);
                            animationPlayer.Play(player.TargetFlagsDigit);
                        }
                    }
                }
            }
        }
    }

    public class FlagBarPlayer
    {
        public AnimatedObject Slash { get; set; }
        public AnimatedObject FlagsDigit2 { get; set; }
        public AnimatedObject FlagsDigit1 { get; set; }
        public AnimatedObject TargetFlagsDigit { get; set; }
    }
}