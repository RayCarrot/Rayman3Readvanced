using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3;

// TODO: Implement
public class Credits : Frame
{
    public override void Init()
    {
        Rom.LoadResource<AnimActor>(126);
        Rom.LoadResource<TextureTable>(127);
    }

    public override void Step()
    {
        
    }
}