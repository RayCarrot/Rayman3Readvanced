using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BinarySerializer;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

BenchmarkRunner.Run<LoadRom>();

// | Method                               | Mean            | Error         | StdDev        | Gen0        | Gen1        | Gen2       | Allocated     |
// |------------------------------------- |----------------:|--------------:|--------------:|------------:|------------:|-----------:|--------------:|
// | ReadFirstScene                       |    49,895.46 us |    486.787 us |    431.524 us |   5700.0000 |   2300.0000 |   900.0000 |   44103.97 KB |
// | ReadFirstScene_HighPerformance       |    12,583.37 us |    194.951 us |    182.358 us |   1828.1250 |    812.5000 |   265.6250 |   13510.47 KB |
// | ReadAllScenes                        | 2,424,941.17 us | 13,130.517 us | 11,639.861 us | 329000.0000 | 139000.0000 | 49000.0000 | 2538589.55 KB |
// | ReadAllScenes_HighPerformance        |   691,361.42 us | 13,772.627 us | 16,395.334 us | 131000.0000 |  55000.0000 | 18000.0000 |  972235.45 KB |
// | ReadAllScenes_Cached                 |   433,602.06 us |  8,520.259 us | 11,374.301 us | 105000.0000 |  31000.0000 |  2000.0000 |  891443.05 KB |
// | ReadAllScenes_Cached_HighPerformance |   379,691.14 us |  7,242.914 us |  8,340.950 us |  99000.0000 |  29000.0000 |  3000.0000 |  827843.16 KB |
// | ReadSoundBank                        |       322.96 us |      2.062 us |      1.828 us |    112.3047 |     49.8047 |          - |     918.36 KB |
// | ReadFont                             |        13.87 us |      0.138 us |      0.122 us |      2.4414 |           - |          - |      19.97 KB |
// | ReadTextBanks                        |     3,556.04 us |     40.489 us |     33.811 us |    906.2500 |    800.7813 |          - |    7422.67 KB |
// | ReadLevelInfo                        |        37.83 us |      0.753 us |      2.048 us |      7.3242 |      0.4272 |          - |      60.13 KB |
// | ReadStoryActs                        |     7,722.41 us |    133.813 us |    173.995 us |    546.8750 |    234.3750 |    78.1250 |    4212.66 KB |
// | ReadBitmaps                          |       686.91 us |      3.889 us |      3.447 us |     55.6641 |     26.3672 |          - |     460.91 KB |
// | ReadNewPowerReplayData               |       210.00 us |      2.270 us |      2.124 us |     27.5879 |      0.7324 |          - |     226.63 KB |

[MemoryDiagnoser]
public class LoadRom
{
    private Context _context;
    private Rayman3Loader _loader;

    // NOTE: Set your ROM file path here
    public static string RomFilePath => "";

    private void Read(Action<Rayman3Loader> action)
    {
        action(_loader);
        _context.Cache.Clear();
    }

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
        Read(static loader => loader.ReadScene(0));
    }

    [Benchmark]
    public void ReadAllScenes()
    {
        Read(static loader =>
        {
            for (int i = 0; i < 65; i++)
            {
                loader.ReadScene(i);
                loader.Context.Cache.Clear();
            }
        });
    }

    [Benchmark]
    public void ReadAllScenes_Cached()
    {
        Read(static loader =>
        {
            for (int i = 0; i < 65; i++)
                loader.ReadScene(i);
        });
    }

    [Benchmark]
    public void ReadSoundBank()
    {
        Read(static loader => loader.ReadSoundBank());
    }

    [Benchmark]
    public void ReadFont()
    {
        Read(static loader =>
        {
            loader.ReadFont8();
            loader.ReadFont16();
            loader.ReadFont32();
        });
    }

    [Benchmark]
    public void ReadTextBanks()
    {
        Read(static loader => loader.ReadTextBanks());
    }

    [Benchmark]
    public void ReadLevelInfo()
    {
        Read(static loader => loader.ReadLevelInfo());
    }

    [Benchmark]
    public void ReadStoryActs()
    {
        Read(static loader =>
        {
            for (int i = 0; i < 6; i++)
                loader.ReadStoryAct(1 + i);
        });
    }

    [Benchmark]
    public void ReadBitmaps()
    {
        Read(static loader =>
        {
            loader.ReadGameOverBitmap();
            loader.ReadGameOverPalette();
            loader.ReadGameCubeMenuBitmap();
            loader.ReadGameCubeMenuPalette();
        });
    }

    [Benchmark]
    public void ReadNewPowerReplayData()
    {
        Read(static loader =>
        {
            for (int i = 0; i < 6; i++)
                loader.ReadNewPowerReplayData(1 + i);
        });
    }
}