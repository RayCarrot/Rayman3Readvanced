using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BinarySerializer;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

BenchmarkRunner.Run<LoadRom>();

// Original:
// | Method                 | Mean            | Error         | StdDev        | Gen0        | Gen1        | Gen2       | Allocated     |
// |------------------------|----------------:|--------------:|--------------:|------------:|------------:|-----------:|--------------:|
// | ReadFirstScene         |    49,895.46 us |    486.787 us |    431.524 us |   5700.0000 |   2300.0000 |   900.0000 |   44103.97 KB |
// | ReadAllScenes          | 2,424,941.17 us | 13,130.517 us | 11,639.861 us | 329000.0000 | 139000.0000 | 49000.0000 | 2538589.55 KB |
// | ReadSoundBank          |       322.96 us |      2.062 us |      1.828 us |    112.3047 |     49.8047 |          - |     918.36 KB |
// | ReadFont               |        13.87 us |      0.138 us |      0.122 us |      2.4414 |           - |          - |      19.97 KB |
// | ReadTextBanks          |     3,556.04 us |     40.489 us |     33.811 us |    906.2500 |    800.7813 |          - |    7422.67 KB |
// | ReadLevelInfo          |        37.83 us |      0.753 us |      2.048 us |      7.3242 |      0.4272 |          - |      60.13 KB |
// | ReadStoryActs          |     7,722.41 us |    133.813 us |    173.995 us |    546.8750 |    234.3750 |    78.1250 |    4212.66 KB |
// | ReadBitmaps            |       686.91 us |      3.889 us |      3.447 us |     55.6641 |     26.3672 |          - |     460.91 KB |
// | ReadNewPowerReplayData |       210.00 us |      2.270 us |      2.124 us |     27.5879 |      0.7324 |          - |     226.63 KB |
// 
// | Method                 | Mean          | Error         | StdDev        | Median        | Gen0       | Gen1       | Gen2      | Allocated    |
// |----------------------- |--------------:|--------------:|--------------:|--------------:|-----------:|-----------:|----------:|-------------:|
// | ReadFirstScene         |   8,618.04 us |    159.675 us |    149.360 us |   8,625.85 us |  1125.0000 |   531.2500 |   46.8750 |    9216.7 KB |
// | ReadAllScenes          | 583,277.68 us | 11,065.936 us | 10,351.084 us | 577,451.00 us | 83000.0000 | 17000.0000 | 4000.0000 | 690996.84 KB |
// | ReadSoundBank          |     332.47 us |      3.597 us |      3.189 us |     331.89 us |   108.3984 |    41.9922 |         - |    886.17 KB |
// | ReadFont               |      14.61 us |      0.134 us |      0.125 us |      14.62 us |     2.4414 |          - |         - |     19.97 KB |
// | ReadTextBanks          |   3,641.31 us |     68.322 us |    176.360 us |   3,560.09 us |   906.2500 |   800.7813 |         - |   7422.67 KB |
// | ReadLevelInfo          |      33.08 us |      0.481 us |      0.426 us |      33.20 us |     7.3242 |     0.4272 |         - |     60.13 KB |
// | ReadStoryActs          |   6,411.58 us |     53.968 us |     50.481 us |   6,420.10 us |   476.5625 |   375.0000 |         - |   3914.98 KB |
// | ReadBitmaps            |     652.97 us |      5.907 us |      5.236 us |     652.90 us |    51.7578 |    14.6484 |         - |    429.91 KB |
// | ReadNewPowerReplayData |     117.85 us |      1.769 us |      1.568 us |     117.66 us |    19.4092 |     0.4883 |         - |    159.49 KB |


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