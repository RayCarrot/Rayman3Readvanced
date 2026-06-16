using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public class ActorTypesResource : ArchiveResource
{
    public ActorTypeEntry[] Entries { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        Entries = s.SerializeObjectArray<ActorTypeEntry>(Entries, Pre_HeaderEntry.DataSize / 11, name: nameof(Entries));
    }
}