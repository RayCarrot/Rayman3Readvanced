using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class FallingPlatform : MovableActor
{
    public FallingPlatform(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        InitialPosition = Position;
        Timer = 0;
        AnimatedObject.ObjPriority = 60;

        State.SetTo(_Fsm_Idle);
    }

    public Vector2 InitialPosition { get; }
    public uint GameTime { get; set; } // NOTE: Won't work if all objects are active, but should be fine
    public byte Timer { get; set; }

    // Custom
    public bool DisabledFromLink { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        switch (message)
        {
            case Message.Resurrect:
                // There's a platform that spawns in the second precipice map. In the original game it spawns off-screen, but
                // in high resolution you can see when it spawns. To hide this we force the lightning to show on the same frame.
                if (Engine.ActiveConfig.Tweaks.VisualImprovements && Frame.Current is ThePrecipice_M2 precipiceM2)
                    precipiceM2.LightningTime = (ushort)(GbaMonoGame.GameTime.ElapsedFrames % 512);
                break;
        }

        if (base.ProcessMessageImpl(sender, message, param))
            return true;

        switch (message)
        {
            case Message.Readvanced_RespawnDeath:
                if (!DisabledFromLink)
                    ProcessMessage(this, Message.Resurrect);
                return true;

            default:
                return false;
        }
    }

    public override void Init(ActorResource actorResource)
    {
        DisabledFromLink = DestroyIfPastLinkedCheckpoint(actorResource);
    }
}