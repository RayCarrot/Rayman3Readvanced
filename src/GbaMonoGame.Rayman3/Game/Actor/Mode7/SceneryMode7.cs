using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public sealed class SceneryMode7 : Mode7Actor
{
    public SceneryMode7(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        AnimatedObject.BgPriority = 0;
        Direction = Angle256.Zero;
        ZPos = 0;

        AnimationChannel channel = AnimatedObject.GetAnimation().Channels[0];
        Constants.Size shape = Constants.GetSpriteShape(channel.SpriteShape, channel.SpriteSize);
        RenderHeight = shape.Height;

        State.SetTo(null);
    }
}