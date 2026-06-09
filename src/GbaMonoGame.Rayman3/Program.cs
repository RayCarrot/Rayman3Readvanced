using System;
using System.Runtime.InteropServices;

internal class Program
{
#if WINDOWSDX
    [STAThread] // Required to use Windows Forms dialogs
#endif
    public static void Main(string[] args)
    {
        try
        {
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
            using var game = new GbaMonoGame.Rayman3.Rayman3GbaGame();
            game.Run();
        }
        catch (Exception ex)
        {
#if WINDOWSDX
            System.Windows.Forms.MessageBox.Show(ex.Message, "FATAL ERROR", System.Windows.Forms.MessageBoxButtons.OK);
#else
            Microsoft.Xna.Framework.Input.MessageBox.Show("FATAL ERROR", ex.Message, ["OK"]);
#endif
        }
    }
}
