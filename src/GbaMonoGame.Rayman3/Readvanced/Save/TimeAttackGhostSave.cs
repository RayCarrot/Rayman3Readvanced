using System.IO.Compression;
using BinarySerializer;

namespace GbaMonoGame.Rayman3.Readvanced;

public class TimeAttackGhostSave : BaseReadvancedSave
{
    public override int Version => 1;
    public override string Id => "GHST";

    public GhostMapData[] MapGhosts { get; set; }

    protected override void SerializeSave(SerializerObject s, int version)
    {
        s.DoEncoded(version >= 1 ? new DeflateEncoder(CompressionLevel.Optimal) : null, () =>
        {
            MapGhosts = s.SerializeArraySize<GhostMapData, int>(MapGhosts, name: nameof(MapGhosts));
            MapGhosts = s.SerializeObjectArray<GhostMapData>(MapGhosts, MapGhosts.Length, name: nameof(MapGhosts));
        });
    }
}