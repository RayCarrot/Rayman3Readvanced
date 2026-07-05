using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class FlyingBombMode7
{
    public bool Fsm_Stationary(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.Stationary;
                break;

            case FsmAction.Step:
                if (Scene.IsDetectedMainActor(this) && ((Mode7Actor)Scene.MainActor).ZPos < COLLISION_HEIGHT)
                {
                    State.MoveTo(_Fsm_Destroyed);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_MoveVertical(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ZPosSpeed = MOVE_VERTICAL_SPEED;
                break;

            case FsmAction.Step:
                if (ZPos >= MOVE_VERTICAL_MAX_Z)
                    ZPosSpeed = -MOVE_VERTICAL_SPEED;
                else if (ZPos <= MOVE_VERTICAL_MIN_Z)
                    ZPosSpeed = MOVE_VERTICAL_SPEED;

                float zPos = ZPos;

                // Prevent moving if outside the current knot to avoid desyncing the cycles
                if (!Scene.KeepAllObjectsActive || IsInCurrentKnot)
                    ZPos += ZPosSpeed;

                if (Scene.IsDetectedMainActor(this) && 
                    ((Mode7Actor)Scene.MainActor).ZPos > zPos - COLLISION_HEIGHT && 
                    ((Mode7Actor)Scene.MainActor).ZPos < zPos + COLLISION_HEIGHT)
                {
                    State.MoveTo(_Fsm_Destroyed);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Move(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                PhysicalType type = Scene.GetPhysicalType(Position);

                Direction = type.Value switch
                {
                    PhysicalTypeValue.Enemy_Left => Angle256.Quarter * 2,
                    PhysicalTypeValue.Enemy_Right => Angle256.Quarter * 0,
                    PhysicalTypeValue.Enemy_Up => Angle256.Quarter * 1,
                    PhysicalTypeValue.Enemy_Down => Angle256.Quarter * 3,
                    _ => Direction
                };

                // Prevent moving if outside the current knot to avoid desyncing the cycles
                if (Scene.KeepAllObjectsActive && !IsInCurrentKnot)
                    Position -= Speed;

                if (Scene.IsDetectedMainActor(this) && ((Mode7Actor)Scene.MainActor).ZPos < COLLISION_HEIGHT)
                {
                    State.MoveTo(_Fsm_Destroyed);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Drop(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                ZPos -= DROP_VERTICAL_SPEED;

                if (ZPos <= DROP_VERTICAL_TARGET_Z)
                {
                    ZPos = DROP_VERTICAL_TARGET_Z;
                    State.MoveTo(_Fsm_Stationary);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Destroyed(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (!Rayman3.GameInfo.IsCheatEnabled(Cheat.Invulnerable))
                {
                    // Deals 255 damage by default
                    if (!Engine.Settings.Active.Difficulty.NoInstaKills)
                    {
                        Scene.MainActor.IsInvulnerable = false;
                        Scene.MainActor.ReceiveDamage(AttackPoints);
                    }
                    else
                    {
                        Scene.MainActor.ReceiveDamage(2);
                    }
                }

                ExplosionMode7 explosion = Scene.CreateProjectile<ExplosionMode7>(ActorType.ExplosionMode7);
                if (explosion != null)
                {
                    explosion.Position = Position;
                    explosion.ActionId = ExplosionMode7.Action.Explode;
                    explosion.ChangeAction();
                }

                ProcessMessage(this, Message.Destroy);
                break;

            case FsmAction.Step:
                // Do nothing
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}