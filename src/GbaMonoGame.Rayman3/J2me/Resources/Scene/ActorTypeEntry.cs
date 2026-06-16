using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public class ActorTypeEntry : BinarySerializable
{
    public ResourceId ImageResourceId { get; set; }
    public sbyte ImageDataIndex { get; set; }
    public ResourceId AnimationResourceId { get; set; }
    public sbyte AnimationDataIndex { get; set; }
    public byte CreateDataImage { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        ImageResourceId = s.SerializeInto<ResourceId>(ImageResourceId, ResourceId.SerializeInto, name: nameof(ImageResourceId));
        ImageDataIndex = s.Serialize<sbyte>(ImageDataIndex, name: nameof(ImageDataIndex));
        AnimationResourceId = s.SerializeInto<ResourceId>(AnimationResourceId, ResourceId.SerializeInto, name: nameof(AnimationResourceId));
        AnimationDataIndex = s.Serialize<sbyte>(AnimationDataIndex, name: nameof(AnimationDataIndex));
        CreateDataImage = s.Serialize<byte>(CreateDataImage, name: nameof(CreateDataImage));
    }
}