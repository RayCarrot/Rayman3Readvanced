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

    public override int Version => 1;
    public override string Id => "SAVE";

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

    protected override void SerializeSave(SerializerObject s, int version)
    {
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