using System;
using System.IO;
using System.Linq;
using System.Text;
using BinarySerializer;
using BinarySerializer.Gameloft.J2me;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.J2me;

// NOTE: In the original game this is part of the Game class and all members are prefixed with 'RM_'
public class ResourceManager
{
    public const int ERRORCODE_OK = 0;
    public const int ERRORCODE_WARNING_SYNC_UP_TO_DATE = -1;
    public const int ERRORCODE_WARNING_RESOURCE_ALREADY_REQUESTED = -2;
    public const int ERRORCODE_WARNING_RESOURCE_NOT_REQUESTED = -3;
    public const int ERRORCODE_WARNING_MANAGER_ALREADY_SYNCHRONIZING = -4;
    public const int ERRORCODE_WARNING_MANAGER_ALREADY_INITIALIZED = -5;
    public const int ERRORCODE_ERROR_UNSPECIFIED = -6;
    public const int ERRORCODE_ERROR_MANAGER_NOT_INITIALIZED = -7;
    public const int ERRORCODE_ERROR_ARCHIVE_NOT_FOUND = -8;
    public const int ERRORCODE_ERROR_ARCHIVE_READ_FAILURE = -9;
    public const int ERRORCODE_ERROR_INVALID_RESOURCE_ID = -10;
    public const int ERRORCODE_ERROR_UNUSABLE_RESOURCE_ID = -11;
    public const int ERRORCODE_ERROR_OUT_OF_MEMORY = -12;
    public const int ERRORCODE_ERROR_INVALID_PARAMETER = -13;

    public MANAGER_STATUS Manager_Status { get; set; }
    public ArchiveResource[] Array_Data { get; set; }
    public Texture2D[] Array_Image { get; set; }
    public Texture2D Image_Default { get; } = Gfx.Pixel;
    public RESOURCE_STATUS[][] Resource_Status { get; set; }
    public ArchiveInformation[] Archive_Information { get; set; }
    public string[] kErrorMessages { get; } =
    [
        "OK", 
        "Warning : Synchronization Already Up-To-Date", 
        "Warning : Resource Already Requested", 
        "Warning : Resource Not Requested", 
        "Warning : Manager Already Synchronizing", 
        "Warning : Manager Already Initialized", 
        "Error : Unspecified", 
        "Error : Manager Not Initialized", 
        "Error : Archive Not Found", 
        "Error : Archive Read Failure", 
        "Error : Invalid Resource ID", 
        "Error : Unusable Resource ID", 
        "Error : Out Of Memory", 
        "Error : Invalid Parameter"
    ];

    // Unused debug function
    public string GetErrorMessage(int ErrorValue)
    {
        if (ErrorValue > 0 || -ErrorValue >= kErrorMessages.Length)
            return kErrorMessages[-ERRORCODE_ERROR_UNSPECIFIED];
        return kErrorMessages[-ErrorValue];
    }

    // Unused debug function
    public bool IsErrorCritical(int ErrorValue)
    {
        if (ErrorValue is > 0 or <= -6)
            return true;
        return false;
    }

    // Unused debug function, but we use it in Readvanced to make the code more readable
    public int ResourceID_To_Index(ResourceId Param_ResourceID)
    {
        int Array_Index_General = Param_ResourceID.ResourceIndex;
        ResourceType Type = Param_ResourceID.ResourceType;
        int Archive_Index = Param_ResourceID.ArchiveIndex;
        int Validation = Param_ResourceID.Validation;

        if (Manager_Status == MANAGER_STATUS.UNINITIALIZED)
            return -1;
        if (Validation != 3)
            return -1;
        if (Archive_Index >= Resource_Status.Length)
            return -1;
        if (Array_Index_General >= Resource_Status[Archive_Index].Length)
            return -1;

        sbyte InformationField;
        if (Type == ResourceType.Data)
        {
            InformationField = 1;
            Array_Index_General -= Archive_Information[Archive_Index].ImageResourcesCount;
        }
        else
        {
            InformationField = 0;
        }

        while (Archive_Index > 0)
        {
            if (InformationField == 1)
                Array_Index_General += Archive_Information[Archive_Index - 1].DataResourcesCount;
            else
                Array_Index_General += Archive_Information[Archive_Index - 1].ImageResourcesCount;
            Archive_Index--;
        }

        return Array_Index_General;
    }

