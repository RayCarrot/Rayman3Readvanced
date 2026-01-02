using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;

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

                // This code looks weird, but it's what the game does and is needed for correct collision detection!
                Vector2 pos = Position.Truncate() + Vector2.Floor((Direction.ToDirectionalVector() * new Vector2(18.98828125f)).FlipY());
                PhysicalType type = Scene.GetPhysicalType(pos);

                TargetDirection = type.Value switch
                {
                    PhysicalTypeValue.MovingPlatform_Left => Angle256.OneEighth * 4,
                    PhysicalTypeValue.MovingPlatform_Right => Angle256.OneEighth * 0,
                    PhysicalTypeValue.MovingPlatform_Up => Angle256.OneEighth * 2,
                    PhysicalTypeValue.MovingPlatform_Down => Angle256.OneEighth * 6,
                    PhysicalTypeValue.MovingPlatform_DownLeft => Angle256.OneEighth * 5,
                    PhysicalTypeValue.MovingPlatform_DownRight => Angle256.OneEighth * 7,
                    PhysicalTypeValue.MovingPlatform_UpRight => Angle256.OneEighth * 1,
                    PhysicalTypeValue.MovingPlatform_UpLeft => Angle256.OneEighth * 3,
                    _ => TargetDirection
                };

                // NOTE: The game has no tolerance check, but it doesn't use floats so it doesn't need to
                if (Math.Abs((Direction - TargetDirection).SignedValue) >= 1)
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

                // NOTE: This cheat is normally only in the game prototypes
                if (Engine.ActiveConfig.Tweaks.AllowPrototypeCheats && JoyPad.IsButtonJustPressed(GbaInput.Select) && JoyPad.IsButtonPressed(GbaInput.L))
                {
                    if (Engine.LocalConfig.Tweaks.PlayCheatTriggerSound)
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Switch1_Mix03);

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
                Scene.MainActor.ProcessMessage(this, Message.Rayman_FinishLevel);
                Timer = 0;
                break;

            case FsmAction.Step:
                SetMode7DirectionalAction((int)Action.Default, ActionRotationSize);

                // Slow down
                if (MechModel.Speed.X > 0)
                    MechModel.Speed -= new Vector2(1 / 32f, 0);

                Timer++;

                FrameMode7 frame = (FrameMode7)Frame.Current;

                // Fade out
                if (Timer == 140)
                {
                    TransitionsFX.FadeOutInit(2);
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