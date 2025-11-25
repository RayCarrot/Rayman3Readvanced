using System;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame;

public static class MultiJoyPad
{
    static MultiJoyPad()
    {
        // Create arrays
        JoyPads = new SimpleJoyPad[MaxPlayersCount][];
        for (int i = 0; i < JoyPads.Length; i++)
            JoyPads[i] = new SimpleJoyPad[BufferedFramesCount];

        ValidFlags = new bool[MaxPlayersCount][];
        for (int i = 0; i < ValidFlags.Length; i++)
            ValidFlags[i] = new bool[BufferedFramesCount];

        FirstReadFlags = new bool[MaxPlayersCount];
    }

    private const int BufferedFramesCount = 4;
    private const int MaxPlayersCount = RSMultiplayer.MaxPlayersCount;

    public static SimpleJoyPad[][] JoyPads { get; }
    public static bool[][] ValidFlags { get; }
    public static bool[] FirstReadFlags { get; }

    public static void Init()
    {
        for (int player = 0; player < MaxPlayersCount; player++)
        {
            FirstReadFlags[player] = false;

            for (int frame = 0; frame < BufferedFramesCount; frame++)
            {
                JoyPads[player][frame] = new SimpleJoyPad();
                ValidFlags[player][frame] = false;
            }
        }
    }

    public static void Read(int machineId, uint machineTimer, GbaInput input)
    {
        if (machineId is < 0 or >= MaxPlayersCount)
            throw new Exception("Invalid machine id");

        uint frame = machineTimer % BufferedFramesCount;
        uint prevFrame = (machineTimer - 1) % BufferedFramesCount;

        if (!FirstReadFlags[machineId])
        {
            JoyPads[machineId][frame].KeyTriggers = GbaInput.None;
            FirstReadFlags[machineId] = true;
        }
        else
        {
            JoyPads[machineId][frame].KeyTriggers = input ^ JoyPads[machineId][prevFrame].KeyStatus;
        }

        JoyPads[machineId][frame].KeyStatus = input;
        ValidFlags[machineId][frame] = true;
    }

    public static void SetInput(int machineId, uint machineTimer, GbaInput input)
    {
        if (machineId is < 0 or >= MaxPlayersCount)
            throw new Exception("Invalid machine id");

        uint frame = machineTimer % BufferedFramesCount;

        JoyPads[machineId][frame].KeyTriggers = input ^ JoyPads[machineId][frame].KeyStatus;
        JoyPads[machineId][frame].KeyStatus = input;
        ValidFlags[machineId][frame] = true;
    }

    public static void NewFrame(int machineId, uint machineTimer)
    {
        if (machineId is < 0 or >= MaxPlayersCount)
            throw new Exception("Invalid machine id");

        uint frame = machineTimer % BufferedFramesCount;
        JoyPads[machineId][frame].KeyTriggers = GbaInput.None;
    }

    public static void Clear(int machineId, uint machineTimer)
    {
        if (machineId is < 0 or >= MaxPlayersCount)
            throw new Exception("Invalid machine id");

        uint frame = machineTimer % BufferedFramesCount;
        uint prevFrame = (machineTimer - 1) % BufferedFramesCount;

        if (!FirstReadFlags[machineId])
            JoyPads[machineId][frame].KeyStatus = GbaInput.None;
        else
            JoyPads[machineId][frame].KeyStatus = JoyPads[machineId][prevFrame].KeyStatus;

        FirstReadFlags[machineId] = true;
        JoyPads[machineId][frame].KeyTriggers = GbaInput.None;
        ValidFlags[machineId][frame] = true;
    }

    public static GbaInput GetInput(int machineId, uint machineTimer)
    {
        return JoyPads[machineId][machineTimer % BufferedFramesCount].KeyStatus;
    }

    public static SimpleJoyPad GetSimpleJoyPadForCurrentFrame(int machineId)
    {
        if (machineId is < 0 or >= MaxPlayersCount)
            throw new Exception("Invalid machine id");

        return JoyPads[machineId][MultiplayerManager.GetMachineTimer() % BufferedFramesCount];
    }

    public static bool IsValid(int machineId, uint machineTimer)
    {
        if (machineId is < 0 or >= MaxPlayersCount)
            throw new Exception("Invalid machine id");

        return ValidFlags[machineId][machineTimer % BufferedFramesCount];
    }

    public static uint? GetNextInvalidTime(int machineId, uint machineTimer)
    {
        for (int i = 0; i < BufferedFramesCount; i++)
        {
            if (!IsValid(machineId, (uint)(machineTimer + i)))
                return (uint)((machineTimer + i) % BufferedFramesCount);
        }

        return null;
    }

    public static void ReleaseJoyPads(uint machineTimer)
    {
        for (int id = 0; id < RSMultiplayer.MaxPlayersCount; id++)
            ValidFlags[id][machineTimer % BufferedFramesCount] = false;
    }

    public static bool IsButtonPressed(int machineId, GbaInput gbaInput)
    {
        if (RSMultiplayer.IsActive)
            return GetSimpleJoyPadForCurrentFrame(machineId).IsButtonPressed(gbaInput);
        else
            return JoyPad.IsButtonPressed(gbaInput);
    }

    public static bool IsButtonReleased(int machineId, GbaInput gbaInput)
    {
        if (RSMultiplayer.IsActive)
            return GetSimpleJoyPadForCurrentFrame(machineId).IsButtonReleased(gbaInput);
        else
            return JoyPad.IsButtonReleased(gbaInput);
    }

    public static bool IsButtonJustPressed(int machineId, GbaInput gbaInput)
    {
        if (RSMultiplayer.IsActive)
            return GetSimpleJoyPadForCurrentFrame(machineId).IsButtonJustPressed(gbaInput);
        else
            return JoyPad.IsButtonJustPressed(gbaInput);
    }

    public static bool IsButtonJustReleased(int machineId, GbaInput gbaInput)
    {
        if (RSMultiplayer.IsActive)
            return GetSimpleJoyPadForCurrentFrame(machineId).IsButtonJustReleased(gbaInput);
        else
            return JoyPad.IsButtonJustReleased(gbaInput);
    }

    public static bool IsButtonPressed(int machineId, Rayman3Input rayman3Input)
    {
        if (RSMultiplayer.IsActive)
            return GetSimpleJoyPadForCurrentFrame(machineId).IsButtonPressed(rayman3Input);
        else
            return JoyPad.IsButtonPressed(rayman3Input);
    }

    public static bool IsButtonReleased(int machineId, Rayman3Input rayman3Input)
    {
        if (RSMultiplayer.IsActive)
            return GetSimpleJoyPadForCurrentFrame(machineId).IsButtonReleased(rayman3Input);
        else
            return JoyPad.IsButtonReleased(rayman3Input);
    }

    public static bool IsButtonJustPressed(int machineId, Rayman3Input rayman3Input)
    {
        if (RSMultiplayer.IsActive)
            return GetSimpleJoyPadForCurrentFrame(machineId).IsButtonJustPressed(rayman3Input);
        else
            return JoyPad.IsButtonJustPressed(rayman3Input);
    }

    public static bool IsButtonJustReleased(int machineId, Rayman3Input rayman3Input)
    {
        if (RSMultiplayer.IsActive)
            return GetSimpleJoyPadForCurrentFrame(machineId).IsButtonJustReleased(rayman3Input);
        else
            return JoyPad.IsButtonJustReleased(rayman3Input);
    }
}