    // Unused debug function
    public int GetResourceStatus(ResourceId Param_ResourceID)
    {
        int Array_Index_General = Param_ResourceID.ResourceIndex;
        int Archive_Index = Param_ResourceID.ArchiveIndex;
        int Validation = Param_ResourceID.Validation;

        if (Manager_Status == MANAGER_STATUS.UNINITIALIZED)
            return ERRORCODE_ERROR_MANAGER_NOT_INITIALIZED;
        if (Validation != 3)
            return ERRORCODE_ERROR_INVALID_RESOURCE_ID;
        if (Archive_Index >= Resource_Status.Length)
            return -1;
        if (Array_Index_General >= Resource_Status[Archive_Index].Length)
            return -1;

        return (Resource_Status[Archive_Index][Array_Index_General] & (RESOURCE_STATUS.REQUESTED | RESOURCE_STATUS.LOADED)) switch
        {
            // No idea what the return values here are
            RESOURCE_STATUS.REQUESTED | RESOURCE_STATUS.LOADED => 1,
            RESOURCE_STATUS.REQUESTED => 2,
            RESOURCE_STATUS.LOADED => 3,
            _ => 0
        };
    }

    // Unused debug function
    public int ListUnusedResources()
    {
        if (Manager_Status == MANAGER_STATUS.UNINITIALIZED)
            return ERRORCODE_ERROR_MANAGER_NOT_INITIALIZED;

        for (int Archive_Index = 0; Archive_Index < J2meRom.ArchiveDefines.Length; Archive_Index++)
        {
            for (int Loop_Resource = 0; Loop_Resource < Resource_Status[Archive_Index].Length; Loop_Resource++)
            {
                if ((Resource_Status[Archive_Index][Loop_Resource] & RESOURCE_STATUS.USED) == 0)
                {
                    string ReportString = "Unused Resource : ";
                    ReportString += $"Archive = {Archive_Index} ({J2meRom.ArchiveDefines[Archive_Index].FileName}), ";
                    ReportString += $"ID = xxx{Loop_Resource}, Type = ";
                    if (Loop_Resource < Archive_Information[Archive_Index].ImageResourcesCount)
                        ReportString += "Image";
                    else
                        ReportString += "Data";
                    System.println(ReportString);
                }
            }
        }

        return ERRORCODE_OK;
    }

    // Unused debug function
    public int ListLoadedResources()
    {
        if (Manager_Status == MANAGER_STATUS.UNINITIALIZED)
            return ERRORCODE_ERROR_MANAGER_NOT_INITIALIZED;

        for (int Archive_Index = 0; Archive_Index < J2meRom.ArchiveDefines.Length; Archive_Index++)
        {
            for (int Loop_Resource = 0; Loop_Resource < Resource_Status[Archive_Index].Length; Loop_Resource++)
            {
                if ((Resource_Status[Archive_Index][Loop_Resource] & RESOURCE_STATUS.LOADED) > 0)
                {
                    string ReportString = "Loaded Resource : ";
                    ReportString += $"Archive = {Archive_Index} ({J2meRom.ArchiveDefines[Archive_Index].FileName}), ";
                    ReportString += $"ID = xxx{Loop_Resource}, Type = ";
                    if (Loop_Resource < Archive_Information[Archive_Index].ImageResourcesCount)
                        ReportString += "Image";
                    else
                        ReportString += "Data";
                    System.println(ReportString);
                }
            }
        }

        return ERRORCODE_OK;
    }

