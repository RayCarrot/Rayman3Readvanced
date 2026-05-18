using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Tests;

public sealed class MockGraphicsDeviceService : IGraphicsDeviceService, IDisposable
{
    public MockGraphicsDeviceService()
    {
        // Create a hidden form
        _form = new Form()
        {
            Visible = false,
            ShowInTaskbar = false
        };

        // Create a graphics device
        PresentationParameters parameters = new()
        {
            BackBufferWidth = 1280,
            BackBufferHeight = 720,
            DeviceWindowHandle = _form.Handle,
            PresentationInterval = PresentInterval.Immediate,
            IsFullScreen = false
        };
        GraphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.Reach, parameters);
    }

    private readonly Form _form;

    public GraphicsDevice GraphicsDevice { get; }
    public event EventHandler<EventArgs>? DeviceCreated;
    public event EventHandler<EventArgs>? DeviceDisposing;
    public event EventHandler<EventArgs>? DeviceReset;
    public event EventHandler<EventArgs>? DeviceResetting;

    public void Dispose()
    {
        GraphicsDevice.Dispose();

        _form.Close();
        _form.Dispose();
    }
}