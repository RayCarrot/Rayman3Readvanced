using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

public readonly struct MechModelParams
{
    public MechModelParams(byte paramsCount, MechModelType type, sbyte[] @params)
    {
        ParamsCount = paramsCount;
        Type = type;
        Params = @params;
    }

    public byte ParamsCount { get; }
    public MechModelType Type { get; }
    public sbyte[] Params { get; }

    public static SerializeInto<MechModelParams> SerializeInto = (s, x) =>
    {
        byte paramsCount = x.ParamsCount;
        MechModelType type = x.Type;
        s.DoBits<byte>(b =>
        {
            paramsCount = b.SerializeBits<byte>(x.ParamsCount, 4, name: nameof(ParamsCount));
            type = b.SerializeBits<MechModelType>(x.Type, 4, name: nameof(Type));
        });
        sbyte[] @params = s.SerializeArray<sbyte>(x.Params, paramsCount, name: nameof(Params));

        return new MechModelParams(paramsCount, type, @params);
    };
}