    public Texture2D GetImage(int Param_ArrayIndex)
    {
        if (Manager_Status == MANAGER_STATUS.UNINITIALIZED || 
            Param_ArrayIndex < 0 || 
            Param_ArrayIndex >= Array_Image.Length ||
            Array_Image[Param_ArrayIndex] == null)
            return Image_Default;

        return Array_Image[Param_ArrayIndex];
    }

    public string GetString(StringId Param_StringID)
    {
        short Offset = Param_StringID.Offset;
        short DataArray_Index = Param_StringID.DataIndex;

        if (Manager_Status == MANAGER_STATUS.UNINITIALIZED || 
            DataArray_Index < 0 || 
            DataArray_Index >= Array_Data.Length || 
            GetData<BinarySerializer.Gameloft.J2me.TextBankResource>(DataArray_Index) is not { } textBank ||
            Offset < 0)
            return String.Empty;

        long fileOffset = textBank.Offset.FileOffset + Offset;
        foreach (TextBankEntry entry in textBank.Entries)
            if (entry.Offset.FileOffset == fileOffset)
                return entry.Value;

        return String.Empty;
    }

    public StringId NextStringID(StringId Param_StringID)
    {
        short Offset = Param_StringID.Offset;
        short DataArray_Index = Param_StringID.DataIndex;

        if (Manager_Status == MANAGER_STATUS.UNINITIALIZED || 
            DataArray_Index < 0 || 
            DataArray_Index >= Array_Data.Length || 
            GetData<BinarySerializer.Gameloft.J2me.TextBankResource>(DataArray_Index) is not { } textBank || 
            Offset < 0)
            return StringId.Null;

        long fileOffset = textBank.Offset.FileOffset + Offset;
        for (int i = 0; i < textBank.Entries.Length - 1; i++)
            if (textBank.Entries[i].Offset.FileOffset == fileOffset)
                return new StringId((short)(textBank.Entries[i + 1].Offset.FileOffset - textBank.Offset.FileOffset), DataArray_Index);
        
        return StringId.Null;
    }

    public int DirectVQ_GetWidth(int Param_ArrayIndex)
    {
        if (Manager_Status == MANAGER_STATUS.UNINITIALIZED || 
            Param_ArrayIndex < 0 || 
            Param_ArrayIndex >= Array_Data.Length ||
            GetData<DirectTwinVQResource>(Param_ArrayIndex) is not { } res ||
            res.Data.Length == 0 ||
            res.BlockSize != res.BlockWidth * res.BlockHeight)
            return 0;

        return res.Width * res.BlockWidth;
    }

    public int DirectVQ_GetHeight(int Param_ArrayIndex)
    {
        if (Manager_Status == MANAGER_STATUS.UNINITIALIZED || 
            Param_ArrayIndex < 0 || 
            Param_ArrayIndex >= Array_Data.Length ||
            GetData<DirectTwinVQResource>(Param_ArrayIndex) is not { } res ||
            res.Data.Length == 0 ||
            res.BlockSize != res.BlockWidth * res.BlockHeight)
            return 0;
        
        return res.Height * res.BlockHeight;
    }

    public int DirectTwinVQ_Read(int Param_ArrayIndex, int Param_Position_X, int Param_Position_Y, int Param_DoubletOffset)
    {
        if (Manager_Status == MANAGER_STATUS.UNINITIALIZED || 
            Param_ArrayIndex < 0 || 
            Param_ArrayIndex >= Array_Data.Length ||
            GetData<DirectTwinVQResource>(Param_ArrayIndex) is not { } res ||
            res.Data.Length == 0 ||
            res.BlockSize != res.BlockWidth * res.BlockHeight || 
            Param_Position_X < 0 || 
            Param_Position_X >= res.Width * res.BlockWidth || 
            Param_Position_Y < 0 || 
            Param_Position_Y >= res.Height * res.BlockHeight)
            return 0;

        // NOTE: Can probably be cleaned up more...
        int off = res.ValuesCount * 2;
        int v1 = res.Data[off + res.BlocksCount * res.BlockSize + Param_Position_X / res.BlockWidth + Param_Position_Y / res.BlockHeight * res.Width];
        int v2 = res.Data[off + v1 * res.BlockSize + Param_Position_X % res.BlockWidth + Param_Position_Y % res.BlockHeight * res.BlockWidth];
        return res.Data[v2 + Param_DoubletOffset * res.ValuesCount];
    }

