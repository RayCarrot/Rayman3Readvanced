using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class MissileMode7
{
    private bool FsmStep_CheckDeath()
    {
        if (BoostTimer != 0)
        {
            BoostTimer--;

            if (BoostTimer == 0)
                field_0x8d = 0;

            if (BoostTimer == 176)
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoWoHoo_Mix01);
        }

        if (field_0x99 != 0)
        {
            if (field_0x99 == 1)
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoWoHoo_Mix01);

            field_0x99--;
        }

        if (field_0x9a != 0)
        {
            if (field_0x9a == 1)
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoJump1__or__OnoJump3_Mix01__or__OnoJump4_Mix01__or__OnoJump5_Mix01__or__OnoJump6_Mix01);

            field_0x9a--;
        }

        if ((GameInfo.Cheats & Cheat.Invulnerable) == 0)
        {
            InvulnerabilityTimer++;

            if (IsInvulnerable && InvulnerabilityTimer > 180)
                IsInvulnerable = false;

            if (Scene.IsHitMainActor(this))
            {
                ReceiveDamage(AttackPoints);
            }
            else
            {
                // Get the current physical type
                Mode7PhysicalTypeDefine physicalType = Scene.GetPhysicalType(Position).Mode7Define;

                if (physicalType.Damage && State != Fsm_Jump && !IsInvulnerable)
                {
                    if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__SplshGen_Mix04) &&
                        InstanceId == Scene.Camera.LinkedObject.InstanceId)
                    {
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SplshGen_Mix04);
                    }

                    ReceiveDamage(1);
                }
            }

            if (HitPoints < PrevHitPoints)
            {
                PrevHitPoints = HitPoints;
                
                if (InstanceId == Scene.Camera.LinkedObject.InstanceId)
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoRcvH1_Mix04);

                InvulnerabilityTimer = 0;
                IsInvulnerable = true;
            }

            if (HitPoints == 0 && RSMultiplayer.IsActive)
            {
                State.MoveTo(FUN_08085394);
                return false;
            }

            if (HitPoints == 0)
            {
                State.MoveTo(Fsm_Dying);
                return false;
            }
        }
        else
        {
            IsInvulnerable = true;
        }

        return true;
    }

    public bool Fsm_Start(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                GameInfo.IsInWorldMap = false;
                break;

            case FsmAction.Step:
                SetMode7DirectionalAction();
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                // Get the current physical type
                Mode7PhysicalTypeDefine physicalType = Scene.GetPhysicalType(Position).Mode7Define;

                if (physicalType.Bumper2 && !field_0x89.Bumper2)
                {
                    // TODO: Implement
                }
                else if (physicalType.Bumper1 && !field_0x89.Bumper1)
                {
                    // TODO: Implement
                }

                bool result;
                if (RSMultiplayer.IsActive)
                {
                    // TODO: Implement
                    result = true;
                }
                else
                {
                    // TODO: Implement
                    result = true;
                }

                if (result)
                {
                    // Update the animation
                    SetMode7DirectionalAction();

                    // Accelerate when holding A
                    if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.A))
                    {
                        MechModel.Acceleration = MathHelpers.DirectionalVector256(Direction) * field_0x96 *
                                                 new Vector2(1, -1);

                        if (BoostTimer != 0)
                            MechModel.Acceleration *= 2;
                    }
                    // End acceleration
                    else if (MultiJoyPad.IsButtonJustReleased(InstanceId, GbaInput.A))
                    {
                        MechModel.Acceleration = Vector2.Zero;
                    }

                    // Round speed to 0 if too low
                    float minSpeed = MathHelpers.FromFixedPoint(0x40); // 0.0009765625
                    if (MechModel.Speed.X < minSpeed &&
                        MechModel.Speed.X > -minSpeed &&
                        MechModel.Speed.Y < minSpeed &&
                        MechModel.Speed.Y > -minSpeed)
                    {
                        MechModel.Speed = Vector2.Zero;
                    }

                    // Brake when holding B or on damage tiles
                    if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.B) || physicalType.Damage)
                    {
                        float factor = MathHelpers.FromFixedPoint(0xC00); // 0.046875
                        MechModel.Speed -= MechModel.Speed * factor;
                    }
                    // Apply friction
                    else
                    {
                        float factor = MathHelpers.FromFixedPoint(0x500); // 0.01953125
                        MechModel.Speed -= MechModel.Speed * factor;
                    }

                    // Update speed
                    MechModel.Speed += MechModel.Acceleration;

                    // Move to the side with L and R
                    if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.L) && !MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.R))
                    {
                        MechModel.Speed = MechModel.Speed with { X = MechModel.Speed.X + MechModel.Speed.Y / 64 };
                        MechModel.Speed = MechModel.Speed with { Y = MechModel.Speed.Y - MechModel.Speed.X / 64 };
                    }
                    else if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.R) && !MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.L))
                    {
                        MechModel.Speed = MechModel.Speed with { X = MechModel.Speed.X - MechModel.Speed.Y / 64 };
                        MechModel.Speed = MechModel.Speed with { Y = MechModel.Speed.Y + MechModel.Speed.X / 64 };
                    }

                    if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.Left))
                        Direction += 2;

                    if (MultiJoyPad.IsButtonPressed(InstanceId, GbaInput.Right))
                        Direction -= 2;

                    if (CustomScaleTimer == 0)
                    {
                        // Gradually return to normal scale
                        if (1 < ScaleY)
                        {
                            ScaleX += 8 / 256f;
                            ScaleY -= 16 / 256f;
                        }
                    }
                    else
                    {
                        CustomScaleTimer--;
                    }

                    if (physicalType.Bounce)
                    {
                        State.MoveTo(Fsm_Jump);
                        return false;
                    }
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Jump(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (InstanceId == Scene.Camera.LinkedObject.InstanceId) 
                {
                    field_0x9a = 15;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__PinBall_Mix02);
                }

                ZPosSpeed = 8;
                ZPosDeacceleration = 0.375f;
                ScaleX = 288 / 256f;
                ScaleY = 224 / 256f;
                field_0x9c = 1;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                bool result;
                if (RSMultiplayer.IsActive)
                {
                    // TODO: Implement
                    result = true;
                }
                else
                {
                    // TODO: Implement
                    result = true;
                }

                if (result)
                {
                    // Update the animation
                    SetMode7DirectionalAction();

                    // Update the jump
                    UpdateJump();

                    if (MultiJoyPad.IsButtonJustReleased(InstanceId, GbaInput.A))
                        MechModel.Acceleration = Vector2.Zero;

                    if (ZPos <= 0)
                    {
                        State.MoveTo(Fsm_Default);
                        return false;
                    }
                }
                break;

            case FsmAction.UnInit:
                ScaleX = 192 / 256f;
                ScaleY = 384 / 256f;
                field_0x9c = 0;
                break;
        }

        return true;
    }

    public bool Fsm_Dying(FsmAction action)
    {
        // TODO: Implement

        return true;
    }

    public bool FUN_08085394(FsmAction action)
    {
        // TODO: Implement

        return true;
    }
}