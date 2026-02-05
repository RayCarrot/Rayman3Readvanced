using BinarySerializer;

namespace GbaMonoGame.Rayman3.Readvanced;

public class GhostMapData : BinarySerializable
{
    public MapId MapId { get; set; }
    public bool IsMode7 { get; set; }
    public GhostFrame[] Frames { get; set; }
    
    public override void SerializeImpl(SerializerObject s)
    {
        MapId = s.Serialize<MapId>(MapId, name: nameof(MapId));
        IsMode7 = s.Serialize<bool>(IsMode7, name: nameof(IsMode7));
        Frames = s.SerializeArraySize<GhostFrame, int>(Frames, name: nameof(Frames));
        Frames = s.SerializeObjectArray<GhostFrame>(Frames, Frames.Length, x => x.Pre_IsMode7 = IsMode7, name: nameof(Frames));
    }
}