    public int Initialize()
    {
        if (Manager_Status != MANAGER_STATUS.UNINITIALIZED)
            return ERRORCODE_WARNING_MANAGER_ALREADY_INITIALIZED;

        Manager_Status = MANAGER_STATUS.INITIALIZED;

        Array_Image = new Texture2D[J2meRom.ArchiveDefines.Sum(x => x.ImageResourcesCount)];
        Array_Data = new ArchiveResource[J2meRom.ArchiveDefines.Sum(x => x.DataResourcesCount)];

        Archive_Information = new ArchiveInformation[J2meRom.ArchiveDefines.Length];
        for (int i = 0; i < Archive_Information.Length; i++)
        {
            Archive_Information[i] = new ArchiveInformation()
            {
                ImageResourcesCount = J2meRom.ArchiveDefines[i].ImageResourcesCount,
                DataResourcesCount = J2meRom.ArchiveDefines[i].DataResourcesCount,
            };
        }

        Resource_Status = new RESOURCE_STATUS[J2meRom.ArchiveDefines.Length][];
        for (int i = 0; i < J2meRom.ArchiveDefines.Length; i++)
            Resource_Status[i] = new RESOURCE_STATUS[Archive_Information[i].ImageResourcesCount + Archive_Information[i].DataResourcesCount];

        return ERRORCODE_OK;
    }

    public int Load(ResourceId Param_ResourceID)
    {
        int Array_Index_General = Param_ResourceID.ResourceIndex;
        int Archive_Index = Param_ResourceID.ArchiveIndex;
        int Validation = Param_ResourceID.Validation;

        if (Manager_Status == MANAGER_STATUS.UNINITIALIZED)
            return ERRORCODE_ERROR_MANAGER_NOT_INITIALIZED;
        if (Validation != 3)
            return ERRORCODE_ERROR_INVALID_RESOURCE_ID;
        if (Archive_Index >= Resource_Status.Length)
            return -1;
        if (Array_Index_General >= Resource_Status[Archive_Index].Length)
            return -1;

        if ((Resource_Status[Archive_Index][Array_Index_General] & RESOURCE_STATUS.REQUESTED) != 0)
            return ERRORCODE_WARNING_RESOURCE_ALREADY_REQUESTED;

        Resource_Status[Archive_Index][Array_Index_General] |= RESOURCE_STATUS.REQUESTED;

        if ((Resource_Status[Archive_Index][Array_Index_General] & RESOURCE_STATUS.LOADED) == 0)
            Archive_Information[Archive_Index].PendingLoad = (sbyte)(Archive_Information[Archive_Index].PendingLoad + 1);
        else
            Archive_Information[Archive_Index].PendingFree = (sbyte)(Archive_Information[Archive_Index].PendingFree - 1);

        if (Manager_Status == MANAGER_STATUS.INITIALIZED)
            Manager_Status = MANAGER_STATUS.PENDING_SYNC;

        return ERRORCODE_OK;
    }

    public int LoadImage(ResourceId Param_ResourceID)
    {
        if (Param_ResourceID.ResourceType != ResourceType.Image)
            throw new Exception($"Resource type {Param_ResourceID.ResourceType} is not a valid image type");

        return Load(Param_ResourceID);
    }

    public int LoadData<T>(ResourceId Param_ResourceID)
        where T : ArchiveResource, new()
    {
        if (Param_ResourceID.ResourceType != ResourceType.Data)
            throw new Exception($"Resource type {Param_ResourceID.ResourceType} is not a valid data type");

        int result = Load(Param_ResourceID);

        if (result == ERRORCODE_OK)
            Array_Data[ResourceID_To_Index(Param_ResourceID)] = new T();

        return result;
    }

