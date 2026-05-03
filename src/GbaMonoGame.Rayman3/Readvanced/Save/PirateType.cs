using System;

namespace GbaMonoGame.Rayman3.Readvanced;

[Flags]
public enum PirateType : byte
{
    None = 0,
    Red = 1 << 0,
    Silver = 1 << 1,
    Green = 1 << 2,
    Blue = 1 << 3,
    All = Red | Silver | Green | Blue
}