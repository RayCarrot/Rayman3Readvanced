using System;
using System.Diagnostics;
using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using ImGuiNET;

namespace GbaMonoGame.Rayman3;

public sealed partial class Rayman : MovableActor
{
    public Rayman(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        Debug.Assert(InstanceId < RSMultiplayer.MaxPlayersCount, "The main actor must be the 4 first game objects");

        Resource = actorResource;

        IsLocalPlayer = true;

        if (RSMultiplayer.IsActive)
        {
            if (Rom.Platform == Platform.NGage)
            {
                FlagData = new CaptureTheFlagData
                {
                    PickedUpFlag = null,
                    InvincibilityTimer = 0,
                    SpeedUpTimer = 0,
                    CanPickUpDroppedFlag = true,
                    PlayerPaletteId = 0,
                    Powers = Power.None,
                };
            }

            if (instanceId >= RSMultiplayer.PlayersCount)
            {
                ProcessMessage(this, Message.Destroy);
            }
            else
            {
                // Disable animation sounds if not the local player
                if (instanceId != RSMultiplayer.MachineId)
                {
                    IsLocalPlayer = false;
                    AnimatedObject.IsSoundEnabled = false;
                }

                // This is some semi-hacky code to add the additional multiplayer palettes. The game doesn't store this
                // in the animated object resource to avoid them being allocated in single player. So the game instead
                // manually allocates them to vram here. The easiest solution for us is to override the palettes, appending
                // the multiplayer palettes and then just changing the base palette index based on the player.
                if (Rom.Platform == Platform.NGage &&
                    MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag &&
                    MultiplayerInfo.CaptureTheFlagMode == CaptureTheFlagMode.Teams)
                {
                    // Load the palette resources
                    Palette16 pal3 = Rom.LoadResource<Resource<Palette16>>(Rayman3DefinedResource.Player3RaymanPalette).Value;

                    // In the original game there is a bug causing the flags to render using the wrong colors. This is because the sprite
                    // data does not match the palettes. The correct colors are there though, so we can fix it by re-ordering them.
                    PaletteResource fixPalette(PaletteResource pal)
                    {
                        if (Engine.Config.Tweaks.FixBugs)
                        {
                            PaletteResource fixedPal = new()
                            {
                                Colors =
                                [
                                    pal.Colors[0],
                                    pal.Colors[3], // 1 -> 3
                                    pal.Colors[2],
                                    pal.Colors[3],
                                    pal.Colors[4],
                                    pal.Colors[5],
                                    pal.Colors[6],
                                    pal.Colors[7],
                                    pal.Colors[8],
                                    pal.Colors[9],
                                    pal.Colors[4], // 10 -> 4
                                    pal.Colors[15], // 11 -> 15
                                    pal.Colors[12],
                                    pal.Colors[6], // 13 -> 6
                                    pal.Colors[7], // 14 -> 7
                                    pal.Colors[8], // 15 -> 8
                                ]
                            };

                            // Set the pointer as the original plus 1 so it gets cached differently
                            fixedPal.Init(pal.Offset + 1);

                            return fixedPal;
                        }
                        else
                        {
                            return pal;
                        }
                    }

                    // Create a new sprite palette combining the multiplayer ones
                    SpritePalettes multiplayerPalettes = new(
                        palettes:
                        [
                            // Team 1
                            AnimatedObject.Resource.Palettes.Palettes[0],
                            AnimatedObject.Resource.Palettes.Palettes[1],
                            fixPalette(pal3),
                            
                            // Team 2
                            AnimatedObject.Resource.Palettes.Palettes[0],
                            pal3,
                            fixPalette(AnimatedObject.Resource.Palettes.Palettes[1]),
                        ],
                        // Set the pointer as the original plus 2 so it gets cached differently
                        cachePointer: AnimatedObject.Resource.Palettes.Offset + 2);

                    // Override the palettes
                    AnimatedObject.Palettes = multiplayerPalettes;

                    // Set the base palette index based on the team
                    if (InstanceId is 0 or 1)
                        AnimatedObject.BasePaletteIndex = 0;
                    else if (InstanceId is 2 or 3)
                        AnimatedObject.BasePaletteIndex = 3;

                    FlagData!.PlayerPaletteId = AnimatedObject.BasePaletteIndex + 1;
                }
                else
                {
                    // Load the palette resources
                    Palette16 pal2 = Rom.LoadResource<Resource<Palette16>>(Rayman3DefinedResource.Player2RaymanPalette).Value;
                    Palette16 pal3 = Rom.LoadResource<Resource<Palette16>>(Rayman3DefinedResource.Player3RaymanPalette).Value;
                    Palette16 pal4 = Rom.LoadResource<Resource<Palette16>>(Rayman3DefinedResource.Player4RaymanPalette).Value;

                    // Create a new sprite palette combining the multiplayer ones
                    SpritePalettes multiplayerPalettes = new(
                        palettes:
                        [
                            // Player 1
                            AnimatedObject.Resource.Palettes.Palettes[0],
                            AnimatedObject.Resource.Palettes.Palettes[1],

                            // Player 2
                            AnimatedObject.Resource.Palettes.Palettes[0],
                            pal2,

                            // Player 3
                            AnimatedObject.Resource.Palettes.Palettes[0],
                            pal3,

                            // PLayer 4
                            AnimatedObject.Resource.Palettes.Palettes[0],
                            pal4
                        ],
                        // Set the pointer as the original plus 1 so it gets cached differently
                        cachePointer: AnimatedObject.Resource.Palettes.Offset + 1);

                    // Override the palettes
                    AnimatedObject.Palettes = multiplayerPalettes;

                    // Set the base palette index
                    AnimatedObject.BasePaletteIndex = InstanceId * 2;

                    if (Rom.Platform == Platform.NGage)
                        FlagData!.PlayerPaletteId = AnimatedObject.BasePaletteIndex + 1;
                }

                if (Rom.Platform == Platform.NGage &&
                    MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag &&
                    IsLocalPlayer)
                {
                    AnimatedObjectResource arrowResource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.NGageCaptureTheFlagArrowAnimations);

                    for (int i = 0; i < RSMultiplayer.PlayersCount - 1; i++)
                    {
                        FlagData!.PlayerArrows[i] = new AnimatedObject(arrowResource, arrowResource.IsDynamic)
                        {
                            IsFramed = true,
                            BgPriority = 0,
                            ObjPriority = 2,
                            CurrentAnimation = 1,
                            AffineMatrix = AffineMatrix.Identity,
                            Palettes = AnimatedObject.Palettes,
                            RenderContext = AnimatedObject.RenderContext,
                        };
                    }
                }

                IsInvulnerable = true;
                SetPower(Power.All);
            }
        }

        LastSafePositionBuffer = new SafePosition[10];
        LastSafePositionBufferIndex = 0;