    // Unused
    public int Load()
    {
        if (Manager_Status == MANAGER_STATUS.UNINITIALIZED)
            return ERRORCODE_ERROR_MANAGER_NOT_INITIALIZED;

        for (int Archive_Index = 0; Archive_Index < J2meRom.ArchiveDefines.Length; Archive_Index++)
        {
            for (int Loop_Resource = 0; Loop_Resource < Resource_Status[Archive_Index].Length; Loop_Resource++)
            {
                if ((Resource_Status[Archive_Index][Loop_Resource] & RESOURCE_STATUS.REQUESTED) == 0)
                {
                    Resource_Status[Archive_Index][Loop_Resource] |= RESOURCE_STATUS.REQUESTED;
                    Archive_Information[Archive_Index].PendingLoad = (sbyte)(Archive_Information[Archive_Index].PendingLoad + 1);
                    if (Manager_Status == MANAGER_STATUS.INITIALIZED)
                        Manager_Status = MANAGER_STATUS.PENDING_SYNC;
                }
            }
            Archive_Information[Archive_Index].PendingFree = 0;
        }

        return ERRORCODE_OK;
    }

    public int Free(ResourceId Param_ResourceID)
    {
        int Array_Index_General = Param_ResourceID.ResourceIndex;
        int Archive_Index = Param_ResourceID.ArchiveIndex;
        int Validation = Param_ResourceID.Validation;

        if (Manager_Status == MANAGER_STATUS.UNINITIALIZED)
            return ERRORCODE_ERROR_MANAGER_NOT_INITIALIZED;
        if (Validation != 3)
            return ERRORCODE_ERROR_INVALID_RESOURCE_ID;
        if (Archive_Index >= Resource_Status.Length)
            return -1;
        if (Array_Index_General >= (Resource_Status[Archive_Index]).Length)
            return -1;

        if ((Resource_Status[Archive_Index][Array_Index_General] & RESOURCE_STATUS.REQUESTED) == 0)
            return ERRORCODE_WARNING_RESOURCE_NOT_REQUESTED;

        Resource_Status[Archive_Index][Array_Index_General] &= ~RESOURCE_STATUS.REQUESTED;

        if ((Resource_Status[Archive_Index][Array_Index_General] & RESOURCE_STATUS.LOADED) == 0)
            Archive_Information[Archive_Index].PendingLoad = (sbyte)(Archive_Information[Archive_Index].PendingLoad - 1);
        else
            Archive_Information[Archive_Index].PendingFree = (sbyte)(Archive_Information[Archive_Index].PendingFree + 1);

        if (Manager_Status == MANAGER_STATUS.INITIALIZED)
            Manager_Status = MANAGER_STATUS.PENDING_SYNC;

        return ERRORCODE_OK;
    }

    // Unused
    public int Free()
    {
        if (Manager_Status == MANAGER_STATUS.UNINITIALIZED)
            return ERRORCODE_ERROR_MANAGER_NOT_INITIALIZED;

        for (int Archive_Index = 0; Archive_Index < J2meRom.ArchiveDefines.Length; Archive_Index++)
        {
            for (int Loop_Resource = 0; Loop_Resource < Resource_Status[Archive_Index].Length; Loop_Resource++)
            {
                if ((Resource_Status[Archive_Index][Loop_Resource] & RESOURCE_STATUS.REQUESTED) != 0)
                {
                    Resource_Status[Archive_Index][Loop_Resource] &= ~RESOURCE_STATUS.REQUESTED;
                    Archive_Information[Archive_Index].PendingFree = (sbyte)(Archive_Information[Archive_Index].PendingFree + 1);

                    if (Manager_Status == MANAGER_STATUS.INITIALIZED)
                        Manager_Status = MANAGER_STATUS.PENDING_SYNC;
                }
            }
            Archive_Information[Archive_Index].PendingLoad = 0;
        }

        return ERRORCODE_OK;
    }

