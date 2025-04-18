using System;
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

        // Create and run the game
        using var game = new GbaMonoGame.Rayman3.Rayman3();
        game.Run();
    }
}
