using System.Collections.Generic;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame;

public partial class SimpleJoyPad
{
    public GbaInput KeyStatus { get; set; }
    public GbaInput KeyTriggers { get; set; } // Just pressed

    public GbaInput[] ReplayData { get; set; }
    public int ReplayDataIndex { get; set; }
    public List<GbaInput> RecordedData { get; set; }

    public bool IsInReplayMode => ReplayData != null;
    public bool IsReplayFinished => KeyStatus == GbaInput.None;
    public bool IsInRecordMode => RecordedData != null;

    public void SetReplayData(GbaInput[] replayData)
    {
        ReplayData = replayData;
        ReplayDataIndex = 0;
    }

    public void BeginRecording()
    {
        RecordedData = new List<GbaInput>();
    }

    public GbaInput[] EndRecording()
    {
        GbaInput[] data = RecordedData.ToArray();
        RecordedData = null;
        return data;
    }

    public void Scan()
    {
        GbaInput inputs;

        if (!IsInReplayMode)
        {
            inputs = InputManager.GetPressedGbaInputs();
        }
        else
        {
            inputs = ReplayData[ReplayDataIndex];

            if (inputs == GbaInput.None)
                ReplayData = null;
            else
                ReplayDataIndex++;
        }

        KeyTriggers = inputs ^ KeyStatus;
        KeyStatus = inputs;

        if (IsInRecordMode)
            RecordedData.Add(KeyStatus);
    }

    public bool IsButtonPressed(GbaInput gbaInput) => (KeyStatus & gbaInput) != 0;
    public bool IsButtonReleased(GbaInput gbaInput) => (KeyStatus & gbaInput) == 0;
    public bool IsButtonJustPressed(GbaInput gbaInput) => (gbaInput & KeyStatus & KeyTriggers) != 0;
    public bool IsButtonJustReleased(GbaInput gbaInput) => (gbaInput & ~KeyStatus & KeyTriggers) != 0;
}