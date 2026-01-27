using BinarySerializer;

namespace GbaMonoGame.Rayman3.Readvanced;

public class GhostMapData : BinarySerializable
{
    public MapId MapId { get; set; }
    public GhostFrame[] Frames { get; set; }
    
    public override void SerializeImpl(SerializerObject s)
    {
        MapId = s.Serialize<MapId>(MapId, name: nameof(MapId));
        Frames = s.SerializeArraySize<GhostFrame, int>(Frames, name: nameof(Frames));
        Frames = s.SerializeObjectArray<GhostFrame>(Frames, Frames.Length, name: nameof(Frames));
    }
}