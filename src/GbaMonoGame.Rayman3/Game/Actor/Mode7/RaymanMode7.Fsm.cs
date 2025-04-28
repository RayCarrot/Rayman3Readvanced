using System;
using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;
using GbaMonoGame.TgxEngine;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace GbaMonoGame.Rayman3;

public partial class RaymanMode7
{
    private bool FsmStep_CheckDeath()
    {
        if (!GameInfo.IsCheatEnabled(Cheat.Invulnerable) && !Scene.GetGameObject<SamMode7>(SamActorId).Debug_NoClip)
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
                Vector2 dir = Direction.ToDirectionalVector();

                PhysicalType type1 = Scene.GetPhysicalType(Position);
                PhysicalType type2 = Scene.GetPhysicalType(Position + new Vector2(dir.Y, dir.X) * Tile.Size);
                PhysicalType type3 = Scene.GetPhysicalType(Position - new Vector2(dir.Y, dir.X) * Tile.Size);

                if ((type3 == PhysicalTypeValue.Damage || type2 == PhysicalTypeValue.Damage || type1 == PhysicalTypeValue.Damage) && State != Fsm_Jump) 
                {
                    if (GameInfo.MapId == MapId.MarshAwakening1)
                        ((MarshAwakening1)Frame.Current).CanShowTextBox = true;

                    ReceiveDamage(1);

                    if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__SkiWeed_Mix02))
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SkiWeed_Mix02);
                }
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

        Vector2 posDiff = (sam.Position - Position).FlipY();
        
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

        ScrollClouds();

        return true;
    }

    public bool Fsm_Default(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                TgxPlayfieldMode7 playfield = (TgxPlayfieldMode7)Scene.Playfield;

                // Set clouds as static since they'll be updated manually in here
                playfield.TextLayers[1].IsStatic = true;

                // Set the initial Y offset, which is later changed when jumping
                playfield.TextLayers[0].ScrolledPosition = playfield.TextLayers[0].ScrolledPosition with { Y = 11 };
                playfield.TextLayers[1].ScrolledPosition = playfield.TextLayers[1].ScrolledPosition with { Y = 11 };
                playfield.TextLayers[2].ScrolledPosition = playfield.TextLayers[2].ScrolledPosition with { Y = 11 };
                playfield.TextLayers[3].ScrolledPosition = playfield.TextLayers[3].ScrolledPosition with { Y = 11 };

                playfield.Camera.Horizon = 67;
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckDeath())
                    return false;

                if (!FsmStep_DoMovement())
                    return false;

                // NOTE: This causes a bug in the original game on the first frame this runs after you jump. That's because of the
                //       animation is still in the delay mode from the jump animation and will thus not reload the tiles, causing
                //       it to reuse the tiles from the jump animation for the default animation. This however is not an issue
                //       here since we load tiles differently.
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
                    Vector3 screenPos = ((TgxPlayfieldMode7)Scene.Playfield).Camera.Project(new Vector3(Position, 0));
                    float screenX = Math.Abs(Scene.Resolution.X / 2 - screenPos.X);

                    // TODO: Update this if we adjust the zoom
                    // NOTE: The screen pos produces slightly different offsets from the original game due to the zoom being different. In
                    //       the original game the range is usually around +/- 40 while we get +/- 30. We multiply here to adjust this.
                    screenX *= 40 / 30f;

                    SoundEventsManager.SetSoundPitch(Rayman3SoundEvent.Play__SkiLoop1, screenX * 16 + 192);
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

                    if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__SkiLoop1))
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__SkiLoop1);
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
                ZPosDeacceleration = 3 / 8f; 
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
                    UpdateJump(newZPos);
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
                Scene.GetGameObject(SamActorId).ProcessMessage(this, Message.Actor_End);
                ActionId = Action.Dying;
                MechModel.Speed = MechModel.Speed with { Y = 0 };
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__SkiLoop1);
                break;

            case FsmAction.Step:
                ScrollClouds();

                Debug.Assert(ProcessJoypad, "Should not die near the end of the map");

                InvulnerabilityTimer++;

                FrameWaterSkiMode7 frame = (FrameWaterSkiMode7)Frame.Current;
                if (InvulnerabilityTimer == 90)
                {
                    TransitionsFX.FadeOutInit(2);
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
                    frame.FadeDecrease = (InvulnerabilityTimer - 80) / 4f;
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}