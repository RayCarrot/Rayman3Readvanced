using BinarySerializer;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3.Readvanced;

public class ReadvancedSlot : BaseReadvancedSave
{
    public SaveGameSlot SaveGame { get; set; }
    public long PlayTime { get; set; }

    public static ReadvancedSlot FromSaveGame(SaveGameSlot save)
    {
        return new ReadvancedSlot()
        {
            SaveGame = save
        };
    }

    public override void SerializeImpl(SerializerObject s)
    {
        base.SerializeImpl(s);
        s.SerializeMagicString("SAVE", 4);

        SaveGame = s.SerializeObject<SaveGameSlot>(SaveGame, name: nameof(SaveGame));
        PlayTime = s.Serialize<long>(PlayTime, name: nameof(PlayTime));
    }
}