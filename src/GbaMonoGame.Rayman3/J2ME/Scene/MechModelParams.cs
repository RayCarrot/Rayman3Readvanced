namespace GbaMonoGame.Rayman3.J2ME;

public readonly struct MechModelParams
{
    public byte ParamsCount { get; init; }
    public MM_TYPE Type { get; init; }
    public sbyte[] Params { get; init; }
}