using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public readonly struct AnimationAction
{
    public AnimationAction(Action action, MechModelParams mechModelParams)
    {
        Action = action;
        MechModelParams = mechModelParams;
    }

    public Action Action { get; }
    public MechModelParams MechModelParams { get; }

    public static SerializeInto<AnimationAction> SerializeInto = (s, x) =>
    {
        Action action = s.SerializeInto<Action>(x.Action, Action.SerializeInto, name: nameof(Action));
        MechModelParams mechModelParams = s.SerializeInto<MechModelParams>(x.MechModelParams, MechModelParams.SerializeInto, name: nameof(MechModelParams));

        return new AnimationAction(action, mechModelParams);
    };
}