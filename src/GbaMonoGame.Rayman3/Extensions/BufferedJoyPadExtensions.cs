namespace GbaMonoGame.Rayman3;

public static class BufferedJoyPadExtensions
{
    extension(BufferedJoyPad bufferedJoyPad)
    {
        public bool IsButtonPressed(Rayman3Input rayman3Input, bool buffered = false)
        {
            if (bufferedJoyPad.AllowBufferedInputs && buffered)
                return bufferedJoyPad.Buffer.IsButtonPressed(rayman3Input);
            else
                return bufferedJoyPad.Current.IsButtonPressed(rayman3Input);
        }

        public bool IsButtonReleased(Rayman3Input rayman3Input, bool buffered = false)
        {
            if (bufferedJoyPad.AllowBufferedInputs && buffered)
                return bufferedJoyPad.Buffer.IsButtonReleased(rayman3Input);
            else
                return bufferedJoyPad.Current.IsButtonReleased(rayman3Input);
        }

        public bool IsButtonJustPressed(Rayman3Input rayman3Input, bool buffered = false)
        {
            if (bufferedJoyPad.AllowBufferedInputs && buffered)
                return bufferedJoyPad.Buffer.IsButtonJustPressed(rayman3Input);
            else
                return bufferedJoyPad.Current.IsButtonJustPressed(rayman3Input);
        }

        public bool IsButtonJustReleased(Rayman3Input rayman3Input, bool buffered = false)
        {
            if (bufferedJoyPad.AllowBufferedInputs && buffered)
                return bufferedJoyPad.Buffer.IsButtonJustReleased(rayman3Input);
            else
                return bufferedJoyPad.Current.IsButtonJustReleased(rayman3Input);
        }
    }
}