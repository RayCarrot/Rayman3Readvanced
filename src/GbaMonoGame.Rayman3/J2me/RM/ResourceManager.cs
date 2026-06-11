using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using BinarySerializer;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.J2me;

// NOTE: In the original game this is part of the Game class and all members are prefixed with 'RM_'
public class ResourceManager
{
    public ResourceManager(JavaArchive javaArchive)
    {
        // Read and cache the archives
        Archives = new byte[ARCHIVES_COUNT][];
        for (int i = 0; i < Archives.Length; i++)
        {
            ZipArchiveEntry archiveEntry = javaArchive.GetFile(kArchive_Names[i]);
            using Stream archiveStream = archiveEntry.Open();
            Archives[i] = new byte[archiveEntry.Length];
            archiveStream.ReadExactly(Archives[i]);
        }
    }

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

    public const int ARCHIVES_COUNT = 3;
    public const int MAX_IMAGE_RESOURCES_COUNT = 21;
    public const int MAX_DATA_RESOURCES_COUNT = 70;

    // Custom to cache the archives
    public byte[][] Archives { get; }

    public MANAGER_STATUS Manager_Status { get; set; }
    public byte[][] Array_Data { get; set; }
    public Texture2D[] Array_Image { get; set; }
    public Texture2D Image_Default { get; } = Gfx.Pixel;
    public byte[] Data_Decompressed { get; set; }
    public int Data_Index_Decompressed { get; set; }
    public RESOURCE_STATUS[][] Resource_Status { get; set; }
    public ArchiveInformation[] Archive_Information { get; set; }
    public string[] kArchive_Names { get; } = ["wbw", "mdg", "04d"];
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
    public int ResourceID_To_Index(int Param_ResourceID)
    {
        ResourceId.GetValues(Param_ResourceID, out int Array_Index_General, out RESOURCE_TYPE Type, out int Archive_Index, out int Validation);

        if (Manager_Status == MANAGER_STATUS.UNINITIALIZED)
            return -1;
        if (Validation != 3)
            return -1;
        if (Archive_Index >= Resource_Status.Length)
            return -1;
        if (Array_Index_General >= Resource_Status[Archive_Index].Length)
            return -1;

        sbyte InformationField;
        if (Type == RESOURCE_TYPE.DATA)
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
    public int GetResourceStatus(int Param_ResourceID)
    {
        ResourceId.GetValues(Param_ResourceID, out int Array_Index_General, out _, out int Archive_Index, out int Validation);

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
            // No idea what the return values here are§
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

        for (int Archive_Index = 0; Archive_Index < ARCHIVES_COUNT; Archive_Index++)
        {
            for (int Loop_Resource = 0; Loop_Resource < Resource_Status[Archive_Index].Length; Loop_Resource++)
            {
                if ((Resource_Status[Archive_Index][Loop_Resource] & RESOURCE_STATUS.USED) == 0)
                {
                    string ReportString = "Unused Resource : ";
                    ReportString += $"Archive = {Archive_Index} ({kArchive_Names[Archive_Index]}), ";
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

        for (int Archive_Index = 0; Archive_Index < ARCHIVES_COUNT; Archive_Index++)
        {
            for (int Loop_Resource = 0; Loop_Resource < Resource_Status[Archive_Index].Length; Loop_Resource++)
            {
                if ((Resource_Status[Archive_Index][Loop_Resource] & RESOURCE_STATUS.LOADED) > 0)
                {
                    string ReportString = "Loaded Resource : ";
                    ReportString += $"Archive = {Archive_Index} ({kArchive_Names[Archive_Index]}), ";
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

    public string GetString(int Param_StringID)
    {
        StringId.GetValues(Param_StringID, out int Offset, out int DataArray_Index);

        if (Manager_Status == MANAGER_STATUS.UNINITIALIZED || 
            DataArray_Index < 0 || 
            DataArray_Index >= Array_Data.Length || 
            Array_Data[DataArray_Index] == null || 
            Offset < 0 || 
            Offset >= Array_Data[DataArray_Index].Length)
            return String.Empty;

        // First byte is the length followed by the string
        return Encoding.UTF8.GetString(Array_Data[DataArray_Index], Offset + 1, Array_Data[DataArray_Index][Offset]);
    }

    public int NextStringID(int Param_StringID)
    {
        StringId.GetValues(Param_StringID, out int Offset, out int DataArray_Index);

        if (Manager_Status == MANAGER_STATUS.UNINITIALIZED || 
            DataArray_Index < 0 || 
            DataArray_Index >= Array_Data.Length || 
            Array_Data[DataArray_Index] == null || 
            Offset < 0 || 
            Offset >= Array_Data[DataArray_Index].Length)
            return -1;
        
        int StringLength = Array_Data[DataArray_Index][Offset] + 1;
        if (Offset + StringLength >= Array_Data[DataArray_Index].Length)
            return -1;
        
        return Param_StringID + StringLength;
    }

    public int DirectVQ_GetWidth(int Param_ArrayIndex)
    {
        if (Manager_Status == MANAGER_STATUS.UNINITIALIZED || 
            Param_ArrayIndex < 0 || 
            Param_ArrayIndex >= Array_Data.Length || 
            Array_Data[Param_ArrayIndex] == null || 
            Array_Data[Param_ArrayIndex].Length <= 6 || 
            Array_Data[Param_ArrayIndex][3] != Array_Data[Param_ArrayIndex][0] * Array_Data[Param_ArrayIndex][1])
            return 0;

        return Array_Data[Param_ArrayIndex][4] * Array_Data[Param_ArrayIndex][0];
    }

    public int DirectVQ_GetHeight(int Param_ArrayIndex)
    {
        if (Manager_Status == MANAGER_STATUS.UNINITIALIZED || 
            Param_ArrayIndex < 0 
            || Param_ArrayIndex >= Array_Data.Length || 
            Array_Data[Param_ArrayIndex] == null || 
            Array_Data[Param_ArrayIndex].Length <= 6 
            || Array_Data[Param_ArrayIndex][3] != Array_Data[Param_ArrayIndex][0] * Array_Data[Param_ArrayIndex][1])
            return 0;
        
        return Array_Data[Param_ArrayIndex][5] * Array_Data[Param_ArrayIndex][1];
    }

    public int DirectTwinVQ_Read(int Param_ArrayIndex, int Param_Position_X, int Param_Position_Y, int Param_DoubletOffset)
    {
        if (Manager_Status == MANAGER_STATUS.UNINITIALIZED || 
            Param_ArrayIndex < 0 || 
            Param_ArrayIndex >= Array_Data.Length || 
            Array_Data[Param_ArrayIndex] == null || 
            Array_Data[Param_ArrayIndex].Length <= 7 || 
            Array_Data[Param_ArrayIndex][3] != Array_Data[Param_ArrayIndex][0] * Array_Data[Param_ArrayIndex][1] || 
            Param_Position_X < 0 || 
            Param_Position_X >= Array_Data[Param_ArrayIndex][4] * Array_Data[Param_ArrayIndex][0] || 
            Param_Position_Y < 0 || 
            Param_Position_Y >= Array_Data[Param_ArrayIndex][5] * Array_Data[Param_ArrayIndex][1])
            return 0;

        return Array_Data[Param_ArrayIndex][7 + Array_Data[Param_ArrayIndex][7 + (Array_Data[Param_ArrayIndex][6] << 1) + Array_Data[Param_ArrayIndex][7 + (Array_Data[Param_ArrayIndex][6] << 1) + Array_Data[Param_ArrayIndex][2] * Array_Data[Param_ArrayIndex][3] + Param_Position_X / Array_Data[Param_ArrayIndex][0] + Param_Position_Y / Array_Data[Param_ArrayIndex][1] * Array_Data[Param_ArrayIndex][4]] * Array_Data[Param_ArrayIndex][3] + Param_Position_X % Array_Data[Param_ArrayIndex][0] + Param_Position_Y % Array_Data[Param_ArrayIndex][1] * Array_Data[Param_ArrayIndex][0]] + Param_DoubletOffset * Array_Data[Param_ArrayIndex][6]];
    }

    public int Initialize()
    {
        if (Manager_Status != MANAGER_STATUS.UNINITIALIZED)
            return ERRORCODE_WARNING_MANAGER_ALREADY_INITIALIZED;

        Manager_Status = MANAGER_STATUS.INITIALIZED;

        Array_Image = new Texture2D[MAX_IMAGE_RESOURCES_COUNT];
        Array_Data = new byte[MAX_DATA_RESOURCES_COUNT][];

        Archive_Information = new ArchiveInformation[ARCHIVES_COUNT];
        Archive_Information[0] = new ArchiveInformation { ImageResourcesCount = 0, DataResourcesCount = 49 }; // Resource_Archive_animation
        Archive_Information[1] = new ArchiveInformation { ImageResourcesCount = 21, DataResourcesCount = 0 }; // Resource_Archive_image
        Archive_Information[2] = new ArchiveInformation { ImageResourcesCount = 0, DataResourcesCount = 21 }; // Resource_Archive_map

        Resource_Status = new RESOURCE_STATUS[ARCHIVES_COUNT][];
        for (int i = 0; i < ARCHIVES_COUNT; i++)
            Resource_Status[i] = new RESOURCE_STATUS[Archive_Information[i].ImageResourcesCount + Archive_Information[i].DataResourcesCount];
        
        return ERRORCODE_OK;
    }

    public int Load(int Param_ResourceID)
    {
        ResourceId.GetValues(Param_ResourceID, out int Array_Index_General, out _, out int Archive_Index, out int Validation);

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

    // Unused
    public int Load()
    {
        if (Manager_Status == MANAGER_STATUS.UNINITIALIZED)
            return ERRORCODE_ERROR_MANAGER_NOT_INITIALIZED;

        for (int Archive_Index = 0; Archive_Index < ARCHIVES_COUNT; Archive_Index++)
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

    public int Free(int Param_ResourceID)
    {
        ResourceId.GetValues(Param_ResourceID, out int Array_Index_General, out _, out int Archive_Index, out int Validation);

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

        for (int Archive_Index = 0; Archive_Index < ARCHIVES_COUNT; Archive_Index++)
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

    private void WriteDWORD(int Value) => WriteDWORD((uint)Value);
    private void WriteDWORD(uint Value)
    {
        Data_Decompressed[Data_Index_Decompressed++] = (byte)(Value >> 24);
        Data_Decompressed[Data_Index_Decompressed++] = (byte)(Value >> 16);
        Data_Decompressed[Data_Index_Decompressed++] = (byte)(Value >> 8);
        Data_Decompressed[Data_Index_Decompressed++] = (byte)Value;
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
        for (Archive_Index = 0; Archive_Index < ARCHIVES_COUNT; Archive_Index++)
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

        // Load resources
        Manager_Status = MANAGER_STATUS.LOADING;
        Index_Image = 0;
        Index_Data = 0;
        for (Archive_Index = 0; Archive_Index < ARCHIVES_COUNT; Archive_Index++)
        {
            if (Archive_Information[Archive_Index].PendingLoad > 0)
            {
                using MemoryStream Archive_InputStream = new(Archives[Archive_Index]);

                int File_CurrentOffset = Resource_Status[Archive_Index].Length * 4;
                int File_DestOffset = File_CurrentOffset;
                int File_TableOffset = 0;
                byte[] HeaderTable = new byte[File_CurrentOffset];
                Archive_InputStream.ReadExactly(HeaderTable, 0, File_CurrentOffset);

                for (sbyte b = 0; b < Resource_Status[Archive_Index].Length; b++)
                {
                    int Data_Size_Archived = (HeaderTable[File_TableOffset++] << 8) + HeaderTable[File_TableOffset++];
                    short Data_Size_CompressionDelta = (short)((HeaderTable[File_TableOffset++] << 8) + HeaderTable[File_TableOffset++]);

                    if ((Resource_Status[Archive_Index][b] & (RESOURCE_STATUS.REQUESTED | RESOURCE_STATUS.LOADED)) == RESOURCE_STATUS.REQUESTED)
                    {
                        if (File_DestOffset > File_CurrentOffset)
                        {
                            Archive_InputStream.Seek(File_DestOffset - File_CurrentOffset, SeekOrigin.Current);
                            File_CurrentOffset = File_DestOffset;
                        }

                        // Decompress image to a PNG file
                        if (b < Archive_Information[Archive_Index].ImageResourcesCount)
                        {
                            byte[] Data_Archived = new byte[Data_Size_Archived];
                            Data_Decompressed = new byte[Data_Size_Archived + Data_Size_CompressionDelta];
                            Archive_InputStream.ReadExactly(Data_Archived, 0, Data_Size_Archived);
                            Data_Index_Decompressed = 0;
                            int Data_Index_Archived = 0;
                            WriteDWORD(0x89504E47);
                            WriteDWORD(0xD0A1A0A);
                            WriteDWORD(0xD);
                            WriteDWORD(0x49484452);
                            int DeChunk_Header = (Data_Archived[Data_Index_Archived++] << 24) | (Data_Archived[Data_Index_Archived++] << 16) | (Data_Archived[Data_Index_Archived++] << 8) | Data_Archived[Data_Index_Archived++];
                            WriteDWORD(DeChunk_Header & 0x3FF);
                            WriteDWORD((DeChunk_Header >> 10) & 0x3FF);
                            Data_Decompressed[Data_Index_Decompressed++] = (byte)(1 << ((DeChunk_Header >> 20) & 0x3));
                            int Temp_Value = (DeChunk_Header >> 22) & 0x3;
                            Data_Decompressed[Data_Index_Decompressed++] = (byte)(Temp_Value == 0 ? 0 : 3);
                            Data_Index_Decompressed += 3;
                            WriteDWORD(((Data_Archived[Data_Index_Archived++] << 24) | (Data_Archived[Data_Index_Archived++] << 16) | (Data_Archived[Data_Index_Archived++] << 8) | Data_Archived[Data_Index_Archived++]));

                            if (Temp_Value == 2)
                            {
                                Temp_Value = Data_Archived[Data_Index_Archived++] * 3;
                                if (Temp_Value == 0)
                                    Temp_Value = 768;
                                WriteDWORD(Temp_Value);
                                WriteDWORD(0x504C5445);
                                while (Temp_Value > 0)
                                {
                                    int CompactColor = (Data_Archived[Data_Index_Archived++] << 8) + Data_Archived[Data_Index_Archived++];
                                    Data_Decompressed[Data_Index_Decompressed++] = (byte)((CompactColor >> 8) * 255 / 15);
                                    Data_Decompressed[Data_Index_Decompressed++] = (byte)(((CompactColor >> 4) & 0xF) * 255 / 15);
                                    Data_Decompressed[Data_Index_Decompressed++] = (byte)((CompactColor & 0xF) * 255 / 15);
                                    Temp_Value -= 3;
                                }
                                WriteDWORD(((Data_Archived[Data_Index_Archived++] << 24) | (Data_Archived[Data_Index_Archived++] << 16) | (Data_Archived[Data_Index_Archived++] << 8) | Data_Archived[Data_Index_Archived++]));
                            }

                            if ((DeChunk_Header & 0x1000000) > 0)
                            {
                                WriteDWORD(0x1);
                                WriteDWORD(0x74524E53);
                                Data_Index_Decompressed++;
                                WriteDWORD(0x40E6D866);
                            }
                            do
                            {
                                Temp_Value = Data_Size_Archived - Data_Index_Archived - 4;
                                if (Temp_Value > 0x2000 && (DeChunk_Header & 0x2000000) > 0)
                                    Temp_Value = 0x2000;
                                WriteDWORD(Temp_Value);
                                WriteDWORD(0x49444154);
                                System.arraycopy(Data_Archived, Data_Index_Archived, Data_Decompressed, Data_Index_Decompressed, Temp_Value);
                                Data_Index_Decompressed += Temp_Value;
                                Data_Index_Archived += Temp_Value;
                                WriteDWORD(((Data_Archived[Data_Index_Archived++] << 24) | (Data_Archived[Data_Index_Archived++] << 16) | (Data_Archived[Data_Index_Archived++] << 8) | Data_Archived[Data_Index_Archived++]));
                            } while (Temp_Value == 0x2000);
                            WriteDWORD(0x0);
                            WriteDWORD(0x49454E44);
                            WriteDWORD(0xAE426082);

                            using MemoryStream ms = new(Data_Decompressed, 0, Data_Index_Decompressed);
                            Array_Image[Index_Image] = Texture2D.FromStream(Engine.Assets.GraphicsDevice, ms); // TODO: Dispose when freeing? And then free all when uninit midlet.

                            Data_Decompressed = null;
                        }
                        else if (Data_Size_CompressionDelta == 0)
                        {
                            Array_Data[Index_Data] = new byte[Data_Size_Archived];
                            Archive_InputStream.ReadExactly(Array_Data[Index_Data], 0, Data_Size_Archived);
                        }

                        Resource_Status[Archive_Index][b] |= RESOURCE_STATUS.LOADED | RESOURCE_STATUS.USED;
                        File_CurrentOffset += Data_Size_Archived;
                        System.gc();
                    }

                    File_DestOffset += Data_Size_Archived;

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

    // Custom
    public Texture2D GetImageResource(int Param_ResourceID)
    {
        int index = ResourceID_To_Index(Param_ResourceID);
        return GetImage(index);
    }
    public byte[] GetDataResource(int Param_ResourceID)
    {
        int index = ResourceID_To_Index(Param_ResourceID);
        return Array_Data[index];   
    }
    public void DumpAllData(string outputPath)
    {
        for (int archiveIndex = 0; archiveIndex < ARCHIVES_COUNT; archiveIndex++)
        {
            for (int dataIndex = 0; dataIndex < Archive_Information[archiveIndex].DataResourcesCount; dataIndex++)
                Load(ResourceId.Create(Archive_Information[archiveIndex].ImageResourcesCount + dataIndex, RESOURCE_TYPE.DATA, archiveIndex));

            Synchronize();

            for (int dataIndex = 0; dataIndex < Archive_Information[archiveIndex].DataResourcesCount; dataIndex++)
            {
                byte[] data = Array_Data[ResourceID_To_Index(ResourceId.Create(Archive_Information[archiveIndex].ImageResourcesCount + dataIndex, RESOURCE_TYPE.DATA, archiveIndex))];
                File.WriteAllBytes(Path.Combine(outputPath, $"{archiveIndex}_{dataIndex}.dat"), data);
            }
        }
    }
    public void DumpAllText(string outputPath)
    {
        int[] textBankResourceIds =
        [
            ResourceId.Create(Game.TEXTBANK_INDEX_GAME, RESOURCE_TYPE.DATA, Game.ARCHIVE_INDEX_ANIM),
            ResourceId.Create(Game.TEXTBANK_INDEX_CREDITS_UNUSED, RESOURCE_TYPE.DATA, Game.ARCHIVE_INDEX_ANIM),
            ResourceId.Create(Game.TEXTBANK_INDEX_CREDITS, RESOURCE_TYPE.DATA, Game.ARCHIVE_INDEX_ANIM),
            ResourceId.Create(Game.TEXTBANK_INDEX_HELP, RESOURCE_TYPE.DATA, Game.ARCHIVE_INDEX_ANIM),
        ];

        foreach (int textBankResourceId in textBankResourceIds)
            Load(textBankResourceId);

        Synchronize();

        foreach (int textBankResourceId in textBankResourceIds)
        {
            int index = ResourceID_To_Index(textBankResourceId);
            byte[] data = Array_Data[index];

            StringBuilder sb = new();
            using MemoryStream stream = new(data);
            using Reader reader = new(stream);
            while (stream.Position < stream.Length)
            {
                int length = reader.ReadByte();
                string str = reader.ReadString(length, Encoding.UTF8);
                sb.AppendLine(str);
            }

            File.WriteAllText(Path.Combine(outputPath, $"{index}.txt"), sb.ToString());
        }
    }
    public void DumpAllImages(string outputPath)
    {
        for (int archiveIndex = 0; archiveIndex < ARCHIVES_COUNT; archiveIndex++)
        {
            for (int imgIndex = 0; imgIndex < Archive_Information[archiveIndex].ImageResourcesCount; imgIndex++)
                Load(ResourceId.Create(imgIndex, RESOURCE_TYPE.IMAGE, archiveIndex));

            Synchronize();

            for (int imgIndex = 0; imgIndex < Archive_Information[archiveIndex].ImageResourcesCount; imgIndex++)
            {
                Texture2D img = GetImage(ResourceID_To_Index(ResourceId.Create(imgIndex, RESOURCE_TYPE.IMAGE, archiveIndex)));
                using FileStream stream = File.Create(Path.Combine(outputPath, $"{archiveIndex}_{imgIndex}.png"));
                img.SaveAsPng(stream, img.Width, img.Height);
            }
        }
    }
}