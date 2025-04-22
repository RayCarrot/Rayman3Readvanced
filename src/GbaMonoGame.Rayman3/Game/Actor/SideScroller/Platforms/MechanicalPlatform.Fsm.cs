using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class MechanicalPlatform
{
    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                SpeedY = 0;
                break;

            case FsmAction.Step:
                float yDist = InitialPosition.Y - Position.Y;

                if (ActionId is Action.SoftHit_Right or Action.SoftHit_Left or Action.HardHit_Right or Action.HardHit_Left && 
                    IsActionFinished)
                {
                    ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                    ChangeAction();
                }

                if (SpeedY <= -8)
                    SpeedY += 0.0625f;
                else if (SpeedY <= -6)
                    SpeedY += 0.125f;
                else if (0 < yDist)
                    SpeedY += 0.25f;

                if (8 <= SpeedY)
                    SpeedY = 8;

                // Don't allow jumping when the platform is moving up
                if (SpeedY < 0)
                    ((Rayman)Scene.MainActor).CanJump = SpeedY >= -MathHelpers.FromFixedPoint(0x57ffe); // Around -5.5
                else
                    IsSolid = false;

                if (yDist >= 180 && SpeedY < 0)
                {
                    MechModel.Speed = MechModel.Speed with { Y = 0 };
                    SpeedY = 0;
                }
                else if (yDist > 0)
                {
                    MechModel.Speed = MechModel.Speed with { Y = SpeedY };
                }
                else if (SpeedY > 0)
                {
                    if (SpeedY >= 7 && Scene.MainActor.LinkedMovementActor == this)
                        Scene.Camera.ProcessMessage(this, Message.Cam_Shake, 16);

                    MechModel.Speed = MechModel.Speed with { Y = 0 };
                    SpeedY = 0;
                }

                MovableActor mainActor = Scene.MainActor;

                // Link with main actor if it collides with it
                if (Scene.IsDetectedMainActor(this) && mainActor.LinkedMovementActor != this && mainActor.Position.Y <= Position.Y)
                {
                    if (!IsSolid)
                        mainActor.ProcessMessage(this, Message.Rayman_LinkMovement, this);
                }
                // Unlink from main actor if no longer colliding
                else if (mainActor.LinkedMovementActor == this)
                {
                    if (!Scene.IsDetectedMainActor(this) || mainActor.Position.Y > Position.Y)
                    {
                        mainActor.ProcessMessage(this, Message.Rayman_UnlinkMovement, this);
                    }
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}