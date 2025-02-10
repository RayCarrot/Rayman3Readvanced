using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class MissileMode7
{
    private bool FsmStep_CheckDeath()
    {
        // TODO: Implement
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

                    if (field_0x9b == 0)
                    {
                        if (256 < field_0x94)
                        {
                            field_0x92 += 8;
                            field_0x94 -= 16;
                        }
                    }
                    else
                    {
                        field_0x9b--;
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
                field_0x92 = 288;
                field_0x94 = 224;
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
                field_0x92 = 192;
                field_0x94 = 384;
                field_0x9c = 0;
                break;
        }

        return true;
    }
}