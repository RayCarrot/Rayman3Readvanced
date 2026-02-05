using BinarySerializer;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3.Readvanced;

public class GhostActorFrame : BinarySerializable
{
    public bool Pre_IsMode7 { get; set; }

    public float XPosition { get; set; }
    public float YPosition { get; set; }
    public int InstanceId { get; set; }

    // 2D
    public int AnimationId { get; set; }
    public int FrameId { get; set; }
    public uint ActiveChannels { get; set; }
    public bool FlipX { get; set; }
    public bool FlipY { get; set; }

    // Mode7
    public float Direction { get; set; }
    public byte BaseActionId { get; set; }
    public byte ActionsSize { get; set; }

    public static GhostActorFrame FromActor(BaseActor actor, bool isMode7)
    {
        if (isMode7)
        {
            Mode7Actor mode7Actor = (Mode7Actor)actor;
            return new GhostActorFrame
            {
                XPosition = actor.Position.X,
                YPosition = actor.Position.Y,
                InstanceId = actor.InstanceId,
                Direction = mode7Actor.Direction,

                // NOTE: Ugly hard-coded values since we only ever use this for the MissileMode7 actor
                BaseActionId = 0,
                ActionsSize = 6,
            };
        }
        else
        {
            return new GhostActorFrame
            {
                XPosition = actor.Position.X,
                YPosition = actor.Position.Y,
                InstanceId = actor.InstanceId,
                AnimationId = actor.AnimatedObject.CurrentAnimation,
                FrameId = actor.AnimatedObject.CurrentFrame,
                ActiveChannels = actor.AnimatedObject.ActiveChannels,
                FlipX = actor.AnimatedObject.FlipX,
                FlipY = actor.AnimatedObject.FlipY
            };
        }
    }

    public override void SerializeImpl(SerializerObject s)
    {
        XPosition = s.Serialize<float>(XPosition, name: nameof(XPosition));
        YPosition = s.Serialize<float>(YPosition, name: nameof(YPosition));
        InstanceId = s.Serialize<int>(InstanceId, name: nameof(InstanceId));

        if (Pre_IsMode7)
        {
            Direction = s.Serialize<float>(Direction, name: nameof(Direction));
            BaseActionId = s.Serialize<byte>(BaseActionId, name: nameof(BaseActionId));
            ActionsSize = s.Serialize<byte>(ActionsSize, name: nameof(ActionsSize));
        }
        else
        {
            AnimationId = s.Serialize<int>(AnimationId, name: nameof(AnimationId));
            FrameId = s.Serialize<int>(FrameId, name: nameof(FrameId));
            ActiveChannels = s.Serialize<uint>(ActiveChannels, name: nameof(ActiveChannels));
            FlipX = s.Serialize<bool>(FlipX, name: nameof(FlipX));
            FlipY = s.Serialize<bool>(FlipY, name: nameof(FlipY));
        }
    }
}