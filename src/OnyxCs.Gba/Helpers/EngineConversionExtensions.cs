﻿namespace OnyxCs.Gba;

public static class EngineConversionExtensions
{
    public static Vector2 ToVector2(this BinarySerializer.Onyx.Gba.Vector2 vector2) => new(vector2.X, vector2.Y);
}