        State.SetTo(Fsm_LevelStart);
    }

    public ActorResource Resource { get; }
    public CaptureTheFlagData FlagData { get; }
    public Action? NextActionId { get; set; }
    public RaymanBody[] ActiveBodyParts { get; } = new RaymanBody[4];
    public BaseActor AttachedObject { get; set; }
    public byte Charge { get; set; }
    public byte HangOnEdgeDelay { get; set; }
    public uint Timer { get; set; }
    public uint InvulnerabilityStartTime { get; set; }
    public byte InvulnerabilityDuration { get; set; }
    public float PreviousXSpeed { get; set; }
    public PhysicalType? SlideType { get; set; } // Game uses 0x20 for null (first not solid type)
    public bool IsSliding => SlideType != null && Math.Abs(PreviousXSpeed) > 1.5f;
    public int PrevHitPoints { get; set; }
    public float PrevSpeedY { get; set; }
    public int CameraTargetY { get; set; }
    public ushort InvisibilityTimer { get; set; }
    public ushort ReverseControlsTimer { get; set; }
    public ushort MultiplayerBlueLumTimer { get; set; }
    public int InitialHitPoints { get; set; }
    public byte ResetCameraOffsetTimer { get; set; }
    public byte FirstLevelIdleTimer { get; set; }
    public byte DisableAttackTimer { get; set; } // Never set to a non-zero value, making it unused
    public bool IsSuperHelicoActive { get; set; }
    public ushort PlumCameraTimer { get; set; } // N-Gage only

    public SafePosition[] LastSafePositionBuffer { get; set; } // Custom to respawn from insta-kill
    public int LastSafePositionBufferIndex { get; set; } // Custom to respawn from insta-kill
    public bool Debug_NoClip { get; set; } // Custom no-clip mode

    // Flags 1
    public bool DisableNearEdge { get; set; }
    public bool UnusedFlag1_1 { get; set; } // Unused
    public bool IsHanging { get; set; }
    public bool DisableWallJumps { get; set; }
    public bool IsInstaKillKnockback { get; set; } // Never set to true
    public bool IsBouncing { get; set; }
    public bool IsInFrontOfLevelCurtain { get; set; }
    public bool StartFlyingWithKegRight { get; set; }
    public bool StartFlyingWithKegLeft { get; set; }
    public bool StopFlyingWithKeg { get; set; }
    public bool DropObject { get; set; }
    public bool SongAlternation { get; set; }
    public bool IsSmallKnockback { get; set; } // Never set to false once it has been set to true
    public bool ResetCameraOffset { get; set; }
    public bool FinishedMap { get; set; }

    // Flags 2
    public bool HasSetJumpSpeed { get; set; }
    public bool TempFlag { get; set; } // General purpose flag, differs depending on the state
    public bool CanJump { get; set; }
    public bool IsLocalPlayer { get; set; }
    public bool CanSafetyJump { get; set; } // Coyote jump

    private void EnableCheats()
    {
        for (int i = 0; i < 3; i++)
        {
            Cheat cheat = (Cheat)(1 << i);

            if (GameInfo.IsCheatEnabled(cheat))
                GameInfo.EnableCheat(Scene, cheat);
        }
    }

    private GbaInput ReverseControls(GbaInput input) => 
        (GbaInput)((input & (GbaInput.Left | GbaInput.Down)) != 0 ? (ushort)input >> 1 : (ushort)input << 1);

    private bool IsDirectionalButtonPressed(GbaInput input)
    {
        if (RSMultiplayer.IsActive)
        {
            if (ReverseControlsTimer != 0)
                input = ReverseControls(input);

            SimpleJoyPad joyPad = MultiJoyPad.GetSimpleJoyPadForCurrentFrame(InstanceId);
            return joyPad.IsButtonPressed(input);
        }
        else
        {
            return JoyPad.IsButtonPressed(input);
        }
    }

    private bool IsDirectionalButtonReleased(GbaInput input)
    {
        if (RSMultiplayer.IsActive)
        {
            if (ReverseControlsTimer != 0)
                input = ReverseControls(input);

            return MultiJoyPad.GetSimpleJoyPadForCurrentFrame(InstanceId).IsButtonReleased(input);
        }
        else
        {
            return JoyPad.IsButtonReleased(input);
        }
    }

    private bool IsDirectionalButtonJustPressed(GbaInput input)
    {
        if (RSMultiplayer.IsActive)
        {
            if (ReverseControlsTimer != 0)
                input = ReverseControls(input);

            return MultiJoyPad.GetSimpleJoyPadForCurrentFrame(InstanceId).IsButtonJustPressed(input);
        }
        else
        {
            return JoyPad.IsButtonJustPressed(input);
        }
    }

    // Unused
    private bool IsDirectionalButtonJustReleased(GbaInput input)
    {
        if (RSMultiplayer.IsActive)
        {
            if (ReverseControlsTimer != 0)
                input = ReverseControls(input);

            return MultiJoyPad.GetSimpleJoyPadForCurrentFrame(InstanceId).IsButtonJustReleased(input);
        }
        else
        {
            return JoyPad.IsButtonJustReleased(input);
        }
    }

    private bool MultiplayerMoveFaster(bool hasNGageBug = false)
    {
        // Get the current tag id
        UserInfoMulti2D userInfo = ((FrameMultiSideScroller)Frame.Current).UserInfo;
        int tagId = userInfo.GetTagId();

        // If you move faster depends on the game type
        return MultiplayerInfo.GameType switch
        {
            MultiplayerGameType.RayTag => InstanceId == tagId,
            MultiplayerGameType.CatAndMouse => InstanceId != tagId,
            // NOTE: Some checks are bugged in the N-Gage version and use the same condition as for Cat and Mouse!
            MultiplayerGameType.CaptureTheFlag when Rom.Platform == Platform.NGage => hasNGageBug && !Engine.Config.Tweaks.FixBugs 
                ? InstanceId != tagId 
                : FlagData.PickedUpFlag == null,
            _ => false
        };
    }

    private void PlaySound(Rayman3SoundEvent soundEventId)
    {
        if (Scene.Camera.LinkedObject == this)
            SoundEventsManager.ProcessEvent(soundEventId);
    }

    private bool IsBossFight()
    {
        // Ly levels use the map id of the previous map, so don't count this as a boss fight then
        if (SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__lyfree))
            return false;

        return GameInfo.MapId is MapId.BossMachine or MapId.BossBadDreams or MapId.BossRockAndLava or MapId.BossScaleMan or MapId.BossFinal_M1;
    }

    private bool CanAttackWithFist(int punchCount)
    {
        if (RSMultiplayer.IsActive && !CanAttackInMultiplayer())
            return false;

        if (ActiveBodyParts[(int)RaymanBody.RaymanBodyPartType.Fist] == null)
            return true;

        if (ActiveBodyParts[(int)RaymanBody.RaymanBodyPartType.SecondFist] == null && punchCount == 2 && HasPower(Power.DoubleFist))
            return true;

        return false;
    }

    private bool CanAttackWithFoot()
    {
        if (RSMultiplayer.IsActive && !CanAttackInMultiplayer())
            return false;

        return ActiveBodyParts[(int)RaymanBody.RaymanBodyPartType.Foot] == null;
    }

    private bool CanAttackWithBody()
    {
        if (RSMultiplayer.IsActive && !CanAttackInMultiplayer())
            return false;

        return ActiveBodyParts[(int)RaymanBody.RaymanBodyPartType.Torso] == null;
    }

    private bool CanAttackInMultiplayer()
    {
        UserInfoMulti2D userInfo = ((FrameMultiSideScroller)Frame.Current).UserInfo;

        if (Rom.Platform == Platform.NGage &&
            MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
            return true;

        int tagId = userInfo.GetTagId();

        if (userInfo.EnergyShots[InstanceId] == 0 || 
            (MultiplayerInfo.GameType == MultiplayerGameType.RayTag && tagId != InstanceId) ||
            (MultiplayerInfo.GameType == MultiplayerGameType.CatAndMouse && tagId == InstanceId))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private bool HasPower(Power power)
    {
        if (Rom.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
            return (FlagData.Powers & power) != 0;
        else
            return GameInfo.IsPowerEnabled(power);
    }

    private void Attack(uint chargePower, RaymanBody.RaymanBodyPartType type, Vector2 offset, bool hasCharged)
    {
        RaymanBody bodyPart = Scene.CreateProjectile<RaymanBody>(ActorType.RaymanBody);

        if (bodyPart == null)
            return;

        if (RSMultiplayer.IsActive)
            ((FrameMultiSideScroller)Frame.Current).UserInfo.DecrementEnergyShots(InstanceId, 1);

        bodyPart.Rayman = this;
        bodyPart.BaseActionId = 0;

        // Hide the body part from Rayman's animations
        switch (type)
        {
            case RaymanBody.RaymanBodyPartType.Fist:
                AnimatedObject.DeactivateChannel(3);
                AnimatedObject.DeactivateChannel(2);
                break;

            case RaymanBody.RaymanBodyPartType.SecondFist:
                AnimatedObject.DeactivateChannel(16);
                AnimatedObject.DeactivateChannel(15);
                break;

            case RaymanBody.RaymanBodyPartType.Foot:
                AnimatedObject.DeactivateChannel(5);
                AnimatedObject.DeactivateChannel(4);
                bodyPart.BaseActionId = 6;
                break;

            case RaymanBody.RaymanBodyPartType.Torso:
                AnimatedObject.DeactivateChannel(12);
                AnimatedObject.DeactivateChannel(11);
                bodyPart.BaseActionId = 12;
                break;

            case RaymanBody.RaymanBodyPartType.SuperFist:
                AnimatedObject.DeactivateChannel(3);
                AnimatedObject.DeactivateChannel(2);
                bodyPart.BaseActionId = 18;
                break;

            case RaymanBody.RaymanBodyPartType.SecondSuperFist:
                AnimatedObject.DeactivateChannel(16);
                AnimatedObject.DeactivateChannel(15);
                bodyPart.BaseActionId = 18;
                break;
        }

        bodyPart.BodyPartType = type;

        if (type == RaymanBody.RaymanBodyPartType.SuperFist)
        {
            ActiveBodyParts[(int)RaymanBody.RaymanBodyPartType.Fist] = bodyPart;
            PlaySound(Rayman3SoundEvent.Play__SuprFist_Mix01);
        }
        else if (type == RaymanBody.RaymanBodyPartType.SecondSuperFist)
        {
            ActiveBodyParts[(int)RaymanBody.RaymanBodyPartType.SecondFist] = bodyPart;
            PlaySound(Rayman3SoundEvent.Play__SuprFist_Mix01);
        }
        else
        {
            ActiveBodyParts[(int)type] = bodyPart;

            if (type != RaymanBody.RaymanBodyPartType.Torso)
            {
                if (ActionId is Action.ChargeFist_Right or Action.ChargeFist_Left or Action.ChargeSecondFist_Right or Action.ChargeSecondFist_Left)
                    PlaySound(Rayman3SoundEvent.Play__RayFist_Mix02);
                else
                    PlaySound(Rayman3SoundEvent.Play__RayFist2_Mix01);
            }
        }

        bodyPart.ChargePower = chargePower;
        bodyPart.HasCharged = hasCharged;

        if (IsFacingLeft)
            offset.X = -offset.X; // Flip x

        bodyPart.Position = Position + offset;

        bodyPart.ActionId = (RaymanBody.Action)(bodyPart.BaseActionId + (IsFacingRight ? 1 : 2));

        if (RSMultiplayer.IsActive && Rom.Platform == Platform.NGage)
        {
            bodyPart.AnimatedObject.Palettes = AnimatedObject.Palettes;
            bodyPart.AnimatedObject.BasePaletteIndex = FlagData.PlayerPaletteId - 1;
        }
        // Optionally fix colors for GBA too, so the body-shot matches the player color
        else if (Engine.Config.Tweaks.FixBugs && RSMultiplayer.IsActive && Rom.Platform == Platform.GBA)
        {
            bodyPart.AnimatedObject.Palettes = AnimatedObject.Palettes;
            bodyPart.AnimatedObject.BasePaletteIndex = AnimatedObject.BasePaletteIndex;
        }
    }

    private void CheckSlide()
    {
        Vector2 pos = Position;
        PhysicalType type = Scene.GetPhysicalType(pos);

        if (type.Value is 
            PhysicalTypeValue.Slide or PhysicalTypeValue.GrabSlide or 
            PhysicalTypeValue.SlideAngle30Left1 or PhysicalTypeValue.SlideAngle30Left2 or 
            PhysicalTypeValue.SlideAngle30Right1 or PhysicalTypeValue.SlideAngle30Right2)
        {
            if (type.Value is PhysicalTypeValue.Slide or PhysicalTypeValue.GrabSlide)
            {
                pos -= new Vector2(0, Tile.Size);
                PhysicalType type2 = Scene.GetPhysicalType(pos);

                if (type2.Value is 
                    PhysicalTypeValue.SlideAngle30Left1 or PhysicalTypeValue.SlideAngle30Left2 or
                    PhysicalTypeValue.SlideAngle30Right1 or PhysicalTypeValue.SlideAngle30Right2)
                    type = type2;
            }

            if (SlideType == null)
            {
                if (Speed.X == 0)
                    PreviousXSpeed = IsFacingRight ? 1 : -1;
                else
                    PreviousXSpeed = Speed.X;
            }

            SlideType = type;
        }
        else
        {
            SlideType = null;
            PlaySound(Rayman3SoundEvent.Stop__SldGreen_SkiLoop1);
        }
    }

    private void ManageSlide()
    {
        if (SlideType == null)
            return;

        MechModel.Speed = new Vector2(PreviousXSpeed, MathHelpers.FromFixedPoint(0x5a001));

        if (IsDirectionalButtonPressed(GbaInput.Left))
        {
            if (PreviousXSpeed > -3)
                PreviousXSpeed -= 0.12109375f;
        }
        else if (IsDirectionalButtonPressed(GbaInput.Right))
        {
            if (PreviousXSpeed < 3)
                PreviousXSpeed += 0.12109375f;
        }
        else
        {
            // Friction
            if (PreviousXSpeed >= 0.05859375f)
            {
                PreviousXSpeed -= 0.015625f;
            }
            else if (PreviousXSpeed <= -0.05859375f)
            {
                PreviousXSpeed += 0.015625f;
            }
            else
            {
                PreviousXSpeed = 0;
            }
        }

        // Slippery
        if (SlideType?.Value is PhysicalTypeValue.SlideAngle30Left1 or PhysicalTypeValue.SlideAngle30Left2)
        {
            PreviousXSpeed -= 0.12109375f;

            if (IsDirectionalButtonPressed(GbaInput.Right))
                PreviousXSpeed -= 0.015625f;
        }
        else if (SlideType?.Value is PhysicalTypeValue.SlideAngle30Right1 or PhysicalTypeValue.SlideAngle30Right2)
        {
            PreviousXSpeed += 0.12109375f;

            if (IsDirectionalButtonPressed(GbaInput.Left))
                PreviousXSpeed += 0.015625f;
        }
    }

    private void MoveInTheAir(float speedX)
    {
        if (IsDirectionalButtonPressed(GbaInput.Left))
        {
            if (IsFacingRight)
                AnimatedObject.FlipX = true;

            speedX -= 1.79998779297f;

            if (speedX <= -3)
                speedX = -3;
        }
        else if (IsDirectionalButtonPressed(GbaInput.Right))
        {
            if (IsFacingLeft)
                AnimatedObject.FlipX = false;

            speedX += 1.79998779297f;

            if (speedX > 3)
                speedX = 3;
        }

        MechModel.Speed = MechModel.Speed with { X = speedX };
    }

    // Unused
    private bool IsFalling()
    {
        return Speed.Y > 1;
    }

    private bool HasLanded()
    {
        if (Speed.Y != 0 || PrevSpeedY < 0)
        {
            PrevSpeedY = Speed.Y;
            return false;
        }

        Box detectionBox = GetDetectionBox();
        
        // Check bottom right
        PhysicalType type = Scene.GetPhysicalType(detectionBox.BottomRight);
        if (type.IsSolid)
        {
            PrevSpeedY = 0;
            return true;
        }

        // Check if on another actor
        if (LinkedMovementActor != null)
        {
            PrevSpeedY = 0;
            return true;
        }

        // Check bottom left
        type = Scene.GetPhysicalType(detectionBox.BottomLeft);
        if (type.IsSolid)
        {
            PrevSpeedY = 0;
            return true;
        }

        // Check bottom middle
        type = Scene.GetPhysicalType(detectionBox.BottomCenter);
        if (type.IsSolid)
        {
            PrevSpeedY = 0;
            return true;
        }

        return false;
    }

    private void BounceJump()
    {
        IsBouncing = false;

        if (Rom.Platform == Platform.NGage)
            ActionId = IsFacingRight ? Action.BouncyJump_Right : Action.BouncyJump_Left;
    }

    private void SlowdownAirSpeed()
    {
        if ((Speed.X > 0 && PreviousXSpeed < 0) ||
            (Speed.X < 0 && PreviousXSpeed > 0))
        {
            PreviousXSpeed = 0;
        }
        else if (PreviousXSpeed > 0)
        {
            PreviousXSpeed -= 0.03125f;

            if (PreviousXSpeed < 0)
                PreviousXSpeed = 0;
        }
        else if (PreviousXSpeed < 0)
        {
            PreviousXSpeed += 0.03125f;

            if (PreviousXSpeed > 0)
                PreviousXSpeed = 0;
        }
    }

    private void AttackInTheAir()
    {
        if (IsActionFinished && ActionId is 
                Action.BeginThrowFistInAir_Right or Action.BeginThrowFistInAir_Left or
                Action.BeginThrowSecondFistInAir_Right or Action.BeginThrowSecondFistInAir_Left or
                Action.Damage_Knockback_Right or Action.Damage_Knockback_Left)
        {
            ActionId = IsFacingRight ? Action.ThrowFistInAir_Right : Action.ThrowFistInAir_Left;
        }

        if (MultiJoyPad.IsButtonJustPressed(InstanceId, GbaInput.B))
        {
            if (CanAttackWithFist(1))
            {
                ActionId = IsFacingRight ? Action.BeginThrowFistInAir_Right : Action.BeginThrowFistInAir_Left;
                Attack(15, RaymanBody.RaymanBodyPartType.Fist, new Vector2(16, -16), false);
            }
            else if (CanAttackWithFist(2))
            {
                ActionId = IsFacingRight ? Action.BeginThrowSecondFistInAir_Right : Action.BeginThrowSecondFistInAir_Left;
                Attack(15, RaymanBody.RaymanBodyPartType.SecondFist, new Vector2(16, -16), false);
            }
        }
    }

    private void CheckForTileDamage()
    {
        Box box = GetVulnerabilityBox();
        box.Bottom -= Tile.Size;

        if (Scene.GetPhysicalType(box.BottomRight) == PhysicalTypeValue.Damage ||
            Scene.GetPhysicalType(box.MiddleRight) == PhysicalTypeValue.Damage ||
            Scene.GetPhysicalType(box.TopRight) == PhysicalTypeValue.Damage ||
            Scene.GetPhysicalType(box.TopCenter) == PhysicalTypeValue.Damage ||
            Scene.GetPhysicalType(box.TopLeft) == PhysicalTypeValue.Damage ||
            Scene.GetPhysicalType(box.MiddleLeft) == PhysicalTypeValue.Damage ||
            Scene.GetPhysicalType(box.BottomLeft) == PhysicalTypeValue.Damage ||
            Scene.GetPhysicalType(box.BottomCenter) == PhysicalTypeValue.Damage)
        {
            ReceiveDamage(1);
        }
    }

    private void OffsetCarryingObject()
    {
        if (AttachedObject != null && AttachedObject.Type != (int)ActorType.Keg)
        {
            if (AttachedObject.Type == (int)ActorType.Caterpillar)
                AttachedObject.Position -= new Vector2(0, 16);

            if (IsFacingLeft)
                AttachedObject.Position -= new Vector2(8, 0);
        }
    }

    private bool ManageHit()
    {
        if (RSMultiplayer.IsActive || GameInfo.IsCheatEnabled(Cheat.Invulnerable))
            return false;

        CheckForTileDamage();

        bool takenDamage = false;

        // Check for taken damage
        if (HitPoints < PrevHitPoints)
        {
            IsInvulnerable = true;
            InvulnerabilityStartTime = GameTime.ElapsedFrames;

            if (InvulnerabilityDuration == 0)
                InvulnerabilityDuration = 120;

            // Drop object if carrying one
            if (AttachedObject != null)
            {
                if ((ActorType)AttachedObject.Type is ActorType.Keg or ActorType.Caterpillar or ActorType.Sphere)
                    AttachedObject.ProcessMessage(this, Message.Actor_Drop);
                AttachedObject = null;
            }

            takenDamage = true;

            if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__OnoRcvH1_Mix04))
                PlaySound(Rayman3SoundEvent.Play__OnoRcvH1_Mix04);
        }

        // Check for invulnerability to end
        if (IsInvulnerable && GameTime.ElapsedFrames - InvulnerabilityStartTime > InvulnerabilityDuration)
        {
            IsInvulnerable = false;
            InvulnerabilityDuration = 0;
        }

        PrevHitPoints = HitPoints;

        return takenDamage;
    }

    private bool CheckDeath()
    {
        Box detectionBox = GetDetectionBox();

        PhysicalTypeValue type = PhysicalTypeValue.None;
        for (int i = 0; i < 3; i++)
        {
            type = Scene.GetPhysicalType(detectionBox.BottomLeft + new Vector2(16 * i, -1));

            if (type is PhysicalTypeValue.InstaKill or PhysicalTypeValue.Lava or PhysicalTypeValue.Water or PhysicalTypeValue.MoltenLava)
                break;
        }

        if (HitPoints == 0 || type is PhysicalTypeValue.InstaKill or PhysicalTypeValue.Lava or PhysicalTypeValue.Water or PhysicalTypeValue.MoltenLava)
        {
            if (State == Fsm_RidingWalkingShell && type is PhysicalTypeValue.InstaKill or PhysicalTypeValue.MoltenLava)
                return false;

            // Drop object if carrying one
            if (AttachedObject != null)
            {
                if ((ActorType)AttachedObject.Type is ActorType.Keg or ActorType.Caterpillar or ActorType.Sphere)
                    AttachedObject.ProcessMessage(this, Message.Actor_Drop);
                AttachedObject = null;
            }

            // Handle drowning
            if (IsLavaInLevel() && type is PhysicalTypeValue.Lava or PhysicalTypeValue.MoltenLava)
            {
                LavaSplash lavaSplash = Scene.CreateProjectile<LavaSplash>(ActorType.LavaSplash);
                if (lavaSplash != null)
                {
                    lavaSplash.Position = Position;
                    lavaSplash.ActionId = LavaSplash.Action.DrownSplash;
                    lavaSplash.ChangeAction();
                }

                ActionId = IsFacingRight ? Action.Drown_Right : Action.Drown_Left;
            }
            else if (type == PhysicalTypeValue.Water)
            {
                WaterSplash waterSplash = Scene.CreateProjectile<WaterSplash>(ActorType.WaterSplash);
                if (waterSplash != null)
                    waterSplash.Position = Position;

                ActionId = IsFacingRight ? Action.Drown_Right : Action.Drown_Left;
            }
            else if (type == PhysicalTypeValue.MoltenLava)
            {
                ActionId = IsFacingRight ? Action.Drown_Right : Action.Drown_Left;
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    private bool ShouldAutoJump()
    {
        // Make sure you're sliding
        if (SlideType == null)
            return false;

        // Make sure you're moving fast enough
        if (Math.Abs(Speed.X) <= 2)
            return false;

        Vector2 topPos = Position;
        Vector2 bottomPos = Position + new Vector2(0, Tile.Size);

        if (Speed.X < 0)
        {
            topPos -= new Vector2(Tile.Size * 2, 0);
            bottomPos -= new Vector2(Tile.Size, 0);
        }
        else
        {
            topPos += new Vector2(Tile.Size * 2, 0);
            bottomPos += new Vector2(Tile.Size, 0);
        }

        PhysicalType topType = Scene.GetPhysicalType(topPos);
        PhysicalType bottomType = Scene.GetPhysicalType(bottomPos);

        if (topType == PhysicalTypeValue.SlideJump)
            return bottomType.IsSolid;

        topPos += new Vector2(Tile.Size, 0);
        bottomPos += new Vector2(Tile.Size, 0);

        topType = Scene.GetPhysicalType(topPos);
        bottomType = Scene.GetPhysicalType(bottomPos);

        if (topType == PhysicalTypeValue.SlideJump) 
            return bottomType.IsSolid;
        
        return false;
    }

    private void SlidingOnSlippery()
    {
        if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__SldGreen_SkiLoop1))
            PlaySound(Rayman3SoundEvent.Play__SldGreen_SkiLoop1);

        SoundEventsManager.SetSoundPitch(Rayman3SoundEvent.Play__SldGreen_SkiLoop1, Math.Abs(Speed.X) * 256);

        if (PreviousXSpeed < -1.5f)
        {
            if (IsFacingRight)
            {
                if (IsDirectionalButtonPressed(GbaInput.Down))
                {
                    if (ActionId != Action.Sliding_Crouch_Right)
                        ActionId = Action.Sliding_Crouch_Right;
                }
                else
                {
                    if (ActionId != Action.Sliding_Slow_Right)
                        ActionId = Action.Sliding_Slow_Right;
                }
            }
            else
            {
                if (IsDirectionalButtonPressed(GbaInput.Down))
                {
                    if (ActionId != Action.Sliding_Crouch_Left)
                        ActionId = Action.Sliding_Crouch_Left;
                }
                else
                {
                    if (ActionId != Action.Sliding_Fast_Left)
                        ActionId = Action.Sliding_Fast_Left;
                }
            }
        }
        else
        {
            if (IsFacingRight)
            {
                if (IsDirectionalButtonPressed(GbaInput.Down))
                {
                    if (ActionId != Action.Sliding_Crouch_Right)
                        ActionId = Action.Sliding_Crouch_Right;
                }
                else
                {
                    if (ActionId != Action.Sliding_Fast_Right)
                        ActionId = Action.Sliding_Fast_Right;
                }
            }
            else
            {
                if (IsDirectionalButtonPressed(GbaInput.Down))
                {
                    if (ActionId != Action.Sliding_Crouch_Left)
                        ActionId = Action.Sliding_Crouch_Left;
                }
                else
                {
                    if (ActionId != Action.Sliding_Slow_Left)
                        ActionId = Action.Sliding_Slow_Left;
                }
            }
        }
    }

    private void CreateSwingProjectiles()
    {
        SwingSparkle swingSparkle;

        int index = 0;
        while (true)
        {
            float dist = index * 16;

            // If close to the purple lum then add sparkles from 64 and downwards
            if (PreviousXSpeed < 80)
                dist = 64 - dist;
            // If far away from the purple lum then add sparkles from the current distance and downwards
            else
                dist = PreviousXSpeed - dist - 16;

            // Stop if the distance is too low
            if (dist <= 2)
                break;

            swingSparkle = Scene.CreateProjectile<SwingSparkle>(ActorType.SwingSparkle);
            swingSparkle.Distance = dist;
            
            index++;
            
            // Normally we only allow a maximum of 8 sparkles, but if Rayman is too far away from the purple lum
            // then this can cause the sparkles to not even reach the purple lum, and the fist projectile won't be
            // created either. So if you have the options set to fix bugs, and also to add projectiles when needed
            // (since we need more than 8!) then allow it to continue creating new sparkle projectiles until
            // reaching the minimum distance.
            if (index > 8 && !(Engine.Config.Tweaks.FixBugs && Engine.Config.Tweaks.AddProjectilesWhenNeeded))
                return;
        }

        // Create the fist
        swingSparkle = Scene.CreateProjectile<SwingSparkle>(ActorType.SwingSparkle);
        swingSparkle.Distance = PreviousXSpeed - 30;
        swingSparkle.AnimatedObject.CurrentAnimation = 1;
    }

    // 0 = false, 1 = right, 2 = left
    private int IsNearEdge()
    {
        Box detectionBox = GetDetectionBox();
        
        PhysicalType centerType = Scene.GetPhysicalType(Position);
        PhysicalType rightType = Scene.GetPhysicalType(detectionBox.BottomRight);
        PhysicalType leftType = Scene.GetPhysicalType(detectionBox.BottomLeft);

        if (centerType.IsSolid)
            return 0;
        else if (leftType.IsSolid)
            return 1;
        else if (rightType.IsSolid)
            return 2;
        else
            return 0;
    }

    private bool IsNearHangableEdge()
    {
        if (HangOnEdgeDelay != 0)
        {
            HangOnEdgeDelay--;
            return false;
        }

        if (Position.Y <= 40)
            return false;

        Vector2 pos = Position - new Vector2(0, 40);

        if (IsFacingRight)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    PhysicalType type = Scene.GetPhysicalType(pos);

                    if (type.Value is PhysicalTypeValue.Grab or PhysicalTypeValue.GrabSlide)
                    {
                        // Get tile to the left of the ledge
                        type = Scene.GetPhysicalType(pos - new Vector2(Tile.Size, 0));
                        
                        // Make sure it's not solid
                        if (!type.IsSolid)
                        {
                            Position = new Vector2(
                                x: pos.X - MathHelpers.Mod(pos.X, Tile.Size) - 17, 
                                y: Position.Y - MathHelpers.Mod(Position.Y, Tile.Size) + y * Tile.Size);
                            return true;
                        }
                    }

                    pos += new Vector2(Tile.Size, 0);
                }

                pos += new Vector2(0, Tile.Size);
                pos.X = Position.X;
            }
        }
        else
        {
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    PhysicalType type = Scene.GetPhysicalType(pos);

                    if (type.Value is PhysicalTypeValue.Grab or PhysicalTypeValue.GrabSlide)
                    {
                        // Get tile to the right of the ledge
                        type = Scene.GetPhysicalType(pos + new Vector2(Tile.Size, 0));

                        // Make sure it's not solid
                        if (!type.IsSolid)
                        {
                            Position = new Vector2(
                                x: pos.X - MathHelpers.Mod(pos.X, Tile.Size) + 24,
                                y: Position.Y - MathHelpers.Mod(Position.Y, Tile.Size) + y * Tile.Size);
                            return true;
                        }
                    }

                    pos -= new Vector2(Tile.Size, 0);
                }

                pos += new Vector2(0, Tile.Size);
                pos.X = Position.X;
            }
        }

        return false;
    }

    private ClimbDirection IsOnClimbableVertical()
    {
        Vector2 pos = Position;

        if (pos.Y <= 48)
            return 0;

        pos -= new Vector2(0, 24);
        PhysicalType bottomType = Scene.GetPhysicalType(pos);

        if (bottomType.Value is 
            PhysicalTypeValue.Spider_Right or 
            PhysicalTypeValue.Spider_Left or 
            PhysicalTypeValue.Spider_Up or 
            PhysicalTypeValue.Spider_Down)
        {
            bottomType = PhysicalTypeValue.Climb;
        }

        pos -= new Vector2(0, 24);
        PhysicalType topType = Scene.GetPhysicalType(pos);

        if (topType.Value is
            PhysicalTypeValue.Spider_Right or
            PhysicalTypeValue.Spider_Left or
            PhysicalTypeValue.Spider_Up or
            PhysicalTypeValue.Spider_Down)
        {
            topType = PhysicalTypeValue.Climb;
        }

        if (bottomType == PhysicalTypeValue.Climb && topType == PhysicalTypeValue.Climb)
            return ClimbDirection.TopAndBottom;

        if (bottomType == PhysicalTypeValue.Climb)
            return ClimbDirection.Bottom;

        if (topType == PhysicalTypeValue.Climb)
            return ClimbDirection.Top;

        return 0;
    }

    private ClimbDirection IsOnClimbableHorizontal()
    {
        Vector2 pos = Position;

        if (pos.Y <= 48)
            return 0;

        pos -= new Vector2(8, 40);
        PhysicalType leftType = Scene.GetPhysicalType(pos);

        if (leftType.Value is
            PhysicalTypeValue.Spider_Right or
            PhysicalTypeValue.Spider_Left or
            PhysicalTypeValue.Spider_Up or
            PhysicalTypeValue.Spider_Down)
        {
            leftType = PhysicalTypeValue.Climb;
        }

        pos += new Vector2(16, 0);
        PhysicalType rightType = Scene.GetPhysicalType(pos);

        if (rightType.Value is
            PhysicalTypeValue.Spider_Right or
            PhysicalTypeValue.Spider_Left or
            PhysicalTypeValue.Spider_Up or
            PhysicalTypeValue.Spider_Down)
        {
            rightType = PhysicalTypeValue.Climb;
        }

        if (leftType == PhysicalTypeValue.Climb && rightType == PhysicalTypeValue.Climb)
            return ClimbDirection.RightAndLeft;

        if (leftType == PhysicalTypeValue.Climb)
            return ClimbDirection.Left;

        if (rightType == PhysicalTypeValue.Climb)
            return ClimbDirection.Right;

        return 0;
    }

    private bool IsOnHangable()
    {
        if (IsHanging)
            return true;

        if (Position.Y <= 48)
            return false;

        PhysicalType type = Scene.GetPhysicalType(Position - new Vector2(0, 56));

        return type == PhysicalTypeValue.Hang;
    }

    private void BeginHang()
    {
        if (IsHanging && AttachedObject != null)
        {
            Position = Position with { Y = AttachedObject.Position.Y + 58 };
            AttachedObject = null;
        }
        else
        {
            Position += new Vector2(0, Tile.Size - MathHelpers.Mod(Position.Y, Tile.Size) - 1);
            PlaySound(Rayman3SoundEvent.Play__HandTap1_Mix04);
        }
    }

    private bool IsOnWallJumpable()
    {
        if (!HasPower(Power.WallJump))
            return false;

        if (DisableWallJumps)
            return false;

        return Scene.GetPhysicalType(Position) == PhysicalTypeValue.WallJump;
    }

    private void BeginWallJump()
    {
        Vector2 pos = Position;

        for (int i = 0; i < 6; i++)
        {
            pos.X += Tile.Size;

            if (Scene.GetPhysicalType(pos) != PhysicalTypeValue.WallJump)
            {
                Position = Position with { X = pos.X - MathHelpers.Mod(pos.X, Tile.Size) - 26 };
                return;
            }
        }
    }

    private void SetRandomIdleAction()
    {
        PlaySound(Rayman3SoundEvent.Stop__Grimace1_Mix04);

        int rand = Random.GetNumber(41);

        if (IsBossFight())
        {
            NextActionId = IsFacingRight ? Action.Idle_Determined_Right : Action.Idle_Determined_Left;
        }
        else if (rand < 5 ||
                 (GameInfo.MapId == MapId.WoodLight_M1 && rand < 20 && GameInfo.LastGreenLumAlive != 0) ||
                 (GameInfo.MapId == MapId.WoodLight_M1 && GameInfo.LastGreenLumAlive == 0))
        {
            NextActionId = IsFacingRight ? Action.Idle_LookAround_Right : Action.Idle_LookAround_Left;
        }
        else if (rand < 10)
        {
            NextActionId = IsFacingRight ? Action.Idle_SpinBody_Right : Action.Idle_SpinBody_Left;
        }
        else if (rand < 15)
        {
            NextActionId = IsFacingRight ? Action.Idle_Bored_Right : Action.Idle_Bored_Left;
        }
        else if (rand < 20)
        {
            NextActionId = IsFacingRight ? Action.Idle_Yoyo_Right : Action.Idle_Yoyo_Left;
        }
        else if (rand < 25)
        {
            NextActionId = IsFacingRight ? Action.Idle_ChewingGum_Right : Action.Idle_ChewingGum_Left;
        }
        else if (rand < 30)
        {
            NextActionId = IsFacingRight ? Action.Idle_BasketBall_Right : Action.Idle_BasketBall_Left;
        }
        else if (rand < 35)
        {
            NextActionId = IsFacingRight ? Action.Idle_Grimace_Right : Action.Idle_Grimace_Left;
            PlaySound(Rayman3SoundEvent.Play__Grimace1_Mix04);
        }
        else
        {
            NextActionId = IsFacingRight ? Action.Idle_ThrowBody_Right : Action.Idle_ThrowBody_Left;
        }
    }

    private bool IsLavaInLevel()
    {
        // The game forgets to check for this level, making it not spawn the lava splash!
        if (Engine.Config.Tweaks.FixBugs)
        {
            if (GameInfo.MapId is MapId.GameCube_Bonus3)
                return true;
        }

        return GameInfo.MapId is 
            MapId.SanctuaryOfStoneAndFire_M1 or 
            MapId.SanctuaryOfStoneAndFire_M2 or 
            MapId.SanctuaryOfStoneAndFire_M3 or 
            MapId.BeneathTheSanctuary_M1 or 
            MapId.BeneathTheSanctuary_M2 or 
            MapId.BossRockAndLava or // Has no lava
            MapId.SanctuaryOfRockAndLava_M1 or 
            MapId.SanctuaryOfRockAndLava_M2 or 
            MapId.SanctuaryOfRockAndLava_M3 or 
            MapId.IronMountains_M1 or 
            MapId.IronMountains_M2 or 
            MapId.MissileRace2 or // Mode7 level, doesn't use the Rayman actor
            MapId.PirateShip_M1 or 
            MapId.PirateShip_M2 or 
            MapId.BossFinal_M1 or // Has no lava
            MapId.Bonus3 or 
            MapId._1000Lums or 
            MapId.GameCube_Bonus6;
    }

    private void AutoSave()
    {
        if ((!FinishedMap || GameInfo.MapId is not (
                MapId.WoodLight_M1 or
                MapId.FairyGlade_M1 or
                MapId.SanctuaryOfBigTree_M1 or
                MapId.EchoingCaves_M1 or
                MapId.CavesOfBadDreams_M1 or
                MapId.MenhirHills_M1 or
                MapId.SanctuaryOfStoneAndFire_M1 or
                MapId.SanctuaryOfStoneAndFire_M2 or
                MapId.BeneathTheSanctuary_M1 or
                MapId.ThePrecipice_M1 or
                MapId.TheCanopy_M1 or
                MapId.SanctuaryOfRockAndLava_M1 or
                MapId.SanctuaryOfRockAndLava_M2 or
                MapId.TombOfTheAncients_M1 or
                MapId.IronMountains_M1 or
                MapId.PirateShip_M1 or
                MapId.BossFinal_M1)) && 
            GameInfo.MapId is not (
                MapId.World1 or 
                MapId.World2 or 
                MapId.World3 or 
                MapId.World4 or 
                MapId.WorldMap))
        {
            GameInfo.PersistentInfo.LastPlayedLevel = (byte)GameInfo.MapId;
            GameInfo.Save(GameInfo.CurrentSlot);
        }
    }

    private bool TryFindPlayerArrowScreenEdgePosition(Vector2 origin, Vector2 dist, Box screenBox, out Vector2 screenEdgePos)
    {
        Box playerArrowBox;
        if (dist.X < 0)
        {
            if (dist.Y < 0)
                playerArrowBox = new Box(origin.X + dist.X, origin.Y + dist.Y, origin.X, origin.Y);
            else
                playerArrowBox = new Box(origin.X + dist.X, origin.Y, origin.X, origin.Y + dist.Y);
        }
        else
        {
            if (dist.Y < 0)
                playerArrowBox = new Box(origin.X, origin.Y + dist.Y, origin.X + dist.X, origin.Y);
            else
                playerArrowBox = new Box(origin.X, origin.Y, origin.X + dist.X, origin.Y + dist.Y);
        }

        if (!DoBoxesIntersect(playerArrowBox, screenBox))
        {
            screenEdgePos = default;
            return false;
        }

        float length = dist.Length();

        if (length == 0)
        {
            screenEdgePos = default;
            return false;
        }

        Vector2 dir = Vector2.Normalize(dist).FlipY();

        Vector2 currentPos = origin;
        for (int i = 0; i < 300; i++)
        {
            BoxOutCode boxOutcode = GetBoxOutcode(currentPos, screenBox);

            if (boxOutcode != BoxOutCode.Inside)
            {
                float yFactor;
                if (dist.Y < 0 && boxOutcode == BoxOutCode.Above)
                    yFactor = -20;
                else
                    yFactor = -12;

                screenEdgePos = new Vector2(
                    x: currentPos.X + dir.X * -12,
                    y: currentPos.Y + dir.Y * yFactor);
                return true;
            }

            currentPos += dir;
        }

        screenEdgePos = default;
        return false;
    }

    private bool DoBoxesIntersect(Box box1, Box box2)
    {
        if (box2.Bottom < box1.Top && 
            box2.Right < box1.Left &&
            box1.Bottom < box2.Top &&
            box1.Right < box2.Left)
        {
            return false;
        }

        return true;
    }

    private BoxOutCode GetBoxOutcode(Vector2 pos, Box box)
    {
        BoxOutCode boxOutCode = BoxOutCode.Inside;

        if (pos.X < box.Left)
            boxOutCode |= BoxOutCode.Left;
        
        if (pos.X >= box.Right)
            boxOutCode |= BoxOutCode.Right;
        
        if (pos.Y < box.Top)
            boxOutCode |= BoxOutCode.Above;
        
        if (pos.Y >= box.Bottom)
            boxOutCode |= BoxOutCode.Below;
        
        return boxOutCode;
    }

    private void DrawPlayerArrows(AnimationPlayer animationPlayer)
    {
        int flagIndex = 0;
        for (int i = 0; i < MultiplayerManager.PlayersCount; i++)
        {
            Rayman player = Scene.GetGameObject<Rayman>(i);
            bool isFramed = Scene.Camera.IsActorFramed(player);

            // Draw the arrow if the player is not on screen and it's not the local player
            if (!isFramed && !player.IsLocalPlayer)
            {
                Vector2 origin = Position - Scene.Playfield.Camera.Position;
                Vector2 dist = (player.Position - Position).FlipY();
                Box screenBox = new(Vector2.Zero, Scene.Resolution);

                if (TryFindPlayerArrowScreenEdgePosition(origin, dist, screenBox, out Vector2 screenEdgePos))
                {
                    // Don't overlap with the HUD
                    if (screenEdgePos.Y < 38)
                    {
                        screenEdgePos.Y = 38;
                    }
                    else if (screenEdgePos.Y > 170 && 
                             MultiplayerInfo.CaptureTheFlagMode == CaptureTheFlagMode.Solo && 
                             MultiplayerManager.PlayersCount > 2)
                    {
                        screenEdgePos.Y = 170;
                    }

                    AnimatedObject playerArrow = FlagData.PlayerArrows[flagIndex];

                    playerArrow.ScreenPos = screenEdgePos;

                    Angle256 dirAngle = MathHelpers.Atan2_256((player.Position - Position).FlipY());
                    playerArrow.AffineMatrix = new AffineMatrix(dirAngle.Inverse(), 1, 1);

                    playerArrow.BasePaletteIndex = player.FlagData.PlayerPaletteId;

                    animationPlayer.Play(playerArrow);

                    flagIndex++;
                }
            }
        }
    }

    private void ToggleNoClip()
    {
        if (Engine.Config.Debug.DebugModeEnabled && InputManager.IsButtonJustPressed(Input.Debug_ToggleNoClip))
        {
            Debug_NoClip = !Debug_NoClip;

            if (Debug_NoClip)
            {
                ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                ChangeAction();
                MechModel.Speed = Vector2.Zero;
            }
            else
            {
                ActionId = IsFacingRight ? Action.Fall_Right : Action.Fall_Left;
                State.MoveTo(Fsm_Fall);
                ChangeAction();
            }
        }
    }

    private void DoNoClipBehavior()
    {
        int speed = JoyPad.IsButtonPressed(GbaInput.A) ? 7 : 4;

        if (JoyPad.IsButtonPressed(GbaInput.Up))
            Position -= new Vector2(0, speed);
        else if (JoyPad.IsButtonPressed(GbaInput.Down))
            Position += new Vector2(0, speed);

        if (JoyPad.IsButtonPressed(GbaInput.Left))
            Position -= new Vector2(speed, 0);
        else if (JoyPad.IsButtonPressed(GbaInput.Right))
            Position += new Vector2(speed, 0);
    }

    private void UpdateSafePosition()
    {
        LastSafePositionBuffer[LastSafePositionBufferIndex] = new SafePosition(Position, IsFacingRight);
      
        LastSafePositionBufferIndex++;
        if (LastSafePositionBufferIndex >= LastSafePositionBuffer.Length)
            LastSafePositionBufferIndex = 0;
    }

    private SafePosition GetSafePosition()
    {
        // Get the safe position from the buffer. This will be one index after the last added one, which is a full loop from now.
        SafePosition safePosition = LastSafePositionBuffer[LastSafePositionBufferIndex];

        // If there is no safe position then fall back to the checkpoint position
        if (safePosition.Position == Vector2.Zero)
        {
            if (GameInfo.LastGreenLumAlive != 0)
                safePosition = new SafePosition(GameInfo.CheckpointPosition, true);
            else
                safePosition = new SafePosition(Resource.Pos.ToVector2(), true);
        }

        return safePosition;
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.RaymanBody_FinishAttack:
                RaymanBody.RaymanBodyPartType bodyPartType = (RaymanBody.RaymanBodyPartType)param;
                ActiveBodyParts[(int)bodyPartType] = null;

                switch (bodyPartType)
                {
                    case RaymanBody.RaymanBodyPartType.Fist:
                    case RaymanBody.RaymanBodyPartType.SuperFist:
                        AnimatedObject.ActivateChannel(3);
                        AnimatedObject.ActivateChannel(2);
                        break;

                    case RaymanBody.RaymanBodyPartType.SecondFist:
                    case RaymanBody.RaymanBodyPartType.SecondSuperFist:
                        AnimatedObject.ActivateChannel(16);
                        AnimatedObject.ActivateChannel(15);
                        break;

                    case RaymanBody.RaymanBodyPartType.Foot:
                        AnimatedObject.ActivateChannel(5);
                        AnimatedObject.ActivateChannel(4);
                        break;

                    case RaymanBody.RaymanBodyPartType.Torso:
                        AnimatedObject.ActivateChannel(12);
                        AnimatedObject.ActivateChannel(11);
                        break;
                }
                return false;

            case Message.Rayman_LinkMovement:
                if (State != Fsm_Dying && State != Fsm_RespawnDeath)
                {
                    if (State == Fsm_Jump && Speed.Y < 1)
                        return false;

                    MovableActor actorToLink = ((MovableActor)param);
                    Box actorToLinkBox = actorToLink.GetDetectionBox();

                    if (Position.Y < actorToLinkBox.Top + 7)
                    {
                        LinkedMovementActor = actorToLink;
                        Position = Position with { Y = actorToLinkBox.Top };
                    }

                    if (State == Fsm_HangOnEdge)
                        State.MoveTo(Fsm_Default);
                }
                return false;

            case Message.Rayman_UnlinkMovement:
                LinkedMovementActor = null;
                CanJump = true;
                return false;

            case Message.Rayman_BeginBounce:
                if (Rom.Platform == Platform.NGage && RSMultiplayer.IsActive)
                {
                    if (State == Fsm_Swing || 
                        State == Fsm_Dying || 
                        State == Fsm_RespawnDeath || 
                        State == Fsm_MultiplayerHit || 
                        State == Fsm_MultiplayerStunned || 
                        State == Fsm_MultiplayerGetUp)
                        return false;
                }
                else
                {
                    if (State == Fsm_Swing || 
                        State == Fsm_Dying || 
                        State == Fsm_RespawnDeath)
                        return false;
                }

                State.MoveTo(Fsm_Bounce);
                return false;

            case Message.Rayman_Bounce:
                if (State == Fsm_Bounce)
                {
                    IsBouncing = true;
                    return false;
                }

                if (Rom.Platform == Platform.NGage && RSMultiplayer.IsActive)
                {
                    if (State == Fsm_MultiplayerHit ||
                        State == Fsm_MultiplayerStunned ||
                        State == Fsm_MultiplayerGetUp)
                        return false;
                }

                ActionId = IsFacingRight ? Action.BouncyJump_Right : Action.BouncyJump_Left;

                State.MoveTo(Fsm_Jump);
                return false;

            case Message.Rayman_CollectYellowLum:
                ((FrameSideScroller)Frame.Current).UserInfo.AddLums(1);
                return false;

            case Message.Rayman_CollectRedLum:
                if (HitPoints < 5 && !Engine.Config.Difficulty.OneHitPoint)
                    HitPoints++;

                ((FrameSideScroller)Frame.Current).UserInfo.UpdateLife();
                return false;

            case Message.Rayman_CollectBlueLum:
                if (!HasPower(Power.SuperHelico)) 
                    return false;
                
                if (!RSMultiplayer.IsActive)
                    GameInfo.IncBlueLumsTime();

                IsSuperHelicoActive = true;
                MultiplayerBlueLumTimer = 300;
                return false;

            case Message.Rayman_CollectWhiteLum:
                if (!RSMultiplayer.IsActive)
                {
                    GameInfo.ModifyLives(1);

                    // Restore hp instead if playing with infinite lives
                    if (Engine.Config.Difficulty.InfiniteLives && !Engine.Config.Difficulty.OneHitPoint)
                    {
                        HitPoints = 5;
                        ((FrameSideScroller)Frame.Current).UserInfo.UpdateLife();
                    }
                }
                return false;

            // Unused
            case Message.Rayman_CollectBigYellowLum:
                ((FrameSideScroller)Frame.Current).UserInfo.AddLums(10);
                return false;

            case Message.Rayman_CollectBigBlueLum:
                if (!HasPower(Power.SuperHelico))
                    return false;

                IsSuperHelicoActive = true;
                MultiplayerBlueLumTimer = 1299;
                return false;

            // Unused
            case Message.Rayman_Victory:
                State.MoveTo(Fsm_Victory);
                return false;

            // Unused
            case Message.Rayman_Determined:
                State.MoveTo(Fsm_Determined);
                return false;

            case Message.Rayman_FinishLevel:
                FinishedMap = true;
                State.MoveTo(Fsm_EndMap);
                return false;

            case Message.Rayman_PickUpObject:
                if (State == Fsm_Walk || State == Fsm_Crawl)
                {
                    AttachedObject = (BaseActor)param;
                    State.MoveTo(Fsm_PickUpObject);
                }
                return false;

            case Message.Rayman_CatchObject:
                if (State == Fsm_Default || State == Fsm_Walk)
                {
                    AttachedObject = (BaseActor)param;
                    State.MoveTo(Fsm_CatchObject);
                }
                return false;

            case Message.Actor_Drop:
                DropObject = true;
                return false;

            case Message.Actor_Hurt:
            case Message.Actor_End: // Unused
            case Message.Rayman_HurtPassthrough:
            case Message.Rayman_HurtSmallKnockback:
                if (State == Fsm_HitKnockback || State == Fsm_Dying || State == Fsm_RespawnDeath || State == Fsm_EndMap || InvulnerabilityDuration != 0)
                    return false;

                if (LinkedMovementActor != null)
                {
                    LinkedMovementActor = null;
                    Position -= new Vector2(0, 7);
                }

                if (AttachedObject is { Type: (int)ActorType.Plum })
                {
                    Box box = ((Plum)AttachedObject).GetActionBox();

                    // NOTE: This is probably a mistake in the original code and meant to set AttachedObject to null. However it
                    //       doesn't matter since the AttachedObject gets overriden further down anyway.
                    LinkedMovementActor = null;

                    Position = Position with { Y = box.Top - 16 };
                }

                if (((BaseActor)sender).Type == (int)ActorType.SpikyFlyingBomb && !IsInvulnerable)
                    InvulnerabilityDuration = 60;

                if (message == Message.Rayman_HurtPassthrough)
                    CheckAgainstObjectCollision = false;
                else if (message == Message.Rayman_HurtSmallKnockback)
                    IsSmallKnockback = true;

                if (AttachedObject != null && (ActorType)AttachedObject.Type is ActorType.Keg or ActorType.Caterpillar or ActorType.Sphere)
                    AttachedObject.ProcessMessage(this, Message.Actor_Drop);

                AttachedObject = (BaseActor)sender;

                if (State == Fsm_Climb)
                    TempFlag = true;

                State.MoveTo(Fsm_HitKnockback);
                return false;

            case Message.Actor_Fall:
                DisableWallJumps = true;
                return false;

            case Message.Rayman_BeginHang:
                IsHanging = true;
                AttachedObject = (BaseActor)param;
                return false;

            case Message.Rayman_EndHang:
                IsHanging = false;
                return false;

            case Message.Rayman_ExitLevel:
                State.MoveTo(Fsm_EndMap);
                return false;

            case Message.Rayman_CollectCage:
                ((FrameSideScroller)Frame.Current).UserInfo.AddCages(1);
                return false;

            case Message.Actor_LightOnFireRight:
            case Message.Actor_LightOnFireLeft:
                if (State != Fsm_HitKnockback && State != Fsm_Dying && State != Fsm_RespawnDeath)
                    State.MoveTo(Fsm_FlyWithKeg);
                return false;

            case Message.Rayman_FlyWithKegRight:
                StartFlyingWithKegRight = true;
                return false;

            case Message.Rayman_FlyWithKegLeft:
                StartFlyingWithKegLeft = true;
                return false;

            case Message.Rayman_EndFlyWithKeg:
                StopFlyingWithKeg = true;
                return false;

            case Message.Actor_Hit:
                if (RSMultiplayer.IsActive)
                {
                    int tagId = ((FrameMultiSideScroller)Frame.Current).UserInfo.TagId;
                    RaymanBody raymanBody = (RaymanBody)param;
                    Rayman attacker = ((RaymanBody)param).Rayman;

                    switch (MultiplayerInfo.GameType)
                    {
                        case MultiplayerGameType.RayTag:
                            if (InstanceId != tagId)
                            {
                                ((FrameMultiSideScroller)Frame.Current).UserInfo.SetTagId(InstanceId);
                                attacker.PlaySound(Rayman3SoundEvent.Play__SuprFist_Mix01);
                                PlaySound(Rayman3SoundEvent.Play__Tag_Mix02);
                            }
                            break;

                        case MultiplayerGameType.CatAndMouse:
                            if (InstanceId == tagId)
                            {
                                ((FrameMultiSideScroller)Frame.Current).UserInfo.SetTagId(attacker.InstanceId);
                                PlaySound(Rayman3SoundEvent.Play__Tag_Mix02);
                                attacker.PlaySound(Rayman3SoundEvent.Play__SuprFist_Mix01);
                            }
                            break;

                        case MultiplayerGameType.CaptureTheFlag when Rom.Platform == Platform.NGage:
                            if (MultiplayerInfo.CaptureTheFlagMode == CaptureTheFlagMode.Solo)
                            {
                                if (FlagData.InvincibilityTimer == 0 && State != Fsm_MultiplayerCapturedFlag)
                                {
                                    AnimatedObject.FlipX = raymanBody.MechModel.Speed.X >= 0;
                                    State.MoveTo(Fsm_MultiplayerHit);
                                }
                            }
                            else
                            {
                                if (FlagData.InvincibilityTimer == 0 && attacker.InstanceId / 2 != InstanceId / 2)
                                {
                                    AnimatedObject.FlipX = raymanBody.MechModel.Speed.X >= 0;
                                    State.MoveTo(Fsm_MultiplayerHit);
                                }
                            }
                            break;
                    }
                }
                return false;

            case Message.Rayman_BeginSwing:
                if (!HasPower(Power.Grab))
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumVioNP_SkulShak_Mix01);
                    return false;
                }

                AttachedObject = (BaseActor)param;
                BaseActor senderObj = (BaseActor)sender;

                if (State == Fsm_Swing)
                    return false;

                if (Position.X < senderObj.Position.X)
                {
                    if (senderObj.Position.X - 200 > Position.X)
                        return false;
                }
                else
                {
                    if (senderObj.Position.X + 200 <= Position.X)
                        return false;
                }

                State.MoveTo(Fsm_Swing);
                return false;

            case Message.Rayman_DetachPlum:
                if (AttachedObject is { Type: (int)ActorType.Plum })
                {
                    State.MoveTo(Fsm_Default);
                    AttachedObject = null;
                }
                return false;

            case Message.Rayman_AttachPlum:
                if (State != Fsm_Dying && State != Fsm_RespawnDeath && AttachedObject is not { Type: (int)ActorType.Plum })
                {
                    NextActionId = IsFacingRight ? Action.Land_Right : Action.Land_Left;
                    State.MoveTo(Fsm_OnPlum);
                    AttachedObject = (BaseActor)param;
                }
                return false;

            case Message.Rayman_AllowSafetyJump:
                if (State != Fsm_Jump && State != Fsm_JumpSlide)
                    CanSafetyJump = true;
                return false;

            case Message.Rayman_QuickFinishBodyShotAttack:
                if (State == Fsm_BodyShotAttack)
                    State.MoveTo(Fsm_QuickFinishBodyShotAttack);
                return false;

            case Message.Rayman_DisableNearEdge:
                DisableNearEdge = true;
                return false;

            case Message.Rayman_Stop:
                State.MoveTo(Fsm_Stop);
                return false;

            case Message.Rayman_Resume:
                if (State == Fsm_Stop || State == Fsm_Cutscene)
                {
                    if (IsOnClimbableVertical() != ClimbDirection.None)
                        State.MoveTo(Fsm_Climb);
                    else
                        State.MoveTo(Fsm_Default);
                }
                return false;

            case Message.Actor_Explode:
                if (RSMultiplayer.IsActive)
                {
                    State.MoveTo(Fsm_MultiplayerDying);
                }
                else
                {
                    if (State != Fsm_Dying && State != Fsm_RespawnDeath && State != Fsm_EndMap)
                        State.MoveTo(Fsm_Dying);
                }
                return false;

            case Message.Rayman_MountWalkingShell:
                if (State != Fsm_RidingWalkingShell && State != Fsm_Dying && State != Fsm_RespawnDeath)
                    State.MoveTo(Fsm_RidingWalkingShell);
                return false;

            case Message.Rayman_UnmountWalkingShell:
                if (State == Fsm_RidingWalkingShell)
                {
                    AttachedObject = (BaseActor)param;
                    Position -= new Vector2(16, 0);
                    State.MoveTo(Fsm_HitKnockback);
                }
                return false;

            case Message.Rayman_CollectMultiItemGlobox:
                ((FrameMultiSideScroller)Frame.Current).UserInfo.InitGlobox(InstanceId);
                PlaySound(Rayman3SoundEvent.Play__LumRed_Mix03);
                return false;

            case Message.Rayman_CollectMultiItemReverse:
                ReverseControlsTimer = 360;
                PlaySound(Rayman3SoundEvent.Play__LumMauve_Mix02);
                return false;

            case Message.Rayman_CollectMultiItemInvisibility:
                InvisibilityTimer = 480;
                AnimatedObject.RenderOptions.BlendMode = BlendMode.AlphaBlend;
                ((FrameMultiSideScroller)Frame.Current).InvisibleActorId = InstanceId;
                PlaySound(Rayman3SoundEvent.Play__LumGreen_Mix04);
                return false;

            case Message.Rayman_CollectMultiItemFist:
                ((FrameMultiSideScroller)Frame.Current).UserInfo.AddEnergyShots(InstanceId, 3);
                PlaySound(Rayman3SoundEvent.Play__LumSlvr_Mix02);
                return false;

            // Unused
            case Message.Rayman_Hide:
                State.MoveTo(Fsm_Hide);
                return false;

            case Message.Rayman_MultiplayerGameOver:
                if (State != Fsm_MultiplayerGameOver)
                    State.MoveTo(Fsm_MultiplayerGameOver);
                return false;

            case Message.Rayman_MultiplayerTagMoved:
                if (State == Fsm_MultiplayerDying && IsLocalPlayer)
                {
                    Scene.Camera.LinkedObject = Scene.GetGameObject<MovableActor>(((FrameMultiSideScroller)Frame.Current).UserInfo.TagId);
                    ((CameraSideScroller)Scene.Camera).HorizontalOffset = CameraOffset.Multiplayer;
                    Scene.Camera.ProcessMessage(this, Message.Cam_MoveToLinkedObject, true);
                }
                return false;

            case Message.Rayman_JumpOffWalkingShell:
                if (State == Fsm_RidingWalkingShell)
                {
                    ActionId = IsFacingRight ? Action.BouncyJump_Right : Action.BouncyJump_Left;
                    State.MoveTo(Fsm_Jump);
                }
                return false;

            case Message.Rayman_EndSuperHelico:
                GameInfo.ResetBlueLumsTime();
                return false;

            case Message.Rayman_EnterLevel:
                if (State != Fsm_EnterLevelCurtain)
                    State.MoveTo(Fsm_EnterLevelCurtain);
                return false;

            case Message.Rayman_BeginInFrontOfLevelCurtain:
                IsInFrontOfLevelCurtain = true;
                return false;

            case Message.Rayman_EndInFrontOfLevelCurtain:
                IsInFrontOfLevelCurtain = false;
                return false;

            case Message.Rayman_BeginCutscene:
                State.MoveTo(Fsm_Cutscene);
                return false;

            case Message.Rayman_HurtShock:
                if (ActionId is not (Action.Damage_Shock_Right or Action.Damage_Shock_Left) && State != Fsm_Climb)
                    ActionId = IsFacingRight ? Action.Damage_Shock_Right : Action.Damage_Shock_Left;
                return false;

            case Message.Rayman_EnterLockedLevel:
                if (State != Fsm_LockedLevelCurtain)
                    State.MoveTo(Fsm_LockedLevelCurtain);
                return false;

            case Message.Rayman_GetPlayerPaletteId when Rom.Platform == Platform.NGage:
                ((MessageRefParam<int>)param).Value = FlagData.PlayerPaletteId;
                return false;

            case Message.Rayman_CollectCaptureTheFlagItem when Rom.Platform == Platform.NGage:
                CaptureTheFlagItems.Action itemAction = (CaptureTheFlagItems.Action)param;

                // The duration is always 300
                const uint duration = 300;

                if (itemAction == CaptureTheFlagItems.Action.Invincibility)
                {
                    if (FlagData.SpeedUpTimer == 0)
                        FlagData.NewState = true;

                    FlagData.SpeedUpTimer = duration;
                }
                else if (itemAction == CaptureTheFlagItems.Action.MagicShoes)
                {
                    FlagData.InvincibilityTimer = duration;
                }
                else if (itemAction == CaptureTheFlagItems.Action.PlayerArrows)
                {
                    if (IsLocalPlayer)
                        FlagData.PlayerArrowsTimer = duration;
                }
                return false;

            case Message.Rayman_GetPickedUpFlag when Rom.Platform == Platform.NGage:
                ((MessageRefParam<CaptureTheFlagFlag>)param).Value = FlagData.PickedUpFlag;
                return false;

            case Message.Rayman_GetCanPickUpDroppedFlag when Rom.Platform == Platform.NGage:
                ((MessageRefParam<bool>)param).Value = FlagData.CanPickUpDroppedFlag;
                return false;

            case Message.Rayman_PickUpFlag when Rom.Platform == Platform.NGage:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play_NGage_Unnamed1);
                RemovePower(Power.All);
                FlagData.PickedUpFlag = (CaptureTheFlagFlag)param;
                FlagData.CanPickUpDroppedFlag = false;
                return false;

            case Message.Rayman_CaptureFlag when Rom.Platform == Platform.NGage:
                if (State != Fsm_MultiplayerCapturedFlag)
                    State.MoveTo(Fsm_MultiplayerCapturedFlag);
                return false;

            case Message.Rayman_SpectateTiedPlayer when Rom.Platform == Platform.NGage:
                // Drop the flag if one is picked up
                if (FlagData.PickedUpFlag != null)
                {
                    AnimatedObject.DeactivateChannel(4);
                    FlagData.PickedUpFlag.ProcessMessage(this, Message.CaptureTheFlagFlag_Drop);
                    FlagData.PickedUpFlag = null;
                }

                // Can't pick up a flag again
                FlagData.CanPickUpDroppedFlag = false;

                if (State != Fsm_MultiplayerDying) 
                    State.MoveTo(Fsm_MultiplayerDying);
                
                FlagData.SpectatePlayerId = (int)param;
                return false;

            default:
                return false;
        }
    }

    public void SetPower(Power power)
    {
        if (Rom.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
            FlagData.Powers |= power;
        else
            GameInfo.EnablePower(power);
    }

    public void RemovePower(Power power)
    {
        if (Rom.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
            FlagData.Powers &= ~power;
        else
            GameInfo.DisablePower(power);
    }

    // Disable collision when debug mode is on
    public override Box GetAttackBox() => Debug_NoClip ? Box.Empty : base.GetAttackBox();
    public override Box GetVulnerabilityBox() => Debug_NoClip ? Box.Empty : base.GetVulnerabilityBox();
    public override Box GetDetectionBox() => Debug_NoClip ? Box.Empty : base.GetDetectionBox();
    public override Box GetActionBox() => Debug_NoClip ? Box.Empty : base.GetActionBox();

    public override void Init(ActorResource actorResource)
    {
        if (Engine.Config.Difficulty.OneHitPoint)
            HitPoints = 1;

        AnimatedObject.ObjPriority = IsLocalPlayer ? 16 : 17;

        Timer = 0;
        InvulnerabilityStartTime = 0;
        MultiplayerBlueLumTimer = 0;
        
        if (Rom.Platform == Platform.NGage)
            PlumCameraTimer = 0;

        NextActionId = null;
        Array.Clear(ActiveBodyParts);

        // Reset flags
        DisableNearEdge = false;
        UnusedFlag1_1 = false;
        IsHanging = false;
        DisableWallJumps = false;
        IsInstaKillKnockback = false;
        IsBouncing = false;
        IsInFrontOfLevelCurtain = false;
        StartFlyingWithKegRight = false;
        StartFlyingWithKegLeft = false;
        StopFlyingWithKeg = false;
        DropObject = false;
        SongAlternation = false;
        IsSmallKnockback = false;
        ResetCameraOffset = false;
        FinishedMap = false;

        PrevHitPoints = HitPoints;
        PrevSpeedY = 0;
        PreviousXSpeed = 0;
        SlideType = null;
        AttachedObject = null;
        CameraTargetY = 0;
        InvisibilityTimer = 0;
        ReverseControlsTimer = 0;
        InitialHitPoints = HitPoints;
        DisableAttackTimer = 0;
        HangOnEdgeDelay = 0;
        InvulnerabilityDuration = 0;
        Charge = 0;
        FirstLevelIdleTimer = 0;

        // Reset flags
        HasSetJumpSpeed = false;
        IsSuperHelicoActive = false;
        TempFlag = false;
        CanJump = true;

        CheckAgainstMapCollision = true;
        CheckAgainstObjectCollision = true;

        // Set the initial action
        ActionId = (Action)Resource.FirstActionId;
        ChangeAction();

        // Respawn at last green lum
        if (GameInfo.LastGreenLumAlive != 0)
        {
            Position = GameInfo.CheckpointPosition;
            ActionId = Action.Idle_Right;
            ChangeAction();
        }
        // Start facing left when returning from certain levels
        else if (GameInfo.MapId is MapId.World1 or MapId.World2 or MapId.World3 or MapId.World4 &&
                 !GameInfo.IsInWorldMap &&
                 (MapId)GameInfo.PersistentInfo.LastPlayedLevel is 
                 MapId.MarshAwakening1 or
                 MapId.BossMachine or
                 MapId.MissileRace1 or
                 MapId.MarshAwakening2 or
                 MapId.BeneathTheSanctuary_M1 or
                 MapId.BeneathTheSanctuary_M2 or
                 MapId.BossRockAndLava or
                 MapId.SanctuaryOfRockAndLava_M1 or
                 MapId.SanctuaryOfRockAndLava_M3 or
                 MapId.BossScaleMan or
                 MapId.Bonus4 or
                 MapId.Power4)
        {
            ActionId = Action.Idle_Left;
            ChangeAction();
        }

        UpdateSafePosition();

        GameInfo.IsInWorldMap = false;

        EnableCheats();
    }

    public override void DoBehavior()
    {
        if (Debug_NoClip)
            DoNoClipBehavior();
        else
            base.DoBehavior();
    }

    public override void Step()
    {
        base.Step();

        if (Engine.Config.Difficulty.OneHitPoint && HitPoints > 1)
        {
            HitPoints = 1;
            PrevHitPoints = HitPoints;
            InitialHitPoints = HitPoints;
        }

        if (IsLinkedCameraObject())
            ToggleNoClip();

        if (SlideType != null && NewAction)
            MechModel.Init(1);

        if (IsSuperHelicoActive && MultiplayerBlueLumTimer != 1299)
        {
            if (RSMultiplayer.IsActive)
            {
                MultiplayerBlueLumTimer--;
                if (MultiplayerBlueLumTimer == 0)
                    IsSuperHelicoActive = false;
            }
            else
            {
                if (GameInfo.IsBlueLumsTimeOver())
                    IsSuperHelicoActive = false;
            }
        }

        if (InvisibilityTimer != 0)
        {
            InvisibilityTimer--;

            if (InstanceId == MultiplayerManager.MachineId)
                AnimatedObject.GbaAlpha = 16 - Math.Abs(InvisibilityTimer % 20 - 10);
            else if (InvisibilityTimer >= 424)
                AnimatedObject.GbaAlpha = 16 - (480 - InvisibilityTimer / 4f);
            else if (InvisibilityTimer >= 57)
                AnimatedObject.GbaAlpha = 2;
            else
                AnimatedObject.GbaAlpha = 16 - InvisibilityTimer / 4f;

            if (InvisibilityTimer == 0)
            {
                AnimatedObject.RenderOptions.BlendMode = BlendMode.None;
                ((FrameMultiSideScroller)Frame.Current).InvisibleActorId = -1;
            }
        }

        if (ReverseControlsTimer != 0)
            ReverseControlsTimer--;

        if (Rom.Platform == Platform.NGage)
        {
            CameraSideScroller cam = (CameraSideScroller)Scene.Camera;

            // Force default camera offset when jumping off of a plum on N-Gage
            if (PlumCameraTimer != 0 && 
                (AttachedObject == null || (ActorType)AttachedObject.Type != ActorType.Plum))
            {
                if (IsFacingRight)
                    cam.HorizontalOffset = Speed.X < 0 ? CameraOffset.DefaultReversed : CameraOffset.Default;
                else
                    cam.HorizontalOffset = Speed.X < 0 ? CameraOffset.Default : CameraOffset.DefaultReversed;

                PlumCameraTimer--;
            }

            // Special camera code for the exclusive N-Gage falling levels
            if ((GameInfo.MapId == MapId.MarshAwakening2 || GameInfo.MapId == MapId.MissileRace2) && Speed.Y > 0)
            {
                cam.HorizontalOffset = CameraOffset.Multiplayer;
                CameraTargetY = 70;
                cam.ProcessMessage(this, Message.Cam_FollowPositionY, CameraTargetY);
            }

            if (RSMultiplayer.IsActive && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
            {
                if (FlagData.InvincibilityTimer != 0)
                    FlagData.InvincibilityTimer--;

                uint prevSpeedUpTimer = FlagData.SpeedUpTimer;

                if (FlagData.SpeedUpTimer != 0)
                    FlagData.SpeedUpTimer--;

                // Rayman has entered a new state
                if (FlagData.NewState)
                {
                    // Speed up horizontal speed by 1.5 if using magic shoes power-up
                    if (FlagData.SpeedUpTimer != 0)
                        MechModel.Speed *= new Vector2(1.5f, 1);

                    FlagData.NewState = false;
                }
                else
                {
                    // If the magic shoes has just run out, then slow back down
                    if (prevSpeedUpTimer != 0 && FlagData.SpeedUpTimer == 0)
                        MechModel.Speed *= new Vector2(1 / 1.5f, 1);
                }
            }
        }
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if (Rom.Platform == Platform.NGage && RSMultiplayer.IsActive && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
        {
            // Determine if the flag should be visible in the animation
            if (FlagData.PickedUpFlag == null)
                AnimatedObject.DeactivateChannel(4);
            else
                AnimatedObject.ActivateChannel(4);

            // Unused
            if (FlagData.PlayerArrowsTimer != 0 && IsLocalPlayer)
            {
                FlagData.PlayerArrowsTimer--;
                DrawPlayerArrows(animationPlayer);
            }
        }

        CameraActor camera = Scene.Camera;

        bool draw = camera.IsActorFramed(this) || forceDraw;

        // NOTE: Instead of checking ElapsedFrames the N-Gage version checks the amount of drawn frames here (since it renders in 30fps)
        // Conditionally don't draw every second frame during invulnerability
        if (draw)
        {
            if (IsInvulnerable &&
                !GameInfo.IsCheatEnabled(Cheat.Invulnerable) && 
                !RSMultiplayer.IsActive && 
                HitPoints != 0 && 
                (GameTime.ElapsedFrames & 1) == 0)
            {
                draw = false;
            }

            if (Rom.Platform == Platform.NGage &&
                RSMultiplayer.IsActive &&
                MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag &&
                FlagData.InvincibilityTimer != 0 &&
                (GameTime.ElapsedFrames & 1) != 0)
            {
                draw = false;
            }
        }

        if (draw)
        {
            AnimatedObject.IsFramed = true;
            animationPlayer.Play(AnimatedObject);
        }
        else
        {
            AnimatedObject.IsFramed = false;
            AnimatedObject.PlayChannelBox();
            AnimatedObject.ComputeNextFrame();
        }
    }

    public override void DrawDebugLayout(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    {
        base.DrawDebugLayout(debugLayout, textureManager);
        ImGui.Text($"IsInFrontOfLevelCurtain: {IsInFrontOfLevelCurtain}");
        ImGui.Text($"PreviousXSpeed: {PreviousXSpeed}");
        ImGui.Text($"Timer: {Timer}");
    }

    public class CaptureTheFlagData
    {
        public CaptureTheFlagFlag PickedUpFlag { get; set; }
        public AnimatedObject[] PlayerArrows { get; } = new AnimatedObject[RSMultiplayer.MaxPlayersCount - 1];
        public uint InvincibilityTimer { get; set; }
        public uint SpeedUpTimer { get; set; }
        public uint PlayerArrowsTimer { get; set; }
        public bool CanPickUpDroppedFlag { get; set; }
        public bool NewState { get; set; }
        public int PlayerPaletteId { get; set; }
        public Power Powers { get; set; }
        public int SpectatePlayerId { get; set; }
    }

    public readonly struct SafePosition(Vector2 position, bool isFacingRight)
    {
        public Vector2 Position { get; } = position;
        public bool IsFacingRight { get; } = isFacingRight;
    }

    [Flags]
    private enum BoxOutCode
    {
        Inside = 0,
        Right = 1,
        Above = 2,
        Left = 4,
        Below = 8
    }

    private enum ClimbDirection
    {
        None = 0,
        TopAndBottom = 1,
        Top = 2,
        Bottom = 3,
        RightAndLeft = 4,
        Right = 5,
        Left = 6,
    }
}