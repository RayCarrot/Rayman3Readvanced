using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame;

// TODO: Option to use "normal" buttons?
public static class NGageJoyPadHelpers
{
    public static bool IsNumpadJustPressed()
    {
        Debug.Assert(Rom.Platform == Platform.NGage);

        // NOTE: The game also checks 0-4 and 9, which it has mapped to new inputs. We however don't have those mapped.
        return JoyPad.IsButtonJustPressed(GbaInput.A) || // 5
               JoyPad.IsButtonJustPressed(GbaInput.L) || // 6
               JoyPad.IsButtonJustPressed(GbaInput.B) || // 7
               JoyPad.IsButtonJustPressed(GbaInput.R);   // 8
    }

    public static bool MultiIsNumpadJustPressed(int machineId)
    {
        Debug.Assert(Rom.Platform == Platform.NGage);

        // NOTE: The game also checks 0-4 and 9, which it has mapped to new inputs. We however don't have those mapped.
        return MultiJoyPad.IsButtonJustPressed(machineId, GbaInput.A) || // 5
               MultiJoyPad.IsButtonJustPressed(machineId, GbaInput.L) || // 6
               MultiJoyPad.IsButtonJustPressed(machineId, GbaInput.B) || // 7
               MultiJoyPad.IsButtonJustPressed(machineId, GbaInput.R);   // 8
    }

    public static bool IsSoftButtonJustPressed()
    {
        Debug.Assert(Rom.Platform == Platform.NGage);

        return JoyPad.IsButtonJustPressed(GbaInput.Start) || // Left soft button
               JoyPad.IsButtonJustPressed(GbaInput.Select);  // Right soft button
    }

    public static bool MultiIsSoftButtonJustPressed(int machineId)
    {
        Debug.Assert(Rom.Platform == Platform.NGage);

        return MultiJoyPad.IsButtonJustPressed(machineId, GbaInput.Start) || // Left soft button
               MultiJoyPad.IsButtonJustPressed(machineId, GbaInput.Select);  // Right soft button
    }

    public static bool IsConfirmButtonJustPressed()
    {
        Debug.Assert(Rom.Platform == Platform.NGage);

        // NOTE: The game also checks numpad 0.
        return JoyPad.IsButtonJustPressed(GbaInput.A) || 
               JoyPad.IsButtonJustPressed(GbaInput.Start);
    }

    public static bool MultiIsConfirmButtonJustPressed(int machineId)
    {
        Debug.Assert(Rom.Platform == Platform.NGage);

        // NOTE: The game also checks numpad 0.
        return MultiJoyPad.IsButtonJustPressed(machineId, GbaInput.A) ||
               MultiJoyPad.IsButtonJustPressed(machineId, GbaInput.Start);
    }

    public static bool IsBackButtonJustPressed()
    {
        Debug.Assert(Rom.Platform == Platform.NGage);

        return JoyPad.IsButtonJustPressed(GbaInput.B) || 
               JoyPad.IsButtonJustPressed(GbaInput.Select);
    }

    public static bool MultiIsBackButtonJustPressed(int machineId)
    {
        Debug.Assert(Rom.Platform == Platform.NGage);

        return MultiJoyPad.IsButtonJustPressed(machineId, GbaInput.B) ||
               MultiJoyPad.IsButtonJustPressed(machineId, GbaInput.Select);
    }
}