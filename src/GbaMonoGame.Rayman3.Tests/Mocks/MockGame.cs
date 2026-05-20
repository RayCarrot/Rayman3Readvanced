using System.ComponentModel.Design;
using BinarySerializer;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Rayman3.Tests;
using Microsoft.Xna.Framework.Graphics;
using Action = System.Action;

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
            settings: new SettingsManager(new LocalGameSettings()),
            app: new MockApplicationManager(),
            input: new InputManager(),
            window: new MockGameWindowManager(),
            viewPort: new ViewPortManager(),
            assets: new AssetManager(serviceProvider),
            config: new GameConfigManager(),
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

    private GbaInput[] ReadRecording(string fileName)
    {
        string filePath = Path.Combine("Recordings", $"{fileName}.rec");
        
        using Stream fileStream = File.OpenRead(filePath);
        using Reader reader = new(fileStream);

        List<GbaInput> inputs = [];
        while (reader.BaseStream.Position < reader.BaseStream.Length)
            inputs.Add((GbaInput)reader.ReadUInt16());

        return inputs.ToArray();
    }

    public void Step()
    {
        Engine.Step();
    }

    public void StepRecording(string fileName, Action stepCallback)
    {
        GbaInput[] inputs = ReadRecording(fileName);

        JoyPad.SetReplayData(inputs);
        for (int i = 0; i < inputs.Length; i++)
        {
            Step();
            stepCallback();
        }
    }

    public void Dispose()
    {
        _mockGraphicsDeviceService.Dispose();
    }
}