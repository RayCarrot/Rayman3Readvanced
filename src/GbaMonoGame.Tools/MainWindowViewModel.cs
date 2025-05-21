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
        const int minLength = 4;
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
        GbaEngineSettings settings = context.GetRequiredSettings<GbaEngineSettings>();

        RawResource resource = offsetTable.ReadResource<RawResource>(context, id);
        int dependenciesCount = resource.OffsetTable.Count;

        // Scene2D
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

        // SoundBank
        if (resource.OffsetTable.Count == 0 && resource.RawData.Length >= 12)
        {
            ushort eventsCount = BitConverter.ToUInt16(resource.RawData, 0);
            uint eventsPointer = BitConverter.ToUInt32(resource.RawData, 4);
            uint resourcesPointer = BitConverter.ToUInt32(resource.RawData, 8);

            if (eventsPointer == 16 && resourcesPointer == eventsPointer + eventsCount * 4)
            {
                return ResourceType.SoundBank;
            }
        }

        // Palette16
        if (resource.OffsetTable.Count == 0 && resource.RawData.Length == 32)
        {
            return ResourceType.Palette16;
        }

        // AnimatedObject
        if (resource.OffsetTable.Count > 2 && resource.RawData.Length >= 4)
        {
            byte idxSpriteTable;
            byte idxPalette;
            if (settings.Game is 
                Game.Rayman3_20020118_DemoRLE or
                Game.Rayman3_20020301_PreAlpha or
                Game.Rayman3_20020418_NintendoE3Approval or
                Game.Rayman3_20020513_E3GameCube)
            {
                idxSpriteTable = resource.RawData[0];
                idxPalette = resource.RawData[1];
            }
            else
            {
                idxSpriteTable = resource.RawData[2];
                idxPalette = resource.RawData[3];
            }

            if (idxSpriteTable == resource.OffsetTable.Count - 2 && idxPalette == resource.OffsetTable.Count - 1)
            {
                return ResourceType.AnimatedObject;
            }
        }

        // ActorModel
        if (resource.OffsetTable.Count > 0 && resource.RawData.Length >= 12 && settings.Game == Game.Rayman3_20020118_DemoRLE)
        {
            byte idxAnimatedObject = resource.RawData[8];

            if (idxAnimatedObject == resource.OffsetTable.Count - 1)
                return ResourceType.ActorModel;
        }

        // Playfield
        if (resource.OffsetTable.Count > 2 && (resource.RawData.Length == 16 || (settings.Game is Game.Rayman3_20020118_DemoRLE or Game.Rayman3_20020301_PreAlpha && resource.RawData.Length == 15)))
        {
            byte type = resource.RawData[0];
            byte idxTileKit = resource.RawData[1];
            byte idxTileMappingTable = resource.RawData[2];
            byte layersCount = resource.RawData[5];

            if (type is 0 or 1 && idxTileKit == resource.OffsetTable.Count - 1 && idxTileMappingTable == resource.OffsetTable.Count - 2)
            {
                if (layersCount is >= 1 and <= 5)
                {
                    return ResourceType.Playfield;
                }
            }
        }

        // AnimActor
        if (resource.OffsetTable.Count > 1 && resource.RawData.Length >= 6)
        {
            byte idxGeometryTable = resource.RawData[0];
            byte animationsCount = resource.RawData[5];

            if (idxGeometryTable == 0 && animationsCount == resource.OffsetTable.Count - 1)
            {
                return ResourceType.AnimActor;
            }
        }

        // PaletteTable
        if (resource.OffsetTable.Count == 0 && resource.RawData.Length == 520)
        {
            return ResourceType.PaletteTable;
        }

        // TextureTable
        if (resource.OffsetTable.Count == 0 && resource.RawData.Length >= 4)
        {
            ushort texturesCount = BitConverter.ToUInt16(resource.RawData, 0);

            int alignedTexturesCount = texturesCount;
            if (texturesCount % 2 != 0)
                alignedTexturesCount++;

            if (resource.RawData.Length >= 4 + alignedTexturesCount * 2)
            {
                bool isValid = true;
                short prevOffset = (short)(4 + alignedTexturesCount * 2 - 4100);
                for (int i = 0; i < texturesCount; i++)
                {
                    short offset = BitConverter.ToInt16(resource.RawData, 4 + i * 2);
                    if (offset != prevOffset + 4100)
                    {
                        isValid = false;
                        break;
                    }
                    prevOffset = offset;
                }

                if (isValid)
                {
                    return ResourceType.TextureTable;
                }
            }
        }

        return ResourceType.Unknown;
    }

    private async Task ExportFromRomsAsync(Action<Rom> romAction)
    {
        OpenFileDialog fileDialog = new()
        {
            Multiselect = true,
            Filter = "GBA files|*.gba;*.bin|N-Gage files|*.dat"
        };

        if (fileDialog.ShowDialog() != true)
            return;

        Log($"Exporting from {fileDialog.FileNames.Length} roms");

        foreach (string romFilePath in fileDialog.FileNames)
        {
            bool isNGage = romFilePath.EndsWith(".dat", StringComparison.InvariantCultureIgnoreCase);

            byte[] romData = await File.ReadAllBytesAsync(romFilePath);

            long offset = isNGage ? 0 : FindOffsetTable(romData);

            string dir = Path.GetDirectoryName(romFilePath)!;
            string fileName = Path.GetFileName(romFilePath);

            if (offset != -1)
            {
                using Context context = new(dir);

                // Create and add the game settings
                Game game = isNGage ? Game.Rayman3 : romData.Length switch
                {
                    1627308 => Game.Rayman3_20020118_DemoRLE,
                    2227428 => Game.Rayman3_20020301_PreAlpha,
                    2273420 => Game.Rayman3_20020301_PreAlpha,
                    1879888 => Game.Rayman3_20020301_PreAlpha,
                    2768568 => Game.Rayman3_20020418_NintendoE3Approval,
                    3674712 => Game.Rayman3_20020513_E3GameCube,
                    3589480 => Game.Rayman3_20020513_E3GameCube,
                    _ => Game.Rayman3,
                };
                GbaEngineSettings settings = new() { Game = game, Platform = isNGage ? Platform.NGage : Platform.GBA };
                context.AddSettings(settings);

                VirtualFile file = context.AddFile<VirtualFile>(isNGage 
                    ? new StreamFile(context, fileName, new MemoryStream(romData)) 
                    : new MemoryMappedStreamFile(context, fileName, Constants.Address_ROM, new MemoryStream(romData)));

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
    private async Task ExportRootResourcesAsync()
    {
        await ExportFromRomsAsync(rom =>
        {
            for (int i = 0; i < rom.OffsetTable.Count; i++)
            {
                RawResource resource = rom.OffsetTable.ReadResource<RawResource>(rom.Context, i);
                File.WriteAllBytes(GetExportFilePath(Path.Combine("RootResources", rom.FileName), $"{i}.dat"), resource.RawData);
            }
        });
    }

    [RelayCommand]
    private async Task ExportResourceTypesAsync()
    {
        HashSet<string> texts = new();

        await ExportFromRomsAsync(rom =>
        {
            StringBuilder sb = new();
            
            for (int i = 0; i < rom.OffsetTable.Count; i++)
            {
                ResourceType resourceType = GetResourceType(rom.Context, rom.OffsetTable, i);
                RawResource resource = rom.OffsetTable.ReadResource<RawResource>(rom.Context, i);
                sb.AppendLine($"{i}: {resourceType} of size {resource.RawData.Length} with {resource.OffsetTable.Count} dependencies");
            }

            string text = sb.ToString();

            if (RemoveDuplicates)
            {
                if (!texts.Add(text))
                    return;
            }

            File.WriteAllText(GetExportFilePath("ResourceTypes", $"{rom.FileName}.txt"), text);
        });
    }

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
                    foreach (Actor actor in scene.Actors.Concat(scene.AlwaysActors).Concat(scene.ProjectileActors))
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

                    foreach (Actor actor in scene.Actors.Concat(scene.AlwaysActors).Concat(scene.ProjectileActors))
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
    private async Task ExportSerializedDataAsync()
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

                    foreach (Actor actor in scene.Actors.Concat(scene.AlwaysActors).Concat(scene.ProjectileActors))
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
            int separatePlayfieldIndex = 0;
            for (int i = 0; i < rom.OffsetTable.Count; i++)
            {
                ResourceType resourceType = GetResourceType(rom.Context, rom.OffsetTable, i);

                int mapId = i;
                Playfield? playfield = null;
                if (resourceType == ResourceType.Scene2D)
                {
                    Scene2D scene = rom.OffsetTable.ReadResource<Scene2D>(rom.Context, i);
                    playfield = scene.Playfield;
                }
                else if (resourceType == ResourceType.Playfield)
                {
                    playfield = rom.OffsetTable.ReadResource<Playfield>(rom.Context, i);
                    mapId = 100 + separatePlayfieldIndex;
                    separatePlayfieldIndex++;
                }

                if (playfield != null)
                {
                    if (playfield.Type == PlayfieldType.Playfield2D)
                    {
                        Playfield2D playfield2D = playfield.Playfield2D;

                        GfxTileKitManager tileKitManager = new();
                        tileKitManager.LoadTileKit(playfield2D.TileKit, playfield2D.TileMappingTable, 0x180, false, playfield2D.DefaultPalette);

                        for (int layerId = 0; layerId < playfield2D.Layers.Length; layerId++)
                        {
                            try
                            {
                                GameLayer layer = playfield2D.Layers[layerId];

                                if (layer.Type == GameLayerType.TileLayer)
                                {
                                    TileLayer tileLayer = layer.TileLayer;

                                    if (tileLayer.IsDynamic)
                                    {
                                        byte[] tileSet = tileLayer.Is8Bit ? playfield2D.TileKit.Tiles8bpp : playfield2D.TileKit.Tiles4bpp;

                                        Color[] texture = CreateTiledTexture(
                                            width: layer.Width,
                                            height: layer.Height,
                                            tileSet: tileSet,
                                            tileMap: tileLayer.TileMap,
                                            baseTileIndex: -1,
                                            palette: tileKitManager.SelectedPalette,
                                            is8Bit: tileLayer.Is8Bit,
                                            ignoreZero: true);

                                        exportTexture(mapId, layerId, layer.Width * Tile.Size, layer.Height * Tile.Size, texture);
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

                                        exportTexture(mapId, layerId, layer.Width * Tile.Size, layer.Height * Tile.Size, texture);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Log($"Error exporting layer {layerId} for map {mapId} in {rom.FileName} ({ex.Message})");
                            }
                        }
                    }
                    else if (playfield.Type == PlayfieldType.PlayfieldMode7)
                    {
                        PlayfieldMode7 playfieldMode7 = playfield.PlayfieldMode7;

                        GfxTileKitManager tileKitManager = new();
                        tileKitManager.LoadTileKit(playfieldMode7.TileKit, playfieldMode7.TileMappingTable, 0x100, true, playfieldMode7.DefaultPalette);

                        for (int layerId = 0; layerId < playfieldMode7.Layers.Length; layerId++)
                        {
                            try
                            {
                                GameLayer layer = playfieldMode7.Layers[layerId];

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

                                    exportTexture(mapId, layerId, layer.Width * Tile.Size, layer.Height * Tile.Size, texture);
                                }
                                else if (layer.Type == GameLayerType.TextLayerMode7)
                                {
                                    TextLayerMode7 textLayer = layer.TextLayerMode7;

                                    Color[] texture = CreateTiledTexture(
                                        width: layer.Width,
                                        height: layer.Height,
                                        tileSet: tileKitManager.TileSet,
                                        tileMap: TgxTextLayerMode7.CreateTileMap(playfieldMode7, layer),
                                        baseTileIndex: 0,
                                        palette: tileKitManager.SelectedPalette,
                                        is8Bit: textLayer.Is8Bit,
                                        ignoreZero: true);

                                    exportTexture(mapId, layerId, layer.Width * Tile.Size, layer.Height * Tile.Size, texture);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log($"Error exporting layer {layerId} for map {mapId} in {rom.FileName} ({ex.Message})");
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
    private async Task ExportAnimActorTexturesAsync()
    {
        await ExportFromRomsAsync(rom =>
        {
            TextureTable? textureTable = null;
            PaletteTable? paletteTable = null;
            for (int i = 0; i < rom.OffsetTable.Count; i++)
            {
                ResourceType resourceType = GetResourceType(rom.Context, rom.OffsetTable, i);

                if (resourceType == ResourceType.TextureTable)
                    textureTable = rom.OffsetTable.ReadResource<TextureTable>(rom.Context, i);
                else if (resourceType == ResourceType.PaletteTable)
                    paletteTable = rom.OffsetTable.ReadResource<PaletteTable>(rom.Context, i);
            }

            if (textureTable != null && paletteTable != null)
            {
                for (int textureIndex = 0; textureIndex < textureTable.TexturesCount; textureIndex++)
                {
                    Texture texture = textureTable.Textures[textureIndex]!;
                    Palette pal = new(paletteTable.Palettes[0]);

                    byte[] rawImgData = new byte[texture.ImgData.Length * 4];
                    for (int i = 0; i < texture.ImgData.Length; i++)
                    {
                        Color c = pal.Colors[texture.ImgData[i]];
                        rawImgData[i * 4 + 0] = c.R;
                        rawImgData[i * 4 + 1] = c.G;
                        rawImgData[i * 4 + 2] = c.B;
                        rawImgData[i * 4 + 3] = c.A;
                    }

                    // Export
                    using MagickImage image = new(rawImgData, new MagickReadSettings()
                    {
                        Format = MagickFormat.Rgba,
                        Width = texture.Width,
                        Height = texture.Height,
                    });

                    image.Flip();

                    image.Write(GetExportFilePath(Path.Combine("AnimActorTextures", rom.FileName), $"{textureIndex}.png"));
                }
            }
        });
    }

    [RelayCommand]
    private async Task ExportSoundBanksAsync()
    {
        HashSet<string> texts = new();

        await ExportFromRomsAsync(rom =>
        {
            for (int i = 0; i < rom.OffsetTable.Count; i++)
            {
                ResourceType resourceType = GetResourceType(rom.Context, rom.OffsetTable, i);

                if (resourceType == ResourceType.SoundBank)
                {
                    SoundBank soundBank = rom.OffsetTable.ReadResource<SoundBank>(rom.Context, i);

                    StringBuilder sb = new();

                    sb.AppendLine($"{soundBank.EventsCount} events");
                    sb.AppendLine();

                    for (int evtId = 0; evtId < soundBank.Events.Length; evtId++)
                    {
                        SoundEvent? evt = soundBank.Events[evtId];

                        sb.Append($"{evtId:000} = ");
                        if (evt == null)
                        {
                            sb.Append("NULL");
                        }
                        else
                        {
                            switch (evt.Type)
                            {
                                case SoundEvent.SoundEventType.Play:
                                    sb.Append($"Play res {evt.ResourceId} as {evt.SoundType}");
                                    break;

                                case SoundEvent.SoundEventType.Stop:
                                    sb.Append($"Stop event {evt.StopEventId} with fadeout {evt.FadeOutTime}");
                                    break;

                                case SoundEvent.SoundEventType.StopAndGo:
                                    sb.Append($"Stop event {evt.StopEventId} and play {evt.NextEventId} with fadeout {evt.FadeOutTime}");
                                    break;
                            }
                        }

                        sb.AppendLine();
                    }

                    sb.AppendLine();
                    sb.AppendLine();

                    sb.AppendLine($"{soundBank.ResourcesCount} resources");
                    sb.AppendLine();

                    for (int resId = 0; resId < soundBank.Resources.Length; resId++)
                    {
                        SoundResource? res = soundBank.Resources[resId];

                        sb.Append($"{resId:000} = ");
                        if (res == null)
                        {
                            sb.Append("NULL");
                        }
                        else
                        {
                            switch (res.Type)
                            {
                                case SoundResource.ResourceType.Song:
                                    sb.Append($"Song {res.SongTableIndex}");

                                    if (res.IsMusic)
                                        sb.Append(" (music)");
                                    else
                                        sb.Append(" (sfx)");
                                    break;
                                
                                case SoundResource.ResourceType.Random:
                                    sb.Append($"Random resource [ {String.Join(" ", res.ResourceIds.Select(x => $"{x:000}"))} ]");
                                    break;
                            }
                        }

                        sb.AppendLine();
                    }

                    string text = sb.ToString();

                    if (RemoveDuplicates)
                    {
                        if (!texts.Add(text))
                            return;
                    }

                    File.WriteAllText(GetExportFilePath("SoundBanks", $"{rom.FileName}.txt"), text);

                }
            }
        });
    }

    [RelayCommand]
    private async Task ExportSpriteAnimationsAsync()
    {
        Dictionary<int, HashSet<string>> hashes = new();

        await ExportFromRomsAsync(rom =>
        {
            Pointer?[] exportedActorTypes = new Pointer?[256];

            for (int i = 0; i < rom.OffsetTable.Count; i++)
            {
                ResourceType resourceType = GetResourceType(rom.Context, rom.OffsetTable, i);

                if (resourceType == ResourceType.Scene2D)
                {
                    Scene2D scene = rom.OffsetTable.ReadResource<Scene2D>(rom.Context, i);

                    foreach (Actor actor in scene.Actors.Concat(scene.AlwaysActors).Concat(scene.ProjectileActors))
                    {
                        Pointer? exportedPointer = exportedActorTypes[actor.Type];
                        Pointer pointer = actor.Model.AnimatedObject.Offset;

                        if (exportedPointer == null)
                        {
                            exportedActorTypes[actor.Type] = pointer;
                            exportAnimatedObject(actor.Model.AnimatedObject, actor.Type, $"{actor.Type} - {(ActorType)actor.Type}");
                        }
                        else if (exportedPointer != pointer)
                        {
                            Log($"WARNING: Actor {actor.Type} has multiple animated objects in {rom.FileName}");
                        }
                    }
                }
                else if (resourceType == ResourceType.AnimatedObject)
                {
                    AnimatedObject animatedObject = rom.OffsetTable.ReadResource<AnimatedObject>(rom.Context, i);
                    exportAnimatedObject(animatedObject, 1000, $"AnimatedObject {i}");
                }
                else if (resourceType == ResourceType.ActorModel)
                {
                    ActorModel actorModel = rom.OffsetTable.ReadResource<ActorModel>(rom.Context, i);
                    exportAnimatedObject(actorModel.AnimatedObject, 1000, $"ActorModel {i}");
                }
            }

            void exportAnimatedObject(AnimatedObject animatedObject, int cacheId, string dirName)
            {
                byte[] tileSet = animatedObject.SpriteTable.Data;

                for (int animId = 0; animId < animatedObject.Animations.Length; animId++)
                {
                    Animation anim = animatedObject.Animations[animId];

                    // Calculate min/max positions
                    int minX = 0;
                    int minY = 0;
                    int maxX = 0;
                    int maxY = 0;
                    foreach (AnimationChannel channel in anim.Channels)
                    {
                        if (channel.ChannelType == AnimationChannelType.Sprite)
                        {
                            Constants.Size size = Constants.GetSpriteShape(channel.SpriteShape, channel.SpriteSize);

                            int startX = channel.XPosition;
                            int startY = channel.YPosition;

                            if (channel.ObjectMode == OBJ_ATTR_ObjectMode.AFF_DBL)
                            {
                                startX -= size.Width / 2;
                                startY -= size.Height / 2;
                            }

                            if (startX < minX) 
                                minX = startX;
                            if (startY < minY) 
                                minY = startY;

                            int endX = startX + size.Width;
                            int endY = startY + size.Height;

                            if (channel.ObjectMode == OBJ_ATTR_ObjectMode.AFF_DBL)
                            {
                                endX += size.Width;
                                endY += size.Height;
                            }

                            if (endX > maxX)
                                maxX = endX;
                            if (endY > maxY)
                                maxY = endY;
                        }
                    }

                    int animWidth = maxX - minX;
                    int animHeight = maxY - minY;

                    // Skip empty animation
                    if (animWidth == 0 && animHeight == 0)
                        continue;

                    MagickImage[] frames = new MagickImage[anim.FramesCount];

                    // Enumerate each frame
                    int channelOffset = 0;
                    for (int frameId = 0; frameId < anim.FramesCount; frameId++)
                    {
                        Color[] imgData = new Color[animWidth * animHeight];

                        // Enumerate each channel
                        for (int channelId = anim.ChannelsPerFrame[frameId] - 1; channelId >= 0; channelId--)
                        {
                            AnimationChannel channel = anim.Channels[channelOffset + channelId];

                            if (channel.ChannelType == AnimationChannelType.Sprite && channel.ObjectMode != OBJ_ATTR_ObjectMode.HIDE)
                            {
                                // Get the size
                                Constants.Size size = Constants.GetSpriteShape(channel.SpriteShape, channel.SpriteSize);

                                // Get the positions
                                int xPos = channel.XPosition - minX;
                                int yPos = channel.YPosition - minY;

                                if (channel.ObjectMode == OBJ_ATTR_ObjectMode.AFF_DBL)
                                {
                                    xPos -= size.Width / 2;
                                    yPos -= size.Height / 2;
                                }

                                int tileSetIndex = channel.TileIndex * 0x20;
                                Palette palette = new(animatedObject.Palettes.Palettes[channel.PalIndex]);

                                if (channel.ObjectMode is OBJ_ATTR_ObjectMode.AFF or OBJ_ATTR_ObjectMode.AFF_DBL)
                                {
                                    var matrix = anim.AffineMatrices.Matrices[channel.AffineMatrixIndex];
                                    int pa = matrix.Pa.Value;
                                    int pb = matrix.Pb.Value;
                                    int pc = matrix.Pc.Value;
                                    int pd = matrix.Pd.Value;

                                    bool doubleSize = channel.ObjectMode == OBJ_ATTR_ObjectMode.AFF_DBL;

                                    int width = doubleSize ? size.Width * 2 : size.Width;
                                    int height = doubleSize ? size.Height * 2 : size.Height;

                                    for (int sprY = 0; sprY < height; sprY++)
                                    {
                                        int absY = yPos + sprY;

                                        int xofs, yofs;
                                        int xfofs, yfofs;

                                        if (doubleSize)
                                        {
                                            xofs = size.Width;
                                            yofs = size.Height;

                                            xfofs = -xofs / 2;
                                            yfofs = -yofs / 2;
                                        }
                                        else
                                        {
                                            xofs = size.Width / 2;
                                            yofs = size.Height / 2;

                                            xfofs = 0;
                                            yfofs = 0;
                                        }

                                        // Left edge
                                        int origXEdge0 = 0 - xofs;
                                        int origY = sprY - yofs;

                                        // Calculate starting parameters for matrix multiplications
                                        int shiftedXOfs = xofs + xfofs << 8;
                                        int shiftedYOfs = yofs + yfofs << 8;
                                        int pBYOffset = pb * origY + shiftedXOfs;
                                        int pDYOffset = pd * origY + shiftedYOfs;

                                        int objPixelXEdge0 = pa * origXEdge0 + pBYOffset;
                                        int objPixelYEdge0 = pc * origXEdge0 + pDYOffset;

                                        for (int sprX = 0; sprX < width; sprX++)
                                        {
                                            int absX = xPos + sprX;

                                            int lerpedObjPixelX = objPixelXEdge0 >> 8;
                                            int lerpedObjPixelY = objPixelYEdge0 >> 8;

                                            if (lerpedObjPixelX >= 0 && lerpedObjPixelX < size.Width &&
                                                lerpedObjPixelY >= 0 && lerpedObjPixelY < size.Height)
                                            {
                                                int intraTileX = lerpedObjPixelX & 7;
                                                int intraTileY = lerpedObjPixelY & 7;

                                                int tileX = lerpedObjPixelX / 8;
                                                int tileY = lerpedObjPixelY / 8;

                                                tileX += tileY * (size.Width / 8);

                                                if (animatedObject.Is8Bit)
                                                {
                                                    byte colorIndex = tileSet[tileSetIndex + tileX * 0x40 + intraTileY * 8 + intraTileX];

                                                    if (colorIndex != 0)
                                                        imgData[absY * animWidth + absX] = palette.Colors[colorIndex];
                                                }
                                                else
                                                {
                                                    byte colorIndex = tileSet[tileSetIndex + tileX * 0x20 + intraTileY * 4 + intraTileX / 2];
                                                    colorIndex = (byte)((colorIndex >> ((intraTileX & 1) * 4)) & 0xF);

                                                    if (colorIndex != 0)
                                                        imgData[absY * animWidth + absX] = palette.Colors[colorIndex];
                                                }
                                            }

                                            objPixelXEdge0 += pa;
                                            objPixelYEdge0 += pc;
                                        }
                                    }
                                }
                                else
                                {
                                    bool flipX = channel.FlipX;
                                    bool flipY = channel.FlipY;

                                    int absY = !flipY ? yPos : yPos + size.Height - Tile.Size;
                                    for (int tileY = 0; tileY < size.TilesHeight; tileY++)
                                    {
                                        int absX = !flipX ? xPos : xPos + size.Width - Tile.Size;
                                        for (int tileX = 0; tileX < size.TilesWidth; tileX++)
                                        {
                                            if (animatedObject.Is8Bit)
                                            {
                                                if (flipX && flipY)
                                                {
                                                    DrawHelpers.DrawTile_8bpp_FlipXY(imgData, absX, absY, animWidth, tileSet, ref tileSetIndex, palette);
                                                }
                                                else if (flipX)
                                                {
                                                    DrawHelpers.DrawTile_8bpp_FlipX(imgData, absX, absY, animWidth, tileSet, ref tileSetIndex, palette);
                                                }
                                                else if (flipY)
                                                {
                                                    DrawHelpers.DrawTile_8bpp_FlipY(imgData, absX, absY, animWidth, tileSet, ref tileSetIndex, palette);
                                                }
                                                else
                                                {
                                                    DrawHelpers.DrawTile_8bpp(imgData, absX, absY, animWidth, tileSet, ref tileSetIndex, palette);
                                                }
                                            }
                                            else
                                            {
                                                if (flipX && flipY)
                                                {
                                                    DrawHelpers.DrawTile_4bpp_FlipXY(imgData, absX, absY, animWidth, tileSet, ref tileSetIndex, palette, 0);
                                                }
                                                else if (flipX)
                                                {
                                                    DrawHelpers.DrawTile_4bpp_FlipX(imgData, absX, absY, animWidth, tileSet, ref tileSetIndex, palette, 0);
                                                }
                                                else if (flipY)
                                                {
                                                    DrawHelpers.DrawTile_4bpp_FlipY(imgData, absX, absY, animWidth, tileSet, ref tileSetIndex, palette, 0);
                                                }
                                                else
                                                {
                                                    DrawHelpers.DrawTile_4bpp(imgData, absX, absY, animWidth, tileSet, ref tileSetIndex, palette, 0);
                                                }
                                            }

                                            if (!flipX)
                                                absX += Tile.Size;
                                            else
                                                absX -= Tile.Size;
                                        }

                                        if (!flipY)
                                            absY += Tile.Size;
                                        else
                                            absY -= Tile.Size;
                                    }
                                }
                            }
                        }

                        // Convert to RGBA
                        byte[] rawImgData = new byte[imgData.Length * 4];
                        for (int i = 0; i < imgData.Length; i++)
                        {
                            rawImgData[i * 4 + 0] = imgData[i].R;
                            rawImgData[i * 4 + 1] = imgData[i].G;
                            rawImgData[i * 4 + 2] = imgData[i].B;
                            rawImgData[i * 4 + 3] = imgData[i].A;
                        }

                        // Create image
                        frames[frameId] = new(rawImgData, new MagickReadSettings()
                        {
                            Format = MagickFormat.Rgba,
                            Width = (uint?)animWidth,
                            Height = (uint?)animHeight
                        });

                        channelOffset += anim.ChannelsPerFrame[frameId];
                    }

                    using MagickImageCollection imgCollection = new();

                    int index = 0;
                    foreach (MagickImage frame in frames)
                    {
                        imgCollection.Add(frame);
                        imgCollection[index].AnimationDelay = (uint)(anim.Speed + 1);
                        imgCollection[index].AnimationTicksPerSecond = 60;
                        imgCollection[index].GifDisposeMethod = GifDisposeMethod.Background;
                        index++;
                    }

                    string dirPath = Path.Combine("SpriteAnimations", $"{dirName}");
                    string filePath = GetExportFilePath(dirPath, $"{animId} - {rom.FileName}.gif");

                    using MemoryStream stream = new();
                    imgCollection.Write(stream, MagickFormat.Gif);

                    // Check for duplicates
                    if (RemoveDuplicates)
                    {
                        using SHA512 sha512 = SHA512.Create();
                        stream.Position = 0;
                        string hash = Convert.ToBase64String(sha512.ComputeHash(stream));

                        if (!hashes.TryGetValue(cacheId, out HashSet<string>? hashSet))
                        {
                            hashSet = new HashSet<string>();
                            hashes.Add(cacheId, hashSet);
                        }

                        if (!hashSet.Add(hash))
                            continue;
                    }

                    stream.Position = 0;
                    using FileStream fileStream = File.Create(filePath);
                    stream.CopyTo(fileStream);
                }
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

    private enum ResourceType { Unknown, Scene2D, SoundBank, Palette16, AnimatedObject, ActorModel, Playfield, AnimActor, PaletteTable, TextureTable, }
    private record Rom(string FileName, Context Context, OffsetTable OffsetTable);
    private record ActorInstance(int Type, int Action, bool ResurrectsImmediately, bool ResurrectsLater);

    #endregion
}