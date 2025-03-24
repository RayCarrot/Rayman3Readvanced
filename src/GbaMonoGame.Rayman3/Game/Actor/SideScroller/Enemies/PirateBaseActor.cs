using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public abstract class PirateBaseActor : MovableActor
{
    protected PirateBaseActor(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        IsObjectCollisionXOnly = true;
        Resource = actorResource;
        SpawnsRedLum = false;
        Reset();
    }

    public ActorResource Resource { get; }

    public bool SpawnsRedLum { get; set; }
    public bool IsSecondAttack { get; set; }

    public uint InvulnerabilityTimer { get; set; }
    public uint IdleDetectionTimer { get; set; }
    public bool HitFromFront { get; set; }
    public int PrevHitPoints { get; set; }

    private void Reset()
    {
        InvulnerabilityTimer = 0;
        IdleDetectionTimer = 0;
        HitFromFront = false;
        PrevHitPoints = HitPoints;
        IsSecondAttack = false;
    }

    protected void SpawnRedLum(float offsetX)
    {
        if (!SpawnsRedLum)
            return;

        Lums lum = Scene.CreateProjectile<Lums>(ActorType.Lums);

        if (lum == null) 
            return;

        lum.AnimatedObject.CurrentAnimation = 3;
        lum.ActionId = Lums.Action.RedLum;

        float xPos;
        if ((IsFacingLeft && HitFromFront) ||
            (!IsFacingLeft && !HitFromFront))
            xPos = Position.X - offsetX;
        else
            xPos = Position.X + offsetX;

        lum.Position = new Vector2(xPos, Position.Y - 25);
    }

    protected void StartInvulnerability()
    {
        InvulnerabilityTimer = GameTime.ElapsedFrames;
        IsInvulnerable = true;
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        // Intercept messages
        switch (message)
        {
            case Message.Resurrect:
                Position = Resource.Pos.ToVector2();
                HitPoints = ActorModel.HitPoints;
                Reset();
                ReInit();
                break;
        }

        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        // Handle messages
        switch (message)
        {
            case Message.Hit:
                BaseActor hitActor = (BaseActor)param;

                if ((hitActor.IsFacingLeft && IsFacingRight) ||
                    (hitActor.IsFacingRight && IsFacingLeft))
                {
                    HitFromFront = false;
                }
                else
                {
                    HitFromFront = true;
                }
                return false;

            default:
                return false;
        }
    }

    protected abstract void ReInit();

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        DrawWithInvulnerability(animationPlayer, forceDraw);
    }
}