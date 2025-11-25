using System;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Plum
{
    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Grow;
                Timer = 0;
                break;

            case FsmAction.Step:
                // Pause animation until it's on screen
                if (Timer == 0)
                {
                    if (ScreenPosition.X >= 25 && 
                        ScreenPosition.X <= Scene.Resolution.X - 25)
                    {
                        Timer = 1;
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PlumSnd2_Mix03);
                    }
                    else
                    {
                        AnimatedObject.CurrentFrame = 0;
                    }
                }

                // Hit
                if (ActionId == Action.Hit && IsActionFinished)
                {
                    State.MoveTo(Fsm_Fall);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Fall(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Fall;
                break;

            case FsmAction.Step:
                Box detectionBox = GetDetectionBox();
                PhysicalType groundType = Scene.GetPhysicalType(new Vector2(Position.X, detectionBox.Bottom));

                // Land in lava
                if (groundType == PhysicalTypeValue.Lava)
                {
                    if (AnimatedObject.IsFramed)
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SplshGen_Mix04);

                    State.MoveTo(Fsm_Float);
                    return false;
                }

                // Land on solid (not used in any level)
                if (groundType.IsSolid)
                {
                    State.MoveTo(Fsm_Bounce);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Float(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Float;
                ChangeAction();
                MechModel.Speed = MechModel.Speed with { Y = 0 };
                Timer = 1;
                ChargePower = 0;
                FloatSpeedX = 0;
                SpawnLavaSplash();
                LavaSplash.ActionId = LavaSplash.Action.PlumLandedSplash;
                break;

            case FsmAction.Step:
                bool fall = false;
                bool idle = false;

                Box detectionBox = GetDetectionBox();
                Rayman mainActor = (Rayman)Scene.MainActor;

                // Attach to main actor
                if (Scene.IsDetectedMainActor(this) && mainActor.AttachedObject != this && mainActor.Position.Y <= Position.Y)
                {
                    if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__VibraFLW_Mix02) && Scene.MainActor.HitPoints != 0)
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__VibraFLW_Mix02);

                    mainActor.ProcessMessage(this, Message.Rayman_AttachPlum, this);

                    // Set initial speed
                    if (ShouldSetInitialSpeed)
                    {
                        if (mainActor.Speed.X < 0)
                        {
                            FloatSpeedX = -1;
                        }
                        else if (mainActor.Speed.X > 0)
                        {
                            FloatSpeedX = 1;
                        }
                        else
                        {
                            if (mainActor.IsFacingLeft)
                                FloatSpeedX = -0.5f;
                            else
                                FloatSpeedX = 0.5f;
                        }

                        ShouldSetInitialSpeed = false;
                    }
                }

                // Set main actor position
                if (mainActor.AttachedObject == this)
                    mainActor.Position = new Vector2(Position.X, detectionBox.Top);

                // Update speed
                MechModel.Speed = new Vector2(FloatSpeedX, 2);

                PhysicalType groundType = Scene.GetPhysicalType(new Vector2(Position.X, detectionBox.Bottom));
                if (groundType == PhysicalTypeValue.Solid)
                {
                    groundType = Scene.GetPhysicalType(new Vector2(Position.X, detectionBox.Bottom) + Tile.Up);

                    if (groundType == PhysicalTypeValue.Lava)
                        groundType = PhysicalTypeValue.Solid;
                }

                float targetSpeed = ChargePower == MinChargePower ? 2 : 3;

                // Angle right
                if (groundType.Value is PhysicalTypeValue.SolidAngle30Right1 or PhysicalTypeValue.SolidAngle30Right2)
                {
                    if (ActionId != Action.FloatAngle)
                        ActionId = Action.FloatAngle;

                    if (FloatSpeedX < targetSpeed)
                        FloatSpeedX += MathHelpers.FromFixedPoint(0x400);
                    else
                        FloatSpeedX = targetSpeed;

                    Timer++;
                }
                // Angle left
                else if (groundType.Value is PhysicalTypeValue.SolidAngle30Left1 or PhysicalTypeValue.SolidAngle30Left2)
                {
                    if (ActionId != Action.FloatAngle)
                        ActionId = Action.FloatAngle;

                    if (FloatSpeedX > -targetSpeed)
                        FloatSpeedX -= MathHelpers.FromFixedPoint(0x400);
                    else
                        FloatSpeedX = -targetSpeed;

                    Timer++;
                }
                // NOTE: There's a bug here where the plum doesn't think it's in the air if it touches
                //       climb collision, which makes it play the wrong animation
                // In the air
                else if (groundType == PhysicalTypeValue.None || 
                         (Engine.ActiveConfig.Tweaks.FixBugs && groundType == PhysicalTypeValue.Climb))
                {
                    if (!DisableMessages)
                    {
                        DisableMessages = true;

                        LinkedMovementActor = null;
                        if (LavaSplash != null)
                        {
                            LavaSplash.ProcessMessage(this, Message.Destroy);
                            LavaSplash.LinkedMovementActor = null;
                        }

                        ActionId = Action.Idle;
                        ChangeAction();

                        Position += new Vector2(0, 11);
                        Timer = 0;
                    }
                    else
                    {
                        Timer++;

                        // Detach main actor from plum
                        if ((JoyPad.IsButtonJustPressed(Rayman3Input.ActorJump) || Timer == 8) && mainActor.AttachedObject == this)
                        {
                            mainActor.ProcessMessage(this, Message.Rayman_DetachPlum);
                            mainActor.ProcessMessage(this, Message.Rayman_AllowSafetyJump, this);
                        }
                    }
                }
                // Fallen off the lava
                else if (groundType.Value is PhysicalTypeValue.MoltenLava or PhysicalTypeValue.InstaKill)
                {
                    if (DisableMessages)
                    {
                        SpawnLavaSplash();
                        LavaSplash.ActionId = LavaSplash.Action.DrownSplash;
                        LavaSplash.LinkedMovementActor = null;
                        
                        FloatSpeedX = 0;
                        DisableMessages = false;
                        ShouldSetInitialSpeed = true;

                        // Detach main actor from plum
                        if (mainActor.AttachedObject == this)
                        {
                            mainActor.ProcessMessage(this, Message.Rayman_DetachPlum);
                            mainActor.ProcessMessage(this, Message.Rayman_AllowSafetyJump, this);
                        }

                        // Reset
                        Position = Resource.Pos.ToVector2();
                        if ((Action)Resource.FirstActionId == Action.Fall)
                            fall = true;
                        else
                            idle = true;
                    }
                }
                // Off-screen
                else if (mainActor.Position.X - Position.X > 240 && FloatSpeedX != 0)
                {
                    if (LavaSplash != null)
                    {
                        LavaSplash.ProcessMessage(this, Message.Destroy);
                        LavaSplash.LinkedMovementActor = null;
                    }

                    FloatSpeedX = 0;
                    DisableMessages = false;
                    ShouldSetInitialSpeed = true;

                    // Reset
                    Position = Resource.Pos.ToVector2();
                    if ((Action)Resource.FirstActionId == Action.Fall)
                        fall = true;
                    else
                        idle = true;
                }
                // Floating
                else
                {
                    if (ActionId != Action.Float)
                        ActionId = Action.Float;

                    if (Math.Abs(FloatSpeedX) < MathHelpers.FromFixedPoint(0x200))
                    {
                        FloatSpeedX = 0;
                        Timer = 0;
                    }
                    else
                    {
                        if (FloatSpeedX <= 0)
                        {
                            if (FloatSpeedX < -targetSpeed)
                                FloatSpeedX += MathHelpers.FromFixedPoint(0x6000);
                            else
                                FloatSpeedX += MathHelpers.FromFixedPoint(0x200);
                        }
                        else
                        {
                            if (FloatSpeedX > targetSpeed)
                                FloatSpeedX -= MathHelpers.FromFixedPoint(0x6000);
                            else
                                FloatSpeedX -= MathHelpers.FromFixedPoint(0x200);
                        }
                    }
                }

                if (fall)
                {
                    State.MoveTo(Fsm_Fall);
                    return false;
                }

                if (idle)
                {
                    State.MoveTo(Fsm_Idle);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    // Unused
    public bool Fsm_Bounce(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.BounceGround;
                BounceSpeedX = 0;
                break;

            case FsmAction.Step:
                Box detectionBox = GetDetectionBox();

                PhysicalType groundType = Scene.GetPhysicalType(new Vector2(Position.X, detectionBox.Bottom));
                PhysicalType leftType = Scene.GetPhysicalType(new Vector2(detectionBox.Left - 1, detectionBox.Bottom));
                PhysicalType rightType = Scene.GetPhysicalType(new Vector2(detectionBox.Right + 1, detectionBox.Bottom));

                if (IsActionFinished && ActionId == Action.BounceGround)
                {
                    ActionId = Action.BounceAir;
                }
                else if (ActionId == Action.BounceAir)
                {
                    if (groundType.IsSolid)
                    {
                        ActionId = Action.BounceGround;
                        MechModel.Speed = MechModel.Speed with { X = 0 };
                    }
                    else
                    {
                        if ((leftType.IsFullySolid && !groundType.IsSolid) || 
                            (rightType.IsFullySolid && !groundType.IsSolid) ||
                            (Speed.X >= 0 && (rightType.Value is PhysicalTypeValue.SolidAngle30Left1 or PhysicalTypeValue.SolidAngle30Left2 || groundType.Value is PhysicalTypeValue.SolidAngle30Left1 or PhysicalTypeValue.SolidAngle30Left2)) ||
                            (Speed.X <= 0 && (leftType.Value is PhysicalTypeValue.SolidAngle30Right1 or PhysicalTypeValue.SolidAngle30Right2 || groundType.Value is PhysicalTypeValue.SolidAngle30Right1 or PhysicalTypeValue.SolidAngle30Right2)))
                        {
                            BounceSpeedX = -BounceSpeedX;
                        }

                        MechModel.Speed = MechModel.Speed with { X = BounceSpeedX };
                    }
                }

                // Land in lava
                if (groundType == PhysicalTypeValue.Lava)
                {
                    State.MoveTo(Fsm_Float);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}