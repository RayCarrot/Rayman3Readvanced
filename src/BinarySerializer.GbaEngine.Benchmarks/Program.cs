using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BinarySerializer;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

BenchmarkRunner.Run<LoadRom>();

// | Method                 | Mean          | Error        | StdDev       | Gen0        | Gen1       | Gen2      | Allocated   |
// |----------------------- |--------------:|-------------:|-------------:|------------:|-----------:|----------:|------------:|
// | ReadFirstScene         |  46,120.21 us |   734.217 us |   686.787 us |   5333.3333 |  2000.0000 |  666.6667 | 44101.68 KB |
// | ReadAllScenes          | 473,157.57 us | 6,184.745 us | 5,785.214 us | 105000.0000 | 31000.0000 | 2000.0000 | 891436.3 KB |
// | ReadSoundBank          |     332.59 us |     1.829 us |     1.528 us |    112.3047 |    49.8047 |         - |   918.36 KB |
// | ReadFont               |      14.00 us |     0.164 us |     0.154 us |      2.4414 |          - |         - |    19.97 KB |
// | ReadTextBanks          |   3,580.67 us |    35.546 us |    31.510 us |    906.2500 |   800.7813 |         - |  7422.67 KB |
// | ReadLevelInfo          |      33.44 us |     0.338 us |     0.317 us |      7.3242 |     0.4272 |         - |    60.13 KB |
// | ReadStoryActs          |   7,619.96 us |    42.592 us |    37.757 us |    546.8750 |   250.0000 |   78.1250 |  4212.53 KB |
// | ReadBitmaps            |     694.94 us |     5.778 us |     5.405 us |     55.6641 |    26.3672 |         - |   460.91 KB |
// | ReadNewPowerReplayData |     205.18 us |     1.429 us |     1.337 us |     27.5879 |     0.7324 |         - |   226.63 KB |

[MemoryDiagnoser]
public class LoadRom
{
    private Context _context;
    private Rayman3Loader _loader;

    // NOTE: Set your ROM file path here
    public static string RomFilePath => "";

    [GlobalSetup]
    public void GlobalSetup()
    {
        // Register encoding provider to be able to use Windows 1252
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        _context = new(String.Empty);
        using (_context)
        {
            // Add settings
            GbaEngineSettings settings = new()
            {
                Game = Game.Rayman3,
                Platform = Platform.GBA,
            };
            _context.AddSettings(settings);

            // Load files and read the ROM header
            _loader = new Rayman3Loader(_context);
            _loader.LoadRom(RomFilePath, cache: true);
            _loader.DefinePointers();
            _loader.DefineResources();
            _loader.LoadResourceTable();
        }
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _context.Dispose();
    }

    [Benchmark]
    public void ReadFirstScene()
    {
        _loader.ReadScene(0);
        _context.Cache.Clear();
    }

    [Benchmark]
    public void ReadAllScenes()
    {
        for (int i = 0; i < 65; i++)
            _loader.ReadScene(i);
        _context.Cache.Clear();
    }

    [Benchmark]
    public void ReadSoundBank()
    {
        _loader.ReadSoundBank();
        _context.Cache.Clear();
    }

    [Benchmark]
    public void ReadFont()
    {
        _loader.ReadFont8();
        _loader.ReadFont16();
        _loader.ReadFont32();
        _context.Cache.Clear();
    }

    [Benchmark]
    public void ReadTextBanks()
    {
        _loader.ReadTextBanks();
        _context.Cache.Clear();
    }

    [Benchmark]
    public void ReadLevelInfo()
    {
        _loader.ReadLevelInfo();
        _context.Cache.Clear();
    }

    [Benchmark]
    public void ReadStoryActs()
    {
        for (int i = 0; i < 6; i++)
            _loader.ReadStoryAct(1 + i);
        _context.Cache.Clear();
    }

    [Benchmark]
    public void ReadBitmaps()
    {
        _loader.ReadGameOverBitmap();
        _loader.ReadGameOverPalette();
        _loader.ReadGameCubeMenuBitmap();
        _loader.ReadGameCubeMenuPalette();
        _context.Cache.Clear();
    }

    [Benchmark]
    public void ReadNewPowerReplayData()
    {
        for (int i = 0; i < 6; i++)
            _loader.ReadNewPowerReplayData(1 + i);
        _context.Cache.Clear();
    }
}