using BinarySerializer;

namespace GbaMonoGame.Rayman3.Readvanced;

// TODO: Maybe compress the data? Files get quite big otherwise.
public class TimeAttackGhostSave : BaseReadvancedSave
{
    public GhostMapData[] MapGhosts { get; set; }
    
    public override void SerializeImpl(SerializerObject s)
    {
        base.SerializeImpl(s);
        s.SerializeMagicString("GHST", 4);

        MapGhosts = s.SerializeArraySize<GhostMapData, int>(MapGhosts, name: nameof(MapGhosts));
        MapGhosts = s.SerializeObjectArray<GhostMapData>(MapGhosts, MapGhosts.Length, name: nameof(MapGhosts));
    }
}