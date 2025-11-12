using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class RockyFlame : InteractableActor
{
    public RockyFlame(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        State.SetTo(Fsm_Default);
        Timer = 0;
        AnimatedObject.ObjPriority = 10;

        // The top of the flame wraps to the bottom on GBA
        if (Rom.Platform == Platform.GBA)
            AnimatedObject.SetAnimationWrap(0, new Box(0, 0, 0, 90));
    }

    public byte Timer { get; set; }
}