using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class FallingBridge : MovableActor
{
    public FallingBridge(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        InitialPosition = Position;
        Link = actorResource.Links[1] == null ? actorResource.Links[0] : actorResource.Links[1];

        AnimatedObject.ObjPriority = 60;

        IsLeftBridgePart = (Action)actorResource.FirstActionId == Action.Idle_Left;

        State.SetTo(_Fsm_Idle);
    }

    public Vector2 InitialPosition { get; }
    public byte? Link { get; }
    public bool IsLeftBridgePart { get; }
    public byte Timer { get; set; }

    // Custom
    public bool DisabledFromLink { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        switch (message)
        {
            case Message.Readvanced_RespawnDeath:
                if (!DisabledFromLink)
                    ProcessMessage(this, Message.Resurrect);
                return false;

            case Message.Actor_Fall:
                if (State == _Fsm_Idle)
                    State.MoveTo(_Fsm_Timed);
                return false;

            default:
                return false;
        }
    }

    public override void Init(ActorResource actorResource)
    {
        DisabledFromLink = DestroyIfPastLinkedCheckpoint(actorResource);
    }
}