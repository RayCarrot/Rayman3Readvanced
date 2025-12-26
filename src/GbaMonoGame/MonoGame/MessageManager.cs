using System.Collections.Generic;
using System.Threading.Tasks;

namespace GbaMonoGame;

public class MessageManager
{
    private readonly Queue<Message> _messageQueue = new();
    private Task _messageTask;

    public void EnqueueMessage(string text, string header)
    {
        _messageQueue.Enqueue(new Message(header, text));
    }

    public void ShowQueuedMessage()
    {
        if (IsShowingMessage())
            return;

        if (_messageQueue.TryDequeue(out Message msg))
        {
#if WINDOWSDX
            // Force windowed mode for this
            DisplayMode prevDisplayMode = Engine.GameWindow.DisplayMode;
            if (Engine.GameWindow.DisplayMode != DisplayMode.Windowed)
                Engine.GameWindow.DisplayMode = DisplayMode.Windowed;

            // Use WinForms messagebox on Windows since it looks nicer than the custom MonoGame one
            System.Windows.Forms.MessageBox.Show(msg.Text, msg.Header, System.Windows.Forms.MessageBoxButtons.OK);
            
            // Restore the display mode
            if (prevDisplayMode != DisplayMode.Windowed)
                Engine.GameWindow.DisplayMode = prevDisplayMode;
#else
            _messageTask = Microsoft.Xna.Framework.Input.MessageBox.Show(msg.Header, msg.Text, ["OK"]);
#endif
        }
    }

    public bool IsShowingMessage()
    {
        if (_messageTask is { IsCompleted: true })
            _messageTask = null;

        return _messageTask != null;
    }

    private readonly struct Message(string header, string text)
    {
        public string Header { get; } = header;
        public string Text { get; } = text;
    }
}