using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class MurfyStone : BaseActor
{
    public MurfyStone(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        // Disable Murfy stones for the time attack mode
        if (TimeAttackInfo.IsActive)
            ProcessMessage(this, Message.Destroy);

        MurfyId = actorResource.Links[0];
        AnimatedObject.ObjPriority = 63;
        Timer = 181;
        State.SetTo(_Fsm_Default);
    }

    public int? MurfyId { get; }
    public uint Timer { get; set; }
    public byte RaymanIdleTimer { get; set; }
    public bool HasTriggered { get; set; }
}