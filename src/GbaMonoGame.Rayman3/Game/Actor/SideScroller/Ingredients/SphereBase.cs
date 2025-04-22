using System;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed partial class SphereBase : ActionActor
{
    public SphereBase(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        AnimatedObject.ObjPriority = 15;

        Resource = actorResource;
        Color = (Action)actorResource.FirstActionId switch
        {
            Action.Init_Yellow_Resurrect or Action.Init_Yellow_OpenGate => Sphere.SphereColor.Yellow,
            Action.Init_Purple_Resurrect or Action.Init_Purple_OpenGate => Sphere.SphereColor.Purple,
            _ => throw new Exception("Undefined sphere color type")
        };

        if (Color == Sphere.SphereColor.Purple)
            AnimatedObject.BasePaletteIndex = 1;

        State.SetTo(Fsm_Deactivated);
    }

    public ActorResource Resource { get; }
    public Sphere.SphereColor Color { get; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        // Handle messages
        switch (message)
        {
            case Message.Actor_ReloadAnimation:
                // Don't need to do anything. The original game sets the palette index again, but we're using local indexes, so it never changes.
                return false;

            default:
                return false;
        }
    }
}