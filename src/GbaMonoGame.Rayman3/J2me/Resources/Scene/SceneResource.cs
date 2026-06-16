using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public class SceneResource : ArchiveResource
{
    public byte ActorsCount { get; set; }
    public ActorInstance[] Actors { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        ActorsCount = s.Serialize<byte>(ActorsCount, name: nameof(ActorsCount));
        s.DoEncoded(new ActorArrayEncoder(ActorsCount), () =>
        {
            Actors = s.SerializeObjectArray<ActorInstance>(Actors, ActorsCount, name: nameof(Actors));
        });
    }
}