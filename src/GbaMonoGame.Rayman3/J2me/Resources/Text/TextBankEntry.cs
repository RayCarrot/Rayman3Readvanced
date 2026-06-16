using System.Text;
using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public class TextBankEntry : BinarySerializable
{
    public string Value { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        Value = s.SerializeLengthPrefixedString<byte>(Value, Encoding.UTF8, name: nameof(Value));
    }
}