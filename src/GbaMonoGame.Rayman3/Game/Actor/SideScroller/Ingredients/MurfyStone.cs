using GbaMonoGame.Engine2d;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

public sealed partial class MurfyStone : BaseActor
{
    public MurfyStone(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        // Disable Murfy stones for the time attack mode
        if (TimeAttackInfo.IsActive)
            ProcessMessage(this, Message.Destroy);

        MurfyId = actorResource.Links[0];
        AnimatedObject.ObjPriority = 63;
        Timer = 181;
        State.SetTo(Fsm_Default);
    }

    public int? MurfyId { get; }
    public uint Timer { get; set; }
    public byte RaymanIdleTimer { get; set; }
    public bool HasTriggered { get; set; }
}