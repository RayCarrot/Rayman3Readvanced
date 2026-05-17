using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame;

public static class JoyPad
{
    public static SimpleJoyPad Current { get; } = new();
    public static JoyPadBuffer Buffer { get; } = new();

    public static bool IsInReplayMode => Current.IsInReplayMode;
    public static bool IsReplayFinished => Current.IsReplayFinished;
    public static bool AllowBufferedInputs => !Current.IsInReplayMode && Engine.Config.Active.Tweaks.UseInputBuffering;

    public static void SetReplayData(GbaInput[] replayData) => Current.SetReplayData(replayData);

    public static void Scan()
    {
        Current.Scan();
        Buffer.Push(Current);
    }

    public static bool IsButtonPressed(GbaInput gbaInput, bool buffered = false)
    {
        if (AllowBufferedInputs && buffered)
            return Buffer.IsButtonPressed(gbaInput);
        else
            return Current.IsButtonPressed(gbaInput);
    }

    public static bool IsButtonReleased(GbaInput gbaInput, bool buffered = false)
    {
        if (AllowBufferedInputs && buffered)
            return Buffer.IsButtonReleased(gbaInput);
        else
            return Current.IsButtonReleased(gbaInput);
    }

    public static bool IsButtonJustPressed(GbaInput gbaInput, bool buffered = false)
    {
        if (AllowBufferedInputs && buffered)
            return Buffer.IsButtonJustPressed(gbaInput);
        else
            return Current.IsButtonJustPressed(gbaInput);
    }

    public static bool IsButtonJustReleased(GbaInput gbaInput, bool buffered = false)
    {
        if (AllowBufferedInputs && buffered)
            return Buffer.IsButtonJustReleased(gbaInput);
        else
            return Current.IsButtonJustReleased(gbaInput);
    }

    public static bool IsButtonPressed(Rayman3Input rayman3Input, bool buffered = false)
    {
        if (AllowBufferedInputs && buffered)
            return Buffer.IsButtonPressed(rayman3Input);
        else
            return Current.IsButtonPressed(rayman3Input);
    }

    public static bool IsButtonReleased(Rayman3Input rayman3Input, bool buffered = false)
    {
        if (AllowBufferedInputs && buffered)
            return Buffer.IsButtonReleased(rayman3Input);
        else
            return Current.IsButtonReleased(rayman3Input);
    }

    public static bool IsButtonJustPressed(Rayman3Input rayman3Input, bool buffered = false)
    {
        if (AllowBufferedInputs && buffered)
            return Buffer.IsButtonJustPressed(rayman3Input);
        else
            return Current.IsButtonJustPressed(rayman3Input);
    }

    public static bool IsButtonJustReleased(Rayman3Input rayman3Input, bool buffered = false)
    {
        if (AllowBufferedInputs && buffered)
            return Buffer.IsButtonJustReleased(rayman3Input);
        else
            return Current.IsButtonJustReleased(rayman3Input);
    }
}