using BinarySerializer;

namespace GbaMonoGame.Rayman3.Readvanced;

public struct CollectedWhiteLum
{
    public CollectedWhiteLum(byte mapId, int instanceId)
    {
        MapId = mapId;
        InstanceId = instanceId;
    }

    public byte MapId { get; }
    public int InstanceId { get; }

    public static SerializeInto<CollectedWhiteLum> SerializeInto = (s, x) =>
    {
        byte mapId = s.Serialize<byte>(x.MapId, name: nameof(MapId));
        int instanceId = s.Serialize<int>(x.InstanceId, name: nameof(InstanceId));
        return new CollectedWhiteLum(mapId, instanceId);
    };
}