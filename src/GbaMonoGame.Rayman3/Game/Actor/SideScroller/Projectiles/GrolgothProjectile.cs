using System;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class GrolgothProjectile : MovableActor
{
    public GrolgothProjectile(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        Scale = 1;
        Field_0x6c = 0;
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
    public float VerticalOscillationOffset { get; set; }
    public float VerticalOscillationSpeed { get; set; }
    public byte Field_0x6c { get; set; } // TODO: Name
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
        vulnerabilityBox = new Box(vulnerabilityBox.MinX - 5, vulnerabilityBox.MinY - 5, vulnerabilityBox.MaxX + 5, vulnerabilityBox.MaxY + 5);

        Rayman rayman = (Rayman)Scene.MainActor;

        for (int i = 0; i < 2; i++)
        {
            RaymanBody activeFist = rayman.ActiveBodyParts[i];

            if (activeFist != null && activeFist.GetDetectionBox().Intersects(vulnerabilityBox) && IsFacingLeft != activeFist.IsFacingLeft)
            {
                activeFist.ProcessMessage(this, Message.RaymanBody_FinishedAttack);
                return !requireSuperFist || 
                       activeFist.BodyPartType is RaymanBody.RaymanBodyPartType.SuperFist or RaymanBody.RaymanBodyPartType.SecondSuperFist;
            }
        }

        return false;
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
            case Message.Hit:
                RaymanBody raymanBody = (RaymanBody)sender;
                if (((ActionId is Action.BigGroundBomb_Right or Action.BigGroundBomb_Left && 
                     raymanBody.BodyPartType is RaymanBody.RaymanBodyPartType.SuperFist or RaymanBody.RaymanBodyPartType.SecondSuperFist) || 
                    ActionId is Action.SmallGroundBomb_Right or Action.SmallGroundBomb_Left) &&
                    IsFacingLeft != raymanBody.IsFacingLeft)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MetlImp1_PiraHit3_Mix03);
                    ActionId = IsFacingRight ? Action.SmallGroundBombReverse_Left : Action.SmallGroundBombReverse_Right;
                }
                else if (ActionId == Action.Action11)
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