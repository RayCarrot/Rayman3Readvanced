using BinarySerializer;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3.J2me;

public class RecordStoreFile : BaseReadvancedSave
{
    public override int Version => 0;
    public override string Id => "J2ME";

    public byte[][] Records { get; set; }

    protected override void SerializeSave(SerializerObject s, int version)
    {
        Records = s.SerializeArraySize<byte[], int>(Records, name: nameof(Records));
        s.DoArray(Records, (obj, _, name) =>
        {
            obj = s.SerializeArraySize<byte, int>(obj, name);
            return s.SerializeArray<byte>(obj, obj.Length, name: name);
        }, name: nameof(Records));
    }
}