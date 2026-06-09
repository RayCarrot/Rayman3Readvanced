using BinarySerializer;

namespace GbaMonoGame.Rayman3.Readvanced;

// TODO: Implement versioning system
public abstract class BaseReadvancedSave : BinarySerializable
{
    public override void SerializeImpl(SerializerObject s)
    {
        s.SerializeMagicString("R3RA", 4);
    }
}