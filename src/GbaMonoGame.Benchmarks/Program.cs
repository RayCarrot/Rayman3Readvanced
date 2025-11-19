using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BinarySerializer;
using BinarySerializer.Ubisoft.GbaEngine;

BenchmarkRunner.Run<LoadRom>();

[MemoryDiagnoser]
public class LoadRom
{
    public LoadRom()
    {
        // Register encoding provider to be able to use Windows 1252
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    // NOTE: Set your ROM file path here
    public static string RomFileName => "";

    [Benchmark]
    public void LoadData()
    {
        using Context context = new(String.Empty);
        GbaEngineSettings settings = new()
        {
            Game = Game.Rayman3,
            Platform = Platform.GBA,
        };
        context.AddSettings(settings);

        GbaLoader loader = new(context);
        loader.LoadFiles(RomFileName, cache: true);
        loader.LoadRomHeader(RomFileName);

        string gameCode = loader.RomHeader.GameCode;

        context.AddPreDefinedPointers(settings.Game switch
        {
            Game.Rayman3 when gameCode is "AYZP" => DefinedPointers.Rayman3_GBA_EU, // Rayman 3 (Europe)
            Game.Rayman3 when gameCode is "AYZE" => DefinedPointers.Rayman3_GBA_US, // Rayman 3 (USA)
            Game.Rayman3 when gameCode is "BX5P" => DefinedPointers.Rayman3_GBA_10thAnniversary_EU, // Rayman 10th Anniversary (Europe)
            Game.Rayman3 when gameCode is "BX5E" => DefinedPointers.Rayman3_GBA_10thAnniversary_US, // Rayman 10th Anniversary (USA)
            Game.Rayman3 when gameCode is "BWZP" => DefinedPointers.Rayman3_GBA_WinnieThePoohPack_EU, // Winnie the Pooh's Rumbly Tumbly Adventure & Rayman 3 (Europe)
            _ => throw new Exception($"Unsupported game {settings.Game} and/or code {gameCode}")
        });

        settings.SetDefinedResources(DefinedResources.Rayman3_GBA);

        loader.LoadData(RomFileName);
    }

    [Benchmark]
    public void LoadDataAndScene()
    {
        using Context context = new(String.Empty);
        GbaEngineSettings settings = new()
        {
            Game = Game.Rayman3,
            Platform = Platform.GBA,
        };
        context.AddSettings(settings);

        GbaLoader loader = new(context);
        loader.LoadFiles(RomFileName, cache: true);
        loader.LoadRomHeader(RomFileName);

        string gameCode = loader.RomHeader.GameCode;

        context.AddPreDefinedPointers(settings.Game switch
        {
            Game.Rayman3 when gameCode is "AYZP" => DefinedPointers.Rayman3_GBA_EU, // Rayman 3 (Europe)
            Game.Rayman3 when gameCode is "AYZE" => DefinedPointers.Rayman3_GBA_US, // Rayman 3 (USA)
            Game.Rayman3 when gameCode is "BX5P" => DefinedPointers.Rayman3_GBA_10thAnniversary_EU, // Rayman 10th Anniversary (Europe)
            Game.Rayman3 when gameCode is "BX5E" => DefinedPointers.Rayman3_GBA_10thAnniversary_US, // Rayman 10th Anniversary (USA)
            Game.Rayman3 when gameCode is "BWZP" => DefinedPointers.Rayman3_GBA_WinnieThePoohPack_EU, // Winnie the Pooh's Rumbly Tumbly Adventure & Rayman 3 (Europe)
            _ => throw new Exception($"Unsupported game {settings.Game} and/or code {gameCode}")
        });

        settings.SetDefinedResources(DefinedResources.Rayman3_GBA);

        loader.LoadData(RomFileName);

        loader.GameOffsetTable.ReadResource<Scene2D>(context, 0);
    }
}