using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame;

public class SimpleJoyPad
{
    public GbaInput KeyStatus { get; set; }
    public GbaInput KeyTriggers { get; set; } // Just pressed

    public GbaInput[] ReplayData { get; set; }
    public int ReplayDataIndex { get; set; }

    public bool IsInReplayMode => ReplayData != null;
    public bool IsReplayFinished => KeyStatus == GbaInput.None;

    public void SetReplayData(GbaInput[] replayData)
    {
        ReplayData = replayData;
        ReplayDataIndex = 0;
    }

    public void Scan()
    {
        GbaInput inputs;

        if (!IsInReplayMode)
        {
            inputs = InputManager.GetGbaInputs();
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
    }

    public bool IsButtonPressed(GbaInput gbaInput) => (KeyStatus & gbaInput) != 0;
    public bool IsButtonReleased(GbaInput gbaInput) => (KeyStatus & gbaInput) == 0;
    public bool IsButtonJustPressed(GbaInput gbaInput) => (gbaInput & KeyStatus & KeyTriggers) != 0;
    public bool IsButtonJustReleased(GbaInput gbaInput) => (gbaInput & ~KeyStatus & KeyTriggers) != 0;
}