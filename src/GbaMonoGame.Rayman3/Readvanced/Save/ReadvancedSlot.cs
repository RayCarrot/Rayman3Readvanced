using BinarySerializer;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3.Readvanced;

public class ReadvancedSlot : BaseReadvancedSave
{
    public ReadvancedSlot()
    {
        SaveGame = null;
        PlayTime = 0;
        DefeatedPirateTypes = PirateType.None;
        CollectedWhiteLums = [];
    }

    public const int Version = 1;

    // Original save
    public SaveGameSlot SaveGame { get; set; }

    // Stats
    public long PlayTime { get; set; }

    // Achievements tracking
    public PirateType DefeatedPirateTypes { get; set; }
    public CollectedWhiteLum[] CollectedWhiteLums { get; set; }

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

        int version = s.Serialize<int>(Version, name: nameof(Version));
        SaveGame = s.SerializeObject<SaveGameSlot>(SaveGame, name: nameof(SaveGame));
        PlayTime = s.Serialize<long>(PlayTime, name: nameof(PlayTime));
        DefeatedPirateTypes = s.Serialize<PirateType>(DefeatedPirateTypes, name: nameof(DefeatedPirateTypes));
        if (version >= 1)
        {
            CollectedWhiteLums = s.SerializeArraySize<CollectedWhiteLum, byte>(CollectedWhiteLums, name: nameof(CollectedWhiteLums));
            CollectedWhiteLums = s.SerializeIntoArray<CollectedWhiteLum>(CollectedWhiteLums, CollectedWhiteLums.Length, CollectedWhiteLum.SerializeInto, name: nameof(CollectedWhiteLums));
        }
    }
}