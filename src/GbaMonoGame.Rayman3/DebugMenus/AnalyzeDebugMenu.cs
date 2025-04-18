#if WINDOWSDX
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BinarySerializer;
using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;
using ImGuiNET;

namespace GbaMonoGame.Rayman3;

public class AnalyzeDebugMenu : DebugMenu
{
    public override string Name => "Analyze";

    private void Log(string message)
    {
        Logger.Info(message);
        Debug.WriteLine(message);
    }

    private long FindOffsetTable(byte[] rom)
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

    private void FindOffsetTablesInRoms()
    {
        OpenFileDialog fileDialog = new()
        {
            Multiselect = true,
            Filter = "GBA files|*.gba;*.bin"
        };

        if (fileDialog.ShowDialog() != DialogResult.OK)
            return;

        foreach (string filePath in fileDialog.FileNames)
        {
            byte[] rom = File.ReadAllBytes(filePath);

            long offset = FindOffsetTable(rom);

            if (offset != -1)
            {
                uint length = BitConverter.ToUInt32(rom, (int)(offset - Constants.Address_ROM));
                Log($"Found offset table for {Path.GetFileName(filePath)} at 0x{offset:X8} with length {length}");
            }
            else
            {
                Log($"Could not find the offset table for {Path.GetFileName(filePath)}");
            }
        }
    }

    // TODO: Expand this to analyze more data, like what all the resources are, graphics, animations etc.
    // TODO: Add support for the early prototypes too - set engine version based on filename
    private void AnalyzeRoms()
    {
        OpenFileDialog fileDialog = new()
        {
            Multiselect = true,
            Filter = "GBA files|*.gba;*.bin"
        };

        if (fileDialog.ShowDialog() != DialogResult.OK)
            return;

        HashSet<int>[] usedActorTypes = new HashSet<int>[111];
        for (int i = 0; i < usedActorTypes.Length; i++)
            usedActorTypes[i] = [];

        StringBuilder usedActorsText = new();

        for (int fileIndex = 0; fileIndex < fileDialog.FileNames.Length; fileIndex++)
        {
            string filePath = fileDialog.FileNames[fileIndex];

            byte[] rom = File.ReadAllBytes(filePath);

            long offset = FindOffsetTable(rom);

            string dir = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);

            usedActorsText.AppendLine($"{fileIndex}: {fileName}");

            if (offset != -1)
            {
                using Context context = new(dir!);

                // Create and add the game settings
                GbaEngineSettings settings = new() { Game = Game.Rayman3, Platform = Platform.GBA };
                context.AddSettings(settings);

                MemoryMappedStreamFile file = context.AddFile(new MemoryMappedStreamFile(context, fileName,
                    Constants.Address_ROM, new MemoryStream(rom)));

                OffsetTable gameOffsetTable = FileFactory.Read<OffsetTable>(context, new Pointer(offset, file));
                settings.RootTable = gameOffsetTable;

                bool isLevel = true;
                for (int i = 0; i < gameOffsetTable.Count; i++)
                {
                    RawResource resource = gameOffsetTable.ReadResource<RawResource>(context, i);
                    int dependencies = resource.OffsetTable.Count;

                    if (isLevel)
                    {
                        if (resource.RawData.Length < 10)
                        {
                            isLevel = false;
                        }
                        else
                        {
                            byte idxPlayfield = resource.RawData[0];

                            if (idxPlayfield >= dependencies)
                            {
                                isLevel = false;
                            }
                            else
                            {
                                byte gameObjectsCount = resource.RawData[4];
                                byte alwaysActorsCount = resource.RawData[5];
                                byte actorsCount = resource.RawData[6];
                                byte projectileActorsCount = resource.RawData[7];
                                byte captorsCount = resource.RawData[9];

                                if (gameObjectsCount <= 0 ||
                                    alwaysActorsCount + actorsCount + projectileActorsCount + captorsCount != gameObjectsCount)
                                {
                                    isLevel = false;
                                }
                                else
                                {
                                    Scene2DResource scene = gameOffsetTable.ReadResource<Scene2DResource>(context, i,
                                        (_, x) => x.Pre_SerializeDependencies = false);
                                    Log($"{fileName}: Scene {i} has {gameObjectsCount} objects");

                                    foreach (ActorResource actor in scene.Actors.Concat(scene.AlwaysActors).Concat(scene.ProjectileActors))
                                        usedActorTypes[actor.Type].Add(fileIndex);
                                }
                            }
                        }
                    }

                    if (!isLevel)
                    {
                        // TODO: Analyse other resource types
                    }
                }
            }
            else
            {
                Log($"{fileName}: Could not find the offset table");
            }
        }

        usedActorsText.AppendLine();

        for (int i = 0; i < usedActorTypes.Length; i++)
            usedActorsText.AppendLine($"{i}: {String.Join(", ", usedActorTypes[i])}");

        Directory.CreateDirectory("Analysis");
        File.WriteAllText("Analysis/UsedActors.txt", usedActorsText.ToString());
    }

    public override void Draw(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    {
        if (ImGui.MenuItem("Find offset tables in ROMs"))
            FindOffsetTablesInRoms();

        if (ImGui.MenuItem("Analyze ROMs"))
            AnalyzeRoms();
    }
}
#endif