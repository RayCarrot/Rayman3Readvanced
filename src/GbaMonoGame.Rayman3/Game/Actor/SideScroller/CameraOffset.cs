using System;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3;

public static class CameraOffset
{
    public static float Default => Rom.Platform switch
    {
        Platform.GBA => 40,
        Platform.NGage => 25,
        _ => throw new UnsupportedPlatformException()
    };

    public static float DefaultReversed => Rom.OriginalResolution.X - Default;

    public static float Center => Rom.OriginalResolution.X / 2;

    public static float Multiplayer => 95;

    public static float WalkingShell => 30;

    public static float NGagePlum => Rom.Platform switch
    {
        Platform.GBA => throw new InvalidOperationException(),
        Platform.NGage => 45,
        _ => throw new UnsupportedPlatformException()
    };
}