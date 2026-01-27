using BinarySerializer;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public class GhostFrame : BinarySerializable
{
    public GhostActorFrame[] Actors { get; set; }
    public GbaInput Input { get; set; } // Currently not used, but might be good to have in the future
    
    public override void SerializeImpl(SerializerObject s)
    {
        Actors = s.SerializeArraySize<GhostActorFrame, byte>(Actors, name: nameof(Actors));
        Actors = s.SerializeObjectArray<GhostActorFrame>(Actors, Actors.Length, name: nameof(Actors));
        Input = s.Serialize<GbaInput>(Input, name: nameof(Input));
    }
}