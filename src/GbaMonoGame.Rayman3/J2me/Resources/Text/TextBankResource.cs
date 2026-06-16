using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public class TextBankResource : ArchiveResource
{
    public TextBankEntry[] Entries { get; set; }
    
    public override void SerializeImpl(SerializerObject s)
    {
        long endOffset = Offset.FileOffset + Pre_HeaderEntry.DataSize;
        Entries = s.SerializeObjectArrayUntil(Entries, _ => s.CurrentFileOffset >= endOffset, name: nameof(Entries));    
    }
}