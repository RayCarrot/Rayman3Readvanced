using System;
using System.Collections.Generic;
using System.IO;
using BinarySerializer;

namespace GbaMonoGame.Rayman3.J2me;

// Replaces javax.microedition.rms.RecordStore
public class RecordStore : IDisposable
{
    #region Constructor

    private RecordStore(string recordStoreName, bool createIfNecessary)
    {
        Name = recordStoreName;

        // Use the shared context if the rom is loaded, otherwise create a temporary one
        if (J2meRom.IsLoaded)
        {
            Context = J2meRom.Context;
        }
        else
        {
            // NOTE: Hard-code the game version for now since it's the only one supported
            // Get the paths
            J2meRom.GetGamePaths(Rayman3J2meVersion.Rayman3_1_0_3_SonyEricssonS700_240x320, out string gameDirectory, out _, out string logName);

            // Create the serializer settings
            SerializerSettings settings = new()
            {
                // Don't cache on read
                IgnoreCacheOnRead = true,
            };

            // Create a serializer logger
            ISerializerLogger serializerLogger = Engine.Settings.Active.Debug.WriteSerializerLog
                ? new FileSerializerLogger(Engine.UserData.GetFile(Paths.GetSerializeLogFileName(logName)))
                : null;

            Context = new Context(gameDirectory, settings: settings, serializerLogger: serializerLogger, systemLogger: BinarySerializerSystemLogger.Create());
        }

        // Add the file
        string filePath = GetBinaryFilePath(Name);
        if (!Context.FileExists(filePath))
        {
            Context.AddFile(new LinearFile(Context, filePath)
            {
                IgnoreCacheOnRead = true,
                RecreateOnWrite = true,
            });
        }

        // Read the file
        using (Context)
        {
            if (((LinearFile)Context.GetRequiredFile(filePath)).SourceFileExists)
            {
                RecordStoreFile recordStoreFile = FileFactory.Read<RecordStoreFile>(Context, filePath);
                Records = new List<byte[]>(recordStoreFile.Records);
            }
            else if (createIfNecessary)
            {
                Records = [];
            }
            else
            {
                throw new Exception($"Record store {recordStoreName} does not exit");
            }
        }
    }

    #endregion

    #region Private Fields

    private const string DirectoryName = "RecordStore";
    private const string FileExtension = ".sav";

    #endregion

    #region Public Properties

    public Context Context { get; }
    public string Name { get; }
    public List<byte[]> Records { get; }
    public bool IsClosed { get; set; }
    public bool HasChanges { get; set; }

    #endregion

    #region Private Static Methods

    private static string GetBinaryFilePath(string recordStoreName)
    {
        return Path.Combine(DirectoryName, $"{recordStoreName}{FileExtension}");
    }

    private static string GetPhysicalDirectory()
    {
        return Path.Combine(J2meRom.GameDirectory, DirectoryName);
    }

    private static string GetPhysicalFilePath(string recordStoreName)
    {
        return Path.Combine(J2meRom.GameDirectory, DirectoryName, $"{recordStoreName}{FileExtension}");
    }

    #endregion

    #region Public Static Methods

    public static RecordStore openRecordStore(string recordStoreName, bool createIfNecessary)
    {
        return new RecordStore(recordStoreName, createIfNecessary);
    }

    public static string[] listRecordStores()
    {
        string directory = GetPhysicalDirectory();

        if (!Directory.Exists(directory))
            return [];

        string[] recordStores = Directory.GetFiles(directory, $"*{FileExtension}", SearchOption.TopDirectoryOnly);

        for (int i = 0; i < recordStores.Length; i++)
            recordStores[i] = Path.GetFileNameWithoutExtension(recordStores[i]);

        return recordStores;
    }

    // Custom method to avoid looping over every file when checking for a single one
    public static bool recordStoreExists(string recordStoreName)
    {
        string filePath = GetPhysicalFilePath(recordStoreName);
        return File.Exists(filePath);
    }

    public static void deleteRecordStore(string recordStoreName)
    {
        string filePath = GetPhysicalFilePath(recordStoreName);
        File.Delete(filePath);
    }

    #endregion

    #region Public Methods

    public byte[] getRecord(int recordId)
    {
        if (IsClosed)
            throw new Exception("The record store has been closed");

        int recordIndex = recordId - 1;

        if (recordIndex >= Records.Count) 
            return null;
        
        byte[] dstData = new byte[Records[recordIndex].Length];
        Array.Copy(Records[recordIndex], dstData, dstData.Length);
        return dstData;
    }

    public void setRecord(int recordId, byte[] newData, int offset, int numBytes)
    {
        if (IsClosed)
            throw new Exception("The record store has been closed");

        int recordIndex = recordId - 1;

        if (recordIndex < 0 || recordIndex >= Records.Count)
            throw new Exception($"Invalid record id {recordId} for a record store of {Records.Count} records");

        newData ??= [];
        byte[] dstData = new byte[numBytes];
        Array.Copy(newData, offset, dstData, 0, numBytes);
        Records[recordIndex] = newData;
        HasChanges = true;
    }

    public void addRecord(byte[] data, int offset, int numBytes)
    {
        if (IsClosed)
            throw new Exception("The record store has been closed");

        data ??= [];
        byte[] dstData = new byte[numBytes];
        Array.Copy(data, offset, dstData, 0, numBytes);
        Records.Add(dstData);
        HasChanges = true;
    }

    public int getNumRecords()
    {
        if (IsClosed)
            throw new Exception("The record store has been closed");

        return Records.Count;
    }

    public void closeRecordStore()
    {
        if (IsClosed)
            throw new Exception("The record store has been closed");

        IsClosed = true;

        if (HasChanges)
        {
            // Create the file data to serialize
            byte[][] records = new byte[Records.Count][];
            for (int i = 0; i < Records.Count; i++)
                records[i] = Records[i];
            RecordStoreFile recordStoreFile = new() { Records = records };

            // Save the file
            using Context context = Context;
            FileFactory.Write<RecordStoreFile>(context, GetBinaryFilePath(Name), recordStoreFile);
        }
    }

    public void Dispose()
    {
        if (!IsClosed)
            closeRecordStore();
    }

    #endregion
}