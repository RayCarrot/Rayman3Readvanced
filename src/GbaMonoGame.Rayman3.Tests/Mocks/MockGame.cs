using System.ComponentModel.Design;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Rayman3.Tests;
using Microsoft.Xna.Framework.Graphics;

[assembly: AssemblyFixture(typeof(MockGame))]

namespace GbaMonoGame.Rayman3.Tests;

public sealed class MockGame : IDisposable
{
    public MockGame()
    {
        _mockGraphicsDeviceService = new MockGraphicsDeviceService();
        
        ServiceContainer serviceProvider = new();
        serviceProvider.AddService(typeof(IGraphicsDeviceService), _mockGraphicsDeviceService);

        // Initialize the engine
        Engine.InitEngine(
            config: new ConfigManager(new LocalGameConfig()),
            app: new MockApplicationManager(),
            input: new InputManager(),
            window: new MockGameWindowManager(),
            viewPort: new ViewPortManager(),
            assets: new AssetManager(serviceProvider),
            messages: new MessageManager(),
            richPresence: new MockRichPresenceManager(),
            frameMngr: new FrameManager());
        Gfx.Load();
        Rayman3.InitEngine();

        // Initialize game
        Rom.Init(Game.Rayman3, Platform.GBA);
        Engine.InitGame(
            sem: new MockSoundEventsManager(),
            font: new FontManager(Rom.Loader.Font8, Rom.Loader.Font16, Rom.Loader.Font32));
        Rayman3.InitGame(
            save: new MockSaveGameManager());

        // Set up game info
        Random.SetSeed(0xDEADBEEF);
        GameTime.Reset();
        GameInfo.Init();
        GameInfo.ResetPersistentInfo();
        GameInfo.StartPlayTime();
        GameInfo.CurrentSlot = 0;
        GameInfo.Init();
    }

    private readonly MockGraphicsDeviceService _mockGraphicsDeviceService;

    public void Step()
    {
        Engine.Step();
    }

    public void Dispose()
    {
        _mockGraphicsDeviceService.Dispose();
    }
}