using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public class SceneMapResource : ArchiveResource
{
    public SceneMapEntry[] Entries { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        Entries = s.SerializeObjectArray<SceneMapEntry>(Entries, Pre_HeaderEntry.DataSize / 15, name: nameof(Entries));
    }
}