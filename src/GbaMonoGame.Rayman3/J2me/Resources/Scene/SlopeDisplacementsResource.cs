using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public class SlopeDisplacementsResource : ArchiveResource
{
    public byte[][] Displacements { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        Displacements = s.InitializeArray(Displacements, Pre_HeaderEntry.DataSize / 8);
        s.DoArray(Displacements, (obj, i, name) => s.SerializeArray<byte>(obj, 8, name: name), name: nameof(Displacements));
    }
}