using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame;

public static class JoyPad
{
    public static SimpleJoyPad Current { get; } = new();

    public static bool IsInReplayMode => Current.IsInReplayMode;
    public static bool IsReplayFinished => Current.IsReplayFinished;

    public static void SetReplayData(GbaInput[] replayData) => Current.SetReplayData(replayData);

    public static void Scan() => Current.Scan();

    public static bool IsButtonPressed(GbaInput gbaInput) => Current.IsButtonPressed(gbaInput);
    public static bool IsButtonReleased(GbaInput gbaInput) => Current.IsButtonReleased(gbaInput);
    public static bool IsButtonJustPressed(GbaInput gbaInput) => Current.IsButtonJustPressed(gbaInput);
    public static bool IsButtonJustReleased(GbaInput gbaInput) => Current.IsButtonJustReleased(gbaInput);

    public static bool IsButtonPressed(Rayman3Input rayman3Input) => Current.IsButtonPressed(rayman3Input);
    public static bool IsButtonReleased(Rayman3Input rayman3Input) => Current.IsButtonReleased(rayman3Input);
    public static bool IsButtonJustPressed(Rayman3Input rayman3Input) => Current.IsButtonJustPressed(rayman3Input);
    public static bool IsButtonJustReleased(Rayman3Input rayman3Input) => Current.IsButtonJustReleased(rayman3Input);
}