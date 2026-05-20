using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame;

// Custom class to allow for buffered inputs
public class JoyPadBuffer
{
    public JoyPadBuffer()
    {
        Buffer = new SimpleJoyPad[BufferLength];
        for (int i = 0; i < BufferLength; i++)
            Buffer[i] = new SimpleJoyPad();
    }

    private const int BufferLength = 5;

    public SimpleJoyPad[] Buffer { get; }
    public int CurrentBufferIndex { get; set; }

    public void Push(SimpleJoyPad joyPad)
    {
        Buffer[CurrentBufferIndex].KeyStatus = joyPad.KeyStatus;
        Buffer[CurrentBufferIndex].KeyTriggers = joyPad.KeyTriggers;
        CurrentBufferIndex = (CurrentBufferIndex + 1) % BufferLength;
    }

    public bool IsButtonPressed(GbaInput gbaInput)
    {
        foreach (SimpleJoyPad joyPad in Buffer)
        {
            if (joyPad.IsButtonPressed(gbaInput))
                return true;
        }

        return false;
    }

    public bool IsButtonReleased(GbaInput gbaInput)
    {
        foreach (SimpleJoyPad joyPad in Buffer)
        {
            if (joyPad.IsButtonReleased(gbaInput))
                return true;
        }

        return false;
    }

    public bool IsButtonJustPressed(GbaInput gbaInput)
    {
        foreach (SimpleJoyPad joyPad in Buffer)
        {
            if (joyPad.IsButtonJustPressed(gbaInput))
                return true;
        }

        return false;
    }

    public bool IsButtonJustReleased(GbaInput gbaInput)
    {
        foreach (SimpleJoyPad joyPad in Buffer)
        {
            if (joyPad.IsButtonJustReleased(gbaInput))
                return true;
        }

        return false;
    }
}