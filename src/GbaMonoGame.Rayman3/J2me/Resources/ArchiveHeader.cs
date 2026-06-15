using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public class ArchiveHeader : BinarySerializable
{
    public int Pre_EntriesCount { get; set; }

    public ArchiveHeaderEntry[] Entries { get; set; }
    public Pointer[] EntryPointers { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        Entries = s.SerializeIntoArray<ArchiveHeaderEntry>(Entries, Pre_EntriesCount, ArchiveHeaderEntry.SerializeInto, name: nameof(ArchiveHeaderEntry));

        EntryPointers = new Pointer[Entries.Length];
        Pointer pointer = s.CurrentPointer;
        for (int i = 0; i < EntryPointers.Length; i++)
        {
            EntryPointers[i] = pointer;
            pointer += Entries[i].DataSize;
        }
    }
}