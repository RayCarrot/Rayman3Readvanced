using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class FlyingShell
{
    public bool Fsm_Init(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Idle_Right;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Motor01_Mix12);
                break;

            case FsmAction.Step:
                UpdateSoundPitch();
                State.MoveTo(Fsm_Fly);
                return false;

            case FsmAction.UnInit:
                Scene.MainActor.ProcessMessage(this, Message.Rayman_Hide); // Unused, since this is the main actor
                Scene.Camera.LinkedObject = this;
                Scene.Camera.ProcessMessage(this, Message.Cam_FollowPositionY, 80);
                break;
        }

        return true;
    }

    public bool Fsm_Fly(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Fly_Right : Action.Fly_Left;
                EnergyBall = null;
                EndTimer = 0;
                MechModel.Speed = MechModel.Speed with { Y = 0 };
                break;

            case FsmAction.Step:
                ((CameraSideScroller)Scene.Camera).HorizontalOffset = CameraOffset.Center;

                // Change direction
                if (ActionId is Action.ChangeDirection_Right or Action.ChangeDirection_Left && IsActionFinished)
                {
                    ActionId = IsFacingRight ? Action.Fly_Left : Action.Fly_Right;
                    ChangeAction();
                }

                // Auto change direction when against wall
                if (Position.X < 50 && ActionId == Action.Fly_Left)
                {
                    Position += new Vector2(16, 0);
                    ActionId = Action.ChangeDirection_Left;
                }
                else if (Position.X > 430 && ActionId == Action.Fly_Right && GameInfo.MapId == MapId.BossFinal_M2)
                {
                    Position -= new Vector2(16, 0);
                    ActionId = Action.ChangeDirection_Right;
                }
                
                // Move down
                if (JoyPad.IsButtonPressed(GbaInput.Down) && Position.Y <= 150)
                {
                    Position += new Vector2(0, 1.5f);

                    if (ActionId is Action.FlyUp_Right or Action.FlyUp_Left or Action.Fly_Right or Action.Fly_Left)
                        ActionId = IsFacingRight ? Action.FlyDown_Right : Action.FlyDown_Left;
                }
                // Move up
                else if (JoyPad.IsButtonPressed(GbaInput.Up) && Position.Y >= 30)
                {
                    Position -= new Vector2(0, 1.5f);

                    if (ActionId is Action.FlyDown_Right or Action.FlyDown_Left or Action.Fly_Right or Action.Fly_Left)
                        ActionId = IsFacingRight ? Action.FlyUp_Right : Action.FlyUp_Left;
                }
                // Move straight
                else if (ActionId is Action.FlyUp_Right or Action.FlyUp_Left or Action.FlyDown_Right or Action.FlyDown_Left)
                {
                    ActionId = IsFacingRight ? Action.Fly_Right : Action.Fly_Left;
                }

                // Change direction
                if (JoyPad.IsButtonJustPressed(GbaInput.Right) && ActionId is Action.Fly_Left or Action.FlyUp_Left or Action.FlyDown_Left)
                    ActionId = Action.ChangeDirection_Left;
                else if (JoyPad.IsButtonJustPressed(GbaInput.Left) && ActionId is Action.Fly_Right or Action.FlyUp_Right or Action.FlyDown_Right)
                    ActionId = Action.ChangeDirection_Right;

                UpdateSoundPitch();

                if (IsCollidingWithWall())
                {
                    State.MoveTo(Fsm_Crash);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Stop(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                break;

            case FsmAction.Step:
                MechModel.Speed = MechModel.Speed with { X = 0 };
                UpdateSoundPitch();
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Crash(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                CrashTimer = 0;
                HitPoints = 0;

                if (GameInfo.LevelType != LevelType.GameCube)
                    GameInfo.ModifyLives(-1);

                CheckAgainstMapCollision = false;

                if (ActionId == Action.ChangeDirection_Right)
                    ActionId = Action.Crash_Left;
                else if (ActionId == Action.ChangeDirection_Left)
                    ActionId = Action.Crash_Right;
                else
                    ActionId = IsFacingRight ? Action.Crash_Right : Action.Crash_Left;

                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__Motor01_Mix12);
                break;

            case FsmAction.Step:
                if (CrashTimer == 0)
                {
                    Explosion explosion = Scene.CreateProjectile<Explosion>(ActorType.Explosion);

                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__BangGen1_Mix07);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BangGen1_Mix07);

                    if (explosion != null)
                        explosion.Position = Position;
                }
                else if (CrashTimer == 20)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__RaDeath_Mix03);
                }
                else if (CrashTimer == 120)
                {
                    if (GameInfo.PersistentInfo.Lives == 0)
                        FrameManager.SetNextFrame(new GameOver());
                    else
                        FrameManager.ReloadCurrentFrame();
                }

                CrashTimer++;
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_GameCubeEndMap(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                EndTimer = 0;
                SoundEventsManager.ReplaceAllSongs(Rayman3SoundEvent.Play__win3, 0);
                break;

            case FsmAction.Step:
                MechModel.Speed = MechModel.Speed with { X = 0 };
                UpdateSoundPitch();
                
                EndTimer++;
                if (EndTimer == 240)
                {
                    FrameSideScrollerGCN frame = (FrameSideScrollerGCN)Frame.Current;
                    GameInfo.MapId = frame.PreviousMapId;
                    GameInfo.Powers = frame.PreviousPowers;

                    if (GameInfo.PersistentInfo.CompletedGCNBonusLevels < frame.GcnMapId + 1)
                        GameInfo.PersistentInfo.CompletedGCNBonusLevels = (byte)(frame.GcnMapId + 1);

                    FrameManager.SetNextFrame(new GameCubeMenu());

                    GameInfo.Save(GameInfo.CurrentSlot);
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}