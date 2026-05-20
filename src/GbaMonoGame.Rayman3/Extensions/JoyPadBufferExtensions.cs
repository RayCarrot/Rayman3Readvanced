namespace GbaMonoGame.Rayman3;

public static class JoyPadBufferExtensions
{
    extension(JoyPadBuffer joyPadBuffer)
    {
        public bool IsButtonPressed(Rayman3Input rayman3Input)
        {
            foreach (SimpleJoyPad joyPad in joyPadBuffer.Buffer)
            {
                if (joyPad.IsButtonPressed(rayman3Input))
                    return true;
            }

            return false;
        }

        public bool IsButtonReleased(Rayman3Input rayman3Input)
        {
            foreach (SimpleJoyPad joyPad in joyPadBuffer.Buffer)
            {
                if (joyPad.IsButtonReleased(rayman3Input))
                    return true;
            }

            return false;
        }

        public bool IsButtonJustPressed(Rayman3Input rayman3Input)
        {
            foreach (SimpleJoyPad joyPad in joyPadBuffer.Buffer)
            {
                if (joyPad.IsButtonJustPressed(rayman3Input))
                    return true;
            }

            return false;
        }

        public bool IsButtonJustReleased(Rayman3Input rayman3Input)
        {
            foreach (SimpleJoyPad joyPad in joyPadBuffer.Buffer)
            {
                if (joyPad.IsButtonJustReleased(rayman3Input))
                    return true;
            }

            return false;
        }
    }
}