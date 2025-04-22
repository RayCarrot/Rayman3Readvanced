using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class MetalShieldedHoodboom
{
    private bool FsmStep_CheckDeath()
    {
        IsHoodboomInvulnerable = GameTime.ElapsedFrames - InvulnerabilityTimer is > 20 and < 90;

        if (Scene.IsHitMainActor(this))
        {
            Scene.MainActor.ReceiveDamage(AttackPoints);
            Scene.MainActor.ProcessMessage(this, Message.Actor_Hurt, this);
        }

        Box detectionBox = GetDetectionBox();
        Rayman rayman = (Rayman)Scene.MainActor;
        bool dying = false;
        for (int i = 0; i < 2; i++)
        {
            RaymanBody activeFist = rayman.ActiveBodyParts[i];

            if (activeFist == null)
                continue;

            if (!detectionBox.Intersects(activeFist.GetDetectionBox()))
                continue;

            if (!IsHoodboomInvulnerable &&
                (LastHitFistType != i || GameTime.ElapsedFrames - InvulnerabilityTimer >= 90))
            {
                LastHitFistType = i;
                dying = HasBeenHitOnce;

                if (!dying)
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CagouHit_Mix03);

                HasBeenHitOnce = true;
                InvulnerabilityTimer = GameTime.ElapsedFrames;
            }

            activeFist.ProcessMessage(this, Message.RaymanBody_FinishAttack);
            break;
        }

        if (dying)
        {
            HitPoints = 0;
            State.MoveTo(Fsm_Dying);
            return false;
        }

        return true;
    }

    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                Timer = GameTime.ElapsedFrames;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                uint time = GameTime.ElapsedFrames - Timer;
                bool readyToAttack = (EarlyAttack && time > 25) || time > 100;

                float addMinX;
                float addMaxX;
                if (Position.X - Scene.MainActor.Position.X < 0)
                {
                    addMinX = 0;
                    addMaxX = 100;
                }
                else
                {
                    addMinX = -100;
                    addMaxX = 0;
                }
                bool detectedMainActor = Scene.IsDetectedMainActor(this, 0, 0, addMinX, addMaxX);

                Rayman rayman = (Rayman)Scene.MainActor;
                bool isAttacked = false;
                for (int i = 0; i < 2; i++)
                {
                    RaymanBody activeFist = rayman.ActiveBodyParts[i];

                    if (activeFist != null && activeFist != ActiveFists[i])
                        isAttacked = true;

                    ActiveFists[i] = activeFist;
                }

                bool isAttackedFromAbove = false;
                bool isAttackedFromBelow = false;
                if (isAttacked)
                {
                    if (Position.Y - Scene.MainActor.Position.Y > 16)
                        isAttackedFromAbove = true;
                    else
                        isAttackedFromBelow = true;
                }

                if (readyToAttack && detectedMainActor)
                {
                    State.MoveTo(Fsm_PrepareGrenade);
                    return false;
                }

                if (isAttackedFromAbove)
                {
                    State.MoveTo(Fsm_GuardAbove);
                    return false;
                }

                if (isAttackedFromBelow)
                {
                    State.MoveTo(Fsm_GuardBelow);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_PrepareGrenade(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Position.X - Scene.MainActor.Position.X < 0 ? Action.Attack_Right : Action.Attack_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (AnimatedObject.CurrentFrame == 10)
                {
                    State.MoveTo(Fsm_ThrowGrenade);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_ThrowGrenade(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Grenade grenade = Scene.CreateProjectile<Grenade>(ActorType.Grenade);

                Debug.Assert(grenade != null, "The grenade projectile could not be created");

                if (grenade != null)
                { 
                    float xPos;
                    if (IsFacingRight)
                    {
                        if (Scene.IsDetectedMainActor(this))
                            grenade.ActionId = Grenade.Action.ShortThrow_Right;
                        else if (Scene.IsDetectedMainActor(this, 0, 0, 0, 20))
                            grenade.ActionId = Grenade.Action.NormalThrow_Right;
                        else
                            grenade.ActionId = Grenade.Action.LongThrow_Right;

                        xPos = 45;
                    }
                    else
                    {
                        if (Scene.IsDetectedMainActor(this))
                            grenade.ActionId = Grenade.Action.ShortThrow_Left;
                        else if (Scene.IsDetectedMainActor(this, 0, 0, -20, 0))
                            grenade.ActionId = Grenade.Action.NormalThrow_Left;
                        else
                            grenade.ActionId = Grenade.Action.LongThrow_Left;

                        xPos = -45;
                    }

                    grenade.Position = Position + new Vector2(xPos, -85);
                    grenade.ChangeAction();
                }
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (IsActionFinished)
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

    public bool Fsm_GuardAbove(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.GuardAbove_Right : Action.GuardAbove_Left;
                ChangeAction();
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (IsActionFinished)
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

    public bool Fsm_GuardBelow(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.GuardBelow_Right : Action.GuardBelow_Left;
                ChangeAction();
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                LevelMusicManager.PlaySpecialMusicIfDetected(this);

                if (IsActionFinished)
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

    public bool Fsm_Dying(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.Dying_Right : Action.Dying_Left;
                IsInvulnerable = false;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Boing_Mix02);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CagoDie2_Mix01);
                break;

            case FsmAction.Step:
                if (IsActionFinished)
                {
                    ProcessMessage(this, Message.Destroy);
                    LevelMusicManager.StopSpecialMusic();
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}