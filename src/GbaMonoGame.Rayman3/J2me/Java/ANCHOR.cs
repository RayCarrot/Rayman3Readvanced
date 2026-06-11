using System;

namespace GbaMonoGame.Rayman3.J2me;

[Flags]
public enum ANCHOR
{
    NONE = 0,
    HCENTER = 1 << 0,
    VCENTER = 1 << 1,
    LEFT = 1 << 2,
    RIGHT = 1 << 3,
    TOP = 1 << 4,
    BOTTOM = 1 << 5,
    BASELINE = 1 << 6,
}