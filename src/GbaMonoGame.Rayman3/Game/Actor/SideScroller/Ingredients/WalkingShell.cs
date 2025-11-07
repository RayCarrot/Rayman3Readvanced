using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class WalkingShell : MovableActor
{
    public WalkingShell(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        AnimatedObject.ObjPriority = 25;

        // Custom to allow respawning
        InitialPosition = Position;

        LoopPosition = Vector2.Zero;
        Timer = 0;
        HasBoostedInLoop = false;
        IsRaymanMounted = false;
        Rayman = (Rayman)Scene.MainActor;
        LoopAngle = 0;
        CurrentType = PhysicalTypeValue.None;
        CurrentBottomType = PhysicalTypeValue.None;

        State.SetTo(Fsm_Idle);
    }

    private const int LoopRadius = 56;

    public Rayman Rayman { get; }
    public Vector2 InitialPosition { get; }
    public Vector2 LoopPosition { get; set; }
    public byte Timer { get; set; }
    public bool HasBoostedInLoop { get; set; } // Unused behavior
    public bool IsRaymanMounted { get; set; }
    public byte LoopAngle { get; set; }
    public byte SafetyJumpTimer { get; set; } // Coyote jump

    public PhysicalType CurrentType { get; set; }
    public PhysicalType CurrentBottomType { get; set; }

    private void UpdateTypes()
    {
        Box detectionBox = GetDetectionBox();

        Vector2 pos = detectionBox.BottomRight + Tile.Up;

        CurrentType = Scene.GetPhysicalType(pos);

        // Check for special types
        if (CurrentType.Value is PhysicalTypeValue.MoltenLava or PhysicalTypeValue.InstaKill or PhysicalTypeValue.Water or PhysicalTypeValue.SlideJump)
        {
            CurrentBottomType = CurrentType;
            return;
        }

        while (!CurrentBottomType.IsSolid)
        {
            pos += Tile.Down;
            CurrentBottomType = Scene.GetPhysicalType(pos);

            if (CurrentBottomType == PhysicalTypeValue.SlideJump)
                LoopPosition = pos - new Vector2(0, LoopRadius);
        }

        pos += Tile.Up;
        CurrentBottomType = Scene.GetPhysicalType(pos);
    }

    private bool IsNearBreakableDoor()
    {
        foreach (BaseActor actor in new EnabledActorIterator(Scene))
        {
            if (actor.Type == (int)ActorType.BreakableDoor && actor.Position.X - Position.X < 220)
            {
                // If all objects are active then make sure we're not past the door
                if (!Scene.KeepAllObjectsActive || actor.Position.X - Position.X >= 0)
                    return true;
            }
        }

        return false;
    }

    private bool IsHitBreakableDoor()
    {
        BaseActor hitActor = Scene.IsHitActor(this);
        if (hitActor is { Type: (int)ActorType.BreakableDoor })
        {
            hitActor.ProcessMessage(this, Message.Actor_Hurt);
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool DetectLoop()
    {
        if (CurrentType == PhysicalTypeValue.SlideJump)
        {
            Box detectionBox = GetDetectionBox();
            LoopPosition = detectionBox.BottomRight + Tile.Up - new Vector2(0, LoopRadius);
            return true;
        }

        if (CurrentBottomType == PhysicalTypeValue.SlideJump) 
            return true;
        
        return false;
    }

    private void Explode()
    {
        Explosion explosion = Scene.CreateProjectile<Explosion>(ActorType.Explosion);

        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__BangGen1_Mix07);
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__BangGen1_Mix07);

        if (explosion != null)
            explosion.Position = Position - new Vector2(0, 12);

        ProcessMessage(this, Message.Destroy);
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.Readvanced_RespawnDeath:
                ProcessMessage(this, Message.Resurrect);
                Position = InitialPosition;
                LoopPosition = Vector2.Zero;
                Timer = 0;
                HasBoostedInLoop = false;
                IsRaymanMounted = false;
                LoopAngle = 0;
                CurrentType = PhysicalTypeValue.None;
                CurrentBottomType = PhysicalTypeValue.None;
                State.SetTo(Fsm_Idle);
                return false;

            default:
                return false;
        }
    }
}