    public int Synchronize()
    {
        if (Manager_Status == MANAGER_STATUS.UNINITIALIZED)
            return ERRORCODE_ERROR_MANAGER_NOT_INITIALIZED;
        if (Manager_Status == MANAGER_STATUS.INITIALIZED)
            return ERRORCODE_WARNING_SYNC_UP_TO_DATE;
        if (Manager_Status != MANAGER_STATUS.PENDING_SYNC)
            return ERRORCODE_WARNING_MANAGER_ALREADY_SYNCHRONIZING;

        // Free resources
        Manager_Status = MANAGER_STATUS.FREEING;
        int Index_Image = 0;
        int Index_Data = 0;
        int Archive_Index;
        for (Archive_Index = 0; Archive_Index < J2meRom.ArchiveDefines.Length; Archive_Index++)
        {
            if (Archive_Information[Archive_Index].PendingFree > 0)
            {
                for (int Loop_Resource = 0; Loop_Resource < Resource_Status[Archive_Index].Length; Loop_Resource++)
                {
                    if ((Resource_Status[Archive_Index][Loop_Resource] & (RESOURCE_STATUS.REQUESTED | RESOURCE_STATUS.LOADED)) == RESOURCE_STATUS.LOADED)
                    {
                        if (Loop_Resource < Archive_Information[Archive_Index].ImageResourcesCount)
                            Array_Image[Index_Image] = null;
                        else
                            Array_Data[Index_Data] = null;

                        Resource_Status[Archive_Index][Loop_Resource] &= ~RESOURCE_STATUS.LOADED;
                    }

                    if (Loop_Resource < Archive_Information[Archive_Index].ImageResourcesCount)
                        Index_Image++;
                    else
                        Index_Data++;
                }
                Archive_Information[Archive_Index].PendingFree = 0;
            }
            else
            {
                Index_Image += Archive_Information[Archive_Index].ImageResourcesCount;
                Index_Data += Archive_Information[Archive_Index].DataResourcesCount;
            }
        }
        System.gc();

        using (J2meRom.Context)
        {
            // Load resources
            Manager_Status = MANAGER_STATUS.LOADING;
            Index_Image = 0;
            Index_Data = 0;
            for (Archive_Index = 0; Archive_Index < J2meRom.ArchiveDefines.Length; Archive_Index++)
            {
                if (Archive_Information[Archive_Index].PendingLoad > 0)
                {
                    for (sbyte b = 0; b < Resource_Status[Archive_Index].Length; b++)
                    {
                        if ((Resource_Status[Archive_Index][b] & (RESOURCE_STATUS.REQUESTED | RESOURCE_STATUS.LOADED)) == RESOURCE_STATUS.REQUESTED)
                        {
                            int Data_Size_Archived = J2meRom.ArchiveHeaders[Archive_Index].Entries[b].DataSize;
                            short Data_Size_CompressionDelta = J2meRom.ArchiveHeaders[Archive_Index].Entries[b].DataSizeCompressionDelta;

                            // Decompress image to a PNG file
                            if (b < Archive_Information[Archive_Index].ImageResourcesCount)
                            {
                                // Read the data
                                RawResource rawResource = J2meRom.ReadResource<RawResource>(Archive_Index, b);
                                byte[] Data_Archived = rawResource.Data;

                                // Decompress the image
                                byte[] Data_Decompressed = DecompressImage(Data_Archived, Data_Size_Archived, Data_Size_CompressionDelta);
                                Array_Image[Index_Image] = Texture2D.FromStream(Engine.Assets.GraphicsDevice, new MemoryStream(Data_Decompressed)); // TODO: Dispose when freeing? And then free all when uninit midlet.
                            }
                            else if (Data_Size_CompressionDelta == 0)
                            {
                                // Deserialize the resource
                                Array_Data[Index_Data] = J2meRom.ReadResource(Archive_Index, b, Array_Data[Index_Data]);
                            }

                            Resource_Status[Archive_Index][b] |= RESOURCE_STATUS.LOADED | RESOURCE_STATUS.USED;
                            System.gc();
                        }

                        if (b < Archive_Information[Archive_Index].ImageResourcesCount)
                            Index_Image++;
                        else
                            Index_Data++;
                    }

                    Archive_Information[Archive_Index].PendingLoad = 0;
                    System.gc();
                }
                else
                {
                    Index_Image += Archive_Information[Archive_Index].ImageResourcesCount;
                    Index_Data += Archive_Information[Archive_Index].DataResourcesCount;
                }
            }

            Manager_Status = MANAGER_STATUS.INITIALIZED;
            return ERRORCODE_OK;
        }
    }

