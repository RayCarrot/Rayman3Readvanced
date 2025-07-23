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
                if (Scene.IsDetectedMainActor(this) && ((Mode7Actor)Scene.MainActor).ZPos < 24)
                {
                    State.MoveTo(Fsm_Destroyed);
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
                ZPosSpeed = 2;
                break;

            case FsmAction.Step:
                if (ZPos >= 180)
                    ZPosSpeed = -2;
                else if (ZPos <= 0)
                    ZPosSpeed = 2;

                float zPos = ZPos;

                ZPos += ZPosSpeed;

                if (Scene.IsDetectedMainActor(this) && 
                    ((Mode7Actor)Scene.MainActor).ZPos > zPos - 24 && 
                    ((Mode7Actor)Scene.MainActor).ZPos < zPos + 24)
                {
                    State.MoveTo(Fsm_Destroyed);
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

                if (Scene.IsDetectedMainActor(this) && ((Mode7Actor)Scene.MainActor).ZPos < 24)
                {
                    State.MoveTo(Fsm_Destroyed);
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
                ZPos -= 16;

                if (ZPos <= 0)
                {
                    ZPos = 0;
                    State.MoveTo(Fsm_Stationary);
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
                if (!GameInfo.IsCheatEnabled(Cheat.Invulnerable))
                {
                    // Deals 255 damage by default
                    if (!Engine.ActiveConfig.Difficulty.NoInstaKills)
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