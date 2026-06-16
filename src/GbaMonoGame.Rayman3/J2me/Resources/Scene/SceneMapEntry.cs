using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public class SceneMapEntry : BinarySerializable
{
    public ResourceId BackgroundResourceId { get; set; }
    public byte BackgroundDataIndex { get; set; }
    public ResourceId SceneResourceId { get; set; }
    public byte SceneDataIndex { get; set; }
    public ResourceId ImageResourceId { get; set; }
    public byte ImageDataIndex { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        BackgroundResourceId = s.SerializeInto<ResourceId>(BackgroundResourceId, ResourceId.SerializeInto, name: nameof(BackgroundResourceId));
        BackgroundDataIndex = s.Serialize<byte>(BackgroundDataIndex, name: nameof(BackgroundDataIndex));
        SceneResourceId = s.SerializeInto<ResourceId>(SceneResourceId, ResourceId.SerializeInto, name: nameof(SceneResourceId));
        SceneDataIndex = s.Serialize<byte>(SceneDataIndex, name: nameof(SceneDataIndex));
        ImageResourceId = s.SerializeInto<ResourceId>(ImageResourceId, ResourceId.SerializeInto, name: nameof(ImageResourceId));
        ImageDataIndex = s.Serialize<byte>(ImageDataIndex, name: nameof(ImageDataIndex));
    }
}