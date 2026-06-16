using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public class AnimationDataResource : ArchiveResource
{
    public byte Type { get; set; }
    public bool HasMechModel { get; set; }
    public byte ModulesCount { get; set; }
    public byte FramesCount { get; set; }
    public byte ActionsCount { get; set; }
    public AnimationModule[] Modules { get; set; }
    public AnimationFrame[] Frames { get; set; }
    public AnimationAction[] Actions { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        s.DoBits<byte>(b =>
        {
            Type = b.SerializeBits<byte>(Type, 7, name: nameof(Type));
            HasMechModel = b.SerializeBits<bool>(HasMechModel, 1, name: nameof(HasMechModel));
        });
        ModulesCount = s.Serialize<byte>(ModulesCount, name: nameof(ModulesCount));
        FramesCount = s.Serialize<byte>(FramesCount, name: nameof(FramesCount));
        ActionsCount = s.Serialize<byte>(ActionsCount, name: nameof(ActionsCount));
        Modules = s.SerializeIntoArray<AnimationModule>(Modules, ModulesCount, AnimationModule.SerializeInto, name: nameof(Modules));
        Frames = s.SerializeIntoArray<AnimationFrame>(Frames, FramesCount, AnimationFrame.SerializeInto, name: nameof(Frames));
        Actions = s.SerializeIntoArray<AnimationAction>(Actions, ActionsCount, AnimationAction.SerializeInto, name: nameof(Actions));
    }
}