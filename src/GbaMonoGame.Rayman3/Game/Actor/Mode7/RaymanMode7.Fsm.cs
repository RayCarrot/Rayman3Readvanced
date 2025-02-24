using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;
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
        
        Direction = Angle256.FromVector(posDiff);

        float posDist = posDiff.Length();

        Angle256 angleDiff = Direction - sam.Direction;
        Vector2 angleDiffVector = angleDiff.ToDirectionalVector();

        float speedX = posDist * (sam.MechModel.Speed.X * angleDiffVector.X) / 60;
        if (SlowDown)
            speedX /= 4;

        float speedY = posDist * (sam.MechModel.Speed.X * angleDiffVector.Y) / 60;
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
                
                SetMode7DirectionalAction((int)Action.Default, ActionRotationSize);
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
                    WaterSplashMode7 waterSplash = Scene.CreateProjectile<WaterSplashMode7>(ActorType.WaterSplashMode7);
                    if (waterSplash != null)
                    {
                        waterSplash.ActionId = WaterSplashMode7.Action.Splash;
                        waterSplash.Position = Position;
                        waterSplash.ChangeAction();
                    }
                }

                if (JoyPad.IsButtonJustPressed(GbaInput.A) && ProcessJoypad)
                {
                    State.MoveTo(Fsm_Jump);
                    return false;
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
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__SkiLoop1);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoJump1__or__OnoJump3_Mix01__or__OnoJump4_Mix01__or__OnoJump5_Mix01__or__OnoJump6_Mix01);
                ZPosSpeed = 8;
                ZPosDeacceleration = 0.375f; 
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                if (!FsmStep_DoMovement())
                    return false;

                SetMode7DirectionalAction((int)Action.Jump, ActionRotationSize);

                float newZPos = ZPosSpeed + ZPos;
                ZPosSpeed -= ZPosDeacceleration;

                if (newZPos <= 0)
                {
                    ZPos = 0;
                }
                else
                {
                    ZPos = newZPos;
                    
                    // TODO: Update horizon
                }

                if (ZPos <= 0)
                {
                    State.MoveTo(Fsm_Default);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SplshGen_Mix04);
                break;
        }

        return true;
    }

    public bool Fsm_Hit(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoRcvH1_Mix04);
                InvulnerabilityTimer = 0;
                IsInvulnerable = true;
                break;

            case FsmAction.Step:
                if (!FsmStep_DoMovement())
                    return false;

                int currentFrame = AnimatedObject.CurrentFrame;
                int animTimer = AnimatedObject.Timer;
                bool isDelayMode = AnimatedObject.IsDelayMode;

                SetMode7DirectionalAction((int)Action.Default, ActionRotationSize);
                ChangeAction();

                AnimatedObject.CurrentFrame = currentFrame;
                AnimatedObject.Timer = animTimer;
                AnimatedObject.IsDelayMode = isDelayMode;

                State.MoveTo(Fsm_Default);
                return false;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Death(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                InvulnerabilityTimer = 0;
                GameInfo.ModifyLives(-1);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__RaDeath_Mix03);
                ReceiveDamage(255);
                Scene.GetGameObject(SamActorId).ProcessMessage(this, Message.Main_Damaged2);
                ActionId = Action.Dying;
                MechModel.Speed = MechModel.Speed with { Y = 0 };
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__SkiLoop1);
                break;

            case FsmAction.Step:
                // TODO: Implement
                // FUN_0807ed9c();

                Debug.Assert(ProcessJoypad, "Should not die near the end of the map");

                InvulnerabilityTimer++;

                FrameWaterSkiMode7 frame = (FrameWaterSkiMode7)Frame.Current;
                if (InvulnerabilityTimer == 90)
                {
                    frame.TransitionsFX.FadeOutInit(2 / 16f);
                    frame.CanPause = false;
                }
                else if (InvulnerabilityTimer == 167)
                {
                    if (GameInfo.PersistentInfo.Lives == 0)
                        FrameManager.SetNextFrame(new GameOver());
                    else
                        FrameManager.ReloadCurrentFrame();
                }

                if (InvulnerabilityTimer > 80)
                {
                    // TODO: Update fog value
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}