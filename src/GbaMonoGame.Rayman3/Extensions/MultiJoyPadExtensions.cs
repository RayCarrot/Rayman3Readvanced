namespace GbaMonoGame.Rayman3;

public static class MultiJoyPadExtensions
{
    extension(MultiJoyPad multiJoyPad)
    {
        public bool IsButtonPressed(int machineId, Rayman3Input rayman3Input, bool buffered = false)
        {
            if (RSMultiplayer.IsActive)
            {
                if (Engine.Settings.Active.Tweaks.UseInputBuffering && buffered)
                    return multiJoyPad.JoyPadBuffers[machineId].IsButtonPressed(rayman3Input);
                else
                    return multiJoyPad.GetSimpleJoyPadForCurrentFrame(machineId).IsButtonPressed(rayman3Input);
            }
            else
            {
                return Engine.JoyPad.IsButtonPressed(rayman3Input, buffered);
            }
        }

        public bool IsButtonReleased(int machineId, Rayman3Input rayman3Input, bool buffered = false)
        {
            if (RSMultiplayer.IsActive)
            {
                if (Engine.Settings.Active.Tweaks.UseInputBuffering && buffered)
                    return multiJoyPad.JoyPadBuffers[machineId].IsButtonReleased(rayman3Input);
                else
                    return multiJoyPad.GetSimpleJoyPadForCurrentFrame(machineId).IsButtonReleased(rayman3Input);
            }
            else
            {
                return Engine.JoyPad.IsButtonReleased(rayman3Input, buffered);
            }
        }

        public bool IsButtonJustPressed(int machineId, Rayman3Input rayman3Input, bool buffered = false)
        {
            if (RSMultiplayer.IsActive)
            {
                if (Engine.Settings.Active.Tweaks.UseInputBuffering && buffered)
                    return multiJoyPad.JoyPadBuffers[machineId].IsButtonJustPressed(rayman3Input);
                else
                    return multiJoyPad.GetSimpleJoyPadForCurrentFrame(machineId).IsButtonJustPressed(rayman3Input);
            }
            else
            {
                return Engine.JoyPad.IsButtonJustPressed(rayman3Input, buffered);
            }
        }

        public bool IsButtonJustReleased(int machineId, Rayman3Input rayman3Input, bool buffered = false)
        {
            if (RSMultiplayer.IsActive)
            {
                if (Engine.Settings.Active.Tweaks.UseInputBuffering && buffered)
                    return multiJoyPad.JoyPadBuffers[machineId].IsButtonJustReleased(rayman3Input);
                else
                    return multiJoyPad.GetSimpleJoyPadForCurrentFrame(machineId).IsButtonJustReleased(rayman3Input);
            }
            else
            {
                return Engine.JoyPad.IsButtonJustReleased(rayman3Input, buffered);
            }
        }
    }
}