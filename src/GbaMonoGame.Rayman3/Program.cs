using System;
using System.Runtime.InteropServices;
using System.Text;

internal class Program
{
#if WINDOWSDX
    [STAThread] // Required to use Windows Forms dialogs
#endif
    public static void Main(string[] args)
    {
        // Register encoding provider to be able to use Windows 1252
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Try and default to use dedicated GPU
        try
        {
            NativeLibrary.Load(Environment.Is64BitProcess ? "nvapi64.dll" : "nvapi.dll");
        }
        catch
        {
            // Do nothing
        }

        // Create and run the game
        using var game = new GbaMonoGame.Rayman3.Rayman3();
        game.Run();
    }
}
