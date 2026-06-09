using BinarySerializer;

namespace GbaMonoGame.Rayman3.Readvanced;

public abstract class BaseReadvancedSave : BinarySerializable
{
    public const string PrimaryId = "R3RA";

    public abstract int Version { get; }
    public abstract string Id { get; }

    protected abstract void SerializeSave(SerializerObject s, int version);

    public override void SerializeImpl(SerializerObject s)
    {
        s.SerializeMagicString(PrimaryId, PrimaryId.Length);
        s.SerializeMagicString(Id, Id.Length);
        int version = s.Serialize<int>(Version, name: nameof(Version));
        SerializeSave(s, version);
    }
}