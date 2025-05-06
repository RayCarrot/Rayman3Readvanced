using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using BinarySerializer;
using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GbaMonoGame.Rayman3;
using GbaMonoGame.TgxEngine;
using ImageMagick;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Game = BinarySerializer.Ubisoft.GbaEngine.Game;

namespace GbaMonoGame.Tools;

// TODO: Add exports for sprite animations
// TODO: Analyse all the game resources
public partial class MainWindowViewModel : ObservableObject
{
    #region Properties

    [ObservableProperty]
    public partial string? LogText { get; set; }

    [ObservableProperty]
    public partial bool RemoveDuplicates { get; set; }

    #endregion

    #region Helper Methods

    private void Log(string message)
    {
        LogText += message;
        LogText += Environment.NewLine;
    }

    private static void WriteJson<T>(T obj, string filePath)
    {
        string json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            Converters = { new StringEnumConverter(), new ByteArrayHexConverter() }
        });

        File.WriteAllText(filePath, json);
    }

    private static string GetExportFilePath(string dir, string fileName)
    {
        string dirPath = Path.Combine("Export", dir);
        Directory.CreateDirectory(dirPath);
        return Path.Combine(dirPath, fileName);
    }

    private static long FindOffsetTable(byte[] rom)
    {
        const int minLength = 5;
        const int maxLength = 200;
        const int minResourceSize = 4;
        const int maxResourceSize = 0x100000;

        uint[] values = new uint[rom.Length / 4];

        for (int i = 0; i < values.Length; i++)
            values[i] = BitConverter.ToUInt32(rom, i * 4);

        for (int i = 0; i < values.Length; i++)
        {
            uint length = values[i];

            if (length is < minLength or > maxLength)
                continue;

            if (i + length >= values.Length)
                continue;

            uint prevOffset = 0;
            bool isValid = true;

            for (int j = 0; j < length; j++)
            {
                uint offset = values[i + 1 + j];
                int prevLength = (int)(offset - prevOffset);

                if (prevLength is < minResourceSize or > maxResourceSize)
                {
                    isValid = false;
                    break;
                }

                long absoluteOffset = i * 4 + 4 + offset * 4;

                if (absoluteOffset >= rom.Length)
                {
                    isValid = false;
                    break;
                }

                prevOffset = offset;
            }

            if (isValid)
                return Constants.Address_ROM + i * 4;
        }

        return -1;
    }

    private static ResourceType GetResourceType(Context context, OffsetTable offsetTable, int id)
    {
        RawResource resource = offsetTable.ReadResource<RawResource>(context, id);
        int dependenciesCount = resource.OffsetTable.Count;

        // Check if it's a scene
        if (resource.RawData.Length >= 10)
        {
            byte idxPlayfield = resource.RawData[0];

            if (idxPlayfield < dependenciesCount)
            {
                byte gameObjectsCount = resource.RawData[4];
                byte alwaysActorsCount = resource.RawData[5];
                byte actorsCount = resource.RawData[6];
                byte projectileActorsCount = resource.RawData[7];
                byte captorsCount = resource.RawData[9];

                if (gameObjectsCount > 0 &&
                    alwaysActorsCount + actorsCount + projectileActorsCount + captorsCount == gameObjectsCount)
                {
                    return ResourceType.Scene2D;
                }
            }
        }

        // TODO: Check for other types

        return ResourceType.Unknown;
    }

    private async Task ExportFromRomsAsync(Action<Rom> romAction)
    {
        OpenFileDialog fileDialog = new()
        {
            Multiselect = true,
            Filter = "GBA files|*.gba;*.bin"
        };

        if (fileDialog.ShowDialog() != true)
            return;

        Log($"Exporting from {fileDialog.FileNames.Length} roms");

        foreach (string romFilePath in fileDialog.FileNames)
        {
            byte[] romData = await File.ReadAllBytesAsync(romFilePath);

            long offset = FindOffsetTable(romData);

            string dir = Path.GetDirectoryName(romFilePath)!;
            string fileName = Path.GetFileName(romFilePath);

            if (offset != -1)
            {
                using Context context = new(dir);

                // Create and add the game settings
                GbaEngineSettings settings = new() { Game = Game.Rayman3, Platform = Platform.GBA };
                context.AddSettings(settings);

                MemoryMappedStreamFile file = context.AddFile(new MemoryMappedStreamFile(context, fileName,
                    Constants.Address_ROM, new MemoryStream(romData)));

                OffsetTable gameOffsetTable = FileFactory.Read<OffsetTable>(context, new Pointer(offset, file));
                settings.RootTable = gameOffsetTable;

                await Task.Run(() => romAction(new Rom(Path.GetFileNameWithoutExtension(romFilePath), context, gameOffsetTable)));

                Log($"Finished exporting {fileName}");
            }
            else
            {
                Log($"Could not find the offset table for {fileName}");
            }

            GC.Collect();
        }

        Log("Finished exporting from all roms");
    }

    private static Color[] CreateTiledTexture(
        int width, 
        int height, 
        byte[] tileSet, 
        MapTile[] tileMap, 
        int baseTileIndex, 
        Palette palette, 
        bool is8Bit, 
        bool ignoreZero)
    {
        int pixelWidth = width * Tile.Size;
        int pixelHeight = height * Tile.Size;

        Color[] texColors = new Color[pixelWidth * pixelHeight];

        if (is8Bit)
        {
            int absTileY = 0;

            for (int tileY = 0; tileY < height; tileY++)
            {
                int absTileX = 0;

                for (int tileX = 0; tileX < width; tileX++)
                {
                    MapTile tile = tileMap[tileY * width + tileX];

                    if (!ignoreZero || tile.TileIndex != 0)
                    {
                        int tilePixelIndex = (baseTileIndex + tile.TileIndex) * 0x40;

                        if (tile.FlipX && tile.FlipY)
                            DrawHelpers.DrawTile_8bpp_FlipXY(texColors, absTileX, absTileY, pixelWidth, tileSet, ref tilePixelIndex, palette);
                        else if (tile.FlipX)
                            DrawHelpers.DrawTile_8bpp_FlipX(texColors, absTileX, absTileY, pixelWidth, tileSet, ref tilePixelIndex, palette);
                        else if (tile.FlipY)
                            DrawHelpers.DrawTile_8bpp_FlipY(texColors, absTileX, absTileY, pixelWidth, tileSet, ref tilePixelIndex, palette);
                        else
                            DrawHelpers.DrawTile_8bpp(texColors, absTileX, absTileY, pixelWidth, tileSet, ref tilePixelIndex, palette);
                    }

                    absTileX += Tile.Size;
                }

                absTileY += Tile.Size;
            }
        }
        else
        {
            int absTileY = 0;

            for (int tileY = 0; tileY < height; tileY++)
            {
                int absTileX = 0;

                for (int tileX = 0; tileX < width; tileX++)
                {
                    MapTile tile = tileMap[tileY * width + tileX];

                    if (!ignoreZero || tile.TileIndex != 0)
                    {
                        int tilePixelIndex = (baseTileIndex + tile.TileIndex) * 0x20;
                        int palOffset = tile.PaletteIndex * 16;

                        if (tile.FlipX && tile.FlipY)
                            DrawHelpers.DrawTile_4bpp_FlipXY(texColors, absTileX, absTileY, pixelWidth, tileSet, ref tilePixelIndex, palette, palOffset);
                        else if (tile.FlipX)
                            DrawHelpers.DrawTile_4bpp_FlipX(texColors, absTileX, absTileY, pixelWidth, tileSet, ref tilePixelIndex, palette, palOffset);
                        else if (tile.FlipY)
                            DrawHelpers.DrawTile_4bpp_FlipY(texColors, absTileX, absTileY, pixelWidth, tileSet, ref tilePixelIndex, palette, palOffset);
                        else
                            DrawHelpers.DrawTile_4bpp(texColors, absTileX, absTileY, pixelWidth, tileSet, ref tilePixelIndex, palette, palOffset);
                    }

                    absTileX += Tile.Size;
                }

                absTileY += Tile.Size;
            }
        }

        return texColors;
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task ExportActorsCsvAsync()
    {
        await ExportFromRomsAsync(rom =>
        {
            ActorModel?[] actorModels = new ActorModel?[256];

            for (int i = 0; i < rom.OffsetTable.Count; i++)
            {
                ResourceType resourceType = GetResourceType(rom.Context, rom.OffsetTable, i);

                if (resourceType == ResourceType.Scene2D)
                {
                    Scene2D scene = rom.OffsetTable.ReadResource<Scene2D>(rom.Context, i);

                    // NOTE: Some actors types have multiple models, but they all appear the same
                    foreach (Actor actor in scene.Actors.Concat(scene.AlwaysActors))
                    {
                        ActorModel model = actor.Model;
                        actorModels[actor.Type] = model;
                    }
                }
            }

            StringBuilder sb = new();

            void addValue(string value) => sb.Append($"{value},");

            // Header
            addValue("Type Id");
            addValue("Type Name");
            addValue("Hit Points");
            addValue("Attack Points");
            addValue("Receives damage");
            addValue("Map Collision");
            addValue("Object Collision");
            addValue("Is Solid");
            addValue("Is Against Captor");
            addValue("Actions");
            addValue("Animations");
            sb.AppendLine();

            for (int i = 0; i < actorModels.Length; i++)
            {
                ActorModel? model = actorModels[i];

                if (model == null)
                    continue;

                addValue($"{i}");
                addValue(Enum.IsDefined(typeof(ActorType), i) ? $"{(ActorType)i}" : "");
                addValue(model.HitPoints != 0 ? model.HitPoints.ToString() : "");
                addValue(model.AttackPoints != 0 ? model.AttackPoints.ToString() : "");
                addValue(model.ReceivesDamage ? "✔️" : "");
                addValue(model.CheckAgainstMapCollision ? $"{model.MapCollisionType}" : "");
                addValue(model.CheckAgainstObjectCollision ? "✔️" : "");
                addValue(model.IsSolid ? "✔️" : "");
                addValue(model.IsAgainstCaptor ? "✔️" : "");
                addValue($"{model.Actions.Length}");
                addValue($"{model.AnimatedObject.Animations.Length}");
                sb.AppendLine();
            }

            File.WriteAllText(GetExportFilePath("Actors", $"{rom.FileName}.csv"), sb.ToString());
        });
    }

    [RelayCommand]
    private async Task ExportActorInstancesCsvAsync()
    {
        await ExportFromRomsAsync(rom =>
        {
            HashSet<ActorInstance> actorInstances = [];

            for (int i = 0; i < rom.OffsetTable.Count; i++)
            {
                ResourceType resourceType = GetResourceType(rom.Context, rom.OffsetTable, i);

                if (resourceType == ResourceType.Scene2D)
                {
                    Scene2D scene = rom.OffsetTable.ReadResource<Scene2D>(rom.Context, i);

                    foreach (Actor actor in scene.Actors.Concat(scene.AlwaysActors))
                        actorInstances.Add(new ActorInstance(actor.Type, actor.FirstActionId, actor.ResurrectsImmediately, actor.ResurrectsLater));
                }
            }

            StringBuilder sb = new();

            void addValue(string value) => sb.Append($"{value},");

            // Header
            addValue("Type Id");
            addValue("Type Name");
            addValue("Action ID");
            addValue("Resurrects Immediately");
            addValue("Resurrects Later");
            sb.AppendLine();

            foreach (ActorInstance actorInstance in actorInstances.OrderBy(x => x.Type).ThenBy(x => x.Action))
            {
                addValue($"{actorInstance.Type}");
                addValue(Enum.IsDefined(typeof(ActorType), actorInstance.Type) ? $"{(ActorType)actorInstance.Type}" : "");
                addValue($"{actorInstance.Action}");
                addValue(actorInstance.ResurrectsImmediately ? "✔️" : "");
                addValue(actorInstance.ResurrectsLater ? "✔️" : "");
                sb.AppendLine();
            }

            File.WriteAllText(GetExportFilePath("ActorInstances", $"{rom.FileName}.csv"), sb.ToString());
        });
    }

    [RelayCommand]
    private async Task ExportRawDataAsync()
    {
        await ExportFromRomsAsync(rom =>
        {
            List<ActorModel>?[] actorModels = new List<ActorModel>?[256];
            List<AnimatedObject> animatedObjects = new();

            for (int i = 0; i < rom.OffsetTable.Count; i++)
            {
                ResourceType resourceType = GetResourceType(rom.Context, rom.OffsetTable, i);

                if (resourceType == ResourceType.Scene2D)
                {
                    Scene2D scene = rom.OffsetTable.ReadResource<Scene2D>(rom.Context, i);

                    foreach (Actor actor in scene.Actors.Concat(scene.AlwaysActors))
                    {
                        ActorModel model = actor.Model;

                        string animatedObjectOutputFile = exportAnimatedObject(model.AnimatedObject, $"{(ActorType)actor.Type}");

                        List<ActorModel>? models = actorModels[actor.Type];

                        if (models == null)
                        {
                            models = new List<ActorModel>();
                            actorModels[actor.Type] = models;
                        }

                        if (models.Contains(model))
                            continue;

                        models.Add(model);

                        writeJson(new
                        {
                            ViewBox = new Box(model.ViewBox),
                            DetectionBox = new Box(model.DetectionBox),
                            AnimatedObject = animatedObjectOutputFile,
                            model.MapCollisionType,
                            model.CheckAgainstMapCollision,
                            model.CheckAgainstObjectCollision,
                            model.IsSolid,
                            model.IsAgainstCaptor,
                            model.ReceivesDamage,
                            model.HitPoints,
                            model.AttackPoints,
                            Actions = model.Actions.Select(x => new
                            {
                                Box = new Box(x.Box),
                                x.AnimationIndex,
                                x.Flags,
                                x.MechModelType,
                                MechModelParams = x.MechModel?.Params.Select(p => p.AsFloat)
                            })
                        }, "Actors", $"{(ActorType)actor.Type}_{models.Count}.json");
                    }
                }
            }

            // TODO: Generate remaining data (playfields, localization, story acts, animated objects etc.). Data with properties should
            //       be exported to .json and raw data, like graphics and maps, should be .dat files (with a .json for the header).
            //       Once done we can use this to load this as the game data, allowing easier edits. We can also compare data between
            //       prototypes, or even import data from prototypes into final version (like the snail actor).

            string exportAnimatedObject(AnimatedObject animatedObject, string name)
            {
                string animatedObjectOutputDir = "Animation";
                string animatedObjectOutputFileName = $"{name}.json";

                if (animatedObjects.Contains(animatedObject))
                    return Path.Combine(animatedObjectOutputDir, animatedObjectOutputFileName);

                animatedObjects.Add(animatedObject);

                writeJson(new
                {
                    animatedObject.Is8Bit,
                    animatedObject.IsDynamic,
                    Animations = animatedObject.Animations.Select(x => new
                    {
                        x.Speed,
                        x.DoNotRepeat,
                        x.ChannelsPerFrame,
                        x.Channels
                    }),
                }, animatedObjectOutputDir, animatedObjectOutputFileName);

                return Path.Combine(animatedObjectOutputDir, animatedObjectOutputFileName);
            }

            void writeJson<T>(T obj, string dir, string fileName) =>
                WriteJson(obj, GetExportFilePath(Path.Combine("RawData", rom.FileName, dir), fileName));
        });
    }

    [RelayCommand]
    private async Task ExportUsedActorTypesAsync()
    {
        HashSet<int>[] usedActorTypes = new HashSet<int>[111];
        for (int i = 0; i < usedActorTypes.Length; i++)
            usedActorTypes[i] = [];

        StringBuilder usedActorsText = new();

        int fileIndex = 0;
        await ExportFromRomsAsync(rom =>
        {
            for (int i = 0; i < rom.OffsetTable.Count; i++)
            {
                ResourceType resourceType = GetResourceType(rom.Context, rom.OffsetTable, i);

                if (resourceType == ResourceType.Scene2D)
                {
                    Scene2D scene = rom.OffsetTable.ReadResource<Scene2D>(rom.Context, i);

                    foreach (Actor actor in scene.Actors.Concat(scene.AlwaysActors).Concat(scene.ProjectileActors))
                        usedActorTypes[actor.Type].Add(fileIndex);
                }
            }

            fileIndex++;
        });

        usedActorsText.AppendLine();

        for (int i = 0; i < usedActorTypes.Length; i++)
            usedActorsText.AppendLine($"{i}: {String.Join(", ", usedActorTypes[i])}");

        await File.WriteAllTextAsync(GetExportFilePath("UsedActorType", "UsedActorType.txt"), usedActorsText.ToString());
    }

    [RelayCommand]
    private async Task ExportTileLayerImagesAsync()
    {
        Dictionary<string, HashSet<string>> hashes = new();

        await ExportFromRomsAsync(rom =>
        {
            for (int i = 0; i < rom.OffsetTable.Count; i++)
            {
                ResourceType resourceType = GetResourceType(rom.Context, rom.OffsetTable, i);

                if (resourceType == ResourceType.Scene2D)
                {
                    Scene2D scene = rom.OffsetTable.ReadResource<Scene2D>(rom.Context, i);

                    if (scene.Playfield.Type == PlayfieldType.Playfield2D)
                    {
                        Playfield2D playfield = scene.Playfield.Playfield2D;

                        GfxTileKitManager tileKitManager = new();
                        tileKitManager.LoadTileKit(playfield.TileKit, playfield.TileMappingTable, 0x180, false, playfield.DefaultPalette);

                        for (int layerId = 0; layerId < playfield.Layers.Length; layerId++)
                        {
                            try
                            {
                                GameLayer layer = playfield.Layers[layerId];

                                if (layer.Type == GameLayerType.TileLayer)
                                {
                                    TileLayer tileLayer = layer.TileLayer;

                                    if (tileLayer.IsDynamic)
                                    {
                                        byte[] tileSet = tileLayer.Is8Bit ? playfield.TileKit.Tiles8bpp : playfield.TileKit.Tiles4bpp;

                                        Color[] texture = CreateTiledTexture(
                                            width: layer.Width,
                                            height: layer.Height,
                                            tileSet: tileSet,
                                            tileMap: tileLayer.TileMap,
                                            baseTileIndex: -1,
                                            palette: tileKitManager.SelectedPalette,
                                            is8Bit: tileLayer.Is8Bit,
                                            ignoreZero: true);

                                        exportTexture(i, layerId, layer.Width * Tile.Size, layer.Height * Tile.Size, texture);
                                    }
                                    else
                                    {
                                        Color[] texture = CreateTiledTexture(
                                            width: layer.Width,
                                            height: layer.Height,
                                            tileSet: tileKitManager.TileSet,
                                            tileMap: tileLayer.TileMap,
                                            baseTileIndex: 0,
                                            palette: tileKitManager.SelectedPalette,
                                            is8Bit: tileLayer.Is8Bit,
                                            ignoreZero: true);

                                        exportTexture(i, layerId, layer.Width * Tile.Size, layer.Height * Tile.Size, texture);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Log($"Error exporting layer {layerId} for map {i} in {rom.FileName} ({ex.Message})");
                            }
                        }
                    }
                    else if (scene.Playfield.Type == PlayfieldType.PlayfieldMode7)
                    {
                        PlayfieldMode7 playfield = scene.Playfield.PlayfieldMode7;

                        GfxTileKitManager tileKitManager = new();
                        tileKitManager.LoadTileKit(playfield.TileKit, playfield.TileMappingTable, 0x100, true, playfield.DefaultPalette);

                        for (int layerId = 0; layerId < playfield.Layers.Length; layerId++)
                        {
                            try
                            {
                                GameLayer layer = playfield.Layers[layerId];

                                if (layer.Type == GameLayerType.RotscaleLayerMode7)
                                {
                                    RotscaleLayerMode7 rotscaleLayer = layer.RotscaleLayerMode7;

                                    Color[] texture = CreateTiledTexture(
                                        width: layer.Width,
                                        height: layer.Height,
                                        tileSet: tileKitManager.TileSet,
                                        tileMap: rotscaleLayer.TileMap,
                                        baseTileIndex: 512,
                                        palette: tileKitManager.SelectedPalette,
                                        is8Bit: true,
                                        ignoreZero: true);

                                    exportTexture(i, layerId, layer.Width * Tile.Size, layer.Height * Tile.Size, texture);
                                }
                                else if (layer.Type == GameLayerType.TextLayerMode7)
                                {
                                    TextLayerMode7 textLayer = layer.TextLayerMode7;

                                    Color[] texture = CreateTiledTexture(
                                        width: layer.Width,
                                        height: layer.Height,
                                        tileSet: tileKitManager.TileSet,
                                        tileMap: TgxTextLayerMode7.CreateTileMap(playfield, layer),
                                        baseTileIndex: 0,
                                        palette: tileKitManager.SelectedPalette,
                                        is8Bit: textLayer.Is8Bit,
                                        ignoreZero: true);

                                    exportTexture(i, layerId, layer.Width * Tile.Size, layer.Height * Tile.Size, texture);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log($"Error exporting layer {layerId} for map {i} in {rom.FileName} ({ex.Message})");
                            }
                        }
                    }
                }
            }

            void exportTexture(int mapId, int layerId, int width, int height, Color[] pixels)
            {
                byte[] rawImgData = new byte[pixels.Length * 4];
                for (int i = 0; i < pixels.Length; i++)
                {
                    rawImgData[i * 4 + 0] = pixels[i].R;
                    rawImgData[i * 4 + 1] = pixels[i].G;
                    rawImgData[i * 4 + 2] = pixels[i].B;
                    rawImgData[i * 4 + 3] = pixels[i].A;
                }

                string dirPath = Path.Combine("Layers", $"{mapId}");

                // Check for duplicates
                if (RemoveDuplicates)
                {
                    using SHA512 sha512 = SHA512.Create();
                    string hash = Convert.ToBase64String(sha512.ComputeHash(rawImgData));

                    if (!hashes.TryGetValue(dirPath, out HashSet<string>? hashSet))
                    {
                        hashSet = new HashSet<string>();
                        hashes.Add(dirPath, hashSet);
                    }

                    if (!hashSet.Add(hash))
                        return;
                }

                // Export
                using MagickImage image = new(rawImgData, new MagickReadSettings()
                {
                    Format = MagickFormat.Rgba,
                    Width = (uint?)width,
                    Height = (uint?)height
                });
                image.Write(GetExportFilePath(dirPath, $"{layerId} - {rom.FileName}.png"));
            }
        });
    }

    [RelayCommand]
    private void OpenExportFolder()
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = GetExportFilePath(String.Empty, String.Empty),
            UseShellExecute = true,
            Verb = "open"
        });
    }

    #endregion

    #region Data Types

    private enum ResourceType { Unknown, Scene2D, }
    private record Rom(string FileName, Context Context, OffsetTable OffsetTable);
    private record ActorInstance(int Type, int Action, bool ResurrectsImmediately, bool ResurrectsLater);

    #endregion
}