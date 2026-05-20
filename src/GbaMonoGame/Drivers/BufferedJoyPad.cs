using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame;

public class BufferedJoyPad
{
    public SimpleJoyPad Current { get; } = new();
    public JoyPadBuffer Buffer { get; } = new();

    public bool IsInReplayMode => Current.IsInReplayMode;
    public bool IsReplayFinished => Current.IsReplayFinished;
    public bool AllowBufferedInputs => !Current.IsInReplayMode && Engine.Settings.Active.Tweaks.UseInputBuffering;

    public void SetReplayData(GbaInput[] replayData) => Current.SetReplayData(replayData);

    public void Scan()
    {
        Current.Scan();
        Buffer.Push(Current);
    }

    public bool IsButtonPressed(GbaInput gbaInput, bool buffered = false)
    {
        if (AllowBufferedInputs && buffered)
            return Buffer.IsButtonPressed(gbaInput);
        else
            return Current.IsButtonPressed(gbaInput);
    }

    public bool IsButtonReleased(GbaInput gbaInput, bool buffered = false)
    {
        if (AllowBufferedInputs && buffered)
            return Buffer.IsButtonReleased(gbaInput);
        else
            return Current.IsButtonReleased(gbaInput);
    }

    public bool IsButtonJustPressed(GbaInput gbaInput, bool buffered = false)
    {
        if (AllowBufferedInputs && buffered)
            return Buffer.IsButtonJustPressed(gbaInput);
        else
            return Current.IsButtonJustPressed(gbaInput);
    }

    public bool IsButtonJustReleased(GbaInput gbaInput, bool buffered = false)
    {
        if (AllowBufferedInputs && buffered)
            return Buffer.IsButtonJustReleased(gbaInput);
        else
            return Current.IsButtonJustReleased(gbaInput);
    }
}