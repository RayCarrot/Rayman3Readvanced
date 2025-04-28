using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class GrolgothProjectile : MovableActor
{
    public GrolgothProjectile(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        Scale = 1;
        MissileTimer = -1;
        Rotation = Angle256.Zero;
        Timer = 0;
        VerticalOscillationAmplitude = 0;
        IsVerticalOscillationMovingDown = true;
        VerticalOscillationOffset = 0;
        BombSoundTimer = 0;
        VerticalOscillationSpeed = 0;
        IsDead = false;

        AnimatedObject.ObjPriority = 10;

        State.SetTo(Fsm_Default);
    }

    public float Scale { get; set; }
    public int MissileTimer { get; set; } // Same value as the Scale in the original game
    public float VerticalOscillationOffset { get; set; }
    public float VerticalOscillationSpeed { get; set; }
    public Angle256 Rotation { get; set; }
    public byte Timer { get; set; }
    public byte VerticalOscillationAmplitude { get; set; }
    public bool IsDead { get; set; }
    public bool IsVerticalOscillationMovingDown { get; set; }
    public byte BombSoundTimer { get; set; }

    private void UpdateVerticalOscillation()
    {
        if (VerticalOscillationAmplitude == 0) 
            return;
        
        if (IsVerticalOscillationMovingDown)
        {
            Position += new Vector2(0, VerticalOscillationSpeed);
            VerticalOscillationOffset += VerticalOscillationSpeed;

            if (Math.Abs(VerticalOscillationOffset) > VerticalOscillationAmplitude)
                IsVerticalOscillationMovingDown = false;
        }
        else
        {
            Position -= new Vector2(0, VerticalOscillationSpeed);
            VerticalOscillationOffset -= VerticalOscillationSpeed;

            if (Math.Abs(VerticalOscillationOffset) > VerticalOscillationAmplitude)
                IsVerticalOscillationMovingDown = true;
        }
    }

    private void UpdateBombSound()
    {
        if (BombSoundTimer != 0)
        {
            BombSoundTimer--;
        }
        else if (AnimatedObject.IsFramed && (GameInfo.ActorSoundFlags & ActorSoundFlags.FlyingBomb) == 0)
        {
            if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__BombFly_Mix03))
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BombFly_Mix03);

            BombSoundTimer = 60;
        }

        if (AnimatedObject.IsFramed)
            GameInfo.ActorSoundFlags |= ActorSoundFlags.FlyingBomb;
    }

    private void ManageHitBomb(bool requireSuperFist)
    {
        if (CheckHit(requireSuperFist))
            ActionId = IsFacingRight ? Action.SmallGroundBombReverse_Left : Action.SmallGroundBombReverse_Right;
    }

    private bool CheckHit(bool requireSuperFist)
    {
        Box vulnerabilityBox = GetVulnerabilityBox();
        vulnerabilityBox.Left -= 5;
        vulnerabilityBox.Top -= 5;
        vulnerabilityBox.Right += 5;
        vulnerabilityBox.Bottom += 5;

        Rayman rayman = (Rayman)Scene.MainActor;

        for (int i = 0; i < 2; i++)
        {
            RaymanBody activeFist = rayman.ActiveBodyParts[i];

            if (activeFist != null && activeFist.GetDetectionBox().Intersects(vulnerabilityBox) && IsFacingLeft != activeFist.IsFacingLeft)
            {
                activeFist.ProcessMessage(this, Message.RaymanBody_FinishAttack);
                return !requireSuperFist || 
                       activeFist.BodyPartType is RaymanBody.RaymanBodyPartType.SuperFist or RaymanBody.RaymanBodyPartType.SecondSuperFist;
            }
        }

        return false;
    }

    private void UpdateMissile()
    {
        // The speeds are different per platform
        float fastSpeed = Rom.Platform switch
        {
            Platform.GBA => 4.5f,
            Platform.NGage => 2.75f,
            _ => throw new ArgumentOutOfRangeException()
        };
        float mediumSpeed = Rom.Platform switch
        {
            Platform.GBA => 2f,
            Platform.NGage => 1f,
            _ => throw new ArgumentOutOfRangeException()
        };
        float slowSpeed = Rom.Platform switch
        {
            Platform.GBA => 1.5f,
            Platform.NGage => 0.75f,
            _ => throw new ArgumentOutOfRangeException()
        };

        // Get speed to use based on distance
        float dist = Vector2.Distance(Position, Scene.MainActor.Position);
        float speed = dist switch
        {
            > 200 => fastSpeed,
            < 80 => slowSpeed,
            _ => mediumSpeed
        };

        // Initialize
        if (MissileTimer == -1)
        {
            AnimatedObject.AffineMatrix = new AffineMatrix(0, 1, 1);
            AnimatedObject.IsDoubleAffine = true;
            MissileTimer = 0;
            Timer = 0;

            Angle256 angle = MathHelpers.Atan2_256(Position - Scene.MainActor.Position);
            
            // Timer = angle; // Ignore doing this since it's irrelevant
            Rotation = angle;
        }

        MissileTimer++;
        
        if (MissileTimer >= 240)
        {
            if (MissileTimer == 240)
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Missile1_Mix01);

            MechModel.Speed = MathHelpers.DirectionalVector256(Rotation) * -fastSpeed;
            return;
        }

        if (MissileTimer > 210)
        {
            if ((MissileTimer & 7) == 0)
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BeepFX01_Mix02);
        }
        else if (MissileTimer == 180)
        {
            ActionId = Action.MissileLockedIn;
        }
        else if (MissileTimer > 150)
        {
            if ((MissileTimer & 0xF) == 0)
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BeepFX01_Mix02);
        }
        else if (MissileTimer > 60)
        {
            if (ActionId != Action.MissileBeep)
                ActionId = Action.MissileBeep;

            if ((MissileTimer & 0x1F) == 0)
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BeepFX01_Mix02);
        }

        if (SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__BeepFX01_Mix02))
            SoundEventsManager.SetSoundPitch(Rayman3SoundEvent.Play__BeepFX01_Mix02, MissileTimer * 4);

        if ((Scene.MainActor.Position.X > Position.X && Speed.X > 0 && Scene.MainActor.Speed.X < 0) ||
            (Scene.MainActor.Position.X < Position.X && Speed.X < 0 && Scene.MainActor.Speed.X > 0))
        {
            if (Position.X < Scene.MainActor.Position.X)
            {
                MechModel.Speed = new Vector2(speed, 0);

                // NOTE: The original game doesn't have a tolerance check, but we need one since we use floats
                if (Math.Abs((Rotation - Angle256.Half).SignedValue) >= 1)
                {
                    if (Rotation >= Angle256.Half)
                        Rotation--;
                    else
                        Rotation++;
                }
                else
                {
                    Rotation = Angle256.Half;
                }
            }
            else
            {
                MechModel.Speed = new Vector2(-speed, 0);

                // NOTE: The original game doesn't have a tolerance check, but we need one since we use floats
                if (Math.Abs((Rotation - Angle256.Zero).SignedValue) >= 1)
                {
                    if (Rotation >= Angle256.Half)
                        Rotation++;
                    else
                        Rotation--;
                }
                else
                {
                    Rotation = Angle256.Zero;
                }
            }

            AnimatedObject.AffineMatrix = new AffineMatrix(Rotation, 1, 1);
        }
        else
        {
            Angle256 angle = MathHelpers.Atan2_256(Position - Scene.MainActor.Position);
            // Timer = angle; // Ignore doing this since it's irrelevant

            Angle256 angleDiff = Rotation - angle;
            
            switch (angleDiff.Value)
            {
                case > 0 and <= 9:
                    Rotation -= MissileTimer & 1;
                    break;
                
                case > 246 or <= 9:
                    Rotation = angle;
                    break;
                
                default:
                    float value = angleDiff >= 128 ? angleDiff.Value - 256 : angleDiff;
                    Rotation += -value / 10;
                    break;
            }


            AnimatedObject.AffineMatrix = new AffineMatrix(Rotation, 1, 1);
            MechModel.Speed = MathHelpers.DirectionalVector256(Rotation) * -speed;
        }

        if ((MissileTimer & 8) != 0)
        {
            GrolgothProjectile smokeProjectile = Scene.CreateProjectile<GrolgothProjectile>(ActorType.GrolgothProjectile);
            if (smokeProjectile != null)
            {
                smokeProjectile.Position = Position + MathHelpers.DirectionalVector256(Rotation) * 16;
                smokeProjectile.ActionId = Random.GetNumber(3) switch
                {
                    0 => Action.MissileSmoke1,
                    1 => Action.MissileSmoke2,
                    2 => Action.MissileSmoke3,
                    _ => throw new Exception("Out of range value")
                };
                smokeProjectile.AnimatedObject.ObjPriority = 30;
            }
        }
    }

    private void UpdateMissileSmoke()
    {
        Timer++;

        if (Timer > 5)
            IsDead = true;
    }

    private void Explode()
    {
        Explosion explosion = Scene.CreateProjectile<Explosion>(ActorType.Explosion);
        if (explosion != null)
        {
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__Missile1_Mix01);
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__BangGen1_Mix07);
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BangGen1_Mix07);
            explosion.Position = Position;
        }

        IsDead = true;
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.Actor_Hit:
                RaymanBody raymanBody = (RaymanBody)sender;
                if (((ActionId is Action.BigGroundBomb_Right or Action.BigGroundBomb_Left && 
                     raymanBody.BodyPartType is RaymanBody.RaymanBodyPartType.SuperFist or RaymanBody.RaymanBodyPartType.SecondSuperFist) || 
                    ActionId is Action.SmallGroundBomb_Right or Action.SmallGroundBomb_Left) &&
                    IsFacingLeft != raymanBody.IsFacingLeft)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MetlImp1_PiraHit3_Mix03);
                    ActionId = IsFacingRight ? Action.SmallGroundBombReverse_Left : Action.SmallGroundBombReverse_Right;
                }
                else if (ActionId == Action.BigExplodingBomb)
                {
                    Explosion explosion = Scene.CreateProjectile<Explosion>(ActorType.Explosion);
                    if (explosion != null)
                    {
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__BangGen1_Mix07);
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BangGen1_Mix07);
                        explosion.Position = Position;
                    }

                    State.MoveTo(Fsm_Default);
                }
                else if (ActionId != Action.FallingBomb)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MetlImp1_PiraHit3_Mix03);
                }
                return false;

            default:
                return false;
        }
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        base.Draw(animationPlayer, forceDraw);
        GameInfo.ActorSoundFlags &= ~ActorSoundFlags.FlyingBomb;
    }
}