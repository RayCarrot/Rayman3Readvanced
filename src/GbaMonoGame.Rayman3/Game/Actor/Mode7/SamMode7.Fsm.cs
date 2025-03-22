using System;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class SamMode7
{
    public bool Fsm_Init(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                WaterSplashObj = Scene.CreateProjectile<WaterSplashMode7>(ActorType.WaterSplashMode7);
                if (WaterSplashObj != null)
                {
                    WaterSplashObj.ActionId = WaterSplashMode7.Action.SurfWaves;
                    WaterSplashObj.Position = Position;
                    WaterSplashObj.ChangeAction();
                }

                State.MoveTo(Fsm_Move);
                return false;

            case FsmAction.UnInit:
                ShouldDraw = true;
                break;
        }

        return true;
    }

    public bool Fsm_Move(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                TargetDirection = Direction;
                MechModel.Speed = MechModel.Speed with { X = 2.25f };
                break;

            case FsmAction.Step:
                SetMode7DirectionalAction((int)Action.Default, ActionRotationSize);

                // NOTE: The game uses 18.98828125 (fixed-point 0x12FD00) instead of 18. However this causes marshes 2 to break due
                //       to it being off by 1 pixel and Sam going off course. Using 18 fixes this.
                Vector2 pos = Position + Direction.ToDirectionalVector() * new Vector2(18) * new Vector2(1, -1);
                PhysicalType type = Scene.GetPhysicalType(pos);

                TargetDirection = type.Value switch
                {
                    PhysicalTypeValue.MovingPlatform_Left => 0x80,
                    PhysicalTypeValue.MovingPlatform_Right => 0,
                    PhysicalTypeValue.MovingPlatform_Up => 0x40,
                    PhysicalTypeValue.MovingPlatform_Down => 0xC0,
                    PhysicalTypeValue.MovingPlatform_DownLeft => 0xA0,
                    PhysicalTypeValue.MovingPlatform_DownRight => 0xE0,
                    PhysicalTypeValue.MovingPlatform_UpRight => 0x20,
                    PhysicalTypeValue.MovingPlatform_UpLeft => 0x60,
                    _ => TargetDirection
                };

                // NOTE: The game has no tolerance check, but it doesn't use floats so it doesn't need to
                if (Math.Abs(Direction - TargetDirection) >= 1)
                {
                    if (Direction - TargetDirection < Angle256.Half)
                        Direction -= 2;
                    else
                        Direction += 2;
                }
                else
                {
                    Direction = TargetDirection;
                }

                Timer++;

                if (WaterSplashObj != null)
                    WaterSplashObj.Position = Position + Speed * -4;

                if (type == PhysicalTypeValue.MovingPlatform_Stop)
                {
                    State.MoveTo(Fsm_End);
                    return false;
                }

                // TODO: Only enable in debug mode - this is a leftover prototype cheat
                if (JoyPad.IsButtonJustPressed(GbaInput.Select) && JoyPad.IsButtonPressed(GbaInput.L))
                {
                    State.MoveTo(Fsm_End);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_End(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Scene.Camera.ProcessMessage(this, Message.CamMode7_Spin, true);
                Scene.MainActor.ProcessMessage(this, Message.Main_LevelEnd);
                Timer = 0;
                break;

            case FsmAction.Step:
                SetMode7DirectionalAction((int)Action.Default, ActionRotationSize);

                // Slow down
                if (MechModel.Speed.X > 0)
                    MechModel.Speed -= new Vector2(MathHelpers.FromFixedPoint(0x800), 0);

                Timer++;

                FrameMode7 frame = (FrameMode7)Frame.Current;

                // Fade out
                if (Timer == 140)
                {
                    frame.TransitionsFX.FadeOutInit(2 / 16f);
                    frame.CanPause = false;
                }
                // Stop music
                else if (Timer == 216)
                {
                    SoundEventsManager.StopAllSongs();
                }
                // Save and end level
                else if (Timer == 218)
                {
                    if (GameInfo.IsFirstTimeCompletingLevel())
                        GameInfo.UpdateLastCompletedLevel();

                    frame.EndOfFrame = true;
                    GameInfo.PersistentInfo.LastPlayedLevel = (byte)GameInfo.MapId;
                    GameInfo.Save(GameInfo.CurrentSlot);
                }

                if (WaterSplashObj != null)
                    WaterSplashObj.Position = Position;
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}