using BinarySerializer;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3.Readvanced;

public class ReadvancedSlot : BaseReadvancedSave
{
    public int Version { get; set; }

    // Original save
    public SaveGameSlot SaveGame { get; set; }

    // Stats
    public long PlayTime { get; set; }

    // Achievements tracking
    public PirateType DefeatedPirateTypes { get; set; }

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

        Version = s.Serialize<int>(Version, name: nameof(Version));
        SaveGame = s.SerializeObject<SaveGameSlot>(SaveGame, name: nameof(SaveGame));
        PlayTime = s.Serialize<long>(PlayTime, name: nameof(PlayTime));
        DefeatedPirateTypes = s.Serialize<PirateType>(DefeatedPirateTypes, name: nameof(DefeatedPirateTypes));
    }
}