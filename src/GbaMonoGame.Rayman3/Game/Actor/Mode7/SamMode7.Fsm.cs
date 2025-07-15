using System;
using BinarySerializer;
using BinarySerializer.Ubisoft.GbaEngine;
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

    private int Sin(byte angle)
    {
        long addr = 0x080e1bac + angle * 4;
        BinaryDeserializer s = Rom.Context.Deserializer;
        s.Goto(new Pointer(addr, Rom.Loader.Font16.Offset.File));
        return s.Serialize<int>(default);
    }

    private int Cos(byte angle)
    {
        angle += 0x40;
        return Sin(angle);
    }

    private Vector2 Trunc(Vector2 v)
    {
        v.X = MathF.Truncate(v.X);
        v.Y = MathF.Truncate(v.Y);
        return v;
    }

    public bool Fsm_Move(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;
                TargetDirection = Direction;
                MechModel.Speed = MechModel.Speed with { X = 2.25f };
                FixedPointX = (int)Position.X * 0x10000;
                FixedPointY = (int)Position.Y * 0x10000;
                FixedPointSpeedX = 0x24000;
                FixedPointSpeedY = 0;
                FixedPointDir = (byte)Direction;
                FixedPointTargetDir = FixedPointDir;
                break;

            case FsmAction.Step:
                SetMode7DirectionalAction((int)Action.Default, ActionRotationSize);

                int uVar4 = (Cos(FixedPointDir) * 0x12fd >> 0x18) + (FixedPointX >> 0x10);
                int uVar5 = (Sin(FixedPointDir) * -0x12fd >> 0x18) + (FixedPointY >> 0x10);

                PhysicalType type1 = Scene.GetPhysicalType(new Vector2(uVar4, uVar5));
                OgTypePos = new Vector2(uVar4, uVar5);
                FixedPointTargetDir = type1.Value switch
                {
                    PhysicalTypeValue.MovingPlatform_Left => 32 * 4,
                    PhysicalTypeValue.MovingPlatform_Right => 32 * 0,
                    PhysicalTypeValue.MovingPlatform_Up => 32 * 2,
                    PhysicalTypeValue.MovingPlatform_Down => 32 * 6,
                    PhysicalTypeValue.MovingPlatform_DownLeft => 32 * 5,
                    PhysicalTypeValue.MovingPlatform_DownRight => 32 * 7,
                    PhysicalTypeValue.MovingPlatform_UpRight => 32 * 1,
                    PhysicalTypeValue.MovingPlatform_UpLeft => 32 * 3,
                    _ => FixedPointTargetDir
                };

                if (FixedPointDir != FixedPointTargetDir)
                {
                    if ((byte)(FixedPointDir - FixedPointTargetDir) < 128)
                        FixedPointDir -= 2;
                    else
                        FixedPointDir += 2;
                }

                Vector2 pos = Trunc(Position) + Vector2.Floor((Direction.ToDirectionalVector() * new Vector2(18.98828125f)).FlipY());
                PhysicalType type = Scene.GetPhysicalType(pos);
                ReTypePos = pos;

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
                if (Engine.Config.Tweaks.AllowPrototypeCheats && JoyPad.IsButtonJustPressed(GbaInput.Select) && JoyPad.IsButtonPressed(GbaInput.L))
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