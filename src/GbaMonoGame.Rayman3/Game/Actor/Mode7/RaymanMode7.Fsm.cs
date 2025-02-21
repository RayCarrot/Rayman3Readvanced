using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public partial class RaymanMode7
{
    private bool FsmStep_CheckDeath()
    {
        if ((GameInfo.Cheats & Cheat.Invulnerable) == 0)
        {
            InvulnerabilityTimer++;

            if (IsInvulnerable && InvulnerabilityTimer > 100)
                IsInvulnerable = false;

            if (Scene.IsHitMainActor(this))
            {
                ReceiveDamage(AttackPoints);
            }
            else
            {
                // TODO: Collision check
            }

            if (HitPoints == 0)
            {
                State.MoveTo(Fsm_Death);
                return false;
            }

            if (HitPoints < PrevHitPoints)
            {
                PrevHitPoints = HitPoints;
                State.MoveTo(Fsm_Hit);
                return false;
            }
        }
        else
        {
            IsInvulnerable = true;
        }

        return true;
    }

    private bool FsmStep_DoMovement()
    {
        if (ProcessJoypad)
        {
            if (JoyPad.IsButtonPressed(GbaInput.Left))
                MoveSpeed = 1.375f;
            else if (JoyPad.IsButtonPressed(GbaInput.Right))
                MoveSpeed = -1.375f;
            else
                MoveSpeed = 0;
        }
        else
        {
            MoveSpeed = 0;
        }

        SamMode7 sam = Scene.GetGameObject<SamMode7>(SamActorId);

        Vector2 posDiff = (sam.Position - Position) * new Vector2(1, -1);
        
        Direction = MathHelpers.Mod(MathHelpers.Atan2_256(posDiff), 256);

        float posDist = posDiff.Length();
        
        float speedX = posDist * (sam.MechModel.Speed.X * MathHelpers.Cos256(Direction - sam.Direction)) / 60;
        if (SlowDown)
            speedX /= 4;

        float speedY = posDist * (sam.MechModel.Speed.X * MathHelpers.Sin256(Direction - sam.Direction)) / 60;
        speedY += MoveSpeed;
        if (SlowDown)
            speedY /= 4;

        MechModel.Speed = new Vector2(speedX, speedY);

        // TODO: Implement
        // FUN_0807ed9c();

        return true;
    }

    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                TgxPlayfieldMode7 playfield = (TgxPlayfieldMode7)Scene.Playfield;
                playfield.TextLayers[1].IsStatic = true;

                playfield.TextLayers[0].ScrolledPosition = playfield.TextLayers[0].ScrolledPosition with { Y = 11 };
                playfield.TextLayers[1].ScrolledPosition = playfield.TextLayers[1].ScrolledPosition with { Y = 11 };
                playfield.TextLayers[2].ScrolledPosition = playfield.TextLayers[2].ScrolledPosition with { Y = 11 };
                playfield.TextLayers[3].ScrolledPosition = playfield.TextLayers[3].ScrolledPosition with { Y = 11 };

                // TODO: The map has to shift too
                playfield.Camera.Horizon = 67;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                if (!FsmStep_DoMovement())
                    return false;

                int currentFrame = AnimatedObject.CurrentFrame;
                int animTimer = AnimatedObject.Timer;
                bool isDelayMode = AnimatedObject.IsDelayMode;
                
                SetMode7DirectionalAction(0, 6);
                ChangeAction();
                
                AnimatedObject.CurrentFrame = currentFrame;
                AnimatedObject.Timer = animTimer;
                AnimatedObject.IsDelayMode = isDelayMode;

                if (SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__SkiLoop1))
                {
                    // TODO: Set sound pitch
                }

                if (MechModel.Speed.X <= 1)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__SkiLoop1);
                    AnimatedObject.CurrentFrame = 0;
                }
                else if ((GameTime.ElapsedFrames & 7) == 0)
                {
                    // TODO: Create watersplash projectile
                }

                if (JoyPad.IsButtonJustPressed(GbaInput.A) && ProcessJoypad)
                {
                    State.MoveTo(Fsm_Jump);
                    return false;
                }
                return true;

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
                // TODO: Implement
                break;

            case FsmAction.Step:
                // TODO: Implement
                return true;

            case FsmAction.UnInit:
                // TODO: Implement
                break;
        }

        return true;
    }

    public bool Fsm_Hit(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // TODO: Implement
                break;

            case FsmAction.Step:
                // TODO: Implement
                return true;

            case FsmAction.UnInit:
                // TODO: Implement
                break;
        }

        return true;
    }

    public bool Fsm_Death(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // TODO: Implement
                break;

            case FsmAction.Step:
                // TODO: Implement
                return true;

            case FsmAction.UnInit:
                // TODO: Implement
                break;
        }

        return true;
    }
}