    // Custom
    public static byte[] DecompressImage(byte[] Data_Archived, int Data_Size_Archived, int Data_Size_CompressionDelta)
    {
        byte[] Data_Decompressed = new byte[Data_Size_Archived + Data_Size_CompressionDelta];

        using MemoryStream decompressedStream = new(Data_Decompressed);
        using MemoryStream archivedStream = new(Data_Archived);
        using Writer writer = new(decompressedStream, isLittleEndian: false);
        using Reader reader = new(archivedStream, isLittleEndian: false);

        // Write PNG file signature
        writer.Write((uint)0x89504E47);
        writer.Write((uint)0xD0A1A0A);

        // Write the IHDR chunk
        writer.Write((int)0xD);
        writer.Write((uint)0x49484452);
        int DeChunk_Header = reader.ReadInt32();
        writer.Write((int)(DeChunk_Header & 0x3FF)); // Width
        writer.Write((int)((DeChunk_Header >> 10) & 0x3FF)); // Height
        writer.Write((byte)(1 << ((DeChunk_Header >> 20) & 0x3))); // Bit depth
        int Temp_Value = (DeChunk_Header >> 22) & 0x3;
        writer.Write((byte)(Temp_Value == 0 ? 0 : 3)); // Color type
        writer.Write((byte)0x00); // Compression method
        writer.Write((byte)0x00); // Filter method
        writer.Write((byte)0x00); // Interlace method
        writer.Write(reader.ReadUInt32()); // CRC

        // Write palette
        if (Temp_Value == 2)
        {
            // Get the palette size
            Temp_Value = reader.ReadByte() * 3;
            if (Temp_Value == 0)
                Temp_Value = 768;

            // Write the PLTE chunk
            writer.Write((uint)Temp_Value);
            writer.Write((uint)0x504C5445);
            while (Temp_Value > 0)
            {
                int CompactColor = reader.ReadInt16();
                writer.Write((byte)((CompactColor >> 8) * 255 / 15));
                writer.Write((byte)(((CompactColor >> 4) & 0xF) * 255 / 15));
                writer.Write((byte)((CompactColor & 0xF) * 255 / 15));
                Temp_Value -= 3;
            }
            writer.Write(reader.ReadUInt32()); // CRC
        }

        // Write transparency data
        if ((DeChunk_Header & 0x1000000) > 0)
        {
            // Write the tRNS chunk
            writer.Write((int)0x1);
            writer.Write((uint)0x74524E53);
            writer.Write((byte)0x00);
            writer.Write((uint)0x40E6D866); // CRC
        }

        // Write the image data
        do
        {
            // Write a IDAT chunk
            Temp_Value = Data_Size_Archived - (int)archivedStream.Position - 4;
            if (Temp_Value > 0x2000 && (DeChunk_Header & 0x2000000) > 0)
                Temp_Value = 0x2000;
            writer.Write((int)Temp_Value);
            writer.Write((uint)0x49444154);
            System.arraycopy(Data_Archived, (int)archivedStream.Position, Data_Decompressed, (int)decompressedStream.Position, Temp_Value);
            archivedStream.Position += Temp_Value;
            decompressedStream.Position += Temp_Value;
            writer.Write(reader.ReadUInt32()); // CRC
        } while (Temp_Value == 0x2000);

        // Write the IEND chunk
        writer.Write((int)0x0);
        writer.Write((uint)0x49454E44);
        writer.Write((uint)0xAE426082); // CRC

        return Data_Decompressed;
    }
    public Texture2D GetImage(ResourceId Param_ResourceID)
    {
        int index = ResourceID_To_Index(Param_ResourceID);
        return GetImage(index);
    }
    public T GetData<T>(int Param_ArrayIndex)
        where T : ArchiveResource, new()
    {
        return (T)Array_Data[Param_ArrayIndex];
    }
    public T GetData<T>(ResourceId Param_ResourceID)
        where T : ArchiveResource, new()
    {
        return (T)Array_Data[ResourceID_To_Index(Param_ResourceID)];
    }
    public void DumpAllData(string outputPath)
    {
        for (byte archiveIndex = 0; archiveIndex < J2meRom.ArchiveDefines.Length; archiveIndex++)
        {
            for (int dataIndex = 0; dataIndex < Archive_Information[archiveIndex].DataResourcesCount; dataIndex++)
                LoadData<RawResource>(new ResourceId((byte)(Archive_Information[archiveIndex].ImageResourcesCount + dataIndex), ResourceType.Data, archiveIndex));

            Synchronize();

            for (int dataIndex = 0; dataIndex < Archive_Information[archiveIndex].DataResourcesCount; dataIndex++)
            {
                RawResource res = GetData<RawResource>(new ResourceId((byte)(Archive_Information[archiveIndex].ImageResourcesCount + dataIndex), ResourceType.Data, archiveIndex));
                File.WriteAllBytes(Path.Combine(outputPath, $"{archiveIndex}_{dataIndex}.dat"), res.Data);
            }
        }
    }
    public void DumpAllText(string outputPath)
    {
        ResourceId[] textBankResourceIds =
        [
            new(Game.TEXTBANK_INDEX_GAME, ResourceType.Data, Game.ARCHIVE_INDEX_ANIM),
            new(Game.TEXTBANK_INDEX_CREDITS_UNUSED, ResourceType.Data, Game.ARCHIVE_INDEX_ANIM),
            new(Game.TEXTBANK_INDEX_CREDITS, ResourceType.Data, Game.ARCHIVE_INDEX_ANIM),
            new(Game.TEXTBANK_INDEX_HELP, ResourceType.Data, Game.ARCHIVE_INDEX_ANIM),
        ];

        foreach (ResourceId textBankResourceId in textBankResourceIds)
            LoadData<BinarySerializer.Gameloft.J2me.TextBankResource>(textBankResourceId);

        Synchronize();

        foreach (ResourceId textBankResourceId in textBankResourceIds)
        {
            BinarySerializer.Gameloft.J2me.TextBankResource textBank = GetData<BinarySerializer.Gameloft.J2me.TextBankResource>(textBankResourceId);

            StringBuilder sb = new();
            foreach (TextBankEntry entry in textBank.Entries)
                sb.AppendLine(entry.Value);

            File.WriteAllText(Path.Combine(outputPath, $"{textBankResourceId.ArchiveIndex}_{textBankResourceId.ResourceIndex}.txt"), sb.ToString());
        }
    }
    public void DumpAllImages(string outputPath)
    {
        for (byte archiveIndex = 0; archiveIndex < J2meRom.ArchiveDefines.Length; archiveIndex++)
        {
            for (byte imgIndex = 0; imgIndex < Archive_Information[archiveIndex].ImageResourcesCount; imgIndex++)
                LoadImage(new ResourceId(imgIndex, ResourceType.Image, archiveIndex));

            Synchronize();

            for (byte imgIndex = 0; imgIndex < Archive_Information[archiveIndex].ImageResourcesCount; imgIndex++)
            {
                Texture2D img = GetImage(ResourceID_To_Index(new ResourceId(imgIndex, ResourceType.Image, archiveIndex)));
                using FileStream stream = File.Create(Path.Combine(outputPath, $"{archiveIndex}_{imgIndex}.png"));
                img.SaveAsPng(stream, img.Width, img.Height);
            }
        }
    }
}