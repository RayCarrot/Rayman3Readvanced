using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;
using GbaMonoGame.SourceGenerators;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class ExplosionMode7 : Mode7Actor
{
    public ExplosionMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        AnimatedObject.BgPriority = 0;
        Direction = Angle256.Half;
        ZPos = 40;
        RenderHeight = 32;
        IsAffine = false;

        State.SetTo(_Fsm_Default);
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        // Intercept messages
        switch (message)
        {
            case Message.ResurrectWakeUp:
                Engine.Sem.ProcessEvent(Rayman3SoundEvent.Stop__BangGen1_Mix07, this);
                Engine.Sem.ProcessEvent(Rayman3SoundEvent.Play__BangGen1_Mix07, this);
                break;
        }

        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        return false;
    }
}