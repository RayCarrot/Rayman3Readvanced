using BinarySerializer;

namespace GbaMonoGame.Rayman3.Readvanced;

// TODO: Maybe compress the data? Files get quite big otherwise.
public class TimeAttackGhostSave : BaseReadvancedSave
{
    public override int Version => 0;
    public override string Id => "GHST";

    public GhostMapData[] MapGhosts { get; set; }

    protected override void SerializeSave(SerializerObject s, int version)
    {
        MapGhosts = s.SerializeArraySize<GhostMapData, int>(MapGhosts, name: nameof(MapGhosts));
        MapGhosts = s.SerializeObjectArray<GhostMapData>(MapGhosts, MapGhosts.Length, name: nameof(MapGhosts));
    }
}