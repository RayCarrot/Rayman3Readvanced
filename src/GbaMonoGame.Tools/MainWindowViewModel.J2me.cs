using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using BinarySerializer;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GbaMonoGame.Rayman3.J2me;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GbaMonoGame.Tools;

public partial class MainWindowViewModel : ObservableObject
{
    #region Helper Methods

    private static int ReadInt(byte[] data, int offset)
    {
        return (data[offset++] << 24) | (data[offset++] << 16) | (data[offset++] << 8) | data[offset++];
    }

    private static bool IsTextBank(byte[] data)
    {
        int offset = 0;
        while (true)
        {
            byte strLength = data[offset++];
            offset += strLength;

            if (offset >= data.Length)
                break;

            for (int i = 0; i < strLength; i++)
            {
                if (data[offset - strLength + i] == 0x00)
                    return false;
            }
        }

        return offset == data.Length;
    }

    private static string ConvertTextBank(byte[] data)
    {
        StringBuilder sb = new();
        int offset = 0;
        while (offset < data.Length)
        {
            byte strLength = data[offset++];
            sb.AppendLine(Encoding.UTF8.GetString(data, offset, strLength));
            offset += strLength;
        }

        return sb.ToString();
    }

    private static SortedDictionary<ACTOR_TYPE, J2meActorType> ConvertActorTypes(byte[] data)
    {
        SortedDictionary<ACTOR_TYPE, J2meActorType> actorTypes = new();
        int offset = 0;
        for (int i = 0; i < data.Length / 11; i++)
        {
            int kImage_ResourceID = ReadInt(data, offset + 0);
            if (kImage_ResourceID != -1)
            {
                int kImage_Index = (sbyte)data[offset + 4];
                int kData_ResourceID = ReadInt(data, offset + 5);
                int kData_Index = (sbyte)data[offset + 9];
                bool bCreateDataImage = (sbyte)data[offset + 10] == 1;
                actorTypes.Add((ACTOR_TYPE)i, new J2meActorType(new J2meResourceId(kImage_ResourceID), kImage_Index, new J2meResourceId(kData_ResourceID), kData_Index, bCreateDataImage));
            }
            else
            {
                actorTypes.Add((ACTOR_TYPE)i, null);
            }
            offset += 11;
        }
        return actorTypes;
    }

    private static byte[] SerializeToJson<T>(T obj)
    {
        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings()
        {
            Converters = 
            [
                new StringEnumConverter()
            ]
        }));
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task ExportJ2meAsync()
    {
        OpenFileDialog fileDialog = new()
        {
            Multiselect = true,
            Filter = "Java files|*.jar"
        };

        if (fileDialog.ShowDialog() != true)
            return;

        Log($"Exporting from {fileDialog.FileNames.Length} roms");

        HashSet<string> hashes = new();

        foreach (string romFilePath in fileDialog.FileNames)
        {
            string romFileName = Path.GetFileName(romFilePath);

            await Task.Run(() =>
            {
                // Open the Java archive
                using JavaArchive jar = new(romFilePath, true);

                // Find the resource archives
                List<string> archiveFiles = [];
                foreach (ZipArchiveEntry entry in jar.GetEntries())
                {
                    // Skip directories
                    if (String.IsNullOrEmpty(entry.Name))
                        continue;

                    if (entry.Name.Length is 2 or 3 || entry.Name.StartsWith("Resource_Archive_"))
                        archiveFiles.Add(entry.FullName);
                }

                Log($"Found {archiveFiles.Count} resource archives ({String.Join(", ", archiveFiles)})");

                foreach (string archiveFile in archiveFiles)
                {
                    ZipArchiveEntry entry = jar.GetFile(archiveFile);
                    using MemoryStream fileMemoryStream = new();
                    using (Stream fileStream = entry.Open())
                        fileStream.CopyTo(fileMemoryStream);
                    fileMemoryStream.Position = 0;

                    List<int> dataSizes = [];
                    List<int> dataSizeCompressionDeltas = [];
                    int totalSize = 0;
                    int dataCount = 0;
                    bool isValid = false;
                    using (Reader reader = new(fileMemoryStream, isLittleEndian: false))
                    {
                        while (true)
                        {
                            if (fileMemoryStream.Position >= entry.Length - 4)
                                break;

                            int dataSize = reader.ReadUInt16();
                            dataSizes.Add(dataSize);
                            int dataSizeCompressionDelta = reader.ReadUInt16();
                            dataSizeCompressionDeltas.Add(dataSizeCompressionDelta);

                            totalSize += dataSize;
                            dataCount++;

                            long expectedTotalSize = fileMemoryStream.Length - fileMemoryStream.Position;
                            if (totalSize == expectedTotalSize)
                            {
                                isValid = true;
                                break;
                            }
                            if (totalSize > expectedTotalSize)
                                break;
                        }

                        if (isValid)
                        {
                            reader.BaseStream.Position = dataCount * 4;
                            for (int i = 0; i < dataCount; i++)
                            {
                                byte[] data = reader.ReadBytes(dataSizes[i]);

                                // Check for duplicates
                                if (RemoveDuplicates)
                                {
                                    string hash = Convert.ToBase64String(SHA512.HashData(data));
                                    if (!hashes.Add(hash))
                                        continue;
                                }

                                string rawExportFilePath = GetExportFilePath(Path.Combine("J2ME", "Raw", romFileName, archiveFile), $"{i}.dat");
                                File.WriteAllBytes(rawExportFilePath, data);
                                
                                // Check for PNG header
                                if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
                                {
                                    exportConvertedData(data, $"{i}_Image.png");
                                }
                                // Check for MIDI header
                                else if (data[0] == 0x4D && data[1] == 0x54 && data[2] == 0x68 && data[3] == 0x64)
                                {
                                    exportConvertedData(data, $"{i}_Sound.mid");
                                }
                                // Check for WAV header
                                else if (data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46)
                                {
                                    exportConvertedData(data, $"{i}_Sound.wav");
                                }
                                // Check if compressed, in which case we assume it's an image
                                else if (dataSizeCompressionDeltas[i] != 0)
                                {
                                    exportConvertedData(ResourceManager.DecompressImage(data, dataSizes[i], dataSizeCompressionDeltas[i]), $"{i}_Image.png");
                                }
                                // Check if text bank
                                else if (IsTextBank(data))
                                {
                                    exportConvertedData(Encoding.UTF8.GetBytes(ConvertTextBank(data)), $"{i}_Text.txt");
                                }
                                // Check if actor types
                                else if (i == 0 && dataSizes[i] == 308)
                                {
                                    exportConvertedData(SerializeToJson(ConvertActorTypes(data)), $"{i}_ActorTypes.json");
                                }
                                else
                                {
                                    exportConvertedData(data, $"{i}.dat");
                                }

                                void exportConvertedData(byte[] convertedData, string fileName)
                                {
                                    string exportFilePath = GetExportFilePath(Path.Combine("J2ME", "Converted", romFileName, archiveFile), fileName);
                                    File.WriteAllBytes(exportFilePath, convertedData);
                                }
                            }

                            Log($"Exported {dataCount} data blocks from {archiveFile}");
                        }
                        else
                        {
                            Log($"Resource archive file {archiveFile} is not a valid archive");
                        }
                    }
                }
            });

            Log($"Finished exporting {romFileName}");
        }

        Log("Finished exporting from all roms");
    }

    #endregion

    #region Data Types

    private record J2meActorType(
        J2meResourceId ImageResourceId,
        int ImageIndex,
        J2meResourceId DataResourceId,
        int DataIndex,
        bool CreateDataImage);
    private readonly struct J2meResourceId
    {
        public J2meResourceId(int value)
        {
            ResourceId.GetValues(value, out int arrayIndexGeneral, out RESOURCE_TYPE type, out int archiveIndex, out _);
            ArchiveIndex = archiveIndex;
            ArrayIndexGeneral = arrayIndexGeneral;
            Type = type;
        }

        public int ArchiveIndex { get; }
        public int ArrayIndexGeneral { get; }
        public RESOURCE_TYPE Type { get; }
    }

    #endregion
}