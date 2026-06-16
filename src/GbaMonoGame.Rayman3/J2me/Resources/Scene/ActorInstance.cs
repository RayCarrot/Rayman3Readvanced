using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public class ActorInstance : BinarySerializable
{
    public byte ParamsCount { get; set; }
    public byte FirstAction { get; set; }
    public byte Flags { get; set; }
    public byte Type { get; set; }
    public short XPosition { get; set; }
    public short YPosition { get; set; }
    public byte[] Params { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        ParamsCount = s.Serialize<byte>(ParamsCount, name: nameof(ParamsCount));
        s.DoBits<byte>(b =>
        {
            FirstAction = b.SerializeBits<byte>(FirstAction, 4, name: nameof(FirstAction));
            Flags = b.SerializeBits<byte>(Flags, 4, name: nameof(Flags));
        });
        Type = s.Serialize<byte>(Type, name: nameof(Type));
        XPosition = s.Serialize<short>(XPosition, name: nameof(XPosition));
        YPosition = s.Serialize<short>(YPosition, name: nameof(YPosition));
        Params = s.SerializeArray<byte>(Params, ParamsCount, name: nameof(Params